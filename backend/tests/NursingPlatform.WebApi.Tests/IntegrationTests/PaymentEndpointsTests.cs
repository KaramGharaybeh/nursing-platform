using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using NursingPlatform.Application.Authorization;
using NursingPlatform.Application.Common.Models;
using NursingPlatform.Application.Payments.Admin.Products;
using NursingPlatform.Application.Payments.Commands.CancelMyPaymentOrder;
using NursingPlatform.Application.Payments.Commands.CreateMyPaymentOrder;
using NursingPlatform.Application.Payments.DTOs;
using NursingPlatform.Application.Payments.Queries.GetMyPaymentOrder;
using NursingPlatform.Application.Payments.Queries.GetPaymentProduct;
using NursingPlatform.Application.Payments.Queries.ListMyPaymentOrders;
using NursingPlatform.Application.Payments.Queries.ListPaymentProducts;
using NursingPlatform.Domain.Payments;

namespace NursingPlatform.WebApi.Tests.IntegrationTests;

[Collection(WebApiTestCollection.Name)]
public class PaymentEndpointsTests
{
    private static readonly (string Method, string Path)[] PaymentEndpoints =
    [
        ("GET", "/api/v1/payment/products"),
        ("GET", "/api/v1/payment/products/11111111-1111-1111-1111-111111111111"),
        ("POST", "/api/v1/me/nurse-profile/payment/orders"),
        ("GET", "/api/v1/me/nurse-profile/payment/orders"),
        ("GET", "/api/v1/me/nurse-profile/payment/orders/11111111-1111-1111-1111-111111111111"),
        ("POST", "/api/v1/me/nurse-profile/payment/orders/11111111-1111-1111-1111-111111111111/cancel"),
        ("GET", "/api/v1/admin/payment/products"),
        ("GET", "/api/v1/admin/payment/products/11111111-1111-1111-1111-111111111111"),
        ("POST", "/api/v1/admin/payment/products"),
        ("PUT", "/api/v1/admin/payment/products/11111111-1111-1111-1111-111111111111"),
        ("POST", "/api/v1/admin/payment/products/11111111-1111-1111-1111-111111111111/archive"),
        ("POST", "/api/v1/admin/payment/products/11111111-1111-1111-1111-111111111111/restore")
    ];

    private static readonly string[] ForbiddenJsonPatterns =
    [
        "\"userId\"",
        "\"passwordHash\"",
        "\"roles\"",
        "\"permissions\"",
        "\"accessToken\"",
        "\"refreshToken\"",
        "\"tokenHash\"",
        "\"provider\"",
        "\"card\"",
        "\"webhook\"",
        "\"secret\"",
        "\"nurseProfile\"",
        "\"paymentProduct\"",
        "\"paymentOrder\"",
        "\"examAccessGrant\""
    ];

    private readonly HttpClient _client;
    private readonly Mock<ISender> _senderMock;
    private readonly Mock<IPermissionService> _permissionServiceMock;

    public PaymentEndpointsTests(WebApiTestFactory factory)
    {
        _senderMock = factory.SenderMock;
        _permissionServiceMock = factory.PermissionServiceMock;
        _senderMock.Reset();
        _permissionServiceMock.Reset();
        _client = factory.CreateClient();
    }

    [Theory]
    [MemberData(nameof(PaymentEndpointData))]
    public async Task PaymentEndpoints_WithoutJwt_ReturnUnauthorized(string method, string path)
    {
        var response = await _client.SendAsync(CreateRequest(method, path));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PaymentProductCatalog_UseRequireAuthorizationOnly_WithoutPermissionSetup()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<ListPaymentProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<PaymentProductDto> { Items = [], Page = 1, PageSize = 20 });

        var response = await _client.GetAsync("/api/v1/payment/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        _permissionServiceMock.Verify(s => s.GetUserPermissionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PaymentProductCatalog_DoesNotRequireNurseRole()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.Is<GetPaymentProductQuery>(q => q.Id != Guid.Empty), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateProductDto());

        var response = await _client.GetAsync($"/api/v1/payment/products/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AdminPaymentProductRead_RequiresExamsViewPermission()
    {
        AuthorizeWith(Permissions.Exams.View);
        _senderMock
            .Setup(s => s.Send(It.IsAny<ListAdminPaymentProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<PaymentProductDto> { Items = [], Page = 1, PageSize = 20 });

        var response = await _client.GetAsync("/api/v1/admin/payment/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AdminPaymentProductWrite_RequiresExamsEditPermission()
    {
        AuthorizeWith(Permissions.Exams.Edit);
        _senderMock
            .Setup(s => s.Send(It.IsAny<CreateAdminPaymentProductCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateProductDto());

        var response = await _client.PostAsJsonAsync("/api/v1/admin/payment/products", new
        {
            examId = Guid.NewGuid(),
            name = "Exam Access",
            currency = "USD",
            unitAmountMinor = 1000,
            isActive = true
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_WithValidRequest_SendsCommand()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        var productId = Guid.NewGuid();
        _senderMock
            .Setup(s => s.Send(It.Is<CreateMyPaymentOrderCommand>(c => c.Request.ProductId == productId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateOrderDto());

        var response = await _client.PostAsJsonAsync("/api/v1/me/nurse-profile/payment/orders", new { productId });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_RequestContainsOnlyProductId()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<CreateMyPaymentOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateOrderDto());

        await _client.PostAsJsonAsync("/api/v1/me/nurse-profile/payment/orders", new { productId = Guid.NewGuid(), quantity = 99 });

        _senderMock.Verify(s => s.Send(It.Is<CreateMyPaymentOrderCommand>(c => c.Request.ProductId != Guid.Empty), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListOrders_WithPaginationAndStatus_SendsQuery()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.Is<ListMyPaymentOrdersQuery>(q => q.Page == 2 && q.PageSize == 5 && q.Status == PaymentOrderStatus.PendingPayment), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<PaymentOrderDto> { Items = [], Page = 2, PageSize = 5 });

        var response = await _client.GetAsync("/api/v1/me/nurse-profile/payment/orders?page=2&pageSize=5&status=PendingPayment");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetOrder_WhenHidden_ReturnsNotFound()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetMyPaymentOrderQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Payment order was not found."));

        var response = await _client.GetAsync($"/api/v1/me/nurse-profile/payment/orders/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CancelOrder_WithConflict_ReturnsConflict()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<CancelMyPaymentOrderCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Payment order has expired."));

        var response = await _client.PostAsync($"/api/v1/me/nurse-profile/payment/orders/{Guid.NewGuid()}/cancel", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task PaymentValidationFailure_ReturnsValidationProblemDetails()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<CreateMyPaymentOrderCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException([new ValidationFailure("ProductId", "ProductId is required.")]));

        var response = await _client.PostAsJsonAsync("/api/v1/me/nurse-profile/payment/orders", new { productId = Guid.NewGuid() });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task PaymentEndpoints_WithInvalidGuid_ReturnBadRequestAndSenderNotCalled()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());

        var response = await _client.GetAsync("/api/v1/payment/products?examId=not-a-guid");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        _senderMock.Verify(s => s.Send(It.IsAny<ListPaymentProductsQuery>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PaymentJson_DoesNotExposeProviderFieldsCardDataAccountInternalsOrEntities()
    {
        NurseEndpointTestAuth.Authorize(_client, Guid.NewGuid());
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetMyPaymentOrderQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateOrderDto());

        var response = await _client.GetAsync($"/api/v1/me/nurse-profile/payment/orders/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        foreach (var pattern in ForbiddenJsonPatterns)
        {
            Assert.DoesNotContain(pattern, json, StringComparison.OrdinalIgnoreCase);
        }
    }

    public static IEnumerable<object[]> PaymentEndpointData()
    {
        return PaymentEndpoints.Select(endpoint => new object[] { endpoint.Method, endpoint.Path });
    }

    private void AuthorizeWith(params string[] permissions)
    {
        var userId = Guid.NewGuid();
        NurseEndpointTestAuth.Authorize(_client, userId);
        _permissionServiceMock
            .Setup(s => s.GetUserPermissionsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions.ToHashSet());
    }

    private static PaymentProductDto CreateProductDto()
    {
        return new PaymentProductDto
        {
            Id = Guid.NewGuid(),
            Type = "ExamAccess",
            ExamId = Guid.NewGuid(),
            ExamTitle = "NCLEX",
            Name = "Exam Access",
            Currency = "USD",
            UnitAmountMinor = 1000,
            IsActive = true
        };
    }

    private static PaymentOrderDto CreateOrderDto()
    {
        return new PaymentOrderDto
        {
            Id = Guid.NewGuid(),
            Status = "PendingPayment",
            Currency = "USD",
            TotalAmountMinor = 1000,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            Items =
            [
                new PaymentOrderItemDto
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    ProductName = "Exam Access",
                    ProductType = "ExamAccess",
                    ExamId = Guid.NewGuid(),
                    Currency = "USD",
                    UnitAmountMinor = 1000,
                    Quantity = 1,
                    LineTotalAmountMinor = 1000
                }
            ]
        };
    }

    private static HttpRequestMessage CreateRequest(string method, string path)
    {
        return new HttpRequestMessage(new HttpMethod(method), path)
        {
            Content = method is "POST" or "PUT"
                ? new StringContent("{}", Encoding.UTF8, "application/json")
                : null
        };
    }
}
