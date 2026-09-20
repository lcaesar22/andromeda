using Andromeda.Domain;
using Andromeda.Features.GetAllSatellites;

namespace Andromeda.Infrastructure;

public interface ISatelliteRepository
{
    Task<Satellite> AddAsync(Satellite satellite);
    Task<Satellite?> GetByIdAsync(int id);
    Task<(List<Satellite> Items, int TotalCount)> GetAllAsync(SatelliteFilter filter, int page, int pageSize);
    Task UpdateAsync(Satellite satellite);
    Task DeleteAsync(Satellite satellite);
}