using Andromeda.Domain;
using Andromeda.Infrastructure;
using FluentValidation;

namespace Andromeda.Features.UpdateSatellite;

public record UpdateSatelliteRequest(
    string Name,
    SatelliteType Type,
    DateOnly LaunchDate,
    OrbitType Orbit,
    string CountryOfOrigin,
    bool IsActive);

public record SatelliteResponse(
    int Id,
    string Name,
    int NoradId,
    SatelliteType Type,
    DateOnly LaunchDate,
    OrbitType Orbit,
    string CountryOfOrigin,
    bool IsActive);

public static class UpdateSatelliteEndpoint
{
    public static void MapUpdateSatellite(this IEndpointRouteBuilder endpoint)
    {
        endpoint.MapPut("/satellites/{id:int}",
            async (int id, UpdateSatelliteRequest request, IValidator<UpdateSatelliteRequest> validator , ISatelliteRepository repo) =>
            {
                // Validate the update request
                if (await validator.ValidateAndReturnProblemsAsync(request) is { } problem)
                    return problem;
                
                var satellite = await repo.GetByIdAsync(id);
                if (satellite is null)
                    return Results.NotFound();

                satellite.Name = request.Name;
                satellite.Type = request.Type;
                satellite.LaunchDate = request.LaunchDate;
                satellite.Orbit = request.Orbit;
                satellite.CountryOfOrigin = request.CountryOfOrigin;
                satellite.IsActive = request.IsActive;
                
                await repo.UpdateAsync(satellite);
                
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
            .RequireAuthorization(policy => policy.RequireRole("Admin"));
    }
}

// Fluent validation for updating satellite
public class UpdateSatelliteValidator : AbstractValidator<UpdateSatelliteRequest>
{
    public UpdateSatelliteValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Orbit).IsInEnum();
        RuleFor(x => x.LaunchDate).NotEmpty().LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Launch date cannot be in the future");
        RuleFor(x => x.CountryOfOrigin).NotEmpty().MaximumLength(100);
    }
}