using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrintPlatform.Domain.Identity;
using PrintPlatform.Domain.Marketplace;
using PrintPlatform.Gamification.Data;
using PrintPlatform.Gamification.Models;
using PrintPlatform.Loyalty.Data;
using PrintPlatform.Loyalty.Models;
using PrintPlatform.Infrastructure.Identity;

namespace PrintPlatform.Infrastructure.Data;

public sealed class DatabaseSeeder
{
    private readonly AppDbContext _appDb;
    private readonly GamificationDbContext _gamificationDb;
    private readonly LoyaltyDbContext _loyaltyDb;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IConfiguration _configuration;

    public DatabaseSeeder(
        AppDbContext appDb,
        GamificationDbContext gamificationDb,
        LoyaltyDbContext loyaltyDb,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IConfiguration configuration)
    {
        _appDb = appDb;
        _gamificationDb = gamificationDb;
        _loyaltyDb = loyaltyDb;
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        await SeedRolesAsync();
        await SeedAdminAsync();
        await SeedMaterialsAsync();
        await SeedGamificationLevelsAsync();
        await SeedLoyaltyTiersAsync();
    }

    private async Task SeedRolesAsync()
    {
        var roles = Enum.GetNames<UserRole>();
        foreach (var roleName in roles)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new ApplicationRole { Name = roleName });
            }
        }
    }

    private async Task SeedAdminAsync()
    {
        var adminConfig = _configuration.GetSection("SeedData:Admin");
        var email = adminConfig["Email"] ?? "admin@printplatform.com";
        var password = adminConfig["Password"] ?? "Admin123!";
        var fullName = adminConfig["FullName"] ?? "System Admin";

        var adminUser = await _userManager.FindByEmailAsync(email);
        if (adminUser == null)
        {
            var user = User.Register(Guid.NewGuid(), email, "+201000000000", fullName, fullName, UserRole.Admin, Lang.English);
            adminUser = new ApplicationUser
            {
                Id = user.Id,
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = true
            };

            var result = await _userManager.CreateAsync(adminUser, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, UserRole.Admin.ToString());
                _appDb.Users.Add(user);
                await _appDb.SaveChangesAsync();
            }
        }
    }

    private async Task SeedMaterialsAsync()
    {
        if (await _appDb.Set<MaterialOption>().AnyAsync()) return;

        var materials = new List<MaterialOption>
        {
            MaterialOption.Create(MaterialType.PLA, "Galaxy Black", "#1A1A1A", 1.5m, 0.5m).Value,
            MaterialOption.Create(MaterialType.PLA, "Snow White", "#FFFFFF", 1.5m, 0.5m).Value,
            MaterialOption.Create(MaterialType.PETG, "Signal Orange", "#FF4500", 2.0m, 0.8m).Value,
            MaterialOption.Create(MaterialType.ABS, "Cool Grey", "#808080", 1.8m, 0.7m).Value,
            MaterialOption.Create(MaterialType.TPU, "Flex Red", "#FF0000", 3.5m, 1.5m).Value,
            MaterialOption.Create(MaterialType.Resin_Standard, "Clear Water", "#E0FFFF", 5.0m, 2.0m).Value
        };

        _appDb.Set<MaterialOption>().AddRange(materials);
        await _appDb.SaveChangesAsync();
    }

    private async Task SeedGamificationLevelsAsync()
    {
        if (await _gamificationDb.Levels.AnyAsync()) return;

        var levels = new List<Level>
        {
            new Level { Tier = 1, Name = "Novice", MinXP = 0 },
            new Level { Tier = 2, Name = "Apprentice", MinXP = 500 },
            new Level { Tier = 3, Name = "Journeyman", MinXP = 2000 },
            new Level { Tier = 4, Name = "Expert", MinXP = 7000 },
            new Level { Tier = 5, Name = "Master", MinXP = 20000 }
        };

        _gamificationDb.Levels.AddRange(levels);
        await _gamificationDb.SaveChangesAsync();
    }

    private async Task SeedLoyaltyTiersAsync()
    {
        if (await _loyaltyDb.Tiers.AnyAsync()) return;

        var tiers = new List<LoyaltyTier>
        {
            new LoyaltyTier { Name = "Bronze", MinPoints = 0, Multiplier = 1.0, SortOrder = 1, BenefitsJson = "{\"discount\": 0}" },
            new LoyaltyTier { Name = "Silver", MinPoints = 1000, Multiplier = 1.2, SortOrder = 2, BenefitsJson = "{\"discount\": 5}" },
            new LoyaltyTier { Name = "Gold", MinPoints = 5000, Multiplier = 1.5, SortOrder = 3, BenefitsJson = "{\"discount\": 10}" },
            new LoyaltyTier { Name = "Platinum", MinPoints = 15000, Multiplier = 2.0, SortOrder = 4, BenefitsJson = "{\"discount\": 15}" }
        };

        _loyaltyDb.Tiers.AddRange(tiers);
        await _loyaltyDb.SaveChangesAsync();
    }
}
