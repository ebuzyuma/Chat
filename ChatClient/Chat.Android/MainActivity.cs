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
		private ServerHelper _serverHelper;

		private EditText _messageForm;

		private AlertDialog _serverAlertDialog;
		private AlertDialog _userNameAlertDialog;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Main);

			// Initialize list adapter
			ListView list = FindViewById<ListView> (Resource.Id.MessagesList);
			list.Adapter = new ArrayAdapter<string> (this, Android.Resource.Layout.SimpleListItem1);

			Button sendButton = FindViewById<Button> (Resource.Id.SendButton);			
			sendButton.Click += (sender, e) => SendMessage();;

			_messageForm = FindViewById<EditText>(Resource.Id.MessageField);
			_messageForm.EditorAction += HandleEnterClick;

			_serverHelper = new ServerHelper (PopulateView, ClearListView, ShowInfoMessage);
			_serverHelper.GetAsync ();

			_serverAlertDialog = CreateAlertDialog ("Server", UpdateServerClick);
			_userNameAlertDialog = CreateAlertDialog ("User Name", UpdateUserNameClick);

			Button serverPopUpButton = FindViewById<Button> (Resource.Id.ServerPopUpButton);			
			serverPopUpButton.Click += ServerPopUpClick;

			Button userNamePopUpButton = FindViewById<Button> (Resource.Id.UserNamePopUpButton);			
			userNamePopUpButton.Click += UserNamePopUpClick;
		}
			
		private void SendMessage ()
		{
			if (!String.IsNullOrWhiteSpace(_messageForm.Text))
			{
				_serverHelper.PostAsync(_messageForm.Text);
				_messageForm.Text = String.Empty;
			}
		}

		private void HandleEnterClick(object sender, TextView.EditorActionEventArgs e)
		{
			e.Handled = false;
			if (e.ActionId == ImeAction.Send)
			{
				SendMessage ();
				e.Handled = true;
			}
		}

		private AlertDialog CreateAlertDialog(string title, EventHandler<DialogClickEventArgs> updateHandler)
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
			editText.Text = _serverHelper.ServerUrl;
		}

		private void UpdateServerClick (object sender, DialogClickEventArgs e)
		{
			EditText editText = _serverAlertDialog.FindViewById<EditText>(Resource.Id.EditText1);
			_serverHelper.UpdateServerUrl (editText.Text);
		}

		private void UserNamePopUpClick(object sender, EventArgs e)
		{
			_userNameAlertDialog.Show();
			EditText editText = _userNameAlertDialog.FindViewById<EditText>(Resource.Id.EditText1);
			editText.Text = _serverHelper.UserName;
		}

		private void UpdateUserNameClick (object sender, DialogClickEventArgs e)
		{
			EditText editText = _userNameAlertDialog.FindViewById<EditText>(Resource.Id.EditText1);
			if (!String.IsNullOrWhiteSpace (editText.Text))
				_serverHelper.UserName = editText.Text;
			else
				_userNameAlertDialog.SetMessage ("User name should't be empty");
		}

		public void PopulateView(List<String> messages)
		{
			RunOnUiThread (() => {
				ListView list = FindViewById<ListView> (Resource.Id.MessagesList);
				((ArrayAdapter)list.Adapter).AddAll (messages);
			});
		}

		public void ClearListView()
		{
			RunOnUiThread (() => {
				ListView list = FindViewById<ListView> (Resource.Id.MessagesList);
				((ArrayAdapter)list.Adapter).Clear();
			});
		}

		public void ShowInfoMessage(string message)
		{
			RunOnUiThread (() => {
				Toast.MakeText(this, message, ToastLength.Long).Show();
			});
		}
			
	}
}


