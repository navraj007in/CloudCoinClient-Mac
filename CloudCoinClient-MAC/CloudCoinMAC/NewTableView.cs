using System;
using AppKit;

namespace CloudCoinMAC
{
    public class NewTableView :NSTableView
    {
        public NewTableView()
        {
        }

        public override void TextDidEndEditing(Foundation.NSNotification notification)
        {
            
            base.TextDidEndEditing(notification);
        }
    }
}
