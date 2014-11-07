using System;
using System.Net;
using System.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace Chat.Core
{
	public class ServerHelper
	{
		private readonly string _defaultServerUrl = "http://172.24.80.54:31337/";
		private readonly string _getUrlTemplate = "{0}?token={1}";

		private CancellationTokenSource _cancellationTokenSource;
		private int _token;
		private string _serverUrl;

		public string ServerUrl { get{ return _serverUrl; } }


		private Action<List<string>> _populateView;
		private Action<string> _showInfo;
		private Action _clearListView;

		public string UserName { get; set; }

		public List<string> Users {	get{ return new List<string>{ "asdf", "asdf", "cvbcvb" }; }}

		public ServerHelper (Action<List<string>> populateView, Action clearListView, Action<string> showInfo, string serverUrl = null)
		{
			_token = 0;
			_cancellationTokenSource = new CancellationTokenSource ();
			_populateView = populateView;
			_clearListView = clearListView;
			_showInfo = showInfo;

			UpdateServerUrl (serverUrl ?? _defaultServerUrl);

			UserName = Guid.NewGuid ().ToString ().Substring (0, 8);
		}

		public void UpdateServerUrl (string url)
		{
			_cancellationTokenSource.Cancel (); // for canceling all requests
			_cancellationTokenSource = new CancellationTokenSource ();

			Uri newUri;
			try {
			
				newUri = new Uri(url);
				_serverUrl = newUri.ToString();
				_clearListView();
				_token = 0;
				GetAsync ();

			} catch (Exception ex) {
				_showInfo("Incorrect url");
				_serverUrl = url;
			}
		}
			
		public async Task GetAsync ()
		{
			string url = String.Format (_getUrlTemplate, _serverUrl, _token);

			HttpWebRequest request = WebRequest.CreateHttp (url);
			HttpWebResponse response = null;
			try {
				response = (HttpWebResponse) await request.GetResponseAsync (_cancellationTokenSource.Token);

				ParseResults (response);

			} catch (WebException ex) {
				Console.WriteLine ("WebException in GetAsync: " + ex.Status);
				switch (ex.Status) 
				{ 
					case WebExceptionStatus.ConnectFailure:
						_showInfo ("Connection to the server failed");
						break;
					case WebExceptionStatus.Timeout:
						GetAsync ();
						break;
				}
			} catch (Exception ex){
				Console.WriteLine (ex.Message);
			} finally {
				if(response != null)
					response.Close();
			}

		}

		private void ParseResults (WebResponse response)
		{
			JsonObject jsonObject = (JsonObject)JsonObject.Load (response.GetResponseStream());
			List<string> incomingMessages = ((JsonArray)jsonObject ["messages"]).Select(p => (string)p).ToList();

			int incomingToken = (int)jsonObject["token"];

			if (incomingToken > _token) 
			{
				_token = incomingToken;

				_populateView(incomingMessages);
			}
		}

		public async Task PostAsync(string message)
		{
			message = String.Format ("{0}: {1}", UserName, message);

			_populateView(new List<string>{message});
			_token++;

			HttpWebRequest request = WebRequest.CreateHttp (_serverUrl);
			request.Method = "POST";

			Stream requestStream = null;
			HttpWebResponse response = null;
			try {				
				requestStream = await request.GetRequestStreamAsync ();			
				var jsonObj = new JsonPrimitive (message);
				jsonObj.Save (requestStream);

				response = await request.GetResponseAsync (_cancellationTokenSource.Token); 

			} catch (WebException ex) {
				Console.WriteLine ("WebException in PostAsync:" + ex.Status);
				_showInfo ("Post to the server failed");
			} catch (Exception ex) {
				Console.WriteLine (ex.Message);			
			} finally {
				if (requestStream != null)
					requestStream.Close ();

				if (response != null)
					response.Close ();
					
			}

		}
	}
}

