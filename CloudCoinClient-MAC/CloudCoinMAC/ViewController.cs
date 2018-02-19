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
using System.Text;
using ZXing.QrCode;
//using ZXing.Core;
using System.DrawingCore;
using System.DrawingCore.Imaging;

namespace CloudCoinMAC
{
    public partial class ViewController : NSViewController
    {
        RAIDA raida = AppDelegate.raida;
        FileSystem FS = AppDelegate.FS;
        //string RootPath;
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

        ProductTableDataSource DataSource;

        //int onesTotal = 0;
        //int fivesTotal = 0;
        //int qtrsTotal = 0;
        //int hundredsTotal = 0;
        //int TwoFiftiesTotal = 0;


        public ViewController(IntPtr handle) : base(handle)
        {
            
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Title = "CloudCoin CE - 2.0";

            printWelcome();
            ShowCoins();
            raidaLevel.MaxValue = raida.nodes.Count();
            Echo();
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
                
                NSRange rang = new NSRange(txtLogs.Value.Length,0);

                txtLogs.SetSelectedRange(rang);
                txtLogs.InsertText(str);

            });
        }



        private byte[] GenerateBarCodeZXing(string data)
        {
            var qrCodeWriter = new ZXing.BarcodeWriterPixelData
            {
                Format = ZXing.BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Height = 250,
                    Width = 250,
                    Margin = 0
                }
            };

            var pixelData = qrCodeWriter.Write(data);
            // creating a bitmap from the raw pixel data; if only black and white colors are used it makes no difference    
            // that the pixel data ist BGRA oriented and the bitmap is initialized with RGB    
            using (var bitmap = new Bitmap(pixelData.Width, pixelData.Height, PixelFormat.Format32bppRgb))
            using (var ms = new MemoryStream())
            {
                var bitmapData = bitmap.LockBits(new System.DrawingCore.Rectangle(0, 0, pixelData.Width, pixelData.Height), 
                                                 ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
                try
                {
                    // we assume that the row stride of the bitmap is aligned to 4 byte multiplied by the width of the image    
                    System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }
                // save to stream as PNG    
                bitmap.Save(ms, ImageFormat.Png);

                ImageConverter converter = new ImageConverter();
                return (byte[])converter.ConvertTo(ms, typeof(byte[]));
                //return null;

            }
        }

        partial void ExportQRClicked(NSObject sender)
        {
                
                Image x = (Bitmap)((new ImageConverter()).ConvertFrom(GenerateBarCodeZXing("abcd")));

                x.Save(FS.RootPath + Path.DirectorySeparatorChar + "abc.jpg", ImageFormat.Jpeg);
           
        }
        partial void jPegClicked(NSObject sender)
        {
            rdbStack.State = NSCellStateValue.Off;
        }

        partial void stackClicked(NSObject sender)
        {
            rdbJpeg.State = NSCellStateValue.Off;
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

            raidaLevel.IntValue = raida.ReadyCount;

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
                        coin.pown = "";
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
                        updateLog("No. " + CoinCount + ". Coin Detected. S. No. : " + coin.sn + ". Pass Count : " + 
                                  coin.PassCount + ". Fail Count  : " + coin.FailCount + ". Result : " + 
                                  coin.DetectionResult + "." + coin.pown + ".Pown Length-"+ coin.pown.Length);


                        Debug.WriteLine("Coin Detected. S. No. - " + coin.sn + ". Pass Count : " + coin.PassCount + ". Fail Count  : " + coin.FailCount + ". Result - " + coin.DetectionResult);
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
                    FS.RemoveCoins(coins,FS.PreDetectFolder);

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
                        Debug.WriteLine(e.Message);
                    }
                });

            }
            pge.MinorProgress = 100;
            Debug.WriteLine("Minor Progress- " + pge.MinorProgress);
            raida.OnProgressChanged(pge);
            var detectedCoins = FS.LoadFolderCoins(FS.DetectedFolder);
            detectedCoins.ForEach(x => x.setAnsToPansIfPassed());
            detectedCoins.ForEach(x => x.calculateHP());
            detectedCoins.ForEach(x => x.calcExpirationDate());

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
            updateLog("Total Passed Coins : " + passedCoins.Count() + "");
            updateLog("Total Failed Coins : " + failedCoins.Count() + "");
            updateLog("Total Lost Coins : " + lostCoins.Count() + "");
            updateLog("Total Suspect Coins : " + suspectCoins.Count() + "");

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

            Debug.WriteLine("Detection Completed in : " + ts.TotalMilliseconds / 1000);
            updateLog("Detection Completed in : " + ts.TotalMilliseconds / 1000);
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
        int[] multiplier = new int[] { 1, 5, 25, 100, 250 };

        private void BindTable() 
        {
            DataSource = new ProductTableDataSource();
            DataSource.Products.Add(new Product("1s", onesCount.ToString(), (onesCount.ToString())));
            DataSource.Products.Add(new Product("5s", fivesCount.ToString(),(fivesCount *5).ToString()));
            DataSource.Products.Add(new Product("25s", qtrCount.ToString(), (qtrCount*25).ToString()));
            DataSource.Products.Add(new Product("100s", hundredsCount.ToString(), (hundredsCount*100).ToString()));
            DataSource.Products.Add(new Product("250s", twoFiftiesCount.ToString(), (twoFiftiesCount*250).ToString()));

            // Populate the Product Table
            ProductTable.DataSource = DataSource;
            ProductTable.Delegate = new ProductTableDelegate(DataSource);

            int total = 0;

            for (int i = 0; i < multiplier.Length;i++) {
                total += Convert.ToInt32(DataSource.Products[i].NotesValue);
            }

            lblBankTotal.IntValue = total;
        }

        partial void BackupClicked(NSObject sender)
        {
            var bankCoins = FS.LoadFolderCoins(FS.BankFolder);
            var frackedCoins = FS.LoadFolderCoins(FS.FrackedFolder);
            var partialCoins = FS.LoadFolderCoins(FS.PartialFolder);

            // Add them all up in a single list for backup

            bankCoins.AddRange(frackedCoins);
            bankCoins.AddRange(partialCoins);

            String[] bankFileNames = new DirectoryInfo(FS.BankFolder).
                                                                                           GetFiles("*.stack").
                                                                                           Select(o => o.Name).ToArray();//Get all files in suspect folder
            if (bankCoins.Count == 0)
            {
                string msg = "No Coins found in bank for backup.";

                var alert = new NSAlert()
                {
                    AlertStyle = NSAlertStyle.Warning,
                    InformativeText = msg,
                    MessageText = "Backup",
                };
                alert.AddButton("OK");
                nint num = alert.RunModal();
                return;
            }
            Banker bank = new Banker(FS);
            int[] bankTotals = bank.countCoins(FS.BankFolder);
            int[] frackedTotals = bank.countCoins(FS.FrackedFolder);
            int[] partialTotals = bank.countCoins(FS.PartialFolder);

            var dlg = NSOpenPanel.OpenPanel;
            dlg.CanChooseFiles = false;
            dlg.CanChooseDirectories = true;
            dlg.AllowsMultipleSelection = false;
            dlg.CanCreateDirectories = true;


            if (dlg.RunModal() == 1)
            {
                string msg = "Are you sure you want to backup your CloudCoin Directory?";

                var alert = new NSAlert()
                {
                    AlertStyle = NSAlertStyle.Warning,
                    InformativeText = msg,
                    MessageText = "Backup CloudCoins",
                };
                alert.AddButton("OK");
                alert.AddButton("Cancel");

                nint num = alert.RunModal();

                if (num == 1000)
                {
                    FS.WriteCoinsToFile(bankCoins, dlg.Urls[0].Path + System.IO.Path.DirectorySeparatorChar + 
                                        "backup" + DateTime.Now.ToString("yyyyMMddHHmmss").ToLower());
                 
                    //export(dlg.Urls[0].Path);
                    String backupDir = dlg.Urls[0].Path;
                    NSWorkspace.SharedWorkspace.SelectFile(backupDir,
                                                           backupDir);

                }

            }
           
           

        }

        partial void ListSerialsClicked(NSObject sender)
        {
            var dlg = NSOpenPanel.OpenPanel;
            dlg.CanChooseFiles = false;
            dlg.CanChooseDirectories = true;
            dlg.AllowsMultipleSelection = false;
            dlg.CanCreateDirectories = true;

            if (dlg.RunModal() == 1)
            {
                string msg = "Are you sure you want to List Serials?";

                var alert = new NSAlert()
                {
                    AlertStyle = NSAlertStyle.Warning,
                    InformativeText = msg,
                    MessageText = "List CloudCoins Serials",
                };
                alert.AddButton("OK");
                alert.AddButton("Cancel");

                nint num = alert.RunModal();

                if (num == 1000)
                {


                    //export(dlg.Urls[0].Path);
                    String backupDir = dlg.Urls[0].Path;

                    var csv = new StringBuilder();
                    var coins = FS.LoadFolderCoins(backupDir).OrderBy(x => x.sn);

                    var headerLine = string.Format("sn,denomination,nn,");
                    string headeranstring = "";
                    for (int i = 0; i < CloudCoinCore.Config.NodeCount; i++)
                    {
                        headeranstring += "an" + (i + 1) + ",";
                    }

                    // Write the Header Record
                    csv.AppendLine(headerLine + headeranstring);

                    // Write the Coin Serial Numbers
                    foreach (var coin in coins)
                    {
                        string anstring = "";
                        for (int i = 0; i < CloudCoinCore.Config.NodeCount; i++)
                        {
                            anstring += coin.an[i] + ",";
                        }
                        var newLine = string.Format("{0},{1},{2},{3}", coin.sn, coin.denomination, coin.nn, anstring);
                        csv.AppendLine(newLine);
                    }
                    File.WriteAllText(backupDir + System.IO.Path.DirectorySeparatorChar + "coinserails" + DateTime.Now.ToString("yyyyMMddHHmmss").ToLower() + ".csv", csv.ToString());
                    //Process.Start(backupDir);

                    NSWorkspace.SharedWorkspace.SelectFile(backupDir,
                                                           backupDir);

                }

            }
        }
        partial void ShowFolderClicked(NSObject sender)
        {
            NSWorkspace.SharedWorkspace.SelectFile(FS.RootPath,
                                                   FS.RootPath);
            var defaults = NSUserDefaults.StandardUserDefaults;
            Console.WriteLine(defaults.StringForKey("workspace"));

        }
        partial void ExportClicked(NSObject sender)
        {
            export();
        }

        private void printWelcome()
        {
            updateLog("CloudCoin Consumers Edition");
            updateLog("Version " + DateTime.Now.ToShortDateString());
            updateLog("Used to Authenticate ,Store,Payout CloudCoins");
            updateLog("This Software is provided as is with all faults, " +
                      "defects and errors, and without warranty of any kind.Free from the CloudCoin Consortium.\n");
        }

        public void export()
        {
            


            exportJpegStack = 2;
            if (rdbStack.State == NSCellStateValue.On)
            {
                exportJpegStack = 2;
            }
            else
                exportJpegStack = 1;
            


            Banker bank = new Banker(FS);
            int[] bankTotals = bank.countCoins(FS.BankFolder);
            int[] frackedTotals = bank.countCoins(FS.FrackedFolder);
            int[] partialTotals = bank.countCoins(FS.PartialFolder);

            //updateLog("  Your Bank Inventory:");
            int grandTotal = (bankTotals[0] + frackedTotals[0] + partialTotals[0]);
            // state how many 1, 5, 25, 100 and 250
            int exp_1 = Convert.ToInt16(DataSource.Products[0].ExportCount);
            int exp_5 = Convert.ToInt16(DataSource.Products[1].ExportCount);
            int exp_25 = Convert.ToInt16(DataSource.Products[2].ExportCount);
            int exp_100 = Convert.ToInt16(DataSource.Products[3].ExportCount);
            int exp_250 = Convert.ToInt16(DataSource.Products[4].ExportCount);
            //Warn if too many coins

            if (exp_1 + exp_5 + exp_25 + exp_100 + exp_250 == 0)
            {
                Console.WriteLine("Can not export 0 coins");
                return;
            }

            //updateLog(Convert.ToString(bankTotals[1] + frackedTotals[1] + bankTotals[2] + frackedTotals[2] + bankTotals[3] + frackedTotals[3] + bankTotals[4] + frackedTotals[4] + bankTotals[5] + frackedTotals[5] + partialTotals[1] + partialTotals[2] + partialTotals[3] + partialTotals[4] + partialTotals[5]));

            if (((bankTotals[1] + frackedTotals[1]) + (bankTotals[2] + frackedTotals[2]) + (bankTotals[3] + frackedTotals[3]) + (bankTotals[4] + frackedTotals[4]) + (bankTotals[5] + frackedTotals[5]) + partialTotals[1] + partialTotals[2] + partialTotals[3] + partialTotals[4] + partialTotals[5]) > 1000)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Out.WriteLine("Warning: You have more than 1000 Notes in your bank. Stack files should not have more than 1000 Notes in them.");
                Console.Out.WriteLine("Do not export stack files with more than 1000 notes. .");
                //updateLog("Warning: You have more than 1000 Notes in your bank. Stack files should not have more than 1000 Notes in them.");
                //updateLog("Do not export stack files with more than 1000 notes. .");

                Console.ForegroundColor = ConsoleColor.White;
            }//end if they have more than 1000 coins

            int file_type = 0; //reader.readInt(1, 2);

            Exporter exporter = new Exporter(FS);
            //exporter.OnUpdateStatus +=  ;
            file_type = exportJpegStack;

            String tag = txtTag.StringValue;// reader.readString();
                                            //Console.Out.WriteLine(("Exporting to:" + exportFolder));

            if (file_type == 1)
            {
                exporter.writeJPEGFiles(exp_1, exp_5, exp_25, exp_100, exp_250, tag);
            }
            else
            {
                exporter.writeJSONFile(exp_1, exp_5, exp_25, exp_100, exp_250, tag);
            }
            // end if type jpge or stack
            Console.Out.WriteLine("  Exporting CloudCoins Completed.");

            NSWorkspace.SharedWorkspace.SelectFile(FS.ExportFolder,
                                                   FS.ExportFolder);

            //RefreshCoins?.Invoke(this, new EventArgs());

            ShowCoins();
        }// end export One
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
