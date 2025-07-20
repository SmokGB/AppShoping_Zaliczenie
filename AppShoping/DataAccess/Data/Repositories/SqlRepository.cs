using AppShoping.DataAccess.Data;
using AppShoping.DataAccess.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AppShoping.DataAccess.Data.Repositories
{
    public class SqlRepository<T> : IRepository<T> where T : class, IEntity, new()
    {
        private readonly ShopAppDbContext _dbContext;
        private readonly DbSet<T> _dbSet;

        public event EventHandler<T?>? ItemAdded;
        public event EventHandler<T?>? ProductDeleted;

        public SqlRepository(ShopAppDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<T>();
        }

        public IEnumerable<T> GetAll() => _dbSet.AsNoTracking();

        public T? GetById(int id) => _dbSet.Find(id);

      
        public void Remove(T item)
        {
            try
            {
                _dbSet.Remove(item);
                ProductDeleted?.Invoke(this, item);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing item: {ex.Message}");
            }
        }

        public void Add(T item)
        {
            try
            {
                _dbSet.Add(item);
                ItemAdded?.Invoke(this, item);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding item: {ex.Message}");
            }
        }

        public void Save()
        {
            try
            {
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving changes: {ex.Message}");
            }
        }

        public void Update(T item)
        {
            try
            {
                _dbContext.Entry(item).State = EntityState.Modified;
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error editing changes : {ex.Message}");
            }
        }
    }
}
