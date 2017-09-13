using System;
using MobileCoreServices;
using Foundation;
using CoreFoundation;
using UIKit;

namespace ClientList
{
    public partial class ContactDetailViewController : UIViewController, IUIDragInteractionDelegate
    {
        public ContactCard TheContactCard;


        public ContactDetailViewController() : base("ContactDetailViewController", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.


            NameLabel.Text = TheContactCard.Name;
            PhoneLabel.Text = TheContactCard.PhoneNumber;
            ContactPhoto.Image = TheContactCard.Photo;

            Title = TheContactCard.Name;

			AttachmentImageButton.Hidden = true;
			ViewDataButton.Hidden = true;

            if (TheContactCard.AttachmentURL != null){
                var dragInteraction = new UIDragInteraction(this);
                AttachmentImageButton.AddInteraction(dragInteraction);
                AttachmentImageButton.Hidden = false;
                ViewDataButton.Hidden = false;
            }

        }

		partial void ViewPurchaseData(UIButton sender)
		{
            var purchaseInfoViewController = new PurchaseDetailViewController();
            var url = Attachment.Url(TheContactCard.Name);
            purchaseInfoViewController.Url = url;
            NavigationController.PushViewController(purchaseInfoViewController, true);
		}

		public UIDragItem[] GetItemsForBeginningSession(UIDragInteraction interaction, IUIDragSession session)
		{
            var itemProvider = new NSItemProvider();
            itemProvider.RegisterFileRepresentation(UTType.JPEG, NSItemProviderFileOptions.OpenInPlace, NSItemProviderRepresentationVisibility.All,(completionHandler) => {
                var url = Attachment.Url(TheContactCard.Name);
                completionHandler(url, true, null);
                return null;
            });
            var dragItem = new UIDragItem(itemProvider);
            return new UIDragItem[]{dragItem};

		}


        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }


    }
}

