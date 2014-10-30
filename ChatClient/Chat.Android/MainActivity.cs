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

namespace Chat.Android
{
	[Activity (Label = "Chat Android", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
		private string _serverUrl = "http://172.24.80.54:31337/";
		private string _getUrlTemplate = "{0}?token={1}";
		//private string _serverUrl = "http://10.163.0.135:31337/";

		private int _token = 0;
		private ArrayAdapter _listAdapter;


		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Main);

			Button button = FindViewById<Button> (Resource.Id.SendButton);			
			button.Click += delegate {
				EditText message = FindViewById<EditText> (Resource.Id.MessageField);			
				if (!String.IsNullOrWhiteSpace(message.Text))
				{
					Post(message.Text);
					message.Text = String.Empty;
				}
			};


			// Add listadapter for view
			_listAdapter = new ArrayAdapter<string> (this, Android.Resource.Layout.ListItemTemplate);

			ListView list = FindViewById<ListView> (Resource.Id.MessagesList);
			list.Adapter = _listAdapter;

			Get();
		}

		public void Get ()
		{
			string url = String.Format(_getUrlTemplate, _serverUrl, _token);

			WebRequest httpReq = WebRequest.Create (url);
			httpReq.BeginGetResponse (ResponseGetCallback, httpReq);
		}

		private void ResponseGetCallback (IAsyncResult ar)
		{
			var httpReq = (WebRequest) ar.AsyncState;

			using (WebResponse httpRes = httpReq.EndGetResponse (ar)) {
				ParseResults (httpRes);
			}

			Get ();
		}

		private void ParseResults (WebResponse httpRes)
		{
			JsonObject jsonObject = (JsonObject)JsonObject.Load (httpRes.GetResponseStream());
			List<string> incomingMessages = ((JsonArray)jsonObject ["messages"]).Select(p => (string)p).ToList();

			int incomingToken = (int)jsonObject["token"];

			if (incomingToken > _token) 
			{
				_token = incomingToken;
				RunOnUiThread (() => _listAdapter.AddAll (incomingMessages));
			}
		}

		private void Post(string message)
		{
			var request = WebRequest.Create (_serverUrl);
			request.Method = "POST";
			using (var stream = request.GetRequestStream ()) 
			{
				using (var streamWriter = new StreamWriter (stream)) 
				{
					streamWriter.Write ("\"" + message + "\"");
					streamWriter.Flush ();

					_token++;
					_listAdapter.Add (message);

					request.BeginGetResponse (ResponsePostCallback, request);


				}
			}
		}

		private void ResponsePostCallback (IAsyncResult ar)
		{
			var httpReq = (WebRequest) ar.AsyncState;

			using (WebResponse httpRes = httpReq.EndGetResponse (ar)) {
			}

		}
			
	}
}


