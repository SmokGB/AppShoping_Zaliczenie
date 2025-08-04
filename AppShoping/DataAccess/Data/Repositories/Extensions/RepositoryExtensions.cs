using AppShoping.DataAccess.Data.Entities;
using System.Xml.Serialization;
using System.Text.Json;
using System.Text;

namespace AppShoping.DataAccess.Data.Repositories.Extensions
{
    public static class RepositoryExtensions
    {

        public static void ImportFoodListFromJson<T>(this IRepository<T> repository)
             where T : class, IEntity
        {
            if (!File.Exists("products.json"))
            {
                Console.WriteLine("Plik 'product.json' nie istnieje.");
                return;
            }

            try
            {
                var data = File.ReadAllText("products.json");
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



        private static List<T>? GetProductsOrNotifyEmpty<T>(IRepository<T> repository) where T : class, IEntity
        {
            List<T> products = repository.GetAll().ToList();

            if (products.Count == 0)
            {
                Console.WriteLine("Brak produktów do eksportu.");
                return null;
            }

            return products;
        }

        public static void ExportFoodListToJsonFiles<T>(this IRepository<T> repository)
            where T : class, IEntity
        {
            var products = GetProductsOrNotifyEmpty(repository);
            if (products == null) return;

            var json = JsonSerializer.Serialize(products);
            File.WriteAllText("products.json", json, Encoding.UTF8);

        }

        public static void ExportFoodListToXMLFiles<T>(this IRepository<T> repository)
            where T : class, IEntity
        {
            var products = GetProductsOrNotifyEmpty(repository);
            if (products == null) return;

            XmlSerializer serializer = new XmlSerializer(typeof(List<T>));
            using (var writer = new StreamWriter("products.xml", false, Encoding.UTF8))
            {
                serializer.Serialize(writer, products);
            }

        }
    }



}