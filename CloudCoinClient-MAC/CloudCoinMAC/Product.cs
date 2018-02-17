using System;
namespace CloudCoinMAC
{
    public class Product
    {
        #region Computed Propoperties
        public string Denomination { get; set; } = "";
        public string NotesCount { get; set; } = "";
        public string NotesValue { get; set; } = "";
        public string ExportCount { get; set; } = "";
        public string ExportValue { get; set; } = "";
        #endregion

        #region Constructors
        public Product()
        {
        }

        public Product(string title, string description,string Total)
        {
            this.Denomination = title;
            this.NotesCount = description;
            this.NotesValue = Total;
            this.ExportCount = "0";
            this.ExportValue = "0";
        }
        #endregion
    }
}
