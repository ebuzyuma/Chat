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

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			SetContentView (Resource.Layout.Main);

			var messagesAdapter = InitializeMessagesListView ();
			InitializeMessageForm ();
			InitializeServerPopUp ();
			InitializeUserNamePopUp ();
			var usersAdapter = InitializeRoomHandlers ();

			_messageManager = new MessageManager (messagesAdapter, usersAdapter, ShowInfoMessage);
			_messageManager.JoinChatAsync ();
		}

		private ListViewAdapter<Chat.Core.Message> InitializeMessagesListView ()
		{
			ListView list = FindViewById<ListView> (Resource.Id.MessagesList);

			list.ItemLongClick += (object sender, AdapterView.ItemLongClickEventArgs e) => 
			{
				var textView = (TextView) e.View;
				string[] splited = textView.Text.Split(':');
				var adapter = (ArrayAdapter<Chat.Core.Message>) ((ListView) sender).Adapter;
				var message = adapter.GetItem(e.Position);
				if (splited[0] == "You" && message.Id == _messageManager.LastMessage.Id)
				{
					UpdateEditView("Update", splited[1].Trim(), ImeAction.Done);
				}
			};

			var messagesAdapter = new ListViewAdapter<Chat.Core.Message> (this, Android.Resource.Layout.SimpleListItem1);
			list.Adapter = messagesAdapter;

			return messagesAdapter;
		}

		private void InitializeMessageForm ()
		{
			EditText messageInput = FindViewById<EditText> (Resource.Id.MessageField);
			messageInput.EditorAction += HandleInputActionClick;

			Button sendButton = FindViewById<Button> (Resource.Id.SendButton);
			sendButton.Click += (sender, e) => 
			{
				if (!String.IsNullOrEmpty(messageInput.Text) )
				{
					if(	sendButton.Text == "Update")
					{	
						_messageManager.UpdateLastMessage(messageInput.Text);
						UpdateEditView();
					}
					else 
					{
						SendMessage ();
					}
				}
			};

		}

		private void SendMessage ()
		{
			EditText messageInput = FindViewById<EditText> (Resource.Id.MessageField);
			if (!String.IsNullOrWhiteSpace(messageInput.Text))
			{
				_messageManager.Send(messageInput.Text.Trim());
				messageInput.Text = String.Empty;
			}
		}

		private void HandleInputActionClick(object sender, TextView.EditorActionEventArgs e)
		{
			e.Handled = false;
			if (e.ActionId == ImeAction.Send)
			{
				SendMessage ();
				e.Handled = true;
			}

			if (e.ActionId == ImeAction.Done)
			{
				EditText messageInput = FindViewById<EditText> (Resource.Id.MessageField);
				_messageManager.UpdateLastMessage (messageInput.Text);
				UpdateEditView ();

				e.Handled = true;
			}
		}

		public void UpdateEditView(string buttonText = "Send", string inputText = "", ImeAction newAction = ImeAction.Send)
		{
			Button sendButton = FindViewById<Button> (Resource.Id.SendButton);
			sendButton.Text = buttonText;

			EditText messageInput = FindViewById<EditText> (Resource.Id.MessageField);
			messageInput.Text = inputText;
			messageInput.ImeOptions = newAction;

			var imm = (InputMethodManager) GetSystemService(Context.InputMethodService);
			imm.RestartInput(messageInput);

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
			_messageManager.JoinChatAsync ();
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

		private ListViewAdapter<User> InitializeRoomHandlers ()
		{
			Button roomsButton = FindViewById<Button> (Resource.Id.RoomButton);
			roomsButton.Click += (object sender, EventArgs e) =>  {
				ToggleLayoutVisability (Resource.Id.MainChatLayout);
				ToggleLayoutVisability (Resource.Id.UsersLayout);
			};

			var usersViewAdapter = new ListViewAdapter<User> (this, Android.Resource.Layout.SimpleListItem1);
			ListView usersList = FindViewById<ListView> (Resource.Id.UsersListView);
			usersList.Adapter = usersViewAdapter;
			usersList.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) =>  {
				roomsButton.CallOnClick ();
				TextView selectedItem = e.View as TextView;
				if (selectedItem != null)
					_messageManager.CurrentRoom = selectedItem.Text;
			};

			return usersViewAdapter;
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

		protected override void OnStop ()
		{
			_messageManager.LeaveChat ();
			base.OnStop ();
		}

	}
}


