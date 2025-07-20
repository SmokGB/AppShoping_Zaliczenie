namespace AppShoping.ApplicationServices.Components.csvImporter
{
    public interface ICsvImporter
    {
        void ImportFoodData(string csvFilePath);
        void ImportPurchaseData(string csvFilePath);
    }
}
