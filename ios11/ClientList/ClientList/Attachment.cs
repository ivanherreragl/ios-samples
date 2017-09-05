using System;
using UIKit;
using Foundation;
namespace ClientList
{
    public class Attachment
    {
        public Attachment()
        {
        }

        public static NSUrl Url(string name){
            return NSUrl.FromString("http://www.microsoft.com");
        }
    }
}
