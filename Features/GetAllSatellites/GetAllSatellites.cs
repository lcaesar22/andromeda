using Andromeda.Domain;
using Andromeda.Infrastructure;

namespace Andromeda.Features.GetAllSatellites;

public record SatelliteResponse(
    int Id,
    string Name,
    int NoradId,
    SatelliteType Type,
    DateOnly LaunchDate,
    OrbitType Orbit,
    string CountryOfOrigin,
    bool IsActive);

public record PagedResponse<T>(List<T> Items, int Page, int PageSize, int TotalCount, int TotalPages);

public record SatelliteFilter(SatelliteType? Type, OrbitType? Orbit, bool? IsActive, string? CountryOfOrigin);

public static class GetAllSatellitesEndpoint
{
    public static void MapGetAllSatellites(this IEndpointRouteBuilder endpoint)
    {
        endpoint.MapGet("/satellites", async (
            int page , 
            int pageSize , 
            SatelliteType? type, 
            OrbitType? orbit, 
            bool? isActive, 
            string? countryOfOrigin,
            ISatelliteRepository repo) =>
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize is <= 0 or > 100 ? 20 : pageSize;

            var filter = new SatelliteFilter(type, orbit, isActive, countryOfOrigin);

            var (satellites, totalCount) = await repo.GetAllAsync(filter, page, pageSize);
            
            var response = new PagedResponse<SatelliteResponse>(satellites.Select(s => new SatelliteResponse(
            s.Id,
            s.Name,
            s.NoradId,
            s.Type, 
            s.LaunchDate, 
            s.Orbit, 
            s.CountryOfOrigin, 
            s.IsActive)).ToList(),
                page, pageSize, totalCount, (int)Math.Ceiling(totalCount / (double)pageSize));
            
            return Results.Ok(response);
        })
        .RequireAuthorization();
    }
}