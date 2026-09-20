using Andromeda.Domain;
using Andromeda.Features.GetAllSatellites;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Andromeda.Infrastructure;

public class SatelliteRepository(AppDbContext db, IMemoryCache cache) : ISatelliteRepository
{
    private static string SatelliteCacheKey(int id) => $"satellite:{id}";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan ListCacheDuration = TimeSpan.FromMinutes(2);
    private const string ListVersionKey = "satellites:list-version";

    private int GetListVersion() => cache.GetOrCreate(ListVersionKey, _ => 0); // Read the current version during caching. Default to zero if unset.
    private void BumpListVersion() => cache.Set(ListVersionKey, GetListVersion() + 1); // Increment and reset GetListVersion in the cache 
    
    public async Task<Satellite> AddAsync(Satellite satellite)
    {
        db.Satellites.Add(satellite);
        await db.SaveChangesAsync();
        BumpListVersion();
        return satellite;
    }

    public async Task<Satellite?> GetByIdAsync(int id)
    {
        if (cache.TryGetValue(SatelliteCacheKey(id), out Satellite?  cached))
            return cached;
        
        var satellite = await db.Satellites.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
        
        if (satellite is not null)
            cache.Set(SatelliteCacheKey(id), satellite, CacheDuration);

        return satellite;
    }
    
    public async Task<(List<Satellite> Items, int TotalCount)> GetAllAsync(SatelliteFilter filter, int page, int pageSize)
    {
        var version = GetListVersion();
        var cacheKey = $"satellites:v{version}:pages={page}:size={pageSize}:type={filter.Type}:orbit={filter.Orbit}:active={filter.IsActive}:country={filter.CountryOfOrigin}";
        if (cache.TryGetValue(cacheKey, out (List<Satellite> Items, int TotalCount) cached))
            return cached;
        
        var query = db.Satellites.AsNoTracking().AsQueryable();
        
        if (filter.Type is { } type)
            query = query.Where(x => x.Type == type);
        
        if (filter.Orbit is { } orbit)
            query = query.Where(x => x.Orbit == orbit);
        
        if (filter.IsActive is { } isActive)
            query = query.Where(x => x.IsActive == isActive);
        
        if (!string.IsNullOrWhiteSpace(filter.CountryOfOrigin))
            query = query.Where(x => x.CountryOfOrigin == filter.CountryOfOrigin);
        
        var totalCount = await query.CountAsync();
        
        var items = await query.OrderBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var result = (items, totalCount);
        cache.Set(cacheKey, result, ListCacheDuration);

        return result;
    }

    public async Task UpdateAsync(Satellite satellite)
    {
        db.Satellites.Update(satellite);
        await db.SaveChangesAsync();
        cache.Remove(SatelliteCacheKey(satellite.Id));
        BumpListVersion();
    }

    public async Task DeleteAsync(Satellite satellite)
    {
        db.Satellites.Remove(satellite);
        await db.SaveChangesAsync();
        cache.Remove(SatelliteCacheKey(satellite.Id));
        BumpListVersion();
    }
}