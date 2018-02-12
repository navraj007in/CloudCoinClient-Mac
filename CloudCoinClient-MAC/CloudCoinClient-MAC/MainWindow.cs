using System;

using Foundation;
using AppKit;
using CloudCoinCore;

namespace CloudCoinClientMAC
{
    public partial class MainWindow : NSWindow
    {
        RAIDA raida = RAIDA.GetInstance();

        public MainWindow(IntPtr handle) : base(handle)
        {
            this.Title = "CloudCoin MAC Client";

        }

        [Export("initWithCoder:")]
        public MainWindow(NSCoder coder) : base(coder)
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
        }
    }
}
