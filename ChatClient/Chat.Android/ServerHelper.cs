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
	    private const string DefaultServerUrl = "http://172.24.80.54:31337/";
	    private const string GetUrlTemplate = "{0}?token={1}";

	    private CancellationTokenSource _cancellationTokenSource;
        
        private string _serverUrl;
		public string ServerUrl 
		{ 
			get { return _serverUrl; } 
			set 
			{
				_cancellationTokenSource.Cancel (); // for canceling all requests
				_cancellationTokenSource = new CancellationTokenSource ();
				try 
				{			
					var newUri = new Uri (value);
					_serverUrl = newUri.ToString ();
					Token = 0; // reset token when url updated
				} 
				catch (Exception ex) 
				{
					_showInfo ("Incorrect url");
					_serverUrl = value;
				}
			}		
		}

		private Action<string> _showInfo;
		private Action<HttpWebResponse> _getResponseCallback;
        
        public int Token { get; set; }
	
		public ServerHelper (Action<string> showInfo, Action<HttpWebResponse> getResponseCallback)
		{
			Token = 0;
			_cancellationTokenSource = new CancellationTokenSource ();
			_showInfo = showInfo;
			_getResponseCallback = getResponseCallback;

			_serverUrl = DefaultServerUrl;
		}

		public async Task GetAsync ()
		{
			string url = String.Format (GetUrlTemplate, _serverUrl, Token);

			HttpWebRequest request = WebRequest.CreateHttp (url);
			HttpWebResponse response = null;
			try 
			{
				response = await request.GetResponseAsync (_cancellationTokenSource.Token);

				_getResponseCallback (response);
				GetAsync();
			} 
			catch (WebException ex) 
			{
				Console.WriteLine ("WebException in GetAsync: " + ex.Status);
				switch (ex.Status) 
				{ 
					case WebExceptionStatus.ConnectFailure:
						//TODO perform request after some delay
						_showInfo ("Connection to the server failed");
						break;
					case WebExceptionStatus.Timeout:
						GetAsync ();
						break;
				}
			} 
			catch (Exception ex)
			{
				Console.WriteLine (ex.Message);
			} 
			finally 
			{
				if(response != null)
					response.Close();
			}
		}
			
		public async Task PostAsync(string message)
		{
			HttpWebRequest request = WebRequest.CreateHttp (_serverUrl);
			request.Method = "POST";

			Stream requestStream = null;
			HttpWebResponse response = null;
			try 
			{				
				requestStream = await request.GetRequestStreamAsync ();			
				var jsonObj = new JsonPrimitive (message);
				jsonObj.Save (requestStream);

				Token++;
				response = await request.GetResponseAsync (_cancellationTokenSource.Token); 

			} 
			catch (WebException ex) 
			{
				Console.WriteLine ("WebException in PostAsync:" + ex.Status);
				_showInfo ("Post to the server failed");
			}
			catch (Exception ex) 
			{
				Console.WriteLine (ex.Message);			
			}
			finally 
			{
				if (requestStream != null)
					requestStream.Close ();

				if (response != null)
					response.Close ();					
			}
		}
	}
}

