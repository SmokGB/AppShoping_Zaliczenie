using AppShoping.DataAccess.Data.Entities;
using AppShoping.DataAccess.Data.Repositories;

namespace AppShoping.DataAccess.Data.Repositories.Extensions;

public interface IRepositoryExtensions
{
    void ExportFoodListToJsonFiles<T>(IRepository<T> repository) where T : class, IEntity;
    void ImportFoodListFromJson<T>(IRepository<T> repository) where T : class, IEntity;
    void WriteAllToConsole(IReadRepository<IEntity> allFoods);

}
