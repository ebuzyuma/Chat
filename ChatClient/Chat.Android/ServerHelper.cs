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

		public int Token { get; set; }
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
					Uri newUri = new Uri (value);
					_serverUrl = newUri.ToString ();
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

		public ServerHelper (Action<string> showInfo, Action<HttpWebResponse> getResponseCallback)
		{
			Token = 0;
			_cancellationTokenSource = new CancellationTokenSource ();
			_showInfo = showInfo;
			_getResponseCallback = getResponseCallback;

			_serverUrl = _defaultServerUrl;
		}

		public async Task GetAsync ()
		{
			string url = String.Format (_getUrlTemplate, _serverUrl, Token);

			HttpWebRequest request = WebRequest.CreateHttp (url);
			HttpWebResponse response = null;
			try 
			{
				response = (HttpWebResponse) await request.GetResponseAsync (_cancellationTokenSource.Token);

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

