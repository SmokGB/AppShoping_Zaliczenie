using AppShoping.ApplicationServices.Components.csvReader.Models;

namespace AppShoping.ApplicationServices.Components.csvReader
{
    public interface ICsvReader
    {
        List<Product> ProcessProduct(string filePath);
        List<Purchase> ToProcessStatisc (string filePath);

    }
}
