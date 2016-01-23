using System;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
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
			
			Client = new ImapClient (Server, Port, User, Pass, AuthMethod.Auto);
			if (Client.Authed) {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine ("IMAP:: Connected to {0} via Port {1}", Server, Port);
				Console.ForegroundColor = ConsoleColor.White;

				if (Client.Supports ("IDLE") == false) {
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine ("IMAP:: Server does not support IMAP IDLE");
					Console.ForegroundColor = ConsoleColor.White;
					return;
				}

				// Setup event handlers.
				Client.NewMessage += new EventHandler<IdleMessageEventArgs> (OnNewMessage);

				Client.IdleError += client_IdleError;
			}
		}

		static void client_IdleError (object sender, IdleErrorEventArgs e)
		{
			Console.Write ("An error occurred while idling: ");
			Console.WriteLine (e.Exception.Message);

			reconnectEvent.Set ();
		}

		static async void OnNewMessage (object sender, IdleMessageEventArgs e)
		{
			MailMessage Message = Client.GetMessage (e.MessageUID);
			if (Message.Body.Contains ("Angriffswarnung")) {
				await Task.Run (() => Irc.FilterEmail (Message));
			}
		}

	}
}