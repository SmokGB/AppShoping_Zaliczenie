using System.Xml.Linq;
using AppShoping.ApplicationServices.Components.csvImporter;
using AppShoping.ApplicationServices.Components.xmlImporter;
using AppShoping.DataAccess.Data;
using AppShoping.DataAccess.Data.Entities;
using AppShoping.DataAccess.Data.Repositories;
using AppShoping.DataAccess.Data.Repositories.Extensions;

namespace AppShoping.UI.Menu;

public class UserCommunication : IUserCommunication
{
    private readonly IRepository<Food> _foodRepository;
    private readonly IRepository<PurchaseStatistics> _purchaseRepository;
    private readonly ShopAppDbContext _shopAppDbContext;
    private readonly ICsvImporter _csvImporter;
    private readonly IXmlImporter _xmlImporter;

    private const string AuditDataPath = "Audit.txt";
    private enum ProductOperation { View, Edit, Delete, Add, DeleteAll };


    private readonly List<string> shopMenu =
    [
        "1  - Wykaz produktów do zakupu",
        "2  - Dodaj produkt",
        "3  - Dodaj produkt bio",
        "4  - Edycja produktów do zakupu",
        "5  - Usuń produkt z listy zakupów",
        "6  - Zestawienie danych produkt - zakup",
        "7  - Dodaj dane do statystyk zakupów",
        "8  - Import produktow do zakupu z pliku CSV do DB",
        "9  - Import statystyk zakupów z pliku CSV do DB",
        "10 - Import produktów z pliku XML do DB",
        "11 - Usuń wszystkie dane z DB",
        "12 - Wyjście z programu",
    ];

    public UserCommunication(IRepository<Food> foodRepository,
        IRepository<PurchaseStatistics> purchaseRepository,
        ShopAppDbContext shopAppDbContext,
        ICsvImporter csvImporter,
        IXmlImporter xmlImporter
      )
    {

        _foodRepository = foodRepository;
        _purchaseRepository = purchaseRepository;
        _shopAppDbContext = shopAppDbContext;
        _shopAppDbContext.Database.EnsureCreated();
        _csvImporter = csvImporter;
        _xmlImporter = xmlImporter;
        _foodRepository.ItemAdded += OnWriteToFile;
        _foodRepository.ProductDeleted += OnProductDeleted;

    }

    private void OnProductDeleted(object? sender, Food? e)
    {
        WriteAuditLog($"usunięto : {e?.ProductName} z  {e?.GetType().Name}");
    }


    private void OnWriteToFile(object? sender, Food? e)
    {
        WriteAuditLog($"dodano : {e?.ProductName} z {e?.GetType().Name}");
    }

    private void WriteAuditLog(string message)
    {
        using (var write = File.AppendText(AuditDataPath))
        {
            write.WriteLine($"{DateTime.Now} {message}");
        }
    }


    public void DisplayMenu()
    {
        Console.Clear();
        Console.WriteLine("Wykaz zakupów do zrobienia\n");

        foreach (var i in shopMenu)
            Console.WriteLine(i);

        Console.WriteLine("\n------------- Wybierz opcję [1 - 11] ---------------\n");
    }

    public void ChoiceOfMenu()
    {
        while (true)
        {
            DisplayMenu();
            if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice < 13)
            {
                ExecuteMenuAction(choice);
            }
            else
            {
                ShowAlert("Niewłaściwy wybór w Menu");
            }
        }
    }

    private void ExecuteMenuAction(int choice)
    {
        switch (choice)
        {
            case 1:
                ManageProduct(ProductOperation.View, 1, 15);
                break;
            case 2:
            case 3:
                ManageProduct(ProductOperation.Add, 1, 15, choice);
                break;
            case 4:
                ManageProduct(ProductOperation.Edit, 1, 15);
                break;
            case 5:
                ManageProduct(ProductOperation.Delete, 1, 15);
                break;
            case 6:
                CombinedDataFromTables();
                break;

            case 7:
                AddToPurchaseStatistics();
                break;
            case 8:
                _csvImporter.ImportFoodData("Resources\\Files\\Product.csv");
                break;
            case 9:
                _csvImporter.ImportPurchaseData("Resources\\Files\\Purchase.csv");
                break;

            case 10:
                _xmlImporter.ImportFoodData();
                break;
            case 11:
                DeleteAllDataFromDB();
                break;

            case 12:
                ExportFoodListToFiles();
                break;
        }
    }

    private void ExportFoodListToFiles()
    {
        try
        {
            _foodRepository.ExportFoodListToJsonFiles();
            _foodRepository.ExportFoodListToXMLFiles();
            Environment.Exit(0);
        }
        catch (Exception e)
        {
            Console.WriteLine($"{e.Message}");
            Console.ReadKey();
        }
    }

    private void CombinedDataFromTables()
    {
        var products = GetProductData(_shopAppDbContext);
        var purchase = GetPurchaseData(_shopAppDbContext);

        if (products.Count == 0 || purchase.Count == 0)
        {
            Console.WriteLine("Brak danych do wyświetlenia.");
            Console.ReadKey();
            return;
        }

        GroupBy(purchase);
        Console.WriteLine("\n\n------------------\n\n");
        JoinTables(products, purchase);
        Console.ReadKey();
        ExportToXml(purchase);
    }

    private static void JoinTables(List<Food> products, List<PurchaseStatistics> purchase)
    {
        var priceProduct = products.Join(purchase,
                                    x => x.ProductName,
                                    x => x.Name,
                                    (products, purchase) => new
                                    {
                                        products.ProductName,
                                        purchase.ShopName,
                                        purchase.Price,
                                        purchase.Promotion,

                                    }).OrderBy(x => x.ProductName)
                                    .ThenBy(x => x.Price);

        foreach (var price in priceProduct)
        {

            Console.WriteLine($"\t Shop:{price.ShopName}");
            Console.WriteLine($"{price.ProductName}");
            Console.WriteLine($"\t Price : {price.Price}");
            Console.WriteLine($"\t Promotion : {price.Promotion}");


        }
    }

    private static void GroupBy(List<PurchaseStatistics> purchase)
    {
        var groups = purchase.GroupBy(x => x.Name)
            .Select(g => new
            {
                Name = g.Key,
                Max = g.Max(c => c.Price),
                Min = g.Min(c => c.Price),
                Average = g.Average(c => c.Price)
            })
            .OrderBy(x => x.Name);

        foreach (var grup in groups)
        {
            Console.WriteLine($"{grup.Name}");
            Console.WriteLine($"\t Max : {grup.Max}");
            Console.WriteLine($"\t Min : {grup.Min}");
            Console.WriteLine($"\t Avg : {Math.Round(grup.Average, 2)}");
        }
    }

    private static void ExportToXml(List<PurchaseStatistics> purchase)
    {
        if (purchase.Count == 0)
        {
            Console.WriteLine("Brak danych do eksportu.");
            return;
        }

        var document = new XDocument();
        var xmlProducts = new XElement("ProductsList", purchase
            .Select(x => new XElement("ProductList",
                new XAttribute("ShopName", x.ShopName!),
                new XAttribute("ProductName", x.Name!),
                new XElement("Price", Math.Round(x.Price, 2)),
                new XElement("OrganicProduct", x.BioFood),
                new XElement("Promotion", x.Promotion),
                new XElement("AmountSpentInShop",
                    new XAttribute("TotalSpent",
                        Math.Round(purchase.Where(c => c.ShopName == x.ShopName).Sum(c => c.Price), 2)),
                    new XElement("Purchases",
                        purchase.Where(c => c.ShopName == x.ShopName)
                            .Select(p => new XElement("PurchaseList",
                                new XAttribute("Product", p.Name!),
                                new XAttribute("Price", Math.Round(p.Price, 2))
                            ))
                    )
                )
            ))
        );

        document.Add(xmlProducts);
        document.Save("Products.xml");
    }

    private static List<Food> GetProductData(ShopAppDbContext foods)
    {
        return foods.Foods.ToList()!;
    }

    private static List<PurchaseStatistics> GetPurchaseData(ShopAppDbContext purchase)
    {
        return purchase.Purchase.ToList()!;
    }

    private void AddToPurchaseStatistics()
    {
        Console.WriteLine("--> Wprowadź dane do statystyk zakupów");

        var name = GetData("Podaj nazwę produktu");
        var price = GetProductPrice();
        var isBioProduct = GetBooleanResponse("Czy produkt bio ? (TAK / NIE)");
        var shopName = GetData("Podaj nazwę sklepu");
        var isOnPromotion = GetBooleanResponse("Czy produkt w promocji (TAK/NIE)");

        try
        {
            _purchaseRepository.Add(new PurchaseStatistics
            {
                Name = name,
                Price = price,
                BioFood = isBioProduct,
                ShopName = shopName,
                Promotion = isOnPromotion
            });
            _purchaseRepository.Save();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Wystąpił błąd: {ex.Message}");
            Console.ReadKey();
        }
    }

    private static string GetData(string data)
    {
        Console.WriteLine(data);
        return Console.ReadLine()!;
    }

    private static decimal GetProductPrice()
    {
        while (true)
        {
            Console.WriteLine("Podaj cenę");
            if (decimal.TryParse(Console.ReadLine(), out decimal price))
            {
                return price;
            }
            Console.WriteLine("Niepoprawna cena. Proszę podać liczbę.");
        }
    }

    private static bool GetBooleanResponse(string data)
    {
        while (true)
        {
            Console.WriteLine(data);
            var response = Console.ReadLine()!.ToUpper();
            if (response == "TAK") return true;
            if (response == "NIE") return false;
            Console.WriteLine("Niepoprawna odpowiedź. Proszę podać TAK lub NIE.");
        }
    }
    int internalId = 1;
    private void DisplayProductsPage(List<Food> products, int pageNumber, int pageSize, int totalPages, bool isView)
    {


        Console.WriteLine($"\n--- Strona {pageNumber} z {totalPages} ---\n");

        foreach (var product in products)
        {
            string bioInfo = product.BioProduct ? "produkt ekologiczny" : "-";
            if (isView)
            {
                Console.WriteLine($"{internalId,-3} {product.ProductName,-10} {bioInfo,-10}");
            }
            else
            {
                Console.WriteLine($"ID: {product.Id,-3}  Nazwa: {product.ProductName,-10}, {bioInfo,-10}");
            }
            internalId++;
        }

        Console.WriteLine("\n-------------------------\n");
    }

    private void HandlePaginationInput(int currentPage, int totalPages, int pageSize, ProductOperation operation)
    {
        while (true)
        {
            Console.WriteLine("Wpisz 'n' aby przejść do następnej strony, 'p' aby wrócić do poprzedniej, 'q' aby wyjść: ");
            var input = Console.ReadLine()?.ToLower();

            switch (input)
            {
                case "n":
                    if (currentPage < totalPages)
                    {
                        ManageProduct(operation, currentPage + 1, pageSize);
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Jesteś na ostatniej stronie.");
                    }
                    break;
                case "p":
                    if (currentPage > 1)
                    {
                        ManageProduct(operation, currentPage - 1, pageSize);
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Jesteś na pierwszej stronie.");
                    }
                    break;
                case "q":
                    return;
                default:
                    Console.WriteLine("Niepoprawna komenda.");
                    break;
            }
        }
    }

    private static void ShowAlert(string message)
    {
        Console.WriteLine(message);
        Console.ReadKey();
    }

    private void ManageProduct(ProductOperation operation, int pageNumber, int pageSize, int choice = 0)
    {
        switch (operation)
        {
            case ProductOperation.Add:
                HandleAddProduct(choice);
                break;
            case ProductOperation.View:
                HandleViewProducts(pageNumber, pageSize);
                break;
            case ProductOperation.Edit:
                HandleEditProduct(pageNumber, pageSize);
                break;
            case ProductOperation.Delete:
                HandleDeleteProduct(pageNumber, pageSize);
                break;
            default:
                Console.WriteLine("Nieznana operacja.");
                break;
        }
    }

    private void HandleAddProduct(int choice)
    {
        string productType = choice == 3 ? "produktu bio" : "produktu";
        Console.Write($"Podaj nazwę {productType}: ");

        string? productName = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(productName))
        {
            Console.WriteLine("Nazwa produktu nie może być pusta.");
            return;
        }

        bool isBio = choice == 3;

        try
        {
            var newProduct = new Food
            {
                ProductName = productName,
                BioProduct = isBio
            };

            _foodRepository.Add(newProduct);
            _foodRepository.Save();
            Console.WriteLine($"Dodano {productType} '{productName}' do bazy danych.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Wystąpił błąd podczas dodawania produktu: {ex.Message}");
        }

        Console.ReadKey();
    }


    private void HandleViewProducts(int pageNumber, int pageSize)
    {

        var products = _foodRepository.GetAll().ToList();
        int totalProducts = products.Count;


        if (totalProducts == 0)
        {
            Console.WriteLine("Brak danych do wyświetlenia.");
            Console.WriteLine("Baza danych jest pusta. Spróbuj zaimportować dane z pliku CSV lub dodać nowe produkty.");
            Console.ReadKey();
            return;
        }

        int totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

        if (pageNumber > totalPages && totalPages > 0)
        {
            Console.WriteLine("Nie ma takiej strony.");
            Console.ReadKey();
            return;
        }

        var pagedProducts = products
            .OrderBy(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        DisplayProductsPage(pagedProducts, pageNumber, pageSize, totalPages, true);
        HandlePaginationInput(pageNumber, totalPages, pageSize, ProductOperation.View);

    }
    private void HandleEditProduct(int pageNumber, int pageSize)
    {
        HandlePagedProductAction(pageNumber, pageSize, "edycji", EditProduct);
    }

    private void HandleDeleteProduct(int pageNumber, int pageSize)
    {
        HandlePagedProductAction(pageNumber, pageSize, "usunięcia", DeleteProduct);
    }

    private void HandlePagedProductAction(int pageNumber, int pageSize, string actionName, Action<int> productAction)
    {
        var products = _foodRepository.GetAll().ToList();
        int totalProducts = products.Count;

        if (totalProducts == 0)
        {
            Console.WriteLine($"Brak produktów do {actionName}.");
            Console.ReadKey();
            return;
        }

        int totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

        while (true)
        {
            var pagedProducts = products
                .OrderBy(x => x.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            DisplayProductsPage(pagedProducts, pageNumber, pageSize, totalPages, false);

            Console.WriteLine($"Wpisz ID produktu do {actionName}, 'n' aby przejść do następnej strony, 'p' aby wrócić do poprzedniej, lub 'q' aby wyjść: ");
            var input = Console.ReadLine()?.ToLower();

            switch (input)
            {
                case "n":
                    if (pageNumber < totalPages)
                    {
                        pageNumber++;
                    }
                    else
                    {
                        Console.WriteLine("Jesteś na ostatniej stronie.");
                    }
                    break;
                case "p":
                    if (pageNumber > 1)
                    {
                        pageNumber--;
                    }
                    else
                    {
                        Console.WriteLine("Jesteś na pierwszej stronie.");
                    }
                    break;
                case "q":
                    return;
                default:
                    if (int.TryParse(input, out int productId))
                    {
                        productAction(productId);
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Niepoprawna komenda.");
                    }
                    break;
            }
        }
    }

    private void EditProduct(int productId)
    {
        var product = _foodRepository.GetById(productId);
        if (product == null)
        {
            Console.WriteLine("Produkt o podanym ID nie istnieje.");
            Console.ReadKey();
            return;
        }

        Console.Write($"Edytujesz produkt: {product.ProductName}. Zmień nazwę na: ");
        string? newName = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(newName))
        {
            product.ProductName = newName;
            _foodRepository.Update(product);
            _foodRepository.Save();
            Console.WriteLine("Zmiany zapisano.");
        }
        else
        {
            Console.WriteLine("Nazwa nie może być pusta.");
        }
        Console.ReadKey();
    }

    private void DeleteProduct(int productId)
    {
        var product = _foodRepository.GetById(productId);
        if (product == null)
        {
            Console.WriteLine("Produkt o podanym ID nie istnieje.");
            Console.ReadKey();
            return;
        }

        _foodRepository.Remove(product);
        _foodRepository.Save();
        Console.WriteLine("Produkt usunięty.");
        Console.ReadKey();
    }

    private void DeleteAllDataFromDB()
    {
        try
        {

            _shopAppDbContext.Foods.RemoveRange(_shopAppDbContext.Foods);
            _shopAppDbContext.Purchase.RemoveRange(_shopAppDbContext.Purchase);
            _shopAppDbContext.SaveChanges();
            Console.WriteLine("Wyczyszczono dane z tabel.");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Wystąpił błąd podczas usuwania danych: {ex.Message}");
        }
        Console.ReadKey();
    }
}

