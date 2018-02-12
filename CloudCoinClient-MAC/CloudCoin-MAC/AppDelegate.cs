using AppKit;
using Foundation;
using CloudCoinCore;

namespace CloudCoinMAC
{
    [Register("AppDelegate")]
    public class AppDelegate : NSApplicationDelegate
    {
        MainWindowController mainWindowController;
        public static RAIDA raida = RAIDA.GetInstance();

        public AppDelegate()
        {
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            mainWindowController = new MainWindowController();
            mainWindowController.Window.MakeKeyAndOrderFront(this);
        }

        public override void WillTerminate(NSNotification notification)
        {
            // Insert code here to tear down your application
        }
    }
}
