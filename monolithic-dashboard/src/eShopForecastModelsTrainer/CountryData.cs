using Microsoft.MachineLearning.Runtime.Api;

namespace eShopForecastModelsTrainer
{
    public class CountryData
    {
        [ColumnName("Label")]
        public float next;

        public string country;

        public float year;
        public float month;
        public float sales;
        public float avg;
        public float count;
        public float max;
        public float min;
        public float p_max;
        public float p_med;
        public float p_min;
        public float std;
        public float prev;
    }

    public class CountrySalesPrediction
    {
        public float Score;
    }
}
