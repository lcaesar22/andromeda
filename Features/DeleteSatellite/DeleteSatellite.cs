using Andromeda.Infrastructure;

namespace Andromeda.Features.DeleteSatellite;

public static class DeleteSatelliteEndpoint
{
    public static void MapDeleteSatellite(this IEndpointRouteBuilder endpoint)
    {
        endpoint.MapDelete("/satellites/{id:int}", async (int id, ISatelliteRepository repo) =>
        {
            var satellite = await repo.GetByIdAsync(id);
            if (satellite is null)
                return Results.NotFound();

            await repo.DeleteAsync(satellite);
            return Results.NoContent();
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"));
    }
}