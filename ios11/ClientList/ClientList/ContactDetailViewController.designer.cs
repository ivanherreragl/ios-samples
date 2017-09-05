// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace ClientList
{
    [Register ("ContactDetailViewController")]
    partial class ContactDetailViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton AttachmentImageButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView ContactPhoto { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel NameLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel PhoneLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton ViewDataButton { get; set; }

        [Action ("ViewPurchaseData:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void ViewPurchaseData (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (AttachmentImageButton != null) {
                AttachmentImageButton.Dispose ();
                AttachmentImageButton = null;
            }

            if (ContactPhoto != null) {
                ContactPhoto.Dispose ();
                ContactPhoto = null;
            }

            if (NameLabel != null) {
                NameLabel.Dispose ();
                NameLabel = null;
            }

            if (PhoneLabel != null) {
                PhoneLabel.Dispose ();
                PhoneLabel = null;
            }

            if (ViewDataButton != null) {
                ViewDataButton.Dispose ();
                ViewDataButton = null;
            }
        }
    }
}