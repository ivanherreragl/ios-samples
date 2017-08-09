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

namespace AVMetadataRecordPlay.Player
{
    [Register ("PlayerViewController")]
    partial class PlayerViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UISwitch HonorTimedMetadataTracksSwitch { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel LocationOverlayLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIBarButtonItem PauseButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIBarButtonItem PlayButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView PlayerView { get; set; }

        [Action ("PauseButtonTapped:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void PauseButtonTapped (UIKit.UIBarButtonItem sender);

        [Action ("PlayButtonTapped:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void PlayButtonTapped (UIKit.UIBarButtonItem sender);

        [Action ("ToggleHonorTimedMetadataTracksDuringPlayback:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void ToggleHonorTimedMetadataTracksDuringPlayback (UIKit.UISwitch sender);

        [Action ("UIBarButtonItem520_Activated:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void UIBarButtonItem520_Activated (UIKit.UIBarButtonItem sender);

        void ReleaseDesignerOutlets ()
        {
            if (HonorTimedMetadataTracksSwitch != null) {
                HonorTimedMetadataTracksSwitch.Dispose ();
                HonorTimedMetadataTracksSwitch = null;
            }

            if (LocationOverlayLabel != null) {
                LocationOverlayLabel.Dispose ();
                LocationOverlayLabel = null;
            }

            if (PauseButton != null) {
                PauseButton.Dispose ();
                PauseButton = null;
            }

            if (PlayButton != null) {
                PlayButton.Dispose ();
                PlayButton = null;
            }

            if (PlayerView != null) {
                PlayerView.Dispose ();
                PlayerView = null;
            }
        }
    }
}