using System;
using AppKit;

namespace CloudCoinMAC
{
    public class CloudCoinWindow :NSWindow
    {
        public CloudCoinWindow()
        {
        }

        public override void AwakeFromNib()
        {
            BackgroundColor = NSColor.Black;
        }
    }
}
