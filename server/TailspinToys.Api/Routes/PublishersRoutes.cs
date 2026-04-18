/*
 * PublishersRoutes.cs
 * Defines HTTP routes related to the Publishers API endpoints.
 * Exposes an endpoint to return publisher id and name.
 */
namespace TailspinToys.Api.Routes;

using Microsoft.EntityFrameworkCore;
using TailspinToys.Api.Models;

/// <summary>
/// Provides mapping for publisher-related API routes.
/// </summary>
public static class PublishersRoutes
{
    /// <summary>
/// Maps the /api/publishers route group and endpoints to the application.
/// </summary>
public static void MapPublishersRoutes(this WebApplication app)
    {
        var group = app.MapGroup("/api/publishers");

        group.MapGet("/", async (TailspinToysContext db) =>
        {
            var publishers = await db.Publishers
                .AsNoTracking()
                .Select(p => new { id = p.Id, name = p.Name })
                .ToListAsync();

            return Results.Ok(publishers);
        });
    }
}
