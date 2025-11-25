using Microsoft.AspNetCore.Builder;

namespace MagasinService.Infrastructure.Data.Extensions;

public static class DatabaseExtentions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<MagasinServiceDbContext>();

        context.Database.MigrateAsync().GetAwaiter().GetResult();

    }
}

