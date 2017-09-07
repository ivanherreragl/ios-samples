using System;
using FileProvider;
using UIKit;
using Foundation;
using MobileCoreServices;

namespace ClientAttachments
{
    public class FileProviderItem: NSObject, INSFileProviderItem
    {
        

		private string _identifier = string.Empty;

        public NSString Identifier
		{
			get
			{
                return (NSString)_identifier;
			}
			set
			{
				_identifier = value;
			}
		}


		public string TypeIdentifier
		{
			get
			{
				return (string)UTType.JPEG;
			}

		}

        public NSFileProviderItemIdentifier ParentItemIdentifier(){
            return NSFileProviderItemIdentifier.RootContainer;
        }

        public NSString ParentIdentifier
		{
			get
			{
                return (NSString)ParentItemIdentifier().ToString();
			}

		}

        public string Filename
		{
			get
			{
                return _identifier + " Attachment.jpg";
			}

		}


        public NSFileProviderItemCapabilities Capabilities = NSFileProviderItemCapabilities.All;


        public FileProviderItem()
        {
        }

		public FileProviderItem(string name)
		{
            _identifier = name;
		}
    }
}
