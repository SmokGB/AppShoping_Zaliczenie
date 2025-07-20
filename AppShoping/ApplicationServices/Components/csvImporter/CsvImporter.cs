using AppShoping.ApplicationServices.Components.csvReader;
using AppShoping.DataAccess.Data.Entities;
using AppShoping.DataAccess.Data.Repositories;

namespace AppShoping.ApplicationServices.Components.csvImporter
{
    public class CsvImporter : ICsvImporter
    {
        public void ImportFoodData(string csvFilePath)
        {
            ImportProducts(csvFilePath);
        }

        public void ImportPurchaseData(string csvFilePath)
        {
            ImportPurchase(csvFilePath);
        }



        private readonly ICsvReader _csvReader;
        private readonly IRepository<Food> _foodRepository;
        private readonly IRepository<PurchaseStatistics> _purchaseRepositorty;


        public CsvImporter(ICsvReader csvReader,
            IRepository<Food> foodRepository,
            IRepository<PurchaseStatistics> purchaseRepository)
        {

            _foodRepository = foodRepository;
            _csvReader = csvReader;
            _purchaseRepositorty = purchaseRepository;

        }

        void ImportProducts(string csvFilePath)
        {
            var products = _csvReader.ProcessProduct(csvFilePath);
            foreach (var product in products)
            {
               
                    _foodRepository.Add(
                        new Food
                        {
                            ProductName = product.ProductName,
                            BioProduct = product.BioProduct
                        });

                }

            _foodRepository.Save();
        }

        private void ImportPurchase(string csvFilePath)
        {
            var items = _csvReader.ToProcessStatisc(csvFilePath);
            foreach (var item in items)
            {
                _purchaseRepositorty.Add(
                    new PurchaseStatistics
                    {
                        Name = item.Name,
                        Price = Convert.ToDecimal(item.Price),
                        BioFood = item.BioFood,
                        ShopName = item.ShopName!,
                        Promotion = item.Promotion,
                    });
            }
            _purchaseRepositorty.Save();
        }

    }
}





