using System;
using System.Collections.Generic;
using Foundation;
using CoreFoundation;
using UIKit;

namespace ClientList
{
    public partial class ContactsTableViewController : UITableViewController, IUITableViewDragDelegate, IUITableViewDropDelegate
    {
        List<ContactCard> ContactCards = new List<ContactCard> {
		    new ContactCard("Jane Doe","(444) 444-4444", UIImage.FromBundle("Profile1.png"), null),
			new ContactCard("John Doe","(555) 555-5555", UIImage.FromBundle("Profile2.png"), Attachment.Url("John Doe")),
			new ContactCard("Mr X","(434) 434-4444", UIImage.FromBundle("Profile3.png"), null),
			new ContactCard("Mr Z","(655) 655-5555", UIImage.FromBundle("Profile4.png"), Attachment.Url("Mr Z"))
        };



		public ContactsTableViewController(IntPtr p) : base(p)
        {

		}

        public ContactsTableViewController() : base("ContactsTableViewController", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.
            this.NavigationItem.Title = "Customers";
            this.TableView.DragDelegate = this;
            this.TableView.DropDelegate = this;
        }

        public void PerformDrop(UITableView tableView, IUITableViewDropCoordinator coordinator){
            if (coordinator.DestinationIndexPath == null){
                return;
            }
            var insertionIndex = coordinator.DestinationIndexPath;
            foreach (var coordinatorItem in coordinator.Items){
                var itemProvider = coordinatorItem.DragItem.ItemProvider;
                if (itemProvider.CanLoadObject(typeof(ContactCard))){
                    itemProvider.LoadObject(typeof(ContactCard),(obj, err)=>{
                        DispatchQueue.MainQueue.DispatchAsync(()=>{
                            var contactCard = (ContactCard)obj;
                            if (contactCard != null){
                                ContactCards.Insert(insertionIndex.Row, contactCard);
                                tableView.InsertRows(new NSIndexPath[]{insertionIndex}, UITableViewRowAnimation.Automatic);
                            }else{
                                DisplayError(err);
                            }
                        });
                    });
                }
            }
        }

        public UIDragItem[] GetItemsForBeginningDragSession(UITableView tableView, IUIDragSession session, Foundation.NSIndexPath indexPath){
            var contactCard = ContactCards[indexPath.Row];
            var dragItem = new UIDragItem(new NSItemProvider(contactCard));
            return new UIDragItem[] { dragItem };

        }

		public override nint RowsInSection(UITableView tableView, nint section)
		{
            return ContactCards.Count;
		}

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			UITableViewCell cell = tableView.DequeueReusableCell("contactNameCell");
			
			//---- if there are no cells to reuse, create a new one
			if (cell == null)
			{ cell = new UITableViewCell(UITableViewCellStyle.Default, "contactNameCell"); }

            cell.TextLabel.Text = ContactCards[indexPath.Row].Name;

			return cell;
		}



        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

		public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
		{
            if (segue.Identifier == "detailsSegue"){
                var detailsViewController = (ContactDetailViewController)segue.DestinationViewController;
                var contactCard = ContactCards[TableView.IndexPathForSelectedRow.Row];
                detailsViewController.TheContactCard = contactCard;

            }



		}

        public void DisplayError(NSError err){
            var alert = UIAlertController.Create("Unable to load object", err.LocalizedDescription, UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create("Dismiss", UIAlertActionStyle.Default, null));
            PresentViewController(alert, true, null);
        }
    }
}

