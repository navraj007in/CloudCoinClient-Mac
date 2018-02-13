using AppKit;
using Foundation;
using CloudCoinCore;
using CloudCoinClientMAC.CoreClasses;
using System;
using System.IO;

namespace CloudCoinMAC
{
    [Register("AppDelegate")]
    public class AppDelegate : NSApplicationDelegate
    {
        MainWindowController mainWindowController;
        public static RAIDA raida = RAIDA.GetInstance();
        NSUserDefaults defaults = NSUserDefaults.StandardUserDefaults;
        public static FileSystem FS;
        string ws = "";
        string RootPath = "";
        string defaultPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + Path.DirectorySeparatorChar
                                    + Config.HomeFolder + Path.DirectorySeparatorChar;
        
        public AppDelegate()
        {
            defaults.SetString("",Config.WorkSpaceKey);
            try{
                ws = defaults.StringForKey(Config.WorkSpaceKey);
                if (ws == null)
                    ws = "";
                if (ws.Length == 0)
                {
                    ws = defaultPath ;
                    defaults.SetString(defaultPath, Config.WorkSpaceKey);
                }
                else {
                    
                }
            }
            catch(Exception e) {
                
            }
            RootPath = ws;
            Setup();
        }

        public void Setup() {
            FS = new FileSystem(RootPath);

            // Create the Folder Structure
            FS.CreateFolderStructure();
            FS.CopyTemplates();
            // Populate RAIDA Nodes
            raida = RAIDA.GetInstance();
            raida.FS = FS;
            //CoinDetected += Raida_CoinDetected;
            //raida.Echo();
            FS.ClearCoins(FS.PreDetectFolder);
            FS.ClearCoins(FS.DetectedFolder);

            FS.LoadFileSystem();
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
