using Microsoft.EntityFrameworkCore;
using PrintPlatform.Domain.Marketplace;

namespace PrintPlatform.Application.Marketplace;

/// <summary>
/// Abstraction over the marketplace persistence context, exposed to the Application
/// layer so handlers do not depend on the concrete Infrastructure DbContext.
/// </summary>
public interface IMarketplaceDbContext
{
    DbSet<Printer> Printers { get; }
    DbSet<PrinterMaterial> PrinterMaterials { get; }
    DbSet<MaterialOption> MaterialOptions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
