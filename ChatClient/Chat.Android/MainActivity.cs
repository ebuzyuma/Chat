using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Json;
using System.IO;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Chat.Core;
using Android.Views.InputMethods;
using System.Threading.Tasks;

namespace Chat.Droid
{
	[Activity (Label = "Chat", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
		private MessageManager _messageManager;

		private AlertDialog _serverAlertDialog;
		private AlertDialog _userNameAlertDialog;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Main);

			var messagesAdapter = InitializeMessagesListView ();
			InitializeMessageForm ();
			InitializeServerPopUp ();
			InitializeUserNamePopUp ();
			var usersAdapter = InitializeRoomHandlers ();

			_messageManager = new MessageManager (messagesAdapter, usersAdapter, ShowInfoMessage);
			_messageManager.JoinChat ();
		}

		private ListViewAdapter InitializeMessagesListView ()
		{
			ListView list = FindViewById<ListView> (Resource.Id.MessagesList);
			var messagesArrayAdapter = new ArrayAdapter<string> (this, Android.Resource.Layout.SimpleListItem1);
			list.Adapter = messagesArrayAdapter;

			return new ListViewAdapter (messagesArrayAdapter, this);
		}

		private void InitializeMessageForm ()
		{
			Button sendButton = FindViewById<Button> (Resource.Id.SendButton);
			sendButton.Click += (sender, e) => SendMessage ();

			EditText messageInput = FindViewById<EditText> (Resource.Id.MessageField);
			messageInput.EditorAction += HandleSendClick;
		}

		private void SendMessage ()
		{
			EditText messageInput = FindViewById<EditText> (Resource.Id.MessageField);
			if (!String.IsNullOrWhiteSpace(messageInput.Text))
			{
				_messageManager.Send(messageInput.Text);
				messageInput.Text = String.Empty;
			}
		}

		private void HandleSendClick(object sender, TextView.EditorActionEventArgs e)
		{
			e.Handled = false;
			if (e.ActionId == ImeAction.Send)
			{
				SendMessage ();
				e.Handled = true;
			}
		}

		private void InitializeServerPopUp ()
		{
			_serverAlertDialog = CreateEditDialog ("Server", UpdateServerClick);
			Button serverPopUpButton = FindViewById<Button> (Resource.Id.ServerPopUpButton);
			serverPopUpButton.Click += ServerPopUpClick;
		}

		private AlertDialog CreateEditDialog(string title, EventHandler<DialogClickEventArgs> updateHandler)
		{
			AlertDialog.Builder builder = new AlertDialog.Builder (this);

			builder
				.SetTitle (title)
				.SetView (LayoutInflater.Inflate (Resource.Layout.SingleEditView, null))
				.SetNegativeButton ("Cancel", handler: null)
				.SetPositiveButton ("Update", handler: updateHandler);
			return builder.Create ();
		}

		private void ServerPopUpClick(object sender, EventArgs e)
		{
			_serverAlertDialog.Show();
			EditText editText = _serverAlertDialog.FindViewById<EditText>(Resource.Id.EditText1);
			editText.Text = _messageManager.ServerHelper.ServerUrl;
		}

		private void UpdateServerClick (object sender, DialogClickEventArgs e)
		{
			EditText editText = _serverAlertDialog.FindViewById<EditText>(Resource.Id.EditText1);
			_messageManager.ServerHelper.ServerUrl = editText.Text;
			_messageManager.Reset ();
			_messageManager.JoinChat ();
		}

		private void InitializeUserNamePopUp ()
		{
			_userNameAlertDialog = CreateEditDialog ("User Name", UpdateUserNameClick);
			Button userNamePopUpButton = FindViewById<Button> (Resource.Id.UserNamePopUpButton);
			userNamePopUpButton.Click += UserNamePopUpClick;
		}

		private void UserNamePopUpClick(object sender, EventArgs e)
		{
			_userNameAlertDialog.Show();
			EditText editText = _userNameAlertDialog.FindViewById<EditText>(Resource.Id.EditText1);
			editText.Text = _messageManager.UserName;
		}

		private void UpdateUserNameClick (object sender, DialogClickEventArgs e)
		{
			EditText editText = _userNameAlertDialog.FindViewById<EditText>(Resource.Id.EditText1);
			if (!String.IsNullOrWhiteSpace (editText.Text))
				_messageManager.UserName = editText.Text;
			else
				_userNameAlertDialog.SetMessage ("User name should't be empty");
		}

		private ListViewAdapter InitializeRoomHandlers ()
		{
			//handle roombutton click
			Button roomsButton = FindViewById<Button> (Resource.Id.RoomButton);
			roomsButton.Click += (object sender, EventArgs e) =>  {
				ToggleLayoutVisability (Resource.Id.MainChatLayout);
				ToggleLayoutVisability (Resource.Id.UsersLayout);
			};

			var userArrayAdapter = new ArrayAdapter<string> (this, Android.Resource.Layout.SimpleListItem1);
			ListView usersList = FindViewById<ListView> (Resource.Id.UsersListView);
			usersList.Adapter = userArrayAdapter;
			usersList.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) =>  {
				roomsButton.CallOnClick ();
				TextView selectedItem = e.View as TextView;
				if (selectedItem != null)
					_messageManager.CurrentRoom = selectedItem.Text;
			};

			return new ListViewAdapter (userArrayAdapter, this);
		}

		private void ToggleLayoutVisability(int layoutId)
		{
			LinearLayout layout = FindViewById<LinearLayout>(layoutId);
			if (layout != null) {
				if (layout.Visibility == ViewStates.Visible) {
					layout.Visibility = ViewStates.Gone;
					layout.ClearFocus ();
				} else {
					layout.Visibility = ViewStates.Visible;
					layout.RequestFocus ();
				}
			}
		}

		public void ShowInfoMessage(string message)
		{
			RunOnUiThread (() => {
				Toast.MakeText(this, message, ToastLength.Long).Show();
			});
		}

	}
}


