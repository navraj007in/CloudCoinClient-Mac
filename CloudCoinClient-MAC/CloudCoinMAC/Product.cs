using System;
namespace CloudCoinMAC
{
    public class Product
    {
        #region Computed Propoperties
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Total { get; set; } = "";
        #endregion

        #region Constructors
        public Product()
        {
        }

        public Product(string title, string description,string Total)
        {
            this.Title = title;
            this.Description = description;
            this.Total = Total;
        }
        #endregion
    }
}
