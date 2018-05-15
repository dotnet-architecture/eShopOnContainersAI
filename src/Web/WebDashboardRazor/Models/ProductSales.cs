namespace Microsoft.eShopOnContainers.WebDashboardRazor.Models
{
    public class ProductSales
    {
        public float next { get; set; }
        public string productId { get; set; }
        public int year { get; set; }
        public int month { get; set; }
        public int units { get; set; }
        public float avg { get; set; }
        public int count { get; set; }
        public int max { get; set; }
        public int min { get; set; }
        public int prev { get; set; }
    }
}
