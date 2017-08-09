using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using CoreGraphics;

using UIKit;

using CoreFoundation;
using AVFoundation;
using Photos;
using Foundation;

namespace AVMetadataRecordPlay
{
	public class Rects
	{
		public IEnumerable<CGRect> Added { get; set; }
		public IEnumerable<CGRect> Removed { get; set; }
	}

    public partial class AssetGridViewController : UICollectionViewController, IPHPhotoLibraryChangeObserver
    {
        const string cellReuseIdentifier = "AssetGridViewCell";

        public AssetGridViewController() : base("AssetGridViewController", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.

            if (PHPhotoLibrary.AuthorizationStatus == PHAuthorizationStatus.Authorized){
                SetUpPhotoLibrary();
                UpdateTitle();
            }else{
                PHPhotoLibrary.RequestAuthorization((status)=>{
                    if (status == PHAuthorizationStatus.Authorized){
                        DispatchQueue.MainQueue.DispatchAsync(()=>{
                            this.SetUpPhotoLibrary();
                            this.UpdateTitle();
                            this.CollectionView.ReloadData();
                        });
                    }else{
						DispatchQueue.MainQueue.DispatchAsync(() => {
                            var message = "AVMetadataRecordPlay doesn't have permission to the photo library, please change privacy settings";

                            var alertController = UIAlertController.Create("AVMetadataRecordPlay", message, UIAlertControllerStyle.Alert);
                            alertController.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Cancel, null));
                            alertController.AddAction(UIAlertAction.Create("Settings", UIAlertActionStyle.Default, (action)=>{
                                UIApplication.SharedApplication.OpenUrl(NSUrl.FromString(UIApplication.OpenSettingsUrlString));
                            }));
                            this.PresentViewController(alertController, true, null);
						});
                    }
                });
            }
        }

		public override void ViewWillAppear(Boolean animated)
		{

			base.ViewWillAppear(animated);


            var screenScale = UIScreen.MainScreen.Scale;
            var spacing = new nfloat(2.0 / screenScale);
            var cellWidth = (Math.Min(View.Frame.Width, View.Frame.Height) - spacing * 3) / 4.0;

            var flowLayout = (UICollectionViewFlowLayout)this.Layout;
            flowLayout.ItemSize = new CGSize(cellWidth, cellWidth);
            flowLayout.SectionInset = new UIEdgeInsets(spacing, new nfloat(0.0), spacing, new nfloat(0.0));
            flowLayout.MinimumInteritemSpacing = spacing;
            flowLayout.MinimumLineSpacing = spacing;

            // Save the thumbnail size in pixels.
            AssetGridThumbnailSize = new CGSize(cellWidth * screenScale, cellWidth * screenScale);

		}

		public override void ViewDidAppear(Boolean animated)
		{

			base.ViewDidAppear(animated);

            UpdateCachedAssets();
			

		}

        bool IsScrolledToBottom = false;

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            if (!IsScrolledToBottom){
                var numberOfAssets = AssetsFetchResult.Count;
                if (numberOfAssets > 0){
                    var lastIndexPath = NSIndexPath.FromItemSection(numberOfAssets - 1, 0);
                    CollectionView.ScrollToItem(lastIndexPath, UICollectionViewScrollPosition.Bottom, false);
                    IsScrolledToBottom = true;
                }
            }
        }

		public override void ViewDidDisappear(Boolean animated)
		{

			base.ViewDidDisappear(animated);

			if (PHPhotoLibrary.AuthorizationStatus == PHAuthorizationStatus.Authorized)
			{
                PHPhotoLibrary.SharedPhotoLibrary.UnregisterChangeObserver(this);
			}

		}


        // MARK: Photo Library

        PHCachingImageManager ImageManager;

        private void SetUpPhotoLibrary(){
            ImageManager = new PHCachingImageManager();
            ResetCachedAssets();

            var videoSmartAlbumsFetchResult = PHAssetCollection.FetchAssetCollections(PHAssetCollectionType.SmartAlbum, PHAssetCollectionSubtype.SmartAlbumVideos, null);
            var videoSmartAlbum = (PHAssetCollection)videoSmartAlbumsFetchResult[0];
            AssetsFetchResult = PHAsset.FetchAssets(videoSmartAlbum, null);

            PHPhotoLibrary.SharedPhotoLibrary.RegisterChangeObserver(this);

        }

        public void PhotoLibraryDidChange(PHChange changeInstance){

            /*
	        Change notifications may be made on a background queue. Re-dispatch to the
	        main queue before acting on the change as we'll be updating the UI.
            */

            DispatchQueue.MainQueue.DispatchAsync(() =>
            {

                var collectionChanges = changeInstance.GetFetchResultChangeDetails(this.AssetsFetchResult);

                // Get the new fetch result.
                this.AssetsFetchResult = collectionChanges.FetchResultAfterChanges;

                this.UpdateTitle();

                if (!collectionChanges.HasIncrementalChanges || collectionChanges.HasMoves){
                    CollectionView.ReloadData();
                }else{
                    var collectionView = CollectionView;
                    collectionView.PerformBatchUpdates(()=>{

                        var removed = collectionChanges.RemovedIndexes;
						if (removed != null && removed.Count > 0)
							CollectionView.DeleteItems(ToNSIndexPaths(removed));

                        var inserted = collectionChanges.InsertedIndexes;
						if (inserted != null && inserted.Count > 0)
                            CollectionView.InsertItems(ToNSIndexPaths(inserted));

                        var changed = collectionChanges.ChangedIndexes;
						if (changed != null && changed.Count > 0)
                            CollectionView.ReloadItems(ToNSIndexPaths(changed));



                    }, null);
                }

                this.ResetCachedAssets();

			});

		}

		static NSIndexPath[] ToNSIndexPaths(NSIndexSet indexSet)
		{
			var cnt = indexSet.Count;
			var result = new NSIndexPath[(int)cnt];
			int i = 0;
			indexSet.EnumerateIndexes((nuint idx, ref bool stop) => {
				stop = false;
				result[i++] = NSIndexPath.FromItemSection((nint)idx, 0);
			});
			return result;
		}

		private void UpdateTitle()
		{
            Title = $"Videos ({AssetsFetchResult.Count})";

	    }

        // MARK: Asset Management


        CGSize AssetGridThumbnailSize = CGSize.Empty;

        PHFetchResult AssetsFetchResult;

        CGRect previousPreheatRect = CGRect.Empty;

        int AssetRequestID = 0; //PHInvalidImageRequestID FIND THE KEY

        UIAlertController LoadingAssetAlertController = null;

        public AVAsset SelectedAsset = null;

		private void ResetCachedAssets(){
            ImageManager.StopCaching();
            previousPreheatRect = CGRect.Empty;
        }

		void UpdateCachedAssets()
		{
			bool isViewVisible = IsViewLoaded && View.Window != null;
			if (!isViewVisible)
				return;

            if (PHPhotoLibrary.AuthorizationStatus != PHAuthorizationStatus.Authorized){
                return;
            }

			// The preheat window is twice the height of the visible rect.
			CGRect preheatRect = CollectionView.Bounds;
			preheatRect = preheatRect.Inset(0, -preheatRect.Height / 2);

			// Update only if the visible area is significantly different from the last preheated area.
			nfloat delta = NMath.Abs(preheatRect.GetMidY() - previousPreheatRect.GetMidY());
			if (delta <= CollectionView.Bounds.Height / 3)
				return;

			// Compute the assets to start caching and to stop caching.
			var rects = ComputeDifferenceBetweenRect(previousPreheatRect, preheatRect);
			var addedAssets = rects.Added
								   .SelectMany(rect => CollectionView.GetIndexPaths(rect))
                                   .Select(indexPath => AssetsFetchResult.ObjectAt(indexPath.Item))
								   .Cast<PHAsset>()
								   .ToArray();

			var removedAssets = rects.Removed
									 .SelectMany(rect => CollectionView.GetIndexPaths(rect))
									 .Select(indexPath => AssetsFetchResult.ObjectAt(indexPath.Item))
									 .Cast<PHAsset>()
									 .ToArray();

			// Update the assets the PHCachingImageManager is caching.
            ImageManager.StartCaching(addedAssets, AssetGridThumbnailSize, PHImageContentMode.AspectFill, null);
			ImageManager.StopCaching(removedAssets, AssetGridThumbnailSize, PHImageContentMode.AspectFill, null);

			// Store the preheat rect to compare against in the future.
			previousPreheatRect = preheatRect;
		}

		static Rects ComputeDifferenceBetweenRect(CGRect oldRect, CGRect newRect)
		{
			if (!oldRect.IntersectsWith(newRect))
			{
				return new Rects
				{
					Added = new CGRect[] { newRect },
					Removed = new CGRect[] { oldRect }
				};
			}

			var oldMaxY = oldRect.GetMaxY();
			var oldMinY = oldRect.GetMinY();
			var newMaxY = newRect.GetMaxY();
			var newMinY = newRect.GetMinY();

			var added = new List<CGRect>();
			var removed = new List<CGRect>();

			if (newMaxY > oldMaxY)
				added.Add(new CGRect(newRect.X, oldMaxY, newRect.Width, newMaxY - oldMaxY));

			if (oldMinY > newMinY)
				added.Add(new CGRect(newRect.X, newMinY, newRect.Width, oldMinY - newMinY));

			if (newMaxY < oldMaxY)
				removed.Add(new CGRect(newRect.X, newMaxY, newRect.Width, oldMaxY - newMaxY));

			if (oldMinY < newMinY)
				removed.Add(new CGRect(newRect.X, oldMinY, newRect.Width, newMinY - oldMinY));

			return new Rects
			{
				Added = added,
				Removed = removed
			};
		}

		// MARK: Collection View

		public override nint GetItemsCount(UICollectionView collectionView, nint section)
		{
            return AssetsFetchResult.Count;
		}

		public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
		{
			var asset = (PHAsset)AssetsFetchResult[indexPath.Item];

			// Dequeue an GridViewCell.
            var cell = (AssetGridViewCell)collectionView.DequeueReusableCell(cellReuseIdentifier, indexPath);


			// Request an image for the asset from the PHCachingImageManager.
			cell.RepresentedAssetIdentifier = asset.LocalIdentifier;
            ImageManager.RequestImageForAsset(asset, AssetGridThumbnailSize, PHImageContentMode.AspectFill, null, (image, info) => {
				// Set the cell's thumbnail image if it's still showing the same asset.
				if (cell.RepresentedAssetIdentifier == asset.LocalIdentifier)
					cell.ThumbnailImage = image;
			});

			return cell;
		}

		public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
		{
			var asset = (PHAsset)AssetsFetchResult[indexPath.Item];

            var requestOptions = new PHVideoRequestOptions();
            requestOptions.NetworkAccessAllowed = true;

            requestOptions.ProgressHandler = (double progress, NSError error, out bool stop, NSDictionary info) => {

                stop = false;

                if (error != null){
                    Console.WriteLine("Error loading video"); //DO SOMETHING BETTER HERE
                    return;
                }

                var requestID = (NSNumber)info[PHImageKeys.ResultRequestID];

                DispatchQueue.MainQueue.DispatchAsync(() =>
                {

                    if (this.AssetRequestID == (int)requestID){

                        if (this.LoadingAssetAlertController != null){

                            LoadingAssetAlertController.Message = $"Progress: {progress * 100}%";


                        }else{

                            this.LoadingAssetAlertController = UIAlertController.Create("Loading Video", "Progress: 0%", UIAlertControllerStyle.Alert);
                            LoadingAssetAlertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, (action) => {
                                this.ImageManager.CancelImageRequest((int)requestID);
                                this.AssetRequestID = 0; //INVALID REQUEST ID
                                this.LoadingAssetAlertController = null;
							}));

                            this.PresentViewController(LoadingAssetAlertController, true, null);

                        }

                    }

                });

            };

            this.AssetRequestID = ImageManager.RequestAvAsset(asset, requestOptions, (vasset, audioMix, info) => {

                DispatchQueue.MainQueue.DispatchAsync(() =>
                {

                    if (vasset != null){
                        this.SelectedAsset = vasset;
                        this.PerformSegue("backToPlayer", this);
                    }

                });
            
            });

		}

        public override void Scrolled(UIScrollView scrollView){
            UpdateCachedAssets();

        }

		public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}

