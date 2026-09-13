using System.Text;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using NursingPlatform.Application;
using NursingPlatform.Infrastructure;
using NursingPlatform.Infrastructure.Configuration;
using NursingPlatform.WebApi.Contracts;

namespace NursingPlatform.WebApi.Extensions;

public static class ServiceCollectionExtensions
{
    public const string LocalDevelopmentCorsPolicy = "LocalDevelopmentCors";

    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddEndpointsApiExplorer();
        services.AddApplication();
        services.AddInfrastructure(configuration, environment);
        services.AddPresentation(configuration, environment);

        return services;
    }

    public static IServiceCollection AddPresentation(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var healthChecks = services.AddHealthChecks();

        healthChecks.AddDbContextCheck<Infrastructure.Persistence.ApplicationDbContext>(
            name: "postgresql",
            tags: ["ready"]);

        var redisConnectionString = configuration["Redis:ConnectionString"];

        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            healthChecks.AddRedis(
                redisConnectionString,
                name: "redis",
                tags: ["ready"]);
        }

        var jwtSettings = configuration
            .GetSection(JwtSettings.SectionName)
            .Get<JwtSettings>();

        if (jwtSettings is null ||
            string.IsNullOrWhiteSpace(jwtSettings.Secret) ||
            string.IsNullOrWhiteSpace(jwtSettings.Issuer) ||
            string.IsNullOrWhiteSpace(jwtSettings.Audience))
        {
            throw new InvalidOperationException(
                "JWT authentication is not configured. Ensure 'Jwt:Secret', 'Jwt:Issuer', and 'Jwt:Audience' are set.");
        }

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.Secret))
        {
            KeyId = jwtSettings.KeyId
        };

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = signingKey
                };
            });

        services.AddAuthorization();

        if (environment.IsDevelopment())
        {
            var allowedOrigins = configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>()?
                .Where(origin => !string.IsNullOrWhiteSpace(origin))
                .Select(origin => origin.TrimEnd('/'))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray() ?? [];

            if (allowedOrigins.Length > 0)
            {
                services.AddCors(options =>
                {
                    options.AddPolicy(LocalDevelopmentCorsPolicy, policy =>
                    {
                        policy
                            .WithOrigins(allowedOrigins)
                            .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
                            .WithHeaders("Authorization", "Content-Type", "Accept");
                    });
                });
            }
        }

        services.AddOpenApi(options =>
        {
            options.AddSchemaTransformer((schema, context, cancellationToken) =>
            {
                var declaredType = context.JsonPropertyInfo?.PropertyType ?? context.JsonTypeInfo.Type;
                var underlyingType = Nullable.GetUnderlyingType(declaredType);
                var numericType = underlyingType ?? declaredType;
                var allowsNull = underlyingType is not null;
                var writesNumberAsString =
                    context.JsonPropertyInfo?.NumberHandling.HasValue == true &&
                    context.JsonPropertyInfo.NumberHandling.Value.HasFlag(JsonNumberHandling.WriteAsString);

                if (writesNumberAsString && IsIntegerType(numericType))
                {
                    schema.Type = JsonSchemaType.String | (allowsNull ? JsonSchemaType.Null : 0);
                    schema.Format = null;
                    schema.Pattern = "^-?(?:0|[1-9]\\d*)$";
                }
                else if (IsIntegerType(numericType))
                {
                    schema.Type = JsonSchemaType.Integer | (allowsNull ? JsonSchemaType.Null : 0);
                    schema.Pattern = null;
                }
                else if (IsNumberType(numericType))
                {
                    schema.Type = JsonSchemaType.Number | (allowsNull ? JsonSchemaType.Null : 0);
                    schema.Pattern = null;
                }

                if (context.JsonPropertyInfo is null && schema.Properties is not null)
                {
                    var required = context.JsonTypeInfo.Properties
                        .Where(IsRequiredProperty)
                        .Select(property => property.Name)
                        .Where(schema.Properties.ContainsKey)
                        .ToHashSet(StringComparer.Ordinal);

                    if (required.Count > 0)
                    {
                        schema.Required = required;
                    }
                }

                return Task.CompletedTask;
            });

            options.AddOperationTransformer((operation, context, cancellationToken) =>
            {
                var metadata = context.Description.ActionDescriptor.EndpointMetadata;
                var isAnonymous = metadata.OfType<IAllowAnonymous>().Any();
                var isProtected = metadata.OfType<IAuthorizeData>().Any();

                if (isProtected && !isAnonymous)
                {
                    operation.Security ??= [];
                    operation.Security.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
                    });

                    operation.Responses ??= new OpenApiResponses();
                    operation.Responses.TryGetValue(
                        StatusCodes.Status401Unauthorized.ToString(),
                        out var unauthorized);

                    var challengeResponse = new OpenApiResponse
                    {
                        Description = unauthorized?.Description ?? "Unauthorized",
                        Content = null,
                        Headers = unauthorized?.Headers is null
                            ? new Dictionary<string, IOpenApiHeader>()
                            : new Dictionary<string, IOpenApiHeader>(unauthorized.Headers),
                        Links = unauthorized?.Links,
                        Extensions = unauthorized?.Extensions
                    };
                    challengeResponse.Headers["WWW-Authenticate"] = new OpenApiHeader
                    {
                        Description = "Bearer authentication challenge."
                    };
                    operation.Responses[StatusCodes.Status401Unauthorized.ToString()] = challengeResponse;
                }

                if (operation.Responses is not null)
                {
                    foreach (var response in operation.Responses.Values.OfType<OpenApiResponse>())
                    {
                        if (response.Content is not null &&
                            response.Content.TryGetValue("application/problem+json", out var mediaType) &&
                            mediaType.Schema is OpenApiSchemaReference schemaReference &&
                            schemaReference.Reference.Id == "HttpValidationProblemDetails")
                        {
                            mediaType.Schema =
                                new OpenApiSchemaReference("ValidationProblemDetails", context.Document);
                        }
                    }
                }

                return Task.CompletedTask;
            });

            options.AddDocumentTransformer(async (document, context, cancellationToken) =>
            {
                var securityScheme = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "JWT Authorization header using the Bearer scheme."
                };

                var components = document.Components ??= new OpenApiComponents();
                components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                components.SecuritySchemes["Bearer"] = securityScheme;
                components.Schemas ??= new Dictionary<string, IOpenApiSchema>();
                components.Schemas["ProblemDetails"] = await context.GetOrCreateSchemaAsync(
                    typeof(ProblemDetailsContract),
                    null,
                    cancellationToken);
                components.Schemas["ValidationProblemDetails"] = await context.GetOrCreateSchemaAsync(
                    typeof(ValidationProblemDetailsContract),
                    null,
                    cancellationToken);
                components.Schemas["CodedProblemDetails"] = await context.GetOrCreateSchemaAsync(
                    typeof(CodedProblemDetailsContract),
                    null,
                    cancellationToken);
                components.Schemas["RetryableProblemDetails"] = await context.GetOrCreateSchemaAsync(
                    typeof(RetryableProblemDetailsContract),
                    null,
                    cancellationToken);

                AddResponseHeader(document, "CreateMyPaymentOrder", "201", "Location", "URI of the created payment order.");
                AddResponseHeader(document, "StartMyPaymentCheckout", "200", "Cache-Control", "Always `no-store`.");
                AddResponseHeader(document, "CompleteSandboxPaymentCheckout", "200", "Cache-Control", "Always `no-store`.");
                AddResponseHeader(document, "StartMyPaymentCheckout", "409", "Retry-After", "Delay in seconds before retrying checkout initialization.");
                SetResponseSchema(document, "StartMyPaymentCheckout", "409", "RetryableProblemDetails");
                SetResponseSchema(document, "StartPackageExamSession", "409", "CodedProblemDetails");
                SetResponseSchema(document, "GetMyPackageAnalyticalReport", "409", "CodedProblemDetails");

            });
        });

        return services;
    }

    private static void AddResponseHeader(
        OpenApiDocument document,
        string operationId,
        string statusCode,
        string headerName,
        string description)
    {
        var operation = document.Paths
            .SelectMany(path => path.Value.Operations ?? [])
            .Select(pair => pair.Value)
            .Single(candidate => candidate.OperationId == operationId);
        if (operation.Responses?[statusCode] is not OpenApiResponse response)
        {
            throw new InvalidOperationException(
                $"OpenAPI response {statusCode} was not found for operation {operationId}.");
        }

        response.Headers ??= new Dictionary<string, IOpenApiHeader>();
        response.Headers[headerName] = new OpenApiHeader { Description = description };
    }

    private static void SetResponseSchema(
        OpenApiDocument document,
        string operationId,
        string statusCode,
        string schemaName)
    {
        var operation = document.Paths
            .SelectMany(path => path.Value.Operations ?? [])
            .Select(pair => pair.Value)
            .Single(candidate => candidate.OperationId == operationId);
        if (operation.Responses?[statusCode] is not OpenApiResponse response)
        {
            throw new InvalidOperationException(
                $"OpenAPI response {statusCode} was not found for operation {operationId}.");
        }

        response.Content ??= new Dictionary<string, OpenApiMediaType>();
        response.Content["application/problem+json"] = new OpenApiMediaType
        {
            Schema = new OpenApiSchemaReference(schemaName, document)
        };
    }

    private static bool IsIntegerType(Type type) =>
        type == typeof(byte) ||
        type == typeof(sbyte) ||
        type == typeof(short) ||
        type == typeof(ushort) ||
        type == typeof(int) ||
        type == typeof(uint) ||
        type == typeof(long) ||
        type == typeof(ulong);

    private static bool IsNumberType(Type type) =>
        type == typeof(float) ||
        type == typeof(double) ||
        type == typeof(decimal);

    private static bool IsRequiredProperty(JsonPropertyInfo property)
    {
        if (property.IsRequired)
        {
            return true;
        }

        if (property.PropertyType.IsValueType)
        {
            return Nullable.GetUnderlyingType(property.PropertyType) is null;
        }

        return property.AttributeProvider is PropertyInfo propertyInfo &&
            new NullabilityInfoContext().Create(propertyInfo).ReadState == NullabilityState.NotNull;
    }

}
