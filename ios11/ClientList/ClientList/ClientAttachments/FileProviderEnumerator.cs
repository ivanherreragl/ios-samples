using System;
using FileProvider;
using UIKit;
using Foundation;
namespace ClientList
{
    public class FileProviderEnumerator: NSObject, INSFileProviderEnumerator
    {

        public NSFileProviderItemIdentifier EnumeratedItemIdentifier;

        public FileProviderEnumerator()
        {
        }

		public FileProviderEnumerator(NSFileProviderItemIdentifier enumeratedItemIdentifier)
		{
            EnumeratedItemIdentifier = enumeratedItemIdentifier;

		}

        public void Invalidate(){
            
        }

        public void EnumerateItems(INSFileProviderEnumerationObserver observer, NSData page){
            if (page == NSData.FromString("initialPageSortedByName") || page == NSData.FromString("initialPageSortedByData"))
            { //Use Bound Constants instead
                NSArray properties = NSArray.FromNSObjects(NSUrl.NameKey);
                NSError err;
                var contents = NSFileManager.DefaultManager.GetDirectoryContent(Attachment.DirectoryURL(), properties, (NSDirectoryEnumerationOptions.SkipsSubdirectoryDescendants | NSDirectoryEnumerationOptions.SkipsHiddenFiles), out err);
                if (err == null){
					foreach (NSUrl url in contents)
					{
						//observer.DidEnumerateItems();
					}
                }

            }
            NSData op = null;
            observer.FinishEnumerating(op);
        }

        public void EnumerateChanges(INSFileProviderEnumerationObserver observer, NSData anchor){

            observer.FinishEnumerating(anchor);
            
        }

    }
}
