using AppShoping.ApplicationServices.Components.csvReader.Models;
using System.Globalization;

namespace AppShoping.ApplicationServices.Components.csvReader.Extensions
{
    public static class ProductExtensions
    {
        public static IEnumerable<Purchase> ToProcessStatistic(this IEnumerable<string> source)
        {
            foreach (var line in source)
            {
                var columns = line.Split(',');
                yield return new Purchase()
                {
                    Name = columns[0],
                    Price = decimal.Parse(columns[1],CultureInfo.InvariantCulture),
                    BioFood = bool.Parse(columns[2]),
                    ShopName = columns[3],
                    Promotion = bool.Parse(columns[4]),
                };
            }

        }
    }
}

