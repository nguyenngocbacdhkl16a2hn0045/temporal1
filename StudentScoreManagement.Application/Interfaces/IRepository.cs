using System.Linq.Expressions;

namespace StudentScoreManagement.Application.Interfaces;

public interface IRepository<TEntity> where TEntity : class
{
    IQueryable<TEntity> Query(bool asNoTracking = true);
    Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void Remove(TEntity entity);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
