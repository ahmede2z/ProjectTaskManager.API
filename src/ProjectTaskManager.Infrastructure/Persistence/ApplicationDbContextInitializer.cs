using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProjectTaskManager.Domain.Constants;

namespace ProjectTaskManager.Infrastructure.Persistence;

public sealed class ApplicationDbContextInitializer(
    ApplicationDbContext context,
    RoleManager<IdentityRole> roleManager,
    ILogger<ApplicationDbContextInitializer> logger)
{
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (context.Database.IsRelational())
                await context.Database.MigrateAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database");
            throw;
        }
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        try
        {
            await SeedRolesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private async Task SeedRolesAsync(CancellationToken cancellationToken)
    {
        foreach (var role in new[] { Roles.Admin, Roles.User })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}
