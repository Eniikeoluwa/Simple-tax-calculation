using Nova.Infrastructure;

namespace Nova.API.Application.Services.Data;

public abstract class BaseDataService
{
    protected readonly AppDbContext _context;

    protected BaseDataService(AppDbContext context)
    {
        _context = context;
    }

    protected async Task<T?> FindAsync<T>(string id) where T : class
    {
        return await _context.FindAsync<T>(id) as T;
    }

    protected async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
