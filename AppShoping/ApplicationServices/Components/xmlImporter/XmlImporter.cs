using AppShoping.ApplicationServices.Components.Models;
using AppShoping.DataAccess.Data.Entities;
using AppShoping.DataAccess.Data.Repositories;

using System.Xml.Linq;
namespace AppShoping.ApplicationServices.Components.xmlImporter
{
    public class XmlImporter : IXmlImporter
    {
        private readonly IRepository<Food> _foodRepository;

        public XmlImporter(IRepository<Food> repository) => _foodRepository = repository;

        public void ImportFoodData()
        {
            var document = XDocument.Load("products.xml");
            var names = document.Element("ArrayOfFood")?
                .Elements("Food")?
                .Select(x => new Product
                {
                    ProductName = x.Element("ProductName")?.Value,
                    BioProduct = bool.TryParse(x.Element("BioProduct")?.Value, out var bioProduct) && bioProduct
                }).ToList();

            if (names != null)
            {
                foreach (var item in names)
                {
                    _foodRepository.Add(new Food
                    {
                        ProductName = item.ProductName,
                        BioProduct = item.BioProduct
                    });
                }
                _foodRepository.Save();
            }
        }

     
    }
}
