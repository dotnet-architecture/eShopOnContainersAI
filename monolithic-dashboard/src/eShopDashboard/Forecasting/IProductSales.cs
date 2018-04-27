using System.Threading.Tasks;

namespace eShopDashboard.Forecasting
{
    public interface IProductSales
    {
        Task<ProductUnitPrediction> Predict(string modelPath, string productId, int year, int month, float units, float avg, int count, float max, float min, float prev, float price, string color, string size, string shape, string agram, string bgram, string ygram, string zgram);
    }
}