using Microsoft.ML.Runtime.Api;

namespace Microsoft.eShopOnContainers.Services.AI.SalesForecasting.Training.MLNet.API
{
    /// <summary>
    /// Sample for sales prediction by product model
    /// </summary>
    public class ProductData
    {
        /// <summary>
        /// This represents next period sales. Sales are measured in units sold. Period is measured in month/unit
        /// MLNet needs this column to be named as "Label"
        /// </summary>
        [Column(ordinal: "0", name: "Label")]
        public float next;

        /// <summary>
        /// Product ID
        /// </summary>
        [Column(ordinal: "1")]
        public string productId;

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
        /// Sales in current period
        /// </summary>
        [Column(ordinal: "4")]
        public float units;
        /// <summary>
        /// Mean sales in current period
        /// </summary>
        [Column(ordinal: "5")]
        public float avg;
        /// <summary>
        /// Number of sales in current period
        /// </summary>
        [Column(ordinal: "6")]
        public float count;
        /// <summary>
        /// Max products sold in a single sale in current period
        /// </summary>
        [Column(ordinal: "7")]
        public float max;
        /// <summary>
        /// Min products sold in a single sale in current period
        /// </summary>
        [Column(ordinal: "8")]
        public float min;
        /// <summary>
        /// Previous period sales
        /// </summary>
        [Column(ordinal: "9")]
        public float prev;
    }

    public class ProductUnitPrediction
    {
        public float Score;
    }
}
