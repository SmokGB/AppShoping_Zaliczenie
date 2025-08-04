namespace AppShoping.ApplicationServices.Components.Models
{
    public class Purchase
    {
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public bool BioFood { get; set; }
        public string? ShopName { get; set; }
        public bool   Promotion { get; set; }
    }
}
