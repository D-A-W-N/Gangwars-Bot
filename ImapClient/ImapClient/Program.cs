using System;
using System.Threading;
using System.Threading.Tasks;
using GangwarsBot;

namespace GangwarsBot
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			try {
				ImapListener imap = new ImapListener ();
				imap.Server = "imap.1blu.de";
				imap.Port = 143;
				imap.User = "z233474_0-catchall";
				imap.Pass = "catchall";

				imap.Connect ();
			} catch (Exception e) {
				Console.WriteLine (e.Message);
			}

		}
	}
}
