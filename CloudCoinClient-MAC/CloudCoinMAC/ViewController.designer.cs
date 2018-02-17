// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace CloudCoinMAC
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		AppKit.NSButton Export2DBarCodeClicked { get; set; }

		[Outlet]
		AppKit.NSTextField lblBankTotal { get; set; }

		[Outlet]
		AppKit.NSTableView ProductTable { get; set; }

		[Outlet]
		AppKit.NSLevelIndicatorCell raidaLevel { get; set; }

		[Outlet]
		AppKit.NSButton rdbJpeg { get; set; }

		[Outlet]
		AppKit.NSButton rdbStack { get; set; }

		[Outlet]
		AppKit.NSTextView txtLogs { get; set; }

		[Outlet]
		AppKit.NSTextFieldCell txtTag { get; set; }

		[Action ("BackupClicked:")]
		partial void BackupClicked (Foundation.NSObject sender);

		[Action ("EchoClick:")]
		partial void EchoClick (Foundation.NSObject sender);

		[Action ("ExportClicked:")]
		partial void ExportClicked (Foundation.NSObject sender);

		[Action ("ExportQRClicked:")]
		partial void ExportQRClicked (Foundation.NSObject sender);

		[Action ("ImportClicked:")]
		partial void ImportClicked (Foundation.NSObject sender);

		[Action ("jPegClicked:")]
		partial void jPegClicked (Foundation.NSObject sender);

		[Action ("ListSerialsClicked:")]
		partial void ListSerialsClicked (Foundation.NSObject sender);

		[Action ("ShowFolderClicked:")]
		partial void ShowFolderClicked (Foundation.NSObject sender);

		[Action ("stackClicked:")]
		partial void stackClicked (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (lblBankTotal != null) {
				lblBankTotal.Dispose ();
				lblBankTotal = null;
			}

			if (Export2DBarCodeClicked != null) {
				Export2DBarCodeClicked.Dispose ();
				Export2DBarCodeClicked = null;
			}

			if (raidaLevel != null) {
				raidaLevel.Dispose ();
				raidaLevel = null;
			}

			if (ProductTable != null) {
				ProductTable.Dispose ();
				ProductTable = null;
			}

			if (rdbJpeg != null) {
				rdbJpeg.Dispose ();
				rdbJpeg = null;
			}

			if (rdbStack != null) {
				rdbStack.Dispose ();
				rdbStack = null;
			}

			if (txtLogs != null) {
				txtLogs.Dispose ();
				txtLogs = null;
			}

			if (txtTag != null) {
				txtTag.Dispose ();
				txtTag = null;
			}
		}
	}
}
