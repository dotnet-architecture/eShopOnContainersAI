namespace Microsoft.eShopOnContainers.Services.AI.SalesForecasting.TLC.API.Forecasting
{
    public interface ICountrySales
    {
        ScoredCountrySample Predict(string modelPath, string country, int year, int month, float avg, int max, int min, int prev, int count, int units);
    }
}