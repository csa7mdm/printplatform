namespace PrintPlatform.Application.Abstractions;

/// <summary>
/// Abstraction over the background-job scheduler (Hangfire) for enqueuing model
/// geometry analysis. Keeps the Application layer free of a Hangfire dependency.
/// </summary>
public interface IModelAnalysisJobScheduler
{
    /// <summary>Enqueues geometry analysis for the given model file.</summary>
    void EnqueueAnalysis(Guid modelFileId);
}
