using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PrintPlatform.Application.Abstractions;
using PrintPlatform.Domain.Dispatch;
using PrintPlatform.Domain.Identity;
using PrintPlatform.Domain.Orders;
using PrintPlatform.Domain.Shared;
using PrintPlatform.Infrastructure.Identity;
using PrintPlatform.Infrastructure.Data.Configurations;
using System.Linq.Expressions;

namespace PrintPlatform.Infrastructure.Data;

/// <summary>
/// Primary application DbContext. Hosts both the ASP.NET Identity credential tables
/// and the Identity domain aggregates (<see cref="User"/> and profiles). Dispatches
/// domain events after a successful commit and applies a global soft-delete filter.
/// </summary>
public sealed partial class AppDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IAppDbContext
{
    private readonly IDomainEventDispatcher? _dispatcher;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        IDomainEventDispatcher? dispatcher = null)
        : base(options)
        => _dispatcher = dispatcher;

    // -- Identity module (core; tied to IdentityDbContext) --------------------
    // Intentionally shadows IdentityUserContext.Users (ApplicationUser) — the domain
    // User aggregate is distinct from the ASP.NET Identity credential entity.
    public new DbSet<User> Users => Set<User>();
    public DbSet<CustomerProfile> CustomerProfiles => Set<CustomerProfile>();
    public DbSet<PrinterOwnerProfile> PrinterOwnerProfiles => Set<PrinterOwnerProfile>();

    // NOTE: per-module DbSets live in AppDbContext.<Module>.cs partial files so a
    // new module never edits this file. See AppDbContext.Orders.cs / .Dispatch.cs.

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Keep ASP.NET Identity tables on a dedicated schema, domain on "identity".
        builder.HasDefaultSchema("identity");
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Global conventions
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            // 1. Soft-delete global filter
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                builder.Entity(entityType.ClrType).HasQueryFilter(CreateIsDeletedFilter(entityType.ClrType));
            }

            foreach (var property in entityType.GetProperties())
            {
                // 2. Enum -> String conversion
                if (property.ClrType.IsEnum)
                {
                    var converterType = typeof(EnumToStringConverter<>).MakeGenericType(property.ClrType);
                    var converter = (ValueConverter)Activator.CreateInstance(converterType)!;
                    property.SetValueConverter(converter);
                }

                // 3. JSON columns for lists
                if (property.ClrType == typeof(List<string>) || property.ClrType == typeof(List<Guid>))
                {
                    // Get the generic method: Property<TProperty>(string propertyName)
                    var entityBuilder = builder.Entity(entityType.ClrType);
                    var propertyMethod = typeof(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder)
                        .GetMethods()
                        .First(m => m.Name == "Property" &&
                                    m.GetParameters().Length == 1 &&
                                    m.GetParameters()[0].ParameterType == typeof(string) &&
                                    m.ReturnType.IsGenericType &&
                                    m.ReturnType.GetGenericTypeDefinition() == typeof(Microsoft.EntityFrameworkCore.Metadata.Builders.PropertyBuilder<>));

                    // Call entityBuilder.Property<T>(property.Name)
                    var genericPropertyMethod = propertyMethod.MakeGenericMethod(property.ClrType);
                    var propertyBuilder = genericPropertyMethod.Invoke(entityBuilder, new object[] { property.Name });

                    // Find and call JsonConversionExtensions.HasJsonConversion<T>(propertyBuilder)
                    var extensionMethod = typeof(JsonConversionExtensions)
                        .GetMethod(nameof(JsonConversionExtensions.HasJsonConversion))!
                        .MakeGenericMethod(property.ClrType);

                    extensionMethod.Invoke(null, new object?[] { propertyBuilder });
                }
            }
        }

        // Concurrency tokens (ensuring they are set if not already in configs)
        builder.Entity<Order>().Property(o => o.RowVersion).IsRowVersion().HasColumnName("xmin").HasColumnType("xid");
        builder.Entity<JobAssignment>().Property(j => j.RowVersion).IsRowVersion();
    }

    private static LambdaExpression CreateIsDeletedFilter(Type type)
    {
        var parameter = Expression.Parameter(type, "e");
        var body = Expression.Equal(
            Expression.Property(parameter, nameof(ISoftDelete.IsDeleted)),
            Expression.Constant(false));
        return Expression.Lambda(body, parameter);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Collect aggregates with pending events BEFORE the commit.
        var aggregates = ChangeTracker
            .Entries<IAggregateRoot>()
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        if (_dispatcher is not null && aggregates.Count > 0)
            await _dispatcher.DispatchAndClearAsync(aggregates, cancellationToken);

        return result;
    }
}
