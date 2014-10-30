using System;
using System.Net;
using System.Json;
using System.Collections.Generic;
using System.Linq;

namespace Chat.Core
{
	public class ServerHelper
	{
		private readonly string _serverUrl = "http://172.24.80.54:31337/";
		private readonly string _getUrlTemplate = "{0}?token={1}";

		private int _token;

		private Action<List<string>> _populateView;

		public ServerHelper (Action<List<string>> populateView)
		{
			_token = 0;
			_populateView = populateView;
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

				_populateView(incomingMessages);
			}
		}

		public void Post(string message)
		{
			var request = WebRequest.Create (_serverUrl);
			request.Method = "POST";
			using (var stream = request.GetRequestStream ()) 
			{
				var jsonObj = new JsonPrimitive (message);
				jsonObj.Save (stream);

				_token++;
				_populateView (new List<string>{message});

				request.BeginGetResponse (ResponsePostCallback, request);
			}
		}

		private void ResponsePostCallback (IAsyncResult ar)
		{
			var httpReq = (WebRequest) ar.AsyncState;

			using (WebResponse httpRes = httpReq.EndGetResponse (ar))
			{

			}

		}

	}
}

