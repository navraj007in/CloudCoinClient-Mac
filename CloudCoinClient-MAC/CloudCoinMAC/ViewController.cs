﻿using System;
using System.Web.Services;
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
using System.Collections.Generic;
using CoreGraphics;

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
        SimpleLogger logger = new SimpleLogger();
        ProductTableDataSource DataSource;
        Frack_Fixer fixer ;
        //int onesTotal = 0;
        //int fivesTotal = 0;
        //int qtrsTotal = 0;
        //int hundredsTotal = 0;
        //int TwoFiftiesTotal = 0;


        public ViewController(IntPtr handle) : base(handle)
        {
           
        }
        public event EventHandler LogRecieved;
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            fixer = new Frack_Fixer(FS, CloudCoinCore.Config.milliSecondsToTimeOut);
            Title = "CloudCoin CE - 1.2.0";
            lblWorkspace.StringValue = "Workspace : "+FS.RootPath;
            printWelcome();
            ShowCoins();
            raidaLevel.MaxValue = raida.nodes.Count();
            Echo(true);
            txtLogs.TextColor = NSColor.White;
            //txtOnes.TextColor = NSColor.White;
            //txtFives.TextColor = NSColor.White;
            //txtQtrs.TextColor = NSColor.White;
            //txtHundreds.TextColor = NSColor.White;
            //txtTwoFifties.TextColor = NSColor.White;
            //txtOnes.BackgroundColor = NSColor.FromRgb(45, 146, 255);
            //txtOnes.Cell.BackgroundColor = NSColor.FromRgb(45, 146, 255);
            txtOnes.Formatter = new NumberOnlyFormattter();
            txtFives.Formatter = new NumberOnlyFormattter();
            txtQtrs.Formatter = new NumberOnlyFormattter();
            txtHundreds.Formatter = new NumberOnlyFormattter();
            txtQtrs.Formatter = new NumberOnlyFormattter();
            
            logger = new SimpleLogger(FS.LogsFolder + "logs" + DateTime.Now.ToString("yyyyMMdd").ToLower()+".log",true);

            
            //txtOnes.TextColor = NSColor.FromRgb(45, 146, 255);
            raida.LoggerHandler += Raida_LogRecieved;




            
            // Do any additional setup after loading the view.
        }


        
        void Raida_LogRecieved(object sender, EventArgs e)
        {
            ProgressChangedEventArgs pge = (ProgressChangedEventArgs)e;
            //DetectEventArgs eargs = e;
            //updateLog(pge.MajorProgressMessage);
            logger.Info(pge.MajorProgressMessage);
            //Debug.WriteLine("Coin Detection Event Recieved - " + eargs.DetectedCoin.sn);
        }
        partial void EchoClick(NSObject sender)
        {
            Echo();
            ShowCoins();

        }
        
        public void updateLog(string logLine,bool writeUI = true)
        {
            BeginInvokeOnMainThread(() =>
            {
                NSString str = new NSString(logLine + System.Environment.NewLine);
                //txtProgress.StringValue += logLine + System.Environment.NewLine;
                //txtProgress.InsertText(str);
                if(writeUI){
                    NSRange rang = new NSRange(txtLogs.Value.Length, 0);

                    txtLogs.SetSelectedRange(rang);
                    txtLogs.InsertText(str);
                }


                logger.Info(logLine);
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
                    updateLog("Selected "+dlg.Urls[i].Path);


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
                    File.Delete(filename);

                    updateLog("Copied " + filename + " to " +
                              FS.ImportFolder + Path.GetFileName(filename));

                }
            }
            else {
                return false;
            }

            return true;
        }

        private void DisableUI() {
            BeginInvokeOnMainThread(() =>
            {
                cmdEcho.Enabled = false;
                cmdImport.Enabled = false;
            });
        }

        private void EnableUI() {
            BeginInvokeOnMainThread(() =>
            {
                cmdEcho.Enabled = true;
                cmdImport.Enabled = true;
            });
        }
        partial void ImportClicked(NSObject sender)
        {
            if(!(raida.ReadyCount >= CloudCoinCore.Config.MinimumReadyCount) ) {
                updateLog("Not Enough Nodes ready for detection. Quitting.");
                return;
            }
            fixer.continueExecution = false;

            printLineDots();
            updateLog("Starting CloudCoin Import. \n\tPlease do not close the CloudCoin CE program until it is finished." +
                      "\n\tOtherwise it may result in loss of CloudCoins.");
            printLineDots();
            if (fixer.IsFixing)
            {
                updateLog("Stopping Fix");
                Debug.WriteLine("Stopping Fix");
            }
            System.Threading.SpinWait.SpinUntil(() => !fixer.IsFixing);
            var files = Directory
               .GetFiles(FS.ImportFolder)
               .Where(file => CloudCoinCore.Config.allowedExtensions.Any(file.ToLower().EndsWith))
               .ToList();

            int filesCount = Directory.GetFiles(FS.ImportFolder).Length;
            if(files.Count == 0) {
                bool PickResult = PickFiles();
                if (!PickResult){
                    Task.Run(() => {
                        Fix();
                    });
                    return;
                }      
            }
            DisableUI();
            Detect();
            //EnableUI();
        }


        public async void Echo(bool resumeFix=false)
        {
            DisableUI();
            var echos = AppDelegate.raida.GetEchoTasks();
            updateLog("\tEcho RAIDA");
            updateLog("----------------------------------");

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
            updateLog("\tReady Nodes : " + Convert.ToString(raida.ReadyCount) + "\n");
            updateLog("\tNot Ready Nodes : " + Convert.ToString(raida.NotReadyCount) + "");

            Debug.WriteLine("Ready Nodes-" + Convert.ToString(raida.ReadyCount));
            Debug.WriteLine("Not Ready Nodes-" + Convert.ToString(raida.NotReadyCount));
            updateLog("----------------------------------");

            EnableUI();
            if (resumeFix)
                Task.Run(() => {
                    Fix();
                });
            //txtProgress.AppendText("----------------------------------\n");
        }

        public async void Detect()
        {
            updateLog("Starting Detect..");
            printLineDots();
            TimeSpan ts = new TimeSpan();
            DateTime before = DateTime.Now;
            DateTime after;
            FS.LoadFileSystem();

            // Prepare Coins for Import
            FS.DetectPreProcessing();
            FS.MoveCoins(FileSystem.suspectCoins, FS.SuspectFolder, FS.PreDetectFolder);
            
            IEnumerable<CloudCoin> predetectCoins = FS.LoadFolderCoins(FS.PreDetectFolder);
            FileSystem.predetectCoins = predetectCoins;

            IEnumerable<CloudCoin> bankCoins = FileSystem.bankCoins;
            IEnumerable<CloudCoin> frackedCoins1 = FileSystem.frackedCoins;

            var bCoins = bankCoins.ToList();
            bCoins.AddRange(frackedCoins1);
            //bankCoins.ToList().AddRange(frackedCoins1);

            var totalBankCoins = bCoins;

            var snList = (from x in totalBankCoins
                          select x.sn).ToList();

            var newCoins = from x in predetectCoins where !snList.Contains(x.sn) select x;
            var existingCoins = from x in predetectCoins where snList.Contains(x.sn) select x;

            foreach (var coin in existingCoins)
            {
                updateLog("Found existing coin :" + coin.sn + ". Skipping.");
                FS.MoveFile(FS.PreDetectFolder + coin.FileName + ".stack", FS.TrashFolder + coin.FileName + ".stack", IFileSystem.FileMoveOptions.Replace);
            }

            predetectCoins = newCoins;

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
                coins.ToList().ForEach(x=>x.pown="");
                raida.coins = coins;
                if(i == LotCount-1) {
                    updateLog("\tDetecting Coins " + (i * CloudCoinCore.Config.MultiDetectLoad + 1) +
                              " to " + totalCoinCount);
                }
                else
                    updateLog("\tDetecting Coins " + (i * CloudCoinCore.Config.MultiDetectLoad + 1) + 
                          " to " + (i + 1) * CloudCoinCore.Config.MultiDetectLoad);
                var tasks = raida.GetMultiDetectTasks(coins.ToArray(), CloudCoinCore.Config.milliSecondsToTimeOut);
                try
                {
                    string requestFileName = coins.Count()+ ".CloudCoins."+ Utils.RandomString(16).ToLower() + 
                                                                            DateTime.Now.ToString("yyyyMMddHHmmss") + ".stack";
                    // Write Request To file before detect
                    FS.WriteCoinsToFile(coins, FS.RequestsFolder + requestFileName);
                    await Task.WhenAll(tasks.AsParallel().Select(async task => await task()));
                    int j = 0;
                    foreach (var coin in coins)
                    {
                        coin.pown = "";
                        for (int k = 0; k < CloudCoinCore.Config.NodeCount; k++)
                        {
                            coin.response[k] = raida.nodes[k].MultiResponse.responses[j];
                            coin.pown += coin.response[k].outcome.Substring(0, 1);
                        }
                        int countp = coin.response.Where(x => x.outcome == "pass").Count();
                        int countf = coin.response.Where(x => x.outcome == "fail").Count();
                        coin.PassCount = countp;
                        coin.FailCount = countf;
                        CoinCount++;
                        updateLog("No. " + CoinCount + ". Coin Detected. S. No. : " + coin.sn + ". Pass Count : " + 
                                  coin.PassCount + ". Fail Count  : " + coin.FailCount + ". Result : " + 
                                  coin.DetectionResult + "." + coin.pown + ".",false);


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
            updateLog("\tDetection finished.");
            printLineDots();
            updateLog("Starting Grading Coins..");
            var detectedCoins = FS.LoadFolderCoins(FS.DetectedFolder);
            detectedCoins.ForEach(x => x.setAnsToPansIfPassed());
            detectedCoins.ForEach(x => x.calculateHP());
            detectedCoins.ForEach(x => x.calcExpirationDate());

            detectedCoins.ForEach(x => x.SortToFolder());
            updateLog("Grading Coins Completed.");
            //foreach (var coin in detectedCoins)
            //{
            //    //updateLog()
            //    Debug.WriteLine(coin.sn + "-" + coin.pown + "-" + coin.folder);
            //}
            var passedCoins = (from x in detectedCoins
                               where x.folder == FS.BankFolder
                               select x).ToList();

            var frackedCoins = (from x in detectedCoins
                                where x.folder == FS.FrackedFolder
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
            var dangerousCoins = (from x in detectedCoins
                                  where x.folder == FS.DangerousFolder
                                  select x).ToList();
            
           
            Debug.WriteLine("Total Passed Coins - " + passedCoins.Count());
            Debug.WriteLine("Total Failed Coins - " + failedCoins.Count());
            updateLog("Detection and Import of the CloudCoins completed.");
            updateLog("\tTotal Passed Coins : " + (passedCoins.Count() + frackedCoins.Count()) + "");
            updateLog("\tTotal Counterfeit Coins : " + failedCoins.Count() + "");
            updateLog("\tTotal Lost Coins : " + lostCoins.Count() + "");
            updateLog("\tTotal Suspect Coins : " + suspectCoins.Count() + "");
            updateLog("\tTotal Skipped Coins : " + existingCoins.Count() + "");
            updateLog("\tTotal Dangerous Coins : " + dangerousCoins.Count() + "");


            // Move Coins to their respective folders after sort
            FS.TransferCoins(passedCoins, FS.DetectedFolder, FS.BankFolder);
            FS.TransferCoins(frackedCoins, FS.DetectedFolder, FS.FrackedFolder);
            if(failedCoins.Count > 0)
            {
                FS.WriteCoin(failedCoins, FS.CounterfeitFolder, true);
            }
            FS.MoveCoins(lostCoins, FS.DetectedFolder, FS.LostFolder);
            FS.TransferCoins(suspectCoins, FS.DetectedFolder, FS.SuspectFolder);
            FS.MoveCoins(dangerousCoins, FS.DetectedFolder, FS.DangerousFolder);

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
            printLineDots();
            EnableUI();
            ShowCoins();
            Task.Run(() => {
                Fix();
            });
            FS.LoadFileSystem();
 
        }

        private void Fix() {
            //Frack_Fixer fixer = new Frack_Fixer(FS,CloudCoinCore.Config.milliSecondsToTimeOut);
            fixer.continueExecution = true;
            fixer.IsFixing = true;
            fixer.FixAll();
            fixer.IsFixing = false;
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
            DataSource.Products.Add(new Product("1s", string.Format("{0:n0}", onesCount), 
                                                string.Format("{0:n0}", onesCount) + " CC"));
            DataSource.Products.Add(new Product("5s", string.Format("{0:n0}", fivesCount),
                                                string.Format("{0:n0}", (fivesCount*5)) + " CC"));
            DataSource.Products.Add(new Product("25s", string.Format("{0:n0}", qtrCount), 
                                                string.Format("{0:n0}", (qtrCount * 25))+ " CC"));
            DataSource.Products.Add(new Product("100s", string.Format("{0:n0}", hundredsCount), 
                                                string.Format("{0:n0}", (hundredsCount * 100)) + " CC"));
            DataSource.Products.Add(new Product("250s", string.Format("{0:n0}", twoFiftiesCount),
                                                string.Format("{0:n0}", (twoFiftiesCount * 250)) + " CC"));
            
            int total = onesCount + fivesCount + qtrCount + hundredsCount + twoFiftiesCount;
            int totalAmount = onesCount + (fivesCount * 5) + (qtrCount * 25) + (hundredsCount * 100) + (twoFiftiesCount * 250);

            string totalStr = string.Format("{0:n0}", total);
            //totalStr = String.Format("{0:#,###,###.##}", total);
            //totalStr = total.ToString("###,###,####");
            DataSource.Products.Add(new Product("Bank Total", totalStr , string.Format("{0:n0}", totalAmount)
                                                 + " CC"));
            //DataSource.Products.Add(new Product("","Bank Total", string.Format("{0:n0}", totalAmount)
             //                                    + " CC"));
            // Populate the Product Table
            ProductTable.DataSource = DataSource;
            ProductTable.Delegate = new ProductTableDelegate(DataSource);

            //total = 0;

          /*  for (int i = 0; i < multiplier.Length;i++) {
                total += Convert.ToInt32(DataSource.Products[i].NotesValue);
            }
*/
            //lblBankTotal.IntValue = totalAmount;
        }
        private void printLineDots(){
            updateLog("****************************************************************************************************");
        }
        partial void BackupClicked(NSObject sender)
        {
            try{
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
                
                //dlg.DirectoryUrl = new NSUrl(FS.RootPath,UriFormat.UriEscaped.ToString());

                var uri = new Uri(FS.RootPath);
                var nsurl = new NSUrl(uri.GetComponents(UriComponents.HttpRequestUrl, UriFormat.UriEscaped));
                dlg.DirectoryUrl = nsurl;

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
                        String backupDir = dlg.Urls[0].Path;

                        string backupFileName = "backup" + DateTime.Now.ToString("yyyyMMddHHmmss").ToLower(); 
                        FS.WriteCoinsToFile(bankCoins, dlg.Urls[0].Path + System.IO.Path.DirectorySeparatorChar +
                                            backupFileName);
                        printLineDots();
                        updateLog("Backup file " + backupFileName + " saved to " + backupDir + " .");
                        printLineDots();
                        //export(dlg.Urls[0].Path);
                        NSWorkspace.SharedWorkspace.SelectFile(backupDir,
                                                               backupDir);

                    }

                }

            }
            catch(Exception e){
                logger.Error(e.Message);
            }
           
           

        }

        partial void ListSerialsClicked(NSObject sender)
        {
            try {
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
                        String backupDir = (dlg.Urls[0].Path);
                        //backupDir = System.Web.HttpUtility.UrlEncode("/Users/ivanolsak/Desktop/CC tests/MAC 1.0.3/CloudCoin/Export");
                        NSSavePanel panel = new NSSavePanel();
                        printLineDots();
                        updateLog("List Serials Path Selected - "+ backupDir);
                        //panel.DirectoryUrl = new NSUrl(backupDir);
                        panel.DirectoryUrl = dlg.Urls[0];
                        String dirName = new DirectoryInfo(backupDir).Name;
                        string destinationPath = "CoinList" + DateTime.Now.ToString("yyyy.MM.dd").ToLower() + "." + dirName + ".csv";

                        panel.NameFieldStringValue = destinationPath;
                        if(destinationPath.Length>=219) {
                            updateLog("The path you selected has a length more than 219 characters." +
                                      " Please move your folder to a different location or rename the folder, whichever is appropriate.");
                            return;
                        }
                        nint result = panel.RunModal();

                        if (result == 1)
                        {

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

                            string targetPath = panel.Url.Path;
                            File.WriteAllText(targetPath, csv.ToString());
                            updateLog("CSV file " + targetPath + " saved.");
                            printLineDots();
                            NSWorkspace.SharedWorkspace.SelectFile(targetPath,
                                                                   targetPath);
                        }
                    }

                }

            }
            catch(Exception e) {
                logger.Error(e.Message);
            }
        }
        partial void ShowFolderClicked(NSObject sender)
        {
            NSWorkspace.SharedWorkspace.OpenFile(FS.RootPath);
        }
        partial void ExportClicked(NSObject sender)
        {
            exportJpegStack = 2;
            if (rdbStack.State == NSCellStateValue.On)
            {
                exportJpegStack = 2;
            }
            else
                exportJpegStack = 1;
            string tag = txtTag.StringValue;
            int exp_1 = Convert.ToInt16(txtOnes.IntValue);
            int exp_5 = Convert.ToInt16(txtFives.IntValue);
            int exp_25 = Convert.ToInt16(txtQtrs.IntValue);
            int exp_100 = Convert.ToInt16(txtHundreds.IntValue);
            int exp_250 = Convert.ToInt16(txtTwoFifties.IntValue);

            Task.Run(() => {
                Export(tag,exp_1,
                       exp_5,
                       exp_25,
                       exp_100,
                       exp_250);
            });

        }

        private void printWelcome()
        {
            updateLog("CloudCoin Consumers Edition" );
            updateLog("Version " + NSBundle.MainBundle.InfoDictionary["CFBundleShortVersionString"].ToString() 
                      + "\nDated : " + DateTime.Now.ToShortDateString());
            updateLog("Used to Authenticate ,Store and Payout CloudCoins.");
            updateLog("This Software is provided as is, with all faults, " +
                      "defects and errors, and without warranty of any kind. Free from the CloudCoin Consortium.\n");
        }

        partial void ChangeWorkSpace(NSObject sender)
        {
            var dlg = NSOpenPanel.OpenPanel;
            dlg.CanChooseFiles = false;
            dlg.CanChooseDirectories = true;
            dlg.AllowsMultipleSelection = false;
            dlg.CanCreateDirectories = true;


            if (dlg.RunModal() == 1)
            {
                string msg = "Are you sure you want to change your CloudCoin Directory? This will not move your coins to new folders!";

                var alert = new NSAlert()
                {
                    AlertStyle = NSAlertStyle.Warning,
                    InformativeText = msg,
                    MessageText = "Change Workspace",
                };
                alert.AddButton("OK");
                alert.AddButton("Cancel");

                nint num = alert.RunModal();

                if (num == 1000)
                {
                    string msgRestart = "Changing the workspace will require you to manually restart the Application. Contniue?";
                    var alertRestart = new NSAlert()
                    {
                        AlertStyle = NSAlertStyle.Warning,
                        InformativeText = msgRestart,
                        MessageText = "Restart CloudCoin CE",
                    };
                    alertRestart.AddButton("Yes");
                    alertRestart.AddButton("No");

                    nint numRestart = alertRestart.RunModal();
                    if (numRestart == 1000)
                    {
                        Console.WriteLine(dlg.Urls[0].Path);
                        var defaults = NSUserDefaults.StandardUserDefaults;
                        defaults.SetString(dlg.Urls[0].Path + System.IO.Path.DirectorySeparatorChar, "workspace");

                        string RootPath = defaults.StringForKey("workspace");
                        FileSystem fileUtils = new FileSystem(RootPath); 

                        fileUtils.CreateFolderStructure();
                        updateLog("Workspace changed to " + RootPath + " .");
                        System.Diagnostics.Process.GetCurrentProcess().Kill();
                        AppDelegate.FS = new FileSystem(RootPath);

                    }
                    else
                    {

                    }

                }


            }
        }

        public void Export(string tag,int exp_1,int exp_5,int exp_25,int exp_100,int exp_250)
        {
            fixer.continueExecution = false;
            if (fixer.IsFixing)
            {
                updateLog("Stopping Fix");
                Debug.WriteLine("Stopping Fix");
            }
            printLineDots();
            updateLog("Starting CloudCoin Export.");
            updateLog("\tPlease do not close the CloudCoin CE program until it is finished. " +
                      "\n\tOtherwise it may result in loss of CloudCoins.");
            printLineDots();
            System.Threading.SpinWait.SpinUntil(() => !fixer.IsFixing);
           
            //updateLog("Exporting XYZ CloudCoins from Bank. Do not close CloudCoin CE program until it is finished!");
            Banker bank = new Banker(FS);
            int[] bankTotals = bank.countCoins(FS.BankFolder);
            int[] frackedTotals = bank.countCoins(FS.FrackedFolder);
            int[] partialTotals = bank.countCoins(FS.PartialFolder);

            //updateLog("  Your Bank Inventory:");
            int grandTotal = (bankTotals[0] + frackedTotals[0] + partialTotals[0]);
            int exportTotal = exp_1 + (exp_5 * 5) + (exp_25 * 25) + (exp_100 * 100) + (exp_250 * 250);

            if(exp_1> onesCount) {
                updateLog("Export of CloudCoins stopped.");
                updateLog("\tNot sufficient coins in denomination 1.");
                Task.Run(() => {
                    Fix();
                });
                return;
            }
            if (exp_5 > fivesCount)
            {
                updateLog("Export of CloudCoins stopped.");
                updateLog("\tNot sufficient coins in denomination 5.");
                Task.Run(() => {
                    Fix();
                });
                return;
            }
            if (exp_25 > qtrCount)
            {
                updateLog("Export of CloudCoins stopped.");
                updateLog("\tNot sufficient coins in denomination 25.");
                Task.Run(() => {
                    Fix();
                });
               return;
            }
            if (exp_100 > hundredsCount)
            {
                updateLog("Export of CloudCoins stopped.");
                updateLog("\tNot sufficient coins in denomination 100.");
                Task.Run(() => {
                    Fix();
                });
                return;
            }
            if (exp_250 > twoFiftiesCount)
            {
                updateLog("Export of CloudCoins stopped.");
                updateLog("\tNot sufficient coins in denomination 250.");
                Task.Run(() => {
                    Fix();
                });
                return;
            }


            updateLog("Exporting "+ exportTotal +" CloudCoins from Bank.");
            printLineDots();

            // state how many 1, 5, 25, 100 and 250
            //int exp_1 = Convert.ToInt16(txtOnes.IntValue);
            //int exp_5 = Convert.ToInt16(txtFives.IntValue);
            //int exp_25 = Convert.ToInt16(txtQtrs.IntValue);
            //int exp_100 = Convert.ToInt16(txtHundreds.IntValue);
            //int exp_250 = Convert.ToInt16(txtTwoFifties.IntValue);
            //Warn if too many coins

            if (exp_1 + exp_5 + exp_25 + exp_100 + exp_250 == 0)
            {
                Console.WriteLine("Can not export 0 coins");
                updateLog("Can not export 0 coins");
                Task.Run(() => {
                    Fix();
                });
                return;
            }

            //updateLog(Convert.ToString(bankTotals[1] + frackedTotals[1] + bankTotals[2] + frackedTotals[2] + bankTotals[3] + frackedTotals[3] + bankTotals[4] + frackedTotals[4] + bankTotals[5] + frackedTotals[5] + partialTotals[1] + partialTotals[2] + partialTotals[3] + partialTotals[4] + partialTotals[5]));

            if (((bankTotals[1] + frackedTotals[1]) + (bankTotals[2] + frackedTotals[2]) + (bankTotals[3] + frackedTotals[3]) + (bankTotals[4] + frackedTotals[4]) + (bankTotals[5] + frackedTotals[5]) + partialTotals[1] + partialTotals[2] + partialTotals[3] + partialTotals[4] + partialTotals[5]) > 1000)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                printLineDots();
                updateLog("Warning!: You have more than 1000 Notes in your bank. \n\tStack files should not have more than 1000 Notes in them.");
                updateLog("\tDo not export stack files with more than 1000 notes. .");
                printLineDots();
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

            //String tag = txtTag.StringValue;// reader.readString();
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
            Console.Out.WriteLine("Exporting of the CloudCoins Completed.");
            updateLog("Export of the CloudCoins Completed.");
            updateLog("\tExported "+ String.Format("{0:n0}", (exp_1 + exp_5 + exp_25 + exp_100 + exp_250)) 
                      +" coins in Total of "+ String.Format("{0:n}", exportTotal) +" CC into " +
                      " " + FS.ExportFolder + " .");
            printLineDots();
            //NSWorkspace.SharedWorkspace.SelectFile(FS.ExportFolder,
              //                                     FS.ExportFolder);
            NSWorkspace.SharedWorkspace.OpenFile(FS.ExportFolder);

            BeginInvokeOnMainThread(() =>
            {
                ShowCoins();
            });
            Task.Run(() => {
                Fix();
            });
           
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
