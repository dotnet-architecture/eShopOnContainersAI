using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Services.AI.SalesForecasting.MLNet.API.Forecasting
{
    public interface ICountrySales
    {
        Task<CountrySalesPrediction> Predict(string modelPath, string country, int year, int month, float max, float min, float std, int count, float sales, float med, float prev);
    }
}