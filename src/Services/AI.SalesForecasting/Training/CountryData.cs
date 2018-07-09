using Microsoft.ML.Runtime.Api;

namespace Microsoft.eShopOnContainers.Services.AI.SalesForecasting.Training.MLNet.API
{
    public class CountryData
    {
        /// <summary>
        /// This represent next period country sales. Sales are measured in dollars in logaithmic scale. Period is measured in month/unit
        /// MLNet needs this column to be named as "Label"
        /// </summary>
        [Column(ordinal: "0", name: "Label")]
        public float next;

        /// <summary>
        /// Contry ID
        /// </summary>
        [Column(ordinal: "1")]
        public string country;

        /// <summary>
        /// Period year
        /// </summary>
        [Column(ordinal: "2")]
        public float year;
        /// <summary>
        /// Period month
        /// </summary>
        [Column(ordinal: "3")]
        public float month;
        /// <summary>
        /// 80th percentile sale in current period
        /// </summary>
        [Column(ordinal: "4")]
        public float max;
        /// <summary>
        /// 20th percentile sale in current period
        /// </summary>
        [Column(ordinal: "5")]
        public float min;
        /// <summary>
        /// Sale standard deviation in current period
        /// </summary>
        [Column(ordinal: "6")]
        public float std;
        /// <summary>
        /// Number of sales in current period
        /// </summary>
        [Column(ordinal: "7")]
        public float count;
        /// <summary>
        /// Sum of sales in current period
        /// </summary>
        [Column(ordinal: "8")]
        public float sales;
        /// <summary>
        /// Sale median (50th percentile) in current period
        /// </summary>
        [Column(ordinal: "9")]
        public float med;
        /// <summary>
        /// Previous period sale
        /// </summary>
        [Column(ordinal: "10")]
        public float prev;
    }

    public class CountrySalesPrediction
    {
        public float Score;
    }
}
