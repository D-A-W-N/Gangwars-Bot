using System;
using System.Threading;
using S22.Imap;

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

		public ImapListener ()
		{
		}

		public void Connect ()
		{
			try {
				while (true) {
					Console.Write ("Connecting...");
					InitializeClient ();
					Console.WriteLine ("OK");

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
			// Setup event handlers.
			Client.NewMessage += client_NewMessage;
			Client.IdleError += client_IdleError;
		}

		static void client_IdleError (object sender, IdleErrorEventArgs e)
		{
			Console.Write ("An error occurred while idling: ");
			Console.WriteLine (e.Exception.Message);

			reconnectEvent.Set ();
		}

		static void client_NewMessage (object sender, IdleMessageEventArgs e)
		{
			Console.WriteLine ("Got a new message, uid = " + e.MessageUID);
		}
	}
}

