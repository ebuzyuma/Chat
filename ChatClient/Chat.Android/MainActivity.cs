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

namespace Chat.Droid
{
	[Activity (Label = "Chat", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
		private ServerHelper _serverHelper;
		private Button _sendButton;
		private EditText _messageForm;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Main);

			// Initialize list adapter
			ListView list = FindViewById<ListView> (Resource.Id.MessagesList);
			list.Adapter = new ArrayAdapter<string> (this, Android.Resource.Layout.SimpleListItem1);

			_sendButton = FindViewById<Button> (Resource.Id.SendButton);			
			_sendButton.Click += SendButtonClick;

			_messageForm = FindViewById<EditText>(Resource.Id.MessageField);
			_messageForm.KeyPress += HandleEnterClick;

			_serverHelper = new ServerHelper (PopulateView);
			_serverHelper.GetAsync ();
		}

		private void SendButtonClick(object sender, EventArgs e)
		{
			if (!String.IsNullOrWhiteSpace(_messageForm.Text))
			{
				_serverHelper.Post(_messageForm.Text);
				_messageForm.Text = String.Empty;
			}
		}

		private void HandleEnterClick(object sender, View.KeyEventArgs e)
		{
			e.Handled = false;
			if ((e.Event.Action == KeyEventActions.Down) && (e.KeyCode == Keycode.Enter))
			{
				_sendButton.PerformClick();
				e.Handled = true;
			}
		}

		public void PopulateView(List<String> messages)
		{
			RunOnUiThread (() => {
				ListView list = FindViewById<ListView> (Resource.Id.MessagesList);
				((ArrayAdapter)list.Adapter).AddAll (messages);
			});
		}
			
	}
}


