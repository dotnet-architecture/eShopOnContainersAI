namespace Microsoft.eShopOnContainers.Services.AI.SalesForecasting.MLNet.API.Forecasting
{
    public interface ICountrySales
    {
        ScoredCountrySample Predict(string modelPath, string country, int year, int month, float sales, float avg, int count, float max, float min, float p_max, float p_med, float p_min, float std, float prev);
    }
}