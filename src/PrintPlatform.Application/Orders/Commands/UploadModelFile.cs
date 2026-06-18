using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using PrintPlatform.Application.Abstractions;
using PrintPlatform.Domain.Orders;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Application.Orders.Commands;

/// <summary>
/// Uploads a 3D model file to object storage (MinIO/S3) and persists a <see cref="ModelFile"/>
/// record in the <see cref="ModelFileStatus.AnalysisPending"/> state. The
/// <see cref="ModelFileUploadedEvent"/> raised by the aggregate triggers geometry analysis.
/// </summary>
public sealed record UploadModelFileCommand(
    Guid CustomerUserId,
    Stream Content,
    string OriginalFileName,
    long FileSizeBytes,
    string ContentType) : IRequest<Result<ModelFileDto>>;

public sealed class UploadModelFileValidator : AbstractValidator<UploadModelFileCommand>
{
    public const long MaxFileSizeBytes = 50L * 1024 * 1024; // 50 MB

    public UploadModelFileValidator()
    {
        RuleFor(x => x.CustomerUserId).NotEmpty();
        RuleFor(x => x.OriginalFileName).NotEmpty().MaximumLength(260);
        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0).WithMessage("The uploaded file is empty.")
            .LessThanOrEqualTo(MaxFileSizeBytes).WithMessage("The uploaded file exceeds the 50 MB limit.");
    }
}

public sealed class UploadModelFileHandler
    : IRequestHandler<UploadModelFileCommand, Result<ModelFileDto>>
{
    private readonly IAppDbContext _db;
    private readonly IFileStorageService _storage;
    private readonly IModelAnalysisJobScheduler _jobs;
    private readonly ILogger<UploadModelFileHandler> _logger;

    public UploadModelFileHandler(
        IAppDbContext db,
        IFileStorageService storage,
        IModelAnalysisJobScheduler jobs,
        ILogger<UploadModelFileHandler> logger)
    {
        _db = db;
        _storage = storage;
        _jobs = jobs;
        _logger = logger;
    }

    public async Task<Result<ModelFileDto>> Handle(
        UploadModelFileCommand request, CancellationToken cancellationToken)
    {
        var formatResult = ResolveFormat(request.OriginalFileName);
        if (formatResult.IsFailure)
            return Result.Failure<ModelFileDto>(formatResult.Error);

        // Object key partitioned by customer; binary lives only in storage, never the DB.
        var extension = Path.GetExtension(request.OriginalFileName).TrimStart('.').ToLowerInvariant();
        var objectKey = $"models/{request.CustomerUserId}/{Guid.NewGuid():N}.{extension}";

        await _storage.UploadAsync(request.Content, objectKey, request.ContentType, cancellationToken);

        var model = ModelFile.Create(
            request.CustomerUserId,
            objectKey,
            request.OriginalFileName,
            request.FileSizeBytes,
            formatResult.Value);
        model.MarkAnalysisPending();

        _db.ModelFiles.Add(model);
        await _db.SaveChangesAsync(cancellationToken);

        // Enqueue out-of-band analysis after the row is committed.
        _jobs.EnqueueAnalysis(model.Id);

        _logger.LogInformation(
            "Model file {ModelFileId} uploaded for customer {CustomerId} at key {Key}",
            model.Id, request.CustomerUserId, objectKey);

        return ModelFileMapper.ToDto(model);
    }

    private static Result<FileFormat> ResolveFormat(string fileName)
    {
        var ext = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
        return ext switch
        {
            "stl" => FileFormat.Stl,
            "3mf" => FileFormat.ThreeMF,
            "obj" => FileFormat.Obj,
            _ => Result.Failure<FileFormat>(OrderErrors.UnsupportedFormat(ext)),
        };
    }
}
