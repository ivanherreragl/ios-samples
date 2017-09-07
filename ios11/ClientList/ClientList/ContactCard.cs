using System;
using UIKit;
using Foundation;
using MobileCoreServices;
using Contacts;
namespace ClientList
{
	enum ContactCardError
	{
        InvalidTypeIdentifier,
        invalidVCard
	};

    public class ContactCard : NSObject, INSItemProviderReading, INSItemProviderWriting
    {

        public string Name;
        public string PhoneNumber;
        public UIImage Photo;
        public NSUrl AttachmentURL;

        public ContactCard(string name, string phone, UIImage picture, NSUrl attachment)
        {
            Name = name;
            PhoneNumber = phone;
            Photo = picture;
            AttachmentURL = attachment;
        }

        public static string[] ReadableTypeIdentifiersForItemProvider = new string[] { (string)UTType.VCard, (string)UTType.UTF8PlainText };

		public ContactCard(NSData data, string typeIdentifier)
		{
            if (typeIdentifier == (string)UTType.VCard){
                NSError err;
                var contacts = CNContactVCardSerialization.GetContactsFromData(data, out err);
                if (err == null){
                    if (contacts.Length > 0){
                        var contact = contacts[0];
                        Name = contact.GivenName + " " + contact.FamilyName;
                        if (contact.PhoneNumbers.Length > 0){
                            PhoneNumber = contact.PhoneNumbers[0].Value.ToString();

						}
                        if (contact.ImageData != null){
                            Photo = new UIImage(contact.ImageData);
                        }

                    }else{
                        
                        //throw ContactCardError.invalidVCard;
                    }
                }else{
					//throw ContactCardError.invalidVCard;
				}
            }else if (typeIdentifier == (string)UTType.UTF8PlainText){
                Name = (string)NSString.FromData(data, NSStringEncoding.UTF8);
            }else{
                //throw ContactCardError.InvalidTypeIdentifier;
            }
		}

        public static string[] WritableTypeIdentifiersForItemProvider = new string[] { (string)UTType.VCard, (string)UTType.UTF8PlainText };

        public NSProgress LoadData(string typeIdentifier, Action<NSData, NSError> completionHandler){
            if (typeIdentifier == (string)UTType.VCard){
                completionHandler(CreateVCard(), null);
            }else if (typeIdentifier == (string)UTType.UTF8PlainText){
                completionHandler(NSData.FromString(Name, NSStringEncoding.UTF8), null);
            }else{
                NSError err = new NSError();
				//ContactCardError.InvalidTypeIdentifier;
				completionHandler(null, err);
            }
			
            return null;
        }

        public NSData CreateVCard(){
            var vCardText = "BEGIN:VCARD\nVERSION3.0";

            vCardText = vCardText + $"\nFN:{Name}";

            if (PhoneNumber != null){
                vCardText = vCardText + $"\nTEL;type=pref:{PhoneNumber}";
            }

			if (Photo != null)
			{
                var pngData = Photo.AsPNG();
                var base64String = pngData.GetBase64EncodedString(NSDataBase64EncodingOptions.None);
                vCardText = vCardText + $"\nPHOTO;ENCODING=BASE64;TYPE=PNG:{base64String}";
			}

            vCardText = vCardText + "\nEND:VCARD";

            return NSData.FromString(vCardText, NSStringEncoding.UTF8);
        }
    }
}
