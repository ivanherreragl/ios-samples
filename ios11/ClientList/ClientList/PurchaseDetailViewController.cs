using System;
using Foundation;
using UIKit;

namespace ClientList
{
    public partial class PurchaseDetailViewController : UIViewController
    {

        public NSUrl Url;

        public PurchaseDetailViewController() : base("PurchaseDetailViewController", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.
            View.BackgroundColor = UIColor.White;
            var imageview = new UIImageView();
            imageview.ContentMode = UIViewContentMode.ScaleAspectFit;
            View.AddSubview(imageview);
            imageview.TranslatesAutoresizingMaskIntoConstraints = false;
            imageview.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor, 20).Active = true;
            imageview.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor, -20).Active = true;
            imageview.TopAnchor.ConstraintEqualTo(View.TopAnchor, 100).Active = true;
            imageview.BottomAnchor.ConstraintEqualTo(View.BottomAnchor, -10).Active = true;

            var fileCoordinator = new NSFileCoordinator();
            NSError error;
            fileCoordinator.PurposeIdentifier = Attachment.PurposeIdentifier;
            fileCoordinator.CoordinateRead(Url, NSFileCoordinatorReadingOptions.WithoutChanges, out error, (NSUrl url) => {
                NSError err;
                var imageData = NSData.FromUrl(url, NSDataReadingOptions.Uncached, out err);
                if (err == null){
                    imageview.Image = new UIImage(imageData);
                }else{
                    imageview.Image = null;
                }
            });

        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}

