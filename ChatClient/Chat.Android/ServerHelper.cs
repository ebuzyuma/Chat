using System;
using System.Net;
using System.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace Chat.Core
{
	public class ServerHelper
	{
		//private readonly string _serverUrl = "http://172.24.80.54:31337/";
		private readonly string _defaultServerUrl = "http://192.168.137.1:31337/";
		private readonly string _getUrlTemplate = "{0}?token={1}";

		private int _token;
		private Action<List<string>> _populateView;

		public string ServerUrl { get; set; }

		public ServerHelper (Action<List<string>> populateView, string serverUrl = null)
		{
			_token = 0;
			_populateView = populateView;
			ServerUrl = serverUrl ?? _defaultServerUrl;
		}

		public async Task GetAsync ()
		{
			string url = String.Format(_getUrlTemplate, ServerUrl, _token);

			WebRequest request = WebRequest.Create (url);
			WebResponse response = await request.GetResponseAsync ();

			ParseResults (response);

			GetAsync ();
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

		public async Task Post(string message)
		{
			_token++;
			Task.Factory.StartNew(
				() => _populateView(new List<string>{message}));

			WebRequest request = WebRequest.Create (ServerUrl);
			request.Method = "POST";

			using (var stream = request.GetRequestStream ()) 
			{
				var jsonObj = new JsonPrimitive (message);
				jsonObj.Save (stream);

				using (var response = request.GetResponse ()) 
				{
				}
			}
		}
	}
}

