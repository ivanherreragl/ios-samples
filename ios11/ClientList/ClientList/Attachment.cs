using System;
using UIKit;
using Foundation;
namespace ClientList
{
    public class Attachment: NSObject
    {

        public static string ApplicationGroup = "group.com.example.xamarin-samplecode.ClientList";

        public static string AttachmentName = "Attachment.jpg";

        public static string PurposeIdentifier = "com.example.xamarin-samplecode.ClientList";

        public static NSUrl DirectoryURL(){
            return NSFileManager.DefaultManager.GetContainerUrl(ApplicationGroup).Append("File Provider Storage", true);
        }

        public Attachment()
        {
        }

        public static NSUrl Url(string name){
            return DirectoryURL().Append(name, true).Append(AttachmentName, true);
        }

        public static void Load(NSUrl url){
            var name = url.RemoveLastPathComponent().LastPathComponent;
            var resourceName = name + " Data";
            var bundleURL = NSBundle.MainBundle.GetUrlForResource(resourceName, "jpg");



            NSError err;
            NSFileAttributes fileAttributes = new NSFileAttributes();
            fileAttributes.PosixPermissions = Convert.ToInt16("755", 8);
            NSFileManager.DefaultManager.CreateDirectory(url.RemoveLastPathComponent().Path, true, fileAttributes, out err );

            NSError err2;
            NSFileManager.DefaultManager.Copy(bundleURL, url, out err2);

			NSError err3;
			NSFileAttributes fileAttributes2 = new NSFileAttributes();
			fileAttributes2.PosixPermissions = Convert.ToInt16("644", 8);
            NSFileManager.DefaultManager.SetAttributes(fileAttributes, url.Path, out err3);

        }

        public static string Identifier(NSUrl url){
            return url.RemoveLastPathComponent().LastPathComponent;
        }


    }
}
