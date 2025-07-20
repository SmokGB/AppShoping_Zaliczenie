using AppShoping.ApplicationServices.Components.csvReader.Extensions;
using AppShoping.ApplicationServices.Components.csvReader.Models;

namespace AppShoping.ApplicationServices.Components.csvReader
{
    public class CsvReader : ICsvReader
    {
        public List<Product> ProcessProduct(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new List<Product>();

            }

            var products = File.ReadAllLines(filePath)
                .Skip(1)
                .Where(x => x.Length > 0)
                .Select(x =>
                {
                    var columns = x.Split(',');
                    return new Product()
                    {
                        ProductName = columns[0],
                        BioProduct = bool.Parse(columns[1]),
                    };

                });
            return products.ToList();

        }

        public  List<Purchase> ToProcessStatisc(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return  new List<Purchase>();
            }

            var products = File.ReadAllLines(filePath)
                .Skip(1)
                .Where(x => x.Length > 0).ToProcessStatistic();

            return products.ToList();   
        }
    }
}