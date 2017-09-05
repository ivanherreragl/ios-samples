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

        public Attachment()
        {
        }

        public static NSUrl Url(string name){
            return NSUrl.FromString("http://www.xamarin.com");
        }
    }
}
