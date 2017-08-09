using System;

using Foundation;
using UIKit;

namespace AVMetadataRecordPlay
{
    public partial class AssetGridViewCell : UICollectionViewCell
    {
        public static readonly NSString Key = new NSString("AssetGridViewCell");
		UIImage thumbnailImage;
		public UIImage ThumbnailImage
		{
			get
			{
				return thumbnailImage;
			}
			set
			{
				thumbnailImage = value;
				imageView.Image = thumbnailImage;
			}
		}



		public string RepresentedAssetIdentifier { get; set; }

		[Export("initWithCoder:")]
		public AssetGridViewCell(NSCoder coder)
            : base (coder)
        {
		}

		public AssetGridViewCell(IntPtr handle)
            : base (handle)
        {
		}

		public override void PrepareForReuse()
		{
			base.PrepareForReuse();
			imageView.Image = null;
			
		}
    }
}
