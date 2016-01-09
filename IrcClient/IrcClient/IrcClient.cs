using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace GangwarsBot
{
	class Colors
	{
		public static char NORMAL = (char)15;
		public static char BOLD = (char)2;
		public static char UNDERLINE = (char)31;
		public static char REVERSE = (char)22;
		public static string WHITE = (char)3 + "00,1";
		public static string BLACK = (char)3 + "01,0";
		public static string DARK_BLUE = (char)3 + "02";
		public static string DARK_GREEN = (char)3 + "03";
		public static string RED = (char)3 + "04";
		public static string BROWN = (char)3 + "05";
		public static string PURPLE = (char)3 + "06";
		public static string OLIVE = (char)3 + "07";
		public static string YELLOW = (char)3 + "08";
		public static string GREEN = (char)3 + "09";
		public static string TEAL = (char)3 + "10";
		public static string CYAN = (char)3 + "11";
		public static string BLUE = (char)3 + "12";
		public static string MAGENTA = (char)3 + "13";
		public static string DARK_GRAY = (char)3 + "14";
		public static string LIGHT_GRAY = (char)3 + "15";
	}

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

		private DataTable getChannels (string Path)
		{
			DataTable ChannelTable = new DataTable ("ChannelEntrys");

			if (File.Exists (Path)) {
				ChannelTable.ReadXml (Path);
			} else {
				// Datei existiert noch nicht - Erstellen der Spalten
				ChannelTable.Columns.Add ("Channel", typeof(string));
				ChannelTable.Columns.Add ("Key", typeof(string));

				//Anlegen der default-Werte
				DataRow row = ChannelTable.NewRow ();
				row ["Channel"] = DefaultChannel;
				row ["Key"] = "";

				ChannelTable.Rows.Add (row);

				ChannelTable.WriteXml (Path, XmlWriteMode.WriteSchema);
			}

			return ChannelTable;
		}

		private DataTable getEntrys (string Path)
		{
			DataTable Table = new DataTable ();
			Table.ReadXml (Path);
			return Table;
		}



		private void setEntrys (string Path, string Column1, string Value1, string Column2 = null, string Value2 = null)
		{
			DataTable Table = getEntrys (Path);
			DataRow row = Table.NewRow ();
			row [Column1] = Value1;
			if (Column2 != null && Value2 != null) {
				row [Column2] = Value2;
			}
			Table.Rows.Add (row);
			Table.WriteXml (Path, XmlWriteMode.WriteSchema);
		}

		private void deleteEntrys (string Path, string Column, string Value)
		{
			DataTable Table = getEntrys (Path);
			int i = 0;
			Table.AcceptChanges ();
			foreach (DataRow row in Table.Rows) {
				
				if (row [Column].ToString () == Value) {
					Table.Rows [i].Delete ();
				}
				i++;
			}
			Table.AcceptChanges ();
			Table.WriteXml (Path, XmlWriteMode.WriteSchema);
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

		private bool CheckChannel (string Path, string Channel)
		{
			DataTable ChannelTable = getChannels ("Channels.xml");

			foreach (DataRow row in ChannelTable.Rows) {
				if (row ["Channel"].ToString () == Channel) {
					return true;
				}
			}
			return false;
		}

		private void JoinChannel (string Channel = null, string Key = null)
		{
			string OutLine;
			if (Channel == null) {
				DataTable ChannelTable = getChannels ("Channels.xml");

				foreach (DataRow row in ChannelTable.Rows) {
					if (!String.IsNullOrEmpty (row ["Key"].ToString ())) {
						OutLine = "JOIN " + row ["Channel"].ToString () + " " + row ["Key"].ToString ();
					} else {
						OutLine = "JOIN " + row ["Channel"].ToString ();
					}

					SendResponse (OutLine, true);
				}

			} else {
				if (Key == null) {
					OutLine = "JOIN " + Channel;
					setEntrys ("Channels.xml", "Channel", Channel);
				} else {
					OutLine = "JOIN " + Channel + " " + Key;
					setEntrys ("Channels.xml", "Channel", Channel, "Key", Key);
				}
				SendResponse (OutLine, true);

			}

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

		public async void FilterEmail (MailMessage Message, IrcClient irc)
		{
			await Task.Run (() => irc.SendResponse ("New Mail", false, "#test.news"));
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

					switch (LineSplit [1]) {
					case "376":
						JoinChannel ();
						break;
					case "366":
						SendResponse (LineSplit [3] + " betretten.", false, CommandChannel);
						break;
					case "475":
						SendResponse (MessageSplit [1], false, CommandChannel);
						break;
					case "PART":
						deleteEntrys ("Channels.xml", "Channel", LineSplit [2]);
						SendResponse (LineSplit [2] + " verlassen.", false, CommandChannel);
						break;
					case "PRIVMSG":
						Match m = Regex.Match (LineSplit [0], @"\:.*\@(.*)");
						string Host = m.Groups [1].Value;

						if (CheckHost (Host)) {
							string ChannelMessage = MessageSplit [1].Trim (new Char[] { ':' });
							CommandChannel = LineSplit [2];
							ParseChannelCommand (ChannelMessage, CommandChannel);
						}
						break;
					}
				}
			}
		}

		private async void SendResponse (string Output, bool Cmd = false, string Channel = null)
		{
			string OutLine;
			if (Cmd) {
				OutLine = Output;
			} else {
				if (Channel == null) {
					Channel = DefaultChannel;
				}
				OutLine = "PRIVMSG " + Channel + " :" + Colors.BLACK + " " + Output + " ";
			}
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine ("OUT: " + OutLine);
			Console.ForegroundColor = ConsoleColor.White;
			Writer.WriteLine (OutLine);
			Writer.Flush ();
			await Task.Delay (1000);
		}
	}
}

