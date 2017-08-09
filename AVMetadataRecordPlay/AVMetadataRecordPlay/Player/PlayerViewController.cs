using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using CoreMedia;
using UIKit;
using AVFoundation;
using CoreFoundation;
using Foundation;
using CoreLocation;
using CoreGraphics;
using Photos;
using CoreVideo;
using CoreAnimation;
using ImageIO;

namespace AVMetadataRecordPlay.Player
{
    public partial class PlayerViewController : UIViewController, IAVPlayerItemMetadataOutputPushDelegate
    {
        public PlayerViewController() : base("PlayerViewController", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.
            PlayButton.Enabled = false;
            PauseButton.Enabled = false;

            PlayerView.Layer.BackgroundColor = UIColor.DarkGray.CGColor;
            var metadataQueue = new DispatchQueue("com.example.metadataqueue");
            ItemMetadataOutput.SetDelegate(this, metadataQueue);

        }

		public override void ViewDidDisappear(Boolean animated)
		{

			base.ViewDidDisappear(animated);

            Player.Pause();
            if (PlayerAsset != null){
                PlayButton.Enabled = false;
                PauseButton.Enabled = false;
                SeekToZeroBeforePlay = false;
                Player.Seek(CMTime.Zero);
            }
		}

		public override void ViewWillTransitionToSize(CoreGraphics.CGSize toSize, IUIViewControllerTransitionCoordinator coordinator)
		{
			base.ViewWillTransitionToSize(toSize, coordinator);
            coordinator.AnimateAlongsideTransition((UIViewControllerTransitionCoordinatorContext)=> { this.PlayerLayer.Frame = this.PlayerView.Layer.Bounds; }, 
                                                   (UIViewControllerTransitionCoordinatorContext)=>{});

		}

        // MARK: Segue

        /*


    @IBAction func unwindBackToPlayer(segue: UIStoryboardSegue) {
        // Pull any data from the view controller which initiated the unwind segue.
        let assetGridViewController = segue.source as! AssetGridViewController
        if let selectedAsset = assetGridViewController.selectedAsset {
            if selectedAsset != playerAsset {
                setUpPlayback(for: selectedAsset)
                playerAsset = selectedAsset
            }
        }
    }
          
         
         */


        [Action("unwindBackToPlayer:")]
        public void UnwindBackToPlayer(UIStoryboardSegue segue)
        {
            var assetGridViewController = (AssetGridViewController)segue.SourceViewController;
            if (assetGridViewController.SelectedAsset != null){
                var selectedAsset = assetGridViewController.SelectedAsset;
                if (selectedAsset != PlayerAsset){
                    SetupPlayback(selectedAsset);
                    PlayerAsset = selectedAsset;
                }
            }
        }

		// MARK: Player

		AVPlayer Player;

        bool SeekToZeroBeforePlay = false;

        AVAsset PlayerAsset;

        AVPlayerLayer PlayerLayer;

        CGAffineTransform DefaultVideoTransform = CGAffineTransform.MakeIdentity();

        public void SetupPlayback(AVAsset asset){
            DispatchQueue.MainQueue.DispatchAsync(()=>{

                if (this.Player != null){
                    var currentItem = this.Player.CurrentItem;
                    currentItem.RemoveOutput(this.ItemMetadataOutput);
                }
                this.SetupPlayer(asset);
                this.PlayButton.Enabled = true;
                this.PauseButton.Enabled = false;
                this.RemoveAllSublayers(this.FacesLayer);

            });
        }

		public void SetupPlayer(AVAsset asset)
		{
            var mutableComposition = new AVMutableComposition();

            // Create a mutableComposition for all the tracks present in the asset.

            var sourceVideoTrack = asset.TracksWithMediaType(AVMediaType.Video).First();

            DefaultVideoTransform = sourceVideoTrack.PreferredTransform;

            var sourceAudioTrack = asset.TracksWithMediaType(AVMediaType.Audio).First();

            int kCMPersistentTrackID_Invalid = 0;

            var mutableCompositionVideoTrack = mutableComposition.AddMutableTrack(AVMediaType.Video, kCMPersistentTrackID_Invalid);

            var mutableCompositionAudioTrack = mutableComposition.AddMutableTrack(AVMediaType.Audio, kCMPersistentTrackID_Invalid);

            NSError err;

            var timeR = new CMTimeRange();
            timeR.Start = CMTime.Zero;
            timeR.Duration = asset.Duration;

            mutableCompositionVideoTrack.InsertTimeRange(timeR, sourceVideoTrack, CMTime.Zero, out err);
            if (err == null){
                NSError err2;
                mutableCompositionAudioTrack.InsertTimeRange(timeR, sourceAudioTrack, CMTime.Zero, out err2);
				if (err2 != null)
				{

					Console.WriteLine($"Could not insert time range into audio mutable composition: {err.LocalizedDescription}");
				}
            }else{
                Console.WriteLine($"Could not insert time range into video mutable composition: {err.LocalizedDescription}");
            }

            foreach (var metadataTrack in asset.TracksWithMediaType(AVMediaType.Metadata))
			{
                
                if (TrackHasMetadataIdentifier(metadataTrack, CMMetadataIdentifier.QuickTimeMetadataLocation_ISO6709) ||
                    TrackHasMetadataIdentifier(metadataTrack, CMMetadataIdentifier.QuickTimeMetadataVideoOrientation) ||
                    TrackHasMetadataIdentifier(metadataTrack, AVMetadataIdentifiers.QuickTimeMetadata.DetectedFace)
                   ){
                    var mutableCompositionMetadataTrack = mutableComposition.AddMutableTrack(AVMediaType.Metadata, kCMPersistentTrackID_Invalid);
                    NSError err3;
                    mutableCompositionMetadataTrack.InsertTimeRange(timeR, metadataTrack, CMTime.Zero, out err3);
					if (err3 != null)
					{

						Console.WriteLine($"Could not insert time range into metadata mutable composition: {err.LocalizedDescription}");
					}
                }
			}
            // Get an instance of AVPlayerItem for the generated mutableComposition.
            // let playerItem = AVPlayerItem(asset: asset) // This doesn't support video orientation hence we use a mutable composition.
            var playerItem = AVPlayerItem.FromAsset(mutableComposition);
            playerItem.AddOutput(ItemMetadataOutput);

            if (Player != null){
                Player.ReplaceCurrentItemWithPlayerItem(playerItem);
            }else{
                // Create AVPlayer with the generated instance of playerItem. Also add the facesLayer as subLayer to this playLayer.
                Player = AVPlayer.FromPlayerItem(playerItem);
                Player.ActionAtItemEnd = AVPlayerActionAtItemEnd.None;

                var playerLayer = AVPlayerLayer.FromPlayer(Player);
                playerLayer.BackgroundColor = UIColor.DarkGray.CGColor;
                playerLayer.AddSublayer(FacesLayer);
                FacesLayer.Frame = playerLayer.VisibleRect;

                this.PlayerLayer = playerLayer;

            }

            // Update the player layer to match the video's default transform. Disable animation so the transform applies immediately.

            CATransaction.Begin();
            CATransaction.DisableActions = true;
            PlayerLayer.Transform = CATransform3D.MakeFromAffine(DefaultVideoTransform);
            PlayerLayer.Frame = PlayerView.Layer.Bounds;
            CATransaction.Commit();

			// When the player item has played to its end time we'll toggle the movie controller Pause button to be the Play button.

            NSNotificationCenter.DefaultCenter.AddObserver(AVPlayerItem.DidPlayToEndTimeNotification, PlayerItemDidReachEnd, Player.CurrentItem);

            SeekToZeroBeforePlay = false;


		}

		private void PlayerItemDidReachEnd(NSNotification notification)
		{
            // After the movie has played to its end time, seek back to time zero to play it again.
            SeekToZeroBeforePlay = true;
            PlayButton.Enabled = true;
            PauseButton.Enabled = false;
            RemoveAllSublayers(FacesLayer);
        }


		partial void PlayButtonTapped(UIBarButtonItem sender)
		{
            if (SeekToZeroBeforePlay){
                SeekToZeroBeforePlay = false;
                Player.Seek(CMTime.Zero);

                // Update the player layer to match the video's default transform.
                PlayerLayer.Transform = CATransform3D.MakeFromAffine(DefaultVideoTransform);
                PlayerLayer.Frame = PlayerView.Layer.Bounds;
			}
            Player.Play();
            PlayButton.Enabled = false;
            PauseButton.Enabled = true;
		}

		partial void PauseButtonTapped(UIBarButtonItem sender)
		{
            Player.Pause();
			PlayButton.Enabled = true;
            PauseButton.Enabled = false;
		}

        // MARK: Timed Metadata

        AVPlayerItemMetadataOutput ItemMetadataOutput = new AVPlayerItemMetadataOutput(null);

        bool HonorTimedMetadataTracksDuringPlayback = false;

        CALayer FacesLayer = new CALayer();

        public void DrawFaceMetadataRects(AVMetadataObject[] faces){
            var playerLayer = PlayerLayer;
            DispatchQueue.MainQueue.DispatchAsync(()=>{

                var viewRect = playerLayer.VideoRect;
                this.FacesLayer.Frame = viewRect;
                this.FacesLayer.MasksToBounds = true;
                this.RemoveAllSublayers(this.FacesLayer);

                foreach (var face in faces)
                {
                    var faceBox = new CALayer();
                    var faceRect = face.Bounds;
                    var viewFaceOrigin = new CGPoint(faceRect.X * viewRect.Width, faceRect.Y * viewRect.Height);
                    var viewFaceSize = new CGSize(faceRect.Width * viewRect.Width, faceRect.Height * viewRect.Height);
                    var viewFaceBounds = new CGRect(viewFaceOrigin.X, viewFaceOrigin.Y, viewFaceSize.Width, viewFaceSize.Height);

                    CATransaction.Begin();
                    CATransaction.DisableActions = true;
                    this.FacesLayer.AddSublayer(faceBox);
                    faceBox.MasksToBounds = true;
                    faceBox.BorderWidth = new nfloat(2.0);
                    faceBox.BorderColor = UIColor.Green.CGColor;
                    faceBox.CornerRadius = new nfloat(5.0);
                    faceBox.Frame = viewFaceBounds;
                    CATransaction.Commit();

                    UpdateAnimation(this.FacesLayer, true);


                }

            });
        }


		partial void ToggleHonorTimedMetadataTracksDuringPlayback(UISwitch sender)
		{
            if (HonorTimedMetadataTracksSwitch.On){
                HonorTimedMetadataTracksDuringPlayback = true;
            }else{
                HonorTimedMetadataTracksDuringPlayback = false;
                RemoveAllSublayers(FacesLayer);
                LocationOverlayLabel.Text = "";
            }
		}

        public virtual void DidOutputTimedMetadataGroups(AVPlayerItemMetadataOutput output, AVTimedMetadataGroup[] groups, AVPlayerItemTrack track){
            foreach (var metadataGroup in groups)
            {
                DispatchQueue.MainQueue.DispatchAsync(()=>{

                    if (metadataGroup.Items.Count() == 0){
                        if (this.TrackHasMetadataIdentifier(track.AssetTrack, AVMetadataIdentifiers.QuickTimeMetadata.DetectedFace)){
                            this.RemoveAllSublayers(this.FacesLayer);
						}
						else if (this.TrackHasMetadataIdentifier(track.AssetTrack, AVMetadataIdentifiers.QuickTimeMetadata.VideoOrientation))
						{
                            this.LocationOverlayLabel.Text = "";
						}
                    }else{
                        if (this.HonorTimedMetadataTracksDuringPlayback){

                            var faces = new AVMetadataObject[]{};

                            foreach (var metdataItem in metadataGroup.Items)
                            {
                                var itemIdentifier = metdataItem.MetadataIdentifier;
                                var itemDataType = metdataItem.DataType;

                                if ((string)itemIdentifier == (string)AVMetadataIdentifiers.QuickTimeMetadata.DetectedFace)
                                {
                                    var itemValue = (AVMetadataObject)metdataItem.Value;
                                    faces.Append(itemValue);

                                }
                                else if ((string)itemIdentifier == (string)AVMetadataIdentifiers.QuickTimeMetadata.VideoOrientation)
                                {
                                    if ((string)itemDataType == (string)CMMetadataBaseDataType.SInt16){
                                        var videoOrientationValue = (NSNumber)metdataItem.Value;
                                        var sourceVideoTrack = this.PlayerAsset.TracksWithMediaType(AVMediaType.Video)[0];

                                        var videoDimensions = ((CMVideoFormatDescription)sourceVideoTrack.FormatDescriptions[0]).GetPresentationDimensions(true, false);

                                        var videoOrientation = (CGImagePropertyOrientation)videoOrientationValue.UInt32Value;
                                        var orientationTransform = this.AffineTransform(videoOrientation, videoDimensions);
                                        var rotationTransform = CATransform3D.MakeFromAffine(orientationTransform);

                                        // Remove faceBoxes before applying transform and then re-draw them as we get new face coordinates.

                                        this.RemoveAllSublayers(this.FacesLayer);
                                        this.PlayerLayer.Transform = rotationTransform;
                                        this.PlayerLayer.Frame = this.PlayerView.Layer.Bounds;

									}
								}
								else if ((string)itemIdentifier == (string)CMMetadataIdentifier.QuickTimeMetadataLocation_ISO6709)
								{
									if ((string)itemDataType == (string)CMMetadataDataType.QuickTimeMetadataLocation_ISO6709)
									{
                                        var itemValue = (NSString)metdataItem.Value;
                                        this.LocationOverlayLabel.Text = (string)itemValue;

									}
                                }else{
                                    Console.WriteLine($"Timed metadata: unrecognized metadata identifier: {itemIdentifier}");
                                }



                            }

                            if (faces.Count() > 0){
                                this.DrawFaceMetadataRects(faces);
                            }
                        }
                    }

                });
            }
        }

        // MARK: Animation Utilities

        private void RemoveAllSublayers(CALayer layer)
        {
            CATransaction.Begin();
            CATransaction.DisableActions = true;
            var sublayers = layer.Sublayers;
            if (sublayers != null){
                foreach (var dlayer in sublayers)
                {
                    dlayer.RemoveFromSuperLayer();
                }
            }
        }

        private bool TrackHasMetadataIdentifier (AVAssetTrack track, NSString metadataIdentifier){

            var formatDescription = (CoreMedia.CMMetadataFormatDescription)track.FormatDescriptions[0];
            var metadataIdentifiers = formatDescription.GetIdentifiers();
            if (metadataIdentifiers.Contains(metadataIdentifier)){
                return true;
            }

            return false;
        }

		// MARK: Animation Utilities

		public CGAffineTransform AffineTransform(CGImagePropertyOrientation videoOrientation, CGSize videoDimensions){
            var transform = CGAffineTransform.MakeIdentity();

			// Determine rotation and mirroring from tag value.

			var rotationDegrees = 0;
            var mirror = false;

            switch (videoOrientation){
                case CGImagePropertyOrientation.Up:
                    rotationDegrees = 0; mirror = false;
                    break;
				case CGImagePropertyOrientation.UpMirrored:
					rotationDegrees = 0; mirror = true;
					break;
				case CGImagePropertyOrientation.Down:
					rotationDegrees = 180; mirror = false;
					break;
                case CGImagePropertyOrientation.DownMirrored:
					rotationDegrees = 180; mirror = true;
					break;
                case CGImagePropertyOrientation.Left:
					rotationDegrees = 270; mirror = false;
					break;
                case CGImagePropertyOrientation.LeftMirrored:
					rotationDegrees = 90; mirror = true;
					break;
                case CGImagePropertyOrientation.Right:
					rotationDegrees = 90; mirror = false;
					break;
                case CGImagePropertyOrientation.RightMirrored:
                    rotationDegrees = 270; mirror = true;
					break;

            }

            // Build the affine transform.

            nfloat angle = new nfloat(0.0);
            nfloat tx = new nfloat(0.0);
            nfloat ty = new nfloat(0.0);

            switch (rotationDegrees){
                case 90:
                    angle = new nfloat(Math.PI / 2.0);
                    tx = new nfloat(videoDimensions.Height);
                    ty = new nfloat(0.0);
                    break;
				case 180:
					angle = new nfloat(Math.PI);
                    tx = new nfloat(videoDimensions.Width);
                    ty = new nfloat(videoDimensions.Height);
					break;
				case 270:
					angle = new nfloat(Math.PI / -2.0);
					tx = new nfloat(0.0);
                    ty = new nfloat(videoDimensions.Width);
					break;
                default:
                    break;
            }

            // Rotate first, then translate to bring 0,0 to top left.

            if (angle == new nfloat(0.0)){
                transform = CGAffineTransform.MakeIdentity();
            }else{
                transform = CGAffineTransform.MakeRotation(angle);
                transform = transform * CGAffineTransform.MakeTranslation(tx, ty);

            }

            // If mirroring, flip along the proper axis.

            if (mirror){
                transform = transform * CGAffineTransform.MakeScale(new nfloat(-1.0), new nfloat(1.0));
                transform = transform * CGAffineTransform.MakeTranslation(new nfloat(videoDimensions.Height), new nfloat(0.0));

            }

            return transform;

		}

	

        public void UpdateAnimation(CALayer layer, bool remove){
            if (remove){
                layer.RemoveAnimation("animateOpacity");
            }
            if (layer.AnimationForKey("animateOpacity") == null){
                layer.Hidden = true;
                var opacityAnimation = CABasicAnimation.FromKeyPath("opacity");
                opacityAnimation.Duration = 0.3;
                opacityAnimation.RepeatCount = 1;
                opacityAnimation.AutoReverses = true;
                opacityAnimation.From = (NSNumber)1.0;
                opacityAnimation.To = (NSNumber)0.0;
                layer.AddAnimation(opacityAnimation, "animateOpacity");

            }
        }

		public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }


    }
}

