using Microsoft.MachineLearning.Runtime.Api;

namespace eShopForecastModelsTrainer
{
    public class ProductData
    {
        [ColumnName("Label")]
        public float next;
        public string productId;
        public float year;
        public float month;
        public float units;
        public float avg;
        public float count;
        public float max;
        public float min;
        public float prev;
        public float price;
        public string color;
        public string size;
        public string shape;
        public string agram;
        public string bgram;
        public string ygram;
        public string zgram;
    }

    public class ProductUnitPrediction
    {
        public float Score;
    }
}
