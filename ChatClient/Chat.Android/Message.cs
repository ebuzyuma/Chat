using System;

namespace Chat.Core
{
	public class Message
	{
		public string From { get; set; }
		public bool IsPrivate { get; set; }
		public string Body { get; set; }

		public Message ()
		{
		}

		//{"message", from, to, body}, to empty when message is broadcast
		public Message (string[] arrayMessage) 
			: this(arrayMessage[1], arrayMessage[3], isPrivate: arrayMessage[2] != String.Empty)
		{
		}

		public Message (string from, string body, bool isPrivate = false)
		{
			this.From = from;
			this.Body = body;
			this.IsPrivate = isPrivate;
		}

		public override string ToString ()
		{
			return String.Join(":", From, Body);
		}
		
	}
}

