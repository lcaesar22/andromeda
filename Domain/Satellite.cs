namespace Andromeda.Domain;

public class Satellite
{
    public int Id { get; set; } 
    public required string Name { get; set; }
    public required int NoradId { get; set; }
    public SatelliteType Type { get; set; }
    public DateOnly LaunchDate { get; set; }
    public OrbitType Orbit { get; set; }
    public required string CountryOfOrigin { get; set; }
    public bool IsActive { get; set; } = true;
}