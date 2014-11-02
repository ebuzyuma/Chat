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

		private int _token;
		private Action<List<string>> _populateView;

		public string ServerUrl { get; set; }
		public string UserName { get; set; }

		public ServerHelper (Action<List<string>> populateView, string serverUrl = null)
		{
			_token = 0;
			_populateView = populateView;
			ServerUrl = serverUrl ?? _defaultServerUrl;
			UserName = Guid.NewGuid ().ToString ().Substring (0, 8);
		}

		public async Task GetAsync()
		{
			await Task.Run (() => Get());
		}

		public void Get ()
		{
			string url = String.Format (_getUrlTemplate, ServerUrl, _token);

			WebRequest request = WebRequest.Create (url);
			WebResponse response = null;
			try {
				response = request.GetResponse ();

				ParseResults (response);

			} catch (WebException ex) {
				Console.WriteLine ("WebException in GetAsync: " + ex.Status);
			}catch (Exception ex){
				Console.WriteLine (ex.Message);
			} finally {
				if(response != null)
					response.Close();
			}

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

		public async Task PostAsync(string message)
		{
			await Task.Run (() => Post (message));
		}

		public void Post(string message)
		{
			message = String.Format ("{0}: {1}", UserName, message);

			WebRequest request = WebRequest.Create (ServerUrl);
			request.Method = "POST";

			Stream requestStream = null;
			WebResponse response = null;
			try {				
				requestStream = request.GetRequestStream ();			
				var jsonObj = new JsonPrimitive (message);
				jsonObj.Save (requestStream);

				response = request.GetResponse (); 

				_populateView(new List<string>{message});
				_token++;
			} catch (WebException ex) {
				Console.WriteLine ("WebException in PostAsync:" + ex.Status);
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

