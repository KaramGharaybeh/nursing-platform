using MediatR;
using NursingPlatform.Application.PreparationPackages.ExamSessions.DTOs;

namespace NursingPlatform.Application.PreparationPackages.ExamSessions.StartPackageExamSession;

public sealed record StartPackageExamSessionCommand(Guid EntitlementId) : IRequest<PackageExamSessionStartDto>;
