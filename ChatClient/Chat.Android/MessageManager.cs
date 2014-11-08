using System;
using System.Collections.Generic;
using System.Net;
using System.Json;
using System.Linq;

namespace Chat.Core
{
	public class MessageManager
	{
		//typical message
		//message:from:to:messageBody

		//to add new user
		//user:userName
		//each one who online should responce with
		//online:userName

		//to update user name
		//update:oldName:newName

		public string UserName { get; set; }
		public List<string> Users { get; set;	}
		public List<Message> Messages { get; private set; }
		public ServerHelper ServerHelper { get; private set; }

		private string _currentRoom;
		public string CurrentRoom 
		{ 
			get { return _currentRoom; } 
			set 
			{
				_messagesViewAdapter.Clear();

				IEnumerable<Message> messagesToShow;
				if (value == "Common") {
					messagesToShow = Messages.Where (p => !p.IsPrivate);
					_currentRoom = String.Empty;
				} else {
					_currentRoom = value;
					messagesToShow = Messages.Where (p => p.From == value && p.IsPrivate);
				}
				_messagesViewAdapter.AddAll(messagesToShow.Select (p => p.ToString()).ToList());
			} 
		}

		private IListViewAdapter _messagesViewAdapter;
		private IListViewAdapter _usersViewAdapter;

		public MessageManager (IListViewAdapter messagesViewAdapter, IListViewAdapter usersViewAdapter, Action<string> showInfo)
		{
			_messagesViewAdapter = messagesViewAdapter;
			_usersViewAdapter = usersViewAdapter;

			Messages = new List<Message> ();
			Users = new List<string> {"Common"};
			_usersViewAdapter.AddAll (Users);
			//UserName = Guid.NewGuid ().ToString ().Substring (0, 8);
			UserName = "mob"; // just for testing
			_currentRoom = String.Empty;

			ServerHelper = new ServerHelper (showInfo, GetResponseCallback);
		}

		public void Reset ()
		{
			_messagesViewAdapter.Clear();
			Messages.Clear ();

			ServerHelper.Token = 0;

			_usersViewAdapter.Clear ();
			Users.Clear ();
			Users.Add ("Common");
			_usersViewAdapter.AddAll (Users);
		}

		public void GetResponseCallback (HttpWebResponse response)
		{
			JsonObject jsonObject = (JsonObject)JsonObject.Load (response.GetResponseStream());
			List<string> incomingMessages = ((JsonArray)jsonObject ["messages"]).Select(p => (string)p).ToList();

			int incomingToken = (int)jsonObject["token"];

			if (incomingToken > ServerHelper.Token) 
			{
				ServerHelper.Token = incomingToken;

				var splited = incomingMessages.Select (p => p.Split (':'));
				var grouped = splited.GroupBy(p => p[0]); 

				foreach (var group in grouped) 
				{
					switch (group.Key)
					{
						case "message":
							AddMessages(group.ToList());
							break;
						case "update":
							break;
						case "user":
							Users.AddRange (group.Select (p => p [1]).ToList ());
							_usersViewAdapter.AddAll (group.Select (p => p [1]).ToList ());
							break;
						case "online":
							Users.Union (group.Select(p => p[1]).ToList ());
							break;
					}				
				}					
			}
		}

			
		public void Send (string text)
		{
			ServerHelper.PostAsync (String.Join (":", "message", UserName, CurrentRoom, text));
		}

		private void AddMessages(List<string[]> messages)
		{
			var messagesForCurrentUser = messages.Where(p => p[2] == UserName || String.IsNullOrEmpty(p[2]));
			var messagesToAdd = messagesForCurrentUser
				.Select (p => new Message (p));

			Messages.AddRange (messagesToAdd);

			IEnumerable<Message> messagesToShow;
			if (CurrentRoom == String.Empty)
				messagesToShow = messagesToAdd.Where (p => !p.IsPrivate);
			else
				messagesToShow = messagesToAdd.Where (p => p.IsPrivate && p.From == CurrentRoom);

			_messagesViewAdapter.AddAll(messagesToShow.Select(p => p.ToString()).ToList());
		}

		public void JoinChat ()
		{
			ServerHelper.GetAsync ();
		}
	}
}

