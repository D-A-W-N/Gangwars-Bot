using System;
using System.Net.Mail;
using System.Threading;
using S22.Imap;
using GangwarsBot;

namespace GangwarsBot
{
	public class ImapListener
	{
		public string Server{ get; set; }

		public int Port{ get; set; }

		public string User{ get; set; }

		public string Pass{ get; set; }

		static AutoResetEvent reconnectEvent = new AutoResetEvent (false);
		static ImapClient Client;
		static IrcClient Irc;

		public ImapListener (IrcClient irc)
		{
			Irc = irc;
		}

		public void Connect ()
		{
			try {
				while (true) {
					InitializeClient ();

					reconnectEvent.WaitOne ();
				}
			} finally {
				if (Client != null)
					Client.Dispose ();
			}
		}

		private void InitializeClient ()
		{
			// Dispose of existing instance, if any.
			if (Client != null)
				Client.Dispose ();
			Client = new ImapClient (Server, Port, User, Pass, AuthMethod.Login);
			if (Client.Authed) {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine ("IMAP:: Connected to {0} via Port {1}", Server, Port);
				Console.ForegroundColor = ConsoleColor.White;
				// Setup event handlers.
				Client.NewMessage += client_NewMessage;
				Client.IdleError += client_IdleError;
			}
		}

		static void client_IdleError (object sender, IdleErrorEventArgs e)
		{
			Console.Write ("An error occurred while idling: ");
			Console.WriteLine (e.Exception.Message);

			reconnectEvent.Set ();
		}

		static void client_NewMessage (object sender, IdleMessageEventArgs e)
		{
			MailMessage Message = Client.GetMessage (e.MessageUID);
			if (Message.Body.Contains ("Angriffswarnung")) {
				IrcClient.FilterEmail (Message, Irc);
			}
		}
			
	}
}

