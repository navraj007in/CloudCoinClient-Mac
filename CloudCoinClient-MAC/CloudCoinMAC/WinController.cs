using System;
using AppKit;
using CoreGraphics;

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
            CGSize size = new CGSize();
            size.Width = 869;
            size.Height = 451;
            this.Window.MinSize = size;
        }
    }
}
