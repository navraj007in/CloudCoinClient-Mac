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
		AppKit.NSTextView txtLogs { get; set; }

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

		[Action ("ShowFolderClicked:")]
		partial void ShowFolderClicked (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (Export2DBarCodeClicked != null) {
				Export2DBarCodeClicked.Dispose ();
				Export2DBarCodeClicked = null;
			}

			if (txtLogs != null) {
				txtLogs.Dispose ();
				txtLogs = null;
			}
		}
	}
}
