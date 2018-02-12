using System;
using System.Diagnostics;
using Foundation;
using AppKit;
using System.Threading.Tasks;
using System.Linq;
using CloudCoinCore;

namespace CloudCoinMAC
{
    public partial class MainWindowController : NSWindowController
    {
        RAIDA raida = AppDelegate.raida;

        public MainWindowController(IntPtr handle) : base(handle)
        {
        }

        partial void cmdEcho(NSObject sender)
        {
            Echo();
        }
        [Export("initWithCoder:")]
        public MainWindowController(NSCoder coder) : base(coder)
        {
        }

        public async void Echo()
        {
            var echos = AppDelegate.raida.GetEchoTasks();
            //txtProgress.AppendText("Starting Echo to RAIDA\n");
            //txtProgress.AppendText("----------------------------------\n");

            await Task.WhenAll(echos.AsParallel().Select(async task => await task()));
            //MessageBox.Show("Finished Echo");
            // lblReady.Content = raida.ReadyCount;
            // lblNotReady.Content = raida.NotReadyCount;
            
            for (int i = 0; i < raida.nodes.Count(); i++)
            {
                txtProgress.StringValue += "Node " + i + " Status --" + raida.nodes[i].RAIDANodeStatus + "\n";

                //txtProgress.AppendText("Node " + i + " Status --" + raida.nodes[i].RAIDANodeStatus + "\n");
                Debug.WriteLine("Node" + i + " Status --" + raida.nodes[i].RAIDANodeStatus);
            }
            Debug.WriteLine("-----------------------------------\n");
            txtProgress.StringValue += "Ready Nodes-" + Convert.ToString(raida.ReadyCount) + "\n";
            txtProgress.StringValue += "Not Ready Nodes-" + Convert.ToString(raida.NotReadyCount) + "\n";

            Debug.WriteLine("Ready Nodes-" + Convert.ToString(raida.ReadyCount));
            Debug.WriteLine("Not Ready Nodes-" + Convert.ToString(raida.NotReadyCount));

            //txtProgress.AppendText("----------------------------------\n");
        }
        public MainWindowController() : base("MainWindow")
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
        }

        public new MainWindow Window
        {
            get { return (MainWindow)base.Window; }
        }
    }
}
