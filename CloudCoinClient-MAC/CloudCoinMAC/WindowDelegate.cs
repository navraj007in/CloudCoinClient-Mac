using System;
using AppKit;

namespace CloudCoinMAC
{
    public class WindowDelegate :NSWindowDelegate
    {
        public WindowDelegate()
        {
        }

        public override bool WindowShouldClose(Foundation.NSObject sender)
        {
            NSApplication.SharedApplication.Terminate(sender);
            return base.WindowShouldClose(sender);
        }
    }
}
