using System;
using System.Collections.Generic;
using System.Threading;
using GangwarsBot;

namespace GangwarsBot
{
	class MainClass
	{

		public static void Main (string[] args)
		{
			try {
				Thread[] tr = new Thread[2];

				IrcClient irc = new IrcClient ();
				irc.Server = "underworld2.no.quakenet.org";
				irc.Port = 6668;
				irc.User = "PBot";
				irc.Nick = "[D]P-Bot";
				irc.DefaultChannel = "#test.news";
	
				tr [0] = new Thread (new ThreadStart (irc.Connect));
				tr [0].Name = String.Format ("IRC");
				tr [0].IsBackground = false;

				ImapListener imap = new ImapListener (irc);
				imap.Server = "localhost";
				imap.Port = 143;
				imap.User = "user";
				imap.Pass = "passwort";

				tr [1] = new Thread (new ThreadStart (imap.Connect));
				tr [1].Name = String.Format ("IMAP");
				tr [1].IsBackground = false;

				foreach (Thread x in tr) {
					x.Start ();
				}
			} catch (Exception e) {
				Console.WriteLine (e.Message);
			}
		}
	}
}
