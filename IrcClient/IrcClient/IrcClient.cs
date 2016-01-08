using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
				row ["Host"] = "Dawn.quakenet.org";

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
			return true;
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

		private void ParseChannelCommand (string ChannelCommand, string Channel)
		{
			
		}

		private void ReadResponse ()
		{
			string ReadedLine;
			while (true) {
				while ((ReadedLine = Reader.ReadLine ()) != null) {
					Console.WriteLine ("IN: " + ReadedLine);
					string[] LineSplit = ReadedLine.Split (new Char[] { ' ' });

					if (LineSplit [0] == "PING") {
						SendResponse ("PONG " + LineSplit [1], true);
					}

					switch (LineSplit [1]) {
					case "221":
						JoinChannel ();
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
			Console.WriteLine ("OUT: " + OutLine);
			Writer.WriteLine (OutLine);
			Writer.Flush ();
		}
	}
}

