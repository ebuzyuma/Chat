using System;
using System.Collections.Generic;
using System.Net;
using System.Json;
using System.Linq;
using System.Threading.Tasks;

namespace Chat.Core
{
	public class MessageManager
	{
		//typical message
		//message:id:from:to:messageBody

		//to add new user
		//user:userName

		//to leave chat
		//leave:userName

		//for updating
		//update:message:id:body
		//update:username:Name - not implemented

		private List<Message> _allMessages;

		private IListViewAdapter<Message> _messagesViewAdapter;
		private IListViewAdapter<User> _usersViewAdapter;

		private string _currentRoom;
		public string CurrentRoom 
		{ 
			get { return _currentRoom; } 
			set 
			{
				_messagesViewAdapter.Clear();

				_currentRoom = value == "Common"? String.Empty : value;

				var messagesToShow = _allMessages.Where (p => 
					(_currentRoom == String.Empty ? p.To == String.Empty : p.From == _currentRoom && p.To == UserName) //messages to you
					|| p.From == String.Empty && p.To == _currentRoom); //your messages to user

				_messagesViewAdapter.AddAll(messagesToShow.ToList());
			} 
		}

		public Message LastMessage 
		{
			get 
			{
				return _messagesViewAdapter.Data.Last (p => p.From == String.Empty);
			}
		}

		private string _userName;
		public string UserName 
		{ 
			get { return _userName; } 
			set 
			{
				ServerHelper.PostAsync (String.Join (":", "leave", _userName));
				_userName = value;
				ServerHelper.PostAsync (String.Join(":", "user", _userName));
			}
		}

		public ServerHelper ServerHelper { get; private set; }

		public MessageManager (IListViewAdapter<Message> messagesViewAdapter, IListViewAdapter<User> usersViewAdapter, Action<string> showInfo)
		{
			_messagesViewAdapter = messagesViewAdapter;

			_usersViewAdapter = usersViewAdapter;
			_usersViewAdapter.Add (new User("Common"));

			_allMessages = new List<Message> ();

			_userName = Guid.NewGuid ().ToString ().Substring (0, 8);

			_currentRoom = String.Empty;

			ServerHelper = new ServerHelper (showInfo, GetResponseCallback);
		}

		public void UpdateLastMessage(string messageBody)
		{
			LastMessage.Body = messageBody;
			_messagesViewAdapter.Update (LastMessage); 
			ServerHelper.PostAsync (LastMessage.ToServerUpdateString());
		}

		public void Reset ()
		{
			_messagesViewAdapter.Clear();
			_allMessages.Clear ();

			_usersViewAdapter.Clear ();
			_usersViewAdapter.Add (new User("Common"));
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
							UpdateFromServer (group.GroupBy(p => p[1]));
							break;
						case "user":
							AddUsers (group.Select (p => p [1]).ToList ());
							break;
						case "leave":
							RemoveUsers (group.Select(p => p[1]).ToList());
							break;
					}				
				}					
			}
		}
							
		public void Send (string text)
		{
			var message = new Message (ServerHelper.Token, String.Empty, CurrentRoom, text);
			_messagesViewAdapter.Add (message);
			_allMessages.Add (message);

			ServerHelper.PostAsync (message.ToServerPostString(UserName));
		}

		private void AddMessages(List<string[]> messages)
		{
			var currentUserMessages = messages.Where(
				p => p[3] == UserName // private messages 
				|| p[3] == String.Empty // common messages 
			);
			var messagesToAdd = currentUserMessages
				.Select (p => new Message (p));

			_allMessages.AddRange (messagesToAdd);

			IEnumerable<Message> messagesToShow;
			if (CurrentRoom == String.Empty)
				messagesToShow = messagesToAdd.Where (p => p.To == String.Empty);
			else
				messagesToShow = messagesToAdd.Where (p => p.From == CurrentRoom && p.To == UserName);

			_messagesViewAdapter.AddAll(messagesToShow.ToList());
		}

		public void AddUsers(List<string> users) 
		{
			users.Remove (UserName);
			_usersViewAdapter.AddAll (users.Select(p => new User(p)).ToList());
		}

		private void RemoveUsers (List<string> usersToRemove)
		{
			usersToRemove.ForEach (p => 
				{ 
					var user = _usersViewAdapter.Data.FirstOrDefault(u => u.Name == p);
					_usersViewAdapter.Remove(user); 
				});
		}

		private void UpdateFromServer (IEnumerable<IGrouping<string, string[]>> groups)
		{
			foreach (IGrouping<string, string[]> items in groups)
            {
				if (items.Key == "message") 
				{
					foreach (string[] messageSplited in items) {
						var message = _allMessages.FirstOrDefault (p => p.Id.ToString() == messageSplited [2]);
						if (message != null) 
                        {
							message.Body = messageSplited [3];
							_messagesViewAdapter.Update (message);
						}
					}
				} 
				else if (items.Key == "username")
				{
					//TODO add full implementation when user change name: update view and notify other users
				}
			}
		}

		public async Task JoinChatAsync ()
		{
			await ServerHelper.PostAsync (String.Join(":", "user", UserName));
			ServerHelper.GetAsync ();
		}

		public async Task LeaveChatAsync ()
		{
			await ServerHelper.PostAsync (String.Join (":", "leave", UserName));
		}
	}
}

