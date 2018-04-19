namespace Microsoft.eShopOnContainers.Services.AI.SalesForecasting.TLC.API.Forecasting
{
    public interface IProductSales
    {
        ScoredProductSample Predict(string modelPath, string productId, int year, int month, float units, float avg, int count, float max, float min, float prev, float price, string color, string size, string shape, string agram, string bgram, string ygram, string zgram);
    }
}