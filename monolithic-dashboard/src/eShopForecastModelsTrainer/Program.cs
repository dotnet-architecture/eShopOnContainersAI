using System;
using System.Threading.Tasks;

namespace eShopForecastModelsTrainer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                CountryModelHelper.SaveModel("data/countries.stats.csv");
                ProductModelHelper.SaveModel("data/products.stats.csv");
                await CountryModelHelper.PredictSamples();
                await ProductModelHelper.PredictSamples();
            } catch(Exception ex)
            {
                Console.Write(ex.Message);
            }
        }
    }
}
