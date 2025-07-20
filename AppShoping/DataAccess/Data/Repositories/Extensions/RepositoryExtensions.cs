using AppShoping.DataAccess.Data.Entities;
using AppShoping.DataAccess.Data.Repositories;
using System.Text.Json;

namespace AppShoping.DataAccess.Data.Repositories.Extensions
{
    public static class RepositoryExtensions
    {
        public static void ExportFoodListToJsonFiles<T>(this IRepository<T> repository)
            where T : class, IEntity
        {
            List<T> products = repository.GetAll().ToList();

            if (products.Count == 0)
            {
                Console.WriteLine("Brak produktów do eksportu.");
                return;
            }
            var json = JsonSerializer.Serialize(products);
            File.WriteAllText("product.json", json);
        }

        public static void ImportFoodListFromJson<T>(this IRepository<T> repository)
             where T : class, IEntity
        {
            if (!File.Exists("product.json"))
            {
                Console.WriteLine("Plik 'product.json' nie istnieje.");
                return;
            }

            try
            {
                var data = File.ReadAllText("product.json");
                var items = JsonSerializer.Deserialize<List<T>>(data);

                if (items != null)
                {
                    foreach (var item in items)
                    {
                        repository.Add(item);
                    }
                    repository.Save();
                }
                else
                {
                    Console.WriteLine("Deserializacja zwróciła pustą listę.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Wystąpił błąd podczas importu: {ex.Message}");
            }
        }

        public static void WriteAllToConsole(this IReadRepository<IEntity> allFoods)
        {
            var items = allFoods.GetAll();
            int counter = items.Count();

            if (counter > 0)
            {
                foreach (var item in items)
                {
                    Console.WriteLine(item);
                }
            }
            else
            {
                Console.WriteLine("Pusta lista zakupów  < Powrót do Menu - Wciśnij dowolny klawisz>");
                Console.ReadKey();
            }
        }
    }
}
