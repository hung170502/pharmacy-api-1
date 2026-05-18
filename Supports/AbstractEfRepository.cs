using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Pharmacy_API.Supports
{
    public class AbstractEfRepository<TDbContext, T>: IRepository<T> where TDbContext : DbContext where T : class
    {
        public TDbContext _db { get; }
        protected readonly ILogger _logger;

        public IUnitOfWork UnitOfWork
        {
            get
            {
                return (IUnitOfWork)_db;
            }
        }

        protected AbstractEfRepository(TDbContext db, ILogger logger)
        {
            _db = db;
            _logger = logger;
        }

        public T Add(T obj)
        {
            return _db.Set<T>().Add(obj).Entity;
        }

        public void Update(T obj)
        {
            _db.Set<T>().Update(obj);
        }

        public void Remove(T obj)
        {
            _db.Set<T>().Remove(obj);
        }

        public async Task<T?> InsertAsync(T obj)
        {
            await _db.Set<T>().AddAsync(obj);
            return await _db.SaveChangesAsync() == 0 ? null : obj;
        }

        public async Task<int> UpdateAsync(T obj)
        {
            _db.Entry<T>(obj).State = EntityState.Modified;
            return await _db.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(object ids)
        {
            var obj = await _db.Set<T>().FindAsync(ids);
            if (obj != null)
            {
                _db.Set<T>().Remove(obj);
                return await _db.SaveChangesAsync();
            }

            return 0;
        }

        public async Task<T?> GetAsync(object ids)
        {
            return await _db.Set<T>().FindAsync(ids);
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await _db.Set<T>().ToListAsync<T>();
        }
    }
}