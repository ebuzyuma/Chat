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
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Main);

			var serverHelper = new ServerHelper (PopulateView);

			Button button = FindViewById<Button> (Resource.Id.SendButton);			
			button.Click += (object sender, EventArgs e) => 
			{
				EditText message = FindViewById<EditText> (Resource.Id.MessageField);			
				if (!String.IsNullOrWhiteSpace(message.Text))
				{
					serverHelper.Post(message.Text);
					message.Text = String.Empty;
				}
			};


			// Initialize list adapter
			ListView list = FindViewById<ListView> (Resource.Id.MessagesList);
			list.Adapter = new ArrayAdapter<string> (this, Android.Resource.Layout.SimpleListItem1);

			serverHelper.Get();
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


