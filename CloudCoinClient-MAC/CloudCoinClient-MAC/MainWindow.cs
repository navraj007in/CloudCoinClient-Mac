using System;

using Foundation;
using AppKit;
using CloudCoinCore;
using CloudCoinClientMAC.CoreClasses;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;

namespace CloudCoinClientMAC
{
    public partial class MainWindow : NSWindow
    {
        RAIDA raida = RAIDA.GetInstance();
        string RootPath;
        FileSystem FS;
        //RAIDA raida;
        int onesCount = 0;
        int fivesCount = 0;
        int qtrCount = 0;
        int hundredsCount = 0;
        int twoFiftiesCount = 0;

        public static int exportOnes = 0;
        public static int exportFives = 0;
        public static int exportTens = 0;
        public static int exportQtrs = 0;
        public static int exportHundreds = 0;
        public static int exportTwoFifties = 0;
        public static int exportJpegStack = 2;
        public static string exportTag = "";
        public MainWindow(IntPtr handle) : base(handle)
        {
            this.Title = "CloudCoin MAC Client";
            Echo();
        }

        public async void Echo() {
            var echos = raida.GetEchoTasks();
            //txtProgress.AppendText("Starting Echo to RAIDA\n");
            //txtProgress.AppendText("----------------------------------\n");

            await Task.WhenAll(echos.AsParallel().Select(async task => await task()));
            //MessageBox.Show("Finished Echo");
           // lblReady.Content = raida.ReadyCount;
           // lblNotReady.Content = raida.NotReadyCount;

            for (int i = 0; i < raida.nodes.Count(); i++)
            {
               // txtProgress.AppendText("Node " + i + " Status --" + raida.nodes[i].RAIDANodeStatus + "\n");
                Debug.WriteLine("Node" + i + " Status --" + raida.nodes[i].RAIDANodeStatus);
            }
            Debug.WriteLine("-----------------------------------\n");
            Debug.WriteLine("Ready Nodes-" + Convert.ToString(raida.ReadyCount));
            Debug.WriteLine("Not Ready Nodes-" + Convert.ToString(raida.NotReadyCount));

            //txtProgress.AppendText("----------------------------------\n");
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
