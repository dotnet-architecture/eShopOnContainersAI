using System.Threading.Tasks;

namespace Microsoft.eShopOnContainers.Services.AI.SalesForecasting.MLNet.API.Forecasting
{
    public interface IProductSales
    {
        Task<ProductUnitPrediction> Predict(string modelPath, string productId, int year, int month, float units, float avg, int count, float max, float min, float prev);
    }
}