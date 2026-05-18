namespace Pharmacy_API.Supports
{
    public interface IRepository<T>
    {
        IUnitOfWork UnitOfWork { get; }

        T Add(T obj);
        void Update(T obj);
        void Remove(T obj);

        Task<T?> InsertAsync(T obj);
        Task<int> UpdateAsync(T obj);
        Task<int> DeleteAsync(object ids);
        Task<T?> GetAsync(object ids);
        Task<List<T>> GetAllAsync();
    }
}