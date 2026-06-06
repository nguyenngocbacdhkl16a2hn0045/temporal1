using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using StudentScoreManagement.Application.Interfaces;
using StudentScoreManagement.Infrastructure.Data;

namespace StudentScoreManagement.Infrastructure.Repositories;

public class EfRepository<TEntity> : IRepository<TEntity> where TEntity : class
{
    private readonly AppDbContext _dbContext;

    public EfRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IQueryable<TEntity> Query(bool asNoTracking = true)
    {
        var query = _dbContext.Set<TEntity>().AsQueryable();
        return asNoTracking ? query.AsNoTracking() : query;
    }

    public async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<TEntity>().FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<TEntity>().AnyAsync(predicate, cancellationToken);
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    public void Update(TEntity entity)
    {
        _dbContext.Set<TEntity>().Update(entity);
    }

    public void Remove(TEntity entity)
    {
        _dbContext.Set<TEntity>().Remove(entity);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
