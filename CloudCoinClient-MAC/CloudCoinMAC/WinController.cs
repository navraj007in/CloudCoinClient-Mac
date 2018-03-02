using System;
using AppKit;

namespace CloudCoinMAC
{
    public class WinController:NSWindowController
    {
        public WinController(IntPtr handle) : base(handle)
        {
        }

        public override void WindowDidLoad()
        {
            base.WindowDidLoad();

            this.Window.TitlebarAppearsTransparent = true;
            this.Window.MovableByWindowBackground = true;

            this.Window.BackgroundColor = NSColor.Blue;
        }
    }
}
