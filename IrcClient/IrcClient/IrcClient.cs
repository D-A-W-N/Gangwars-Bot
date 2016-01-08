using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace GangwarsBot
{
	public class IrcClient
	{
		public string Server{ get; set; }

		public int Port{ get; set; }

		public string User{ get; set; }

		public string Nick{ get; set; }

		public string DefaultChannel{ get; set; }

		private TcpClient Client;
		private StreamReader Reader;
		private StreamWriter Writer;

		private DataTable getHostEntrys (string Path)
		{
			DataTable HostTable = new DataTable ("HostEntrys");

			if (File.Exists (Path)) {
				HostTable.ReadXml (Path);
			} else {
				// Datei existiert noch nicht - Erstellen der Spalten
				HostTable.Columns.Add ("Host", typeof(string));

				//Anlegen der default-Werte
				DataRow row = HostTable.NewRow ();
				row ["Host"] = "Dawn.users.quakenet.org";

				HostTable.Rows.Add (row);

				HostTable.WriteXml (Path, XmlWriteMode.WriteSchema);
			}

			return HostTable;
		}


		public IrcClient ()
		{
		}

		public void Connect ()
		{
			try {
				Client = new TcpClient ();
				Client.Connect (Server, Port);
				Reader = new StreamReader (Client.GetStream ());
				Writer = new StreamWriter (Client.GetStream ());
				SendAuth ();
				ReadResponse ();

			} catch (Exception e) {
				Console.WriteLine (e.Message);
			}
		}

		public void Dispose ()
		{
			Writer.Dispose ();
			Reader.Dispose ();
			Client.Client.Dispose ();
		}

		private void SendAuth ()
		{
			SendResponse ("USER " + User + " 0 * " + User, true);
			SendResponse ("NICK " + Nick, true);
		}

		private bool CheckHost (string Host)
		{
			DataTable HostTable = getHostEntrys ("Hosts.xml");

			foreach (DataRow row in HostTable.Rows) {
				if (row ["Host"].ToString () == Host) {
					return true;
				}
			}
			return false;
		}

		private void JoinChannel (string Channel = null, string Key = null)
		{
			string OutLine;
			if (Channel == null) {
				OutLine = "JOIN " + DefaultChannel;
			} else {
				if (Key == null) {
					OutLine = "JOIN " + Channel;
				} else {
					OutLine = "JOIN " + Channel + " " + Key;
				}
			}
			SendResponse (OutLine, true);
		}

		private void PartChannel (string Channel)
		{
			string OutLine;
			OutLine = "PART " + Channel;
			SendResponse (OutLine, true);
		}

		private void ParseChannelCommand (string ChannelMessage, string Channel)
		{
			string[] CommandSplit = ChannelMessage.Split (new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			string arg1 = null;
			string arg2 = null;
			if (CommandSplit.Length > 1) {
				arg1 = CommandSplit [1];
				if (CommandSplit.Length > 2) {
					arg2 = CommandSplit [2];
				}
			}

			switch (CommandSplit [0]) {
			case "!join":
				if (arg1 != null) {
					if (arg2 != null) {
						JoinChannel (arg1, arg2);
					} else {
						JoinChannel (arg1);
					}
				} else {
					SendResponse ("Nicht genug Argumente. !join <Channel> <?Key>", false, Channel);
				}
				break;
			case "!part":
				if (arg1 != null) {
					PartChannel (arg1);
				} else {
					SendResponse ("Nicht genug Argumente. !part <Channel>", false, Channel);
				}
				break;
			}
		}

		private void ReadResponse ()
		{
			string ReadedLine;
			while (true) {
				while ((ReadedLine = Reader.ReadLine ()) != null) {
					string CommandChannel = DefaultChannel;
					Console.WriteLine ("IN: " + ReadedLine);

					string[] LineSplit = ReadedLine.Split (new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
					string[] MessageSplit = ReadedLine.Split (new Char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);


					if (LineSplit [0] == "PING") {
						SendResponse ("PONG " + LineSplit [1], true);
						continue;
					}
					if (LineSplit [1] == "PRIVMSG") {
						Match m = Regex.Match (LineSplit [0], @"\:.*\@(.*)");
						string Host = m.Groups [1].Value;

						if (CheckHost (Host)) {
							string ChannelMessage = MessageSplit [1].Trim (new Char[] { ':' });
							CommandChannel = LineSplit [2];
							ParseChannelCommand (ChannelMessage, CommandChannel);
						}
					}

					switch (LineSplit [1]) {
					case "376":
						JoinChannel ();
						break;
					case "475":
						SendResponse (MessageSplit [1], false, CommandChannel);
						break;
					}
						
				}
			}
		}

		private void SendResponse (string Output, bool Cmd = false, string Channel = null)
		{
			string OutLine;
			if (Cmd) {
				OutLine = Output;
			} else {
				if (Channel == null) {
					Channel = DefaultChannel;
				}
				OutLine = "PRIVMSG " + Channel + " :" + Output;
			}
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine ("OUT: " + OutLine);
			Console.ForegroundColor = ConsoleColor.White;
			Writer.WriteLine (OutLine);
			Writer.Flush ();
		}
	}
}

