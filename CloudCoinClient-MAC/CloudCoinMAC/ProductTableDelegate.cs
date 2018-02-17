using System;
using AppKit;
using CoreGraphics;
using Foundation;
using System.Collections;
using System.Collections.Generic;

namespace CloudCoinMAC
{
    public class ProductTableDelegate : NSTableViewDelegate
    {
        #region Constants 
        private const string CellIdentifier = "ProdCell";
        #endregion

        #region Private Variables
        private ProductTableDataSource DataSource;
        #endregion

        #region Constructors
        public ProductTableDelegate(ProductTableDataSource datasource)
        {
            this.DataSource = datasource;
        }
        #endregion

        #region Override Methods
        int[] multiplier = new int[] { 1, 5, 25, 100, 250 };

        public override NSView GetViewForItem(NSTableView tableView, NSTableColumn tableColumn, nint row)
        {
            // This pattern allows you reuse existing views when they are no-longer in use.
            // If the returned view is null, you instance up a new view
            // If a non-null view is returned, you modify it enough to reflect the new data
            NSTextField view = (NSTextField)tableView.MakeView(CellIdentifier, this);
            if (view == null)
            {
                view = new NSTextField();
                view.Identifier = CellIdentifier;
                view.BackgroundColor = NSColor.Clear;
                view.Bordered = false;
                view.Selectable = false;
                if(tableColumn.Title == "Export")
                    view.Editable = true;
                view.Tag = row;

                view.EditingEnded += (sender, e) => {

                    // Take action based on type
                    switch (tableColumn.Title)
                    {
                        case "Export":
                            DataSource.Products[(int)view.Tag].ExportCount = view.StringValue;
                            DataSource.Products[(int)view.Tag].ExportValue = 
                                Convert.ToString(view.IntValue * multiplier[row]);
                            //DataSource.Products[(int)view.Tag].Title = view.StringValue;
                            break;
                        case "Details":
                            //DataSource.Products[(int)view.Tag].Description = view.StringValue;
                            break;
                    }
                    tableView.ReloadData();
                };
            }

            // Setup view based on the column selected
            switch (tableColumn.Title)
            {
                case "Denomination":
                    view.StringValue = DataSource.Products[(int)row].Denomination;
                    break;
                case "Count":
                    view.StringValue = DataSource.Products[(int)row].NotesCount;
                    break;
                case "Total":
                    view.StringValue = DataSource.Products[(int)row].NotesValue;
                    break;
                case "Export":
                    view.StringValue = DataSource.Products[(int)row].ExportCount;
                    break;
                case "Export Value":
                    view.StringValue = DataSource.Products[(int)row].ExportValue;
                    break;

            }

            
            return view;
        }
        #endregion
    }
}
