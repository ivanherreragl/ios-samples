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
            //if (page == )
        }

        public void EnumerateChanges(INSFileProviderEnumerationObserver observer, NSData anchor){

            observer.FinishEnumerating(anchor);
            
        }

    }
}
