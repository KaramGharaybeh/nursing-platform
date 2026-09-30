using MediatR;
using NursingPlatform.Application.PreparationPackages.Reports.DTOs;

namespace NursingPlatform.Application.PreparationPackages.Reports.GetPackageAnalyticalReport;

public sealed record GetPackageAnalyticalReportQuery(Guid SessionId) : IRequest<PackageAnalyticalReportDto>;
