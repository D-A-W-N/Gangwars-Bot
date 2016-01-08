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
				IrcClient irc = new IrcClient ();
				irc.Server = "azubu.jp.quakenet.org";
				irc.Port = 6669;
				irc.User = "PBot";
				irc.Nick = "[D]P-Bot";
				irc.DefaultChannel = "#test.news";

				irc.Connect ();
			} catch (Exception e) {
				Console.WriteLine (e.Message);
			}

		}
	}
}
