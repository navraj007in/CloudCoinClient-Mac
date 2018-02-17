using System;

using AppKit;
using CloudCoinClientMAC.CoreClasses;
using CloudCoinCore;
using Foundation;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using ZXing;
using ZXing.Common;
using System.Drawing;
using SharpPdf417;

namespace CloudCoinMAC
{
    public partial class ViewController : NSViewController
    {
        RAIDA raida = AppDelegate.raida;
        FileSystem FS = AppDelegate.FS;
        string RootPath;
        //FileSystem FS;
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

        public ViewController(IntPtr handle) : base(handle)
        {
            
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Title = "CloudCoin CE - 2.0";

            ShowCoins();
            // Do any additional setup after loading the view.
        }

        partial void EchoClick(NSObject sender)
        {
            Echo();
            ShowCoins();
        }

        public void updateLog(string logLine)
        {
            BeginInvokeOnMainThread(() =>
            {
                NSString str = new NSString(logLine + System.Environment.NewLine);
                //txtProgress.StringValue += logLine + System.Environment.NewLine;
                //txtProgress.InsertText(str);
                txtLogs.InsertText(str);

            });
        }

        private bool PickFiles() {
            var dlg = NSOpenPanel.OpenPanel;
            dlg.CanChooseFiles = true;
            dlg.CanChooseDirectories = false;
            dlg.AllowsMultipleSelection = true;

            dlg.AllowedFileTypes = new string[] { "stack", "jpg", "chest" };

            if (dlg.RunModal() == 1)
            {
                // Nab the first file
                var url = dlg.Urls[0];
                for (int i = 0; i < dlg.Urls.Length; i++)
                {
                    Console.WriteLine(dlg.Urls[i].Path);
                    updateLog(dlg.Urls[i].Path);


                    var filename = dlg.Urls[i].Path;



                    //var filename = Path.GetFileName(path);
                    if (!File.Exists(FS.ImportFolder + Path.DirectorySeparatorChar + Path.GetFileName(filename)))
                        File.Copy(filename, FS.ImportFolder + Path.GetFileName(filename));
                    else
                    {
                        string msg = "File " + Path.GetFileName(filename) + " already exists. Do you want to overwrite it?";

                        var alert = new NSAlert()
                        {
                            AlertStyle = NSAlertStyle.Warning,
                            InformativeText = msg,
                            MessageText = "Import File",
                        };
                        alert.AddButton("OK");
                        alert.AddButton("Cancel");

                        nint num = alert.RunModal();

                        if (num == 1000)
                        {
                            File.Copy(filename, FS.ImportFolder + Path.GetFileName(filename), true);
                        }

                    }
                    updateLog("Copied " + filename + "to " +
                              FS.ImportFolder + Path.GetFileName(filename));

                }
            }
            else {
                return false;
            }

            return true;
        }
        partial void ImportClicked(NSObject sender)
        {
            var files = Directory
               .GetFiles(FS.ImportFolder)
               .Where(file => CloudCoinCore.Config.allowedExtensions.Any(file.ToLower().EndsWith))
               .ToList();

            int filesCount = Directory.GetFiles(FS.ImportFolder).Length;
            if(files.Count == 0) {
                bool PickResult = PickFiles();
                if (!PickResult)
                    return;
            }
            Detect();
        }
        public async void Echo()
        {
            var echos = AppDelegate.raida.GetEchoTasks();
            updateLog("Starting Echo to RAIDA\n");
            updateLog("----------------------------------\n");

            await Task.WhenAll(echos.AsParallel().Select(async task => await task()));
            //MessageBox.Show("Finished Echo");
            // lblReady.Content = raida.ReadyCount;
            // lblNotReady.Content = raida.NotReadyCount;

            for (int i = 0; i < raida.nodes.Count(); i++)
            {

                //txtProgress.StringValue += "Node " + i + " Status --" + raida.nodes[i].RAIDANodeStatus + "\n";

                //txtProgress.AppendText("Node " + i + " Status --" + raida.nodes[i].RAIDANodeStatus + "\n");
                Debug.WriteLine("Node" + i + " Status --" + raida.nodes[i].RAIDANodeStatus);
            }
            Debug.WriteLine("-----------------------------------\n");
            updateLog("Ready Nodes-" + Convert.ToString(raida.ReadyCount) + "\n");
            updateLog("Not Ready Nodes-" + Convert.ToString(raida.NotReadyCount) + "\n");

            Debug.WriteLine("Ready Nodes-" + Convert.ToString(raida.ReadyCount));
            Debug.WriteLine("Not Ready Nodes-" + Convert.ToString(raida.NotReadyCount));

            //txtProgress.AppendText("----------------------------------\n");
        }

        public async void Detect()
        {

            updateLog("Starting Multi Detect..");
            TimeSpan ts = new TimeSpan();
            DateTime before = DateTime.Now;
            DateTime after;
            FS.LoadFileSystem();

            // Prepare Coins for Import
            FS.DetectPreProcessing();

            var predetectCoins = FS.LoadFolderCoins(FS.PreDetectFolder);
            FileSystem.predetectCoins = predetectCoins;

            // Process Coins in Lots of 200. Can be changed from Config File
            int LotCount = predetectCoins.Count() / CloudCoinCore.Config.MultiDetectLoad;
            if (predetectCoins.Count() % CloudCoinCore.Config.MultiDetectLoad > 0) LotCount++;
            ProgressChangedEventArgs pge = new ProgressChangedEventArgs();

            int CoinCount = 0;
            int totalCoinCount = predetectCoins.Count();
            for (int i = 0; i < LotCount; i++)
            {
                //Pick up 200 Coins and send them to RAIDA
                var coins = predetectCoins.Skip(i * CloudCoinCore.Config.MultiDetectLoad).Take(200);
                raida.coins = coins;

                var tasks = raida.GetMultiDetectTasks(coins.ToArray(), CloudCoinCore.Config.milliSecondsToTimeOut);
                try
                {
                    string requestFileName = Utils.RandomString(16).ToLower() + DateTime.Now.ToString("yyyyMMddHHmmss") + ".stack";
                    // Write Request To file before detect
                    FS.WriteCoinsToFile(coins, FS.RequestsFolder + requestFileName);
                    await Task.WhenAll(tasks.AsParallel().Select(async task => await task()));
                    int j = 0;
                    foreach (var coin in coins)
                    {
                        //coin.pown = "";
                        for (int k = 0; k < CloudCoinCore.Config.NodeCount; k++)
                        {
                            coin.response[k] = raida.nodes[k].multiResponse.responses[j];
                            coin.pown += coin.response[k].outcome.Substring(0, 1);
                        }
                        int countp = coin.response.Where(x => x.outcome == "pass").Count();
                        int countf = coin.response.Where(x => x.outcome == "fail").Count();
                        coin.PassCount = countp;
                        coin.FailCount = countf;
                        CoinCount++;
                        updateLog("No. " + CoinCount + ". Coin Deteced. S. No. - " + coin.sn + ". Pass Count - " + coin.PassCount + ". Fail Count  - " + coin.FailCount + ". Result - " + coin.DetectionResult + "." + coin.pown);


                        Debug.WriteLine("Coin Deteced. S. No. - " + coin.sn + ". Pass Count - " + coin.PassCount + ". Fail Count  - " + coin.FailCount + ". Result - " + coin.DetectionResult);
                        //coin.sortToFolder();
                        pge.MinorProgress = (CoinCount) * 100 / totalCoinCount;
                        //bar1.Value = pge.MinorProgress;
                        Debug.WriteLine("Minor Progress- " + pge.MinorProgress);
                        raida.OnProgressChanged(pge);
                        j++;
                    }
                    pge.MinorProgress = (CoinCount - 1) * 100 / totalCoinCount;
                    Debug.WriteLine("Minor Progress- " + pge.MinorProgress);
                    raida.OnProgressChanged(pge);
                    FS.WriteCoin(coins, FS.DetectedFolder);

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        //import_Click.Enabled = true;
                    }
                    catch (Exception e)
                    {

                    }
                });

            }
            pge.MinorProgress = 100;
            Debug.WriteLine("Minor Progress- " + pge.MinorProgress);
            raida.OnProgressChanged(pge);
            var detectedCoins = FS.LoadFolderCoins(FS.DetectedFolder);

            detectedCoins.ForEach(x => x.sortToFolder());
            //foreach (var coin in detectedCoins)
            //{
            //    //updateLog()
            //    Debug.WriteLine(coin.sn + "-" + coin.pown + "-" + coin.folder);
            //}
            var passedCoins = (from x in detectedCoins
                               where x.folder == FS.BankFolder
                               select x).ToList();

            var failedCoins = (from x in detectedCoins
                               where x.folder == FS.CounterfeitFolder
                               select x).ToList();
            var lostCoins = (from x in detectedCoins
                             where x.folder == FS.LostFolder
                             select x).ToList();
            var suspectCoins = (from x in detectedCoins
                                where x.folder == FS.SuspectFolder
                                select x).ToList();

            Debug.WriteLine("Total Passed Coins - " + passedCoins.Count());
            Debug.WriteLine("Total Failed Coins - " + failedCoins.Count());
            updateLog("Coin Detection finished.");
            updateLog("Total Passed Coins - " + passedCoins.Count() + "");
            updateLog("Total Failed Coins - " + failedCoins.Count() + "");
            updateLog("Total Lost Coins - " + lostCoins.Count() + "");
            updateLog("Total Suspect Coins - " + suspectCoins.Count() + "");

            // Move Coins to their respective folders after sort
            FS.MoveCoins(passedCoins, FS.DetectedFolder, FS.BankFolder);
            FS.WriteCoin(failedCoins, FS.CounterfeitFolder, true);
            FS.MoveCoins(lostCoins, FS.DetectedFolder, FS.LostFolder);
            FS.MoveCoins(suspectCoins, FS.DetectedFolder, FS.SuspectFolder);

            // Clean up Detected Folder
            FS.RemoveCoins(failedCoins, FS.DetectedFolder);
            FS.RemoveCoins(lostCoins, FS.DetectedFolder);
            FS.RemoveCoins(suspectCoins, FS.DetectedFolder);

            FS.MoveImportedFiles();

            //FileSystem.detectedCoins = FS.LoadFolderCoins(FS.RootPath + System.IO.Path.DirectorySeparatorChar + FS.DetectedFolder);
            after = DateTime.Now;
            ts = after.Subtract(before);

            Debug.WriteLine("Detection Completed in - " + ts.TotalMilliseconds / 1000);
            updateLog("Detection Completed in - " + ts.TotalMilliseconds / 1000);
            ShowCoins();


        }

        private void ShowCoins()
        {
            var bankCoins = FS.LoadFolderCoins(FS.BankFolder);
            var frackedCoins = FS.LoadFolderCoins(FS.FrackedFolder);

            bankCoins.AddRange(frackedCoins);

            onesCount = (from x in bankCoins
                         where x.denomination == 1
                         select x).Count();
            fivesCount = (from x in bankCoins
                          where x.denomination == 5
                          select x).Count();
            qtrCount = (from x in bankCoins
                        where x.denomination == 25
                        select x).Count();
            hundredsCount = (from x in bankCoins
                             where x.denomination == 100
                             select x).Count();
            twoFiftiesCount = (from x in bankCoins
                               where x.denomination == 250
                               select x).Count();
            BindTable();

        }

        private void BindTable() 
        {
            var DataSource = new ProductTableDataSource();
            DataSource.Products.Add(new Product("1s", onesCount.ToString(), (onesCount.ToString())));
            DataSource.Products.Add(new Product("5s", fivesCount.ToString(),(fivesCount *5).ToString()));
            DataSource.Products.Add(new Product("25s", qtrCount.ToString(), (qtrCount*25).ToString()));
            DataSource.Products.Add(new Product("100s", hundredsCount.ToString(), (hundredsCount*100).ToString()));
            DataSource.Products.Add(new Product("250s", twoFiftiesCount.ToString(), (twoFiftiesCount*250).ToString()));

            // Populate the Product Table
            ProductTable.DataSource = DataSource;
            ProductTable.Delegate = new ProductTableDelegate(DataSource);

            
        }
        public override NSObject RepresentedObject
        {
            get
            {
                return base.RepresentedObject;
            }
            set
            {
                base.RepresentedObject = value;
                // Update the view, if already loaded.
            }
        }
    }
}
