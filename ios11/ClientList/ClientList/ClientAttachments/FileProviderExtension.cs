using System;
using FileProvider;
using Foundation;
using UIKit;

namespace ClientList
{
    public class FileProviderExtension: NSFileProviderExtension
    {

        NSFileManager FileManager = new NSFileManager();

        public FileProviderExtension()
        {
        }

        public INSFileProviderItem Item(NSFileProviderItemIdentifier identifier){
            return new FileProviderItem(identifier.ToString());
        }

        public NSUrl UrlForItem(NSFileProviderItemIdentifier identifier){
            return Attachment.Url(identifier.ToString());
        }

        public NSString PersistentIdentifierForItem(NSUrl url){
            return (NSString)Attachment.Identifier(url);

        }

        public void TrashItem(NSString itemIdentifier, Action<INSFileProviderItem, NSError> completionHandler){
            NSError err;
            NSFileManager.DefaultManager.Remove(Attachment.Url(itemIdentifier).RemoveLastPathComponent(), out err);
            if (err == null){
                completionHandler(null, null);
            }else{
                completionHandler(null, err);
            }
        }

        public void DeleteItem(NSString itemIdentifier, Action<NSError> completionHandler){
            completionHandler(null);
        }

        public INSFileProviderEnumerator Enumerator (NSString containerItemIdentifier){
            INSFileProviderEnumerator maybeEnumerator = null;
            if (containerItemIdentifier == NSFileProviderItemIdentifier.RootContainer.ToString()){
                return new FileProviderEnumerator(NSFileProviderItemIdentifier.RootContainer);
            }else if (containerItemIdentifier == NSFileProviderItemIdentifier.WorkingSetContainer.ToString()){
                return new FileProviderEnumerator(NSFileProviderItemIdentifier.WorkingSetContainer);
            } //Need .allDirectories as well

            return maybeEnumerator;
				
        }

    }
}
