using PrintPlatform.Application.Marketplace;

namespace PrintPlatform.Infrastructure.Marketplace;

/// <summary>
/// Placeholder <see cref="IJobRequirementsProvider"/> until the Orders module is wired in.
/// Returns no requirements (job not found). Replace the registration once Orders ships.
/// </summary>
public sealed class NullJobRequirementsProvider : IJobRequirementsProvider
{
    public Task<JobRequirements?> GetAsync(Guid jobId, CancellationToken cancellationToken = default)
        => Task.FromResult<JobRequirements?>(null);
}

/// <summary>
/// Placeholder <see cref="IPrinterLoadProvider"/> until live job tracking exists.
/// Reports zero active jobs for every printer (best-case load score).
/// </summary>
public sealed class NullPrinterLoadProvider : IPrinterLoadProvider
{
    public Task<IReadOnlyDictionary<Guid, int>> GetActiveJobCountsAsync(
        IEnumerable<Guid> printerIds,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyDictionary<Guid, int> empty =
            new Dictionary<Guid, int>();
        return Task.FromResult(empty);
    }
}
