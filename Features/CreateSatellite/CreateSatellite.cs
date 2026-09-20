using Andromeda.Domain;
using Andromeda.Infrastructure;
using FluentValidation;

namespace Andromeda.Features.CreateSatellite;

public record CreateSatelliteRequest(
    string Name,
    int NoradId,
    SatelliteType Type,
    DateOnly LaunchDate,
    OrbitType Orbit,
    string CountryOfOrigin);

public record SatelliteResponse(
    int Id,
    string Name,
    int NoradId,
    SatelliteType Type,
    DateOnly LaunchDate,
    OrbitType Orbit,
    string CountryOfOrigin,
    bool IsActive);

public static class CreateSatelliteEndpoint
{
    public static void MapCreateSatellite(this IEndpointRouteBuilder endpoint)
    {
        endpoint.MapPost("/satellites", HandleAsync)
            .RequireAuthorization(policy => policy.RequireRole("Admin"));
        
    }

    public static async Task<IResult> HandleAsync(CreateSatelliteRequest request,
        IValidator<CreateSatelliteRequest> validator, ISatelliteRepository repo)
    {
        if (await validator.ValidateAndReturnProblemsAsync(request) is { } problem)
            return problem;

        var satellite = new Satellite
        {
            Name = request.Name,
            NoradId = request.NoradId,
            Type = request.Type,
            LaunchDate =  request.LaunchDate,
            Orbit = request.Orbit,
            CountryOfOrigin = request.CountryOfOrigin,
        };
        
        var created = await repo.AddAsync(satellite);
        var response = new SatelliteResponse(
            created.Id, created.Name,created.NoradId, created.Type, created.LaunchDate, created.Orbit, created.CountryOfOrigin, created.IsActive);
        
        return Results.Created($"/satellites/{created.Id}", response);
    }
}

// Validation for fluent validation
public class CreateSatelliteValidator : AbstractValidator<CreateSatelliteRequest>
{
    public CreateSatelliteValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NoradId).GreaterThan(0).WithMessage("Norad id must be greater than 0");
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Orbit).IsInEnum();
        RuleFor(x => x.LaunchDate).NotEmpty().LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Launch date must not be in the future");
        RuleFor(x => x.CountryOfOrigin).NotEmpty().MaximumLength(100);
    }
}