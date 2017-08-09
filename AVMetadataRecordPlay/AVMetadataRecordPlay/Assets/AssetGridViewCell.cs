using System;

using Foundation;
using UIKit;

namespace AVMetadataRecordPlay.Assets
{
    public partial class AssetGridViewCell : UICollectionViewCell
    {
        public static readonly NSString Key = new NSString("AssetGridViewCell");
        public static readonly UINib Nib;

        static AssetGridViewCell()
        {
            Nib = UINib.FromName("AssetGridViewCell", NSBundle.MainBundle);
        }

        protected AssetGridViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }
    }
}
