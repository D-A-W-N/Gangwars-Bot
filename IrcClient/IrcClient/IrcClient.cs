using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace GangwarsBot
{
	public class IrcClient
	{
		private string Server{ get; set; }

		private int Port{ get; set; }

		private string User{ get; set; }

		private string Nick{ get; set; }

		private string DefaultChannel{ get; set; }

		private TcpClient Client;
		private NetworkStream Stream;
		private StreamReader Reader;
		private StreamWriter Writer;


		public IrcClient ()
		{
		}

		public void Connect ()
		{
			Client = new TcpClient ();
			Client.Connect (Server, Port);
			if (Client.Connected) {
				Stream = Client.GetStream ();
				Reader = new StreamReader (Stream);
				Writer = new StreamWriter (Stream);
			}
		}

		public void Dispose ()
		{
			
		}

		private void SendAuth ()
		{
			SendResponse ("USER " + User + " 0 * " + User, true);
			SendResponse ("NICK " + Nick, true);
		}

		private bool CheckHost (string Host)
		{
			return true;
		}

		private void ParseChannelCommand (string ChannelCommand, string Channel)
		{
			
		}

		private void ReadResponse ()
		{
			string ReadLine = null;
			while (true) {
				while ((ReadLine = Reader.ReadLine ()) != null) {
					Console.WriteLine ("IN: " + ReadLine);
					string[] LineSplit = ReadLine.Split (new Char[] { ' ' });

					if (LineSplit [0] == "PING") {
						SendResponse ("PONG " + LineSplit [1], true);
					}

					switch (LineSplit [1]) {
					case "":
						break;
					}
				}
			}
		}

		private void SendResponse (string Output, bool Cmd = null, string Channel = null)
		{
			
		}
	}
}

