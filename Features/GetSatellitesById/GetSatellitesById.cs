using Andromeda.Domain;
using Andromeda.Infrastructure;

namespace Andromeda.Features.GetSatellitesById;

public record SatelliteResponse(
    int Id,
    string Name,
    int NoradId,
    SatelliteType Type,
    DateOnly LaunchDate,
    OrbitType Orbit,
    string CountryOfOrigin,
    bool IsActive);

public static class GetSatellitesByIdEndpoint
{
    public static void MapGetSatelliteById(this IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("/satellite/{id:int}", async (int id, ISatelliteRepository repo) =>
        {
            var satellite = await repo.GetByIdAsync(id);
            if (satellite is null)
                return Results.NotFound();

            var response = new SatelliteResponse(
                satellite.Id,
                satellite.Name,
                satellite.NoradId,
                satellite.Type,
                satellite.LaunchDate,
                satellite.Orbit,
                satellite.CountryOfOrigin,
                satellite.IsActive);
            
            return Results.Ok(response);
        })
        .RequireAuthorization();
    }
}