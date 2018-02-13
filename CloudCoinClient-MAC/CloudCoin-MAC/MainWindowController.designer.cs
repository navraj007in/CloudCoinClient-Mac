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
	[Register ("MainWindowController")]
	partial class MainWindowController
	{
		[Outlet]
		AppKit.NSTextField txtProgress { get; set; }

		[Action ("cmdEcho:")]
		partial void cmdEcho (Foundation.NSObject sender);

		[Action ("cmdImportClicked:")]
		partial void cmdImportClicked (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (txtProgress != null) {
				txtProgress.Dispose ();
				txtProgress = null;
			}
		}
	}
}
