using AppShoping.DataAccess.Data.Entities;
namespace AppShoping.DataAccess.Data.Repositories
{
    public interface IReadRepository<out T> where T : class, IEntity
    {
        IEnumerable<T> GetAll();
        T? GetById(int id);

    }
}
