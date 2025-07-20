namespace AppShoping.DataAccess.Data.Entities;

public class PurchaseStatistics : EntityBase
{
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public bool BioFood { get; set; } = false;
    public string ShopName { get; set; } = "Auchan";
    public bool Promotion { get; set; } = false;

   
}
