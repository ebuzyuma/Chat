using System;

namespace Chat.Core
{
	public class Message : IItemViewModel
	{
		public int Id { get; set; }
		public string From { get; set; }
		public string To { get; set; }
		public string Body { get; set; }

		public Message ()
		{
		}

		//{"message", id, from, to, body} 
		//to empty when message is broadcast
		//from empty when it is your message
		public Message (string[] arrayMessage) 
			: this(Convert.ToInt32(arrayMessage[1]), arrayMessage[2], arrayMessage[3], arrayMessage[4])
		{
		}

		public Message (int id, string from, string to, string body)
		{
			this.Id = id;
			this.From = from;
			this.To = to;
			this.Body = body;
		}

		public override string ToString ()
		{
			string from = From == String.Empty ? "You" : From;
			return String.Join(": ", from, Body);
		}
	
		public string ToServerPostString (string currentUserName)
		{
			return String.Join (":", "message", Id, currentUserName, To, Body);
		}

		public string ToServerUpdateString ()
		{
			return String.Join (":", "update:message", Id, Body);
		}

		public void MapFrom (IItemViewModel old)
		{
			Body = ((Message)old).Body;
		}
	}
}

