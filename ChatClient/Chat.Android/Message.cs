using System;

namespace Chat.Core
{
	public class Message
	{
		public string From { get; set; }
		public string To { get; set; }
		public string Body { get; set; }

		public Message ()
		{
		}

		//{"message", from, to, body} 
		//to empty when message is broadcast
		//frrom empty when it is your message
		public Message (string[] arrayMessage) 
			: this(arrayMessage[1], arrayMessage[2], arrayMessage[3])
		{
		}

		public Message (string from, string to, string body)
		{
			this.From = from;
			this.To = to;
			this.Body = body;
		}

		public override string ToString ()
		{
			string from = From == String.Empty ? "You" : From;
			return String.Join(": ", from, Body);
		}
		
	}
}

