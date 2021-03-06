﻿/*
 * This file is part of Schumix.
 * 
 * Copyright (C) 2010-2012 Megax <http://www.megaxx.info/>
 * Copyright (C) 2012 Jackneill
 * 
 * Schumix is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * Schumix is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with Schumix.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using Schumix.Framework.Extensions;
using Schumix.Framework.Localization;

namespace Schumix.Framework.Config
{
	public sealed class XmlConfig : DefaultConfig
	{
		private readonly LocalizationConsole sLConsole = Singleton<LocalizationConsole>.Instance;
		private readonly Utilities sUtilities = Singleton<Utilities>.Instance;

		public XmlConfig()
		{
		}

		public XmlConfig(string configdir, string configfile, bool colorbindmode)
		{
			var xmldoc = new XmlDocument();
			xmldoc.Load(sUtilities.DirectoryToSpecial(configdir, configfile));

			string LogFileName = !xmldoc.SelectSingleNode("Schumix/Log/FileName").IsNull() ? xmldoc.SelectSingleNode("Schumix/Log/FileName").InnerText : d_logfilename;
			bool LogDateFileName = !xmldoc.SelectSingleNode("Schumix/Log/DateFileName").IsNull() ? Convert.ToBoolean(xmldoc.SelectSingleNode("Schumix/Log/DateFileName").InnerText) : d_logdatefilename;
			int LogMaxFileSize = !xmldoc.SelectSingleNode("Schumix/Log/MaxFileSize").IsNull() ? Convert.ToInt32(xmldoc.SelectSingleNode("Schumix/Log/MaxFileSize").InnerText) : d_logmaxfilesize;
			int LogLevel = !xmldoc.SelectSingleNode("Schumix/Log/LogLevel").IsNull() ? Convert.ToInt32(xmldoc.SelectSingleNode("Schumix/Log/LogLevel").InnerText) : d_loglevel;
			string LogDirectory = !xmldoc.SelectSingleNode("Schumix/Log/LogDirectory").IsNull() ? xmldoc.SelectSingleNode("Schumix/Log/LogDirectory").InnerText : d_logdirectory;
			string IrcLogDirectory = !xmldoc.SelectSingleNode("Schumix/Log/IrcLogDirectory").IsNull() ? xmldoc.SelectSingleNode("Schumix/Log/IrcLogDirectory").InnerText : d_irclogdirectory;
			bool IrcLog = !xmldoc.SelectSingleNode("Schumix/Log/IrcLog").IsNull() ? Convert.ToBoolean(xmldoc.SelectSingleNode("Schumix/Log/IrcLog").InnerText) : d_irclog;

			new LogConfig(LogFileName, LogDateFileName, LogMaxFileSize, LogLevel, sUtilities.GetSpecialDirectory(LogDirectory), sUtilities.GetSpecialDirectory(IrcLogDirectory), IrcLog);

			Log.Initialize(LogFileName, colorbindmode);
			Log.Debug("XmlConfig", ">> {0}", configfile);

			Log.Notice("XmlConfig", sLConsole.Config("Text3"));
			bool ServerEnabled = !xmldoc.SelectSingleNode("Schumix/Server/Enabled").IsNull() ? Convert.ToBoolean(xmldoc.SelectSingleNode("Schumix/Server/Enabled").InnerText) : d_serverenabled;
			string ServerHost = !xmldoc.SelectSingleNode("Schumix/Server/Host").IsNull() ? xmldoc.SelectSingleNode("Schumix/Server/Host").InnerText : d_serverhost;
			int ServerPort = !xmldoc.SelectSingleNode("Schumix/Server/Port").IsNull() ? Convert.ToInt32(xmldoc.SelectSingleNode("Schumix/Server/Port").InnerText) : d_serverport;
			string ServerPassword = !xmldoc.SelectSingleNode("Schumix/Server/Password").IsNull() ? xmldoc.SelectSingleNode("Schumix/Server/Password").InnerText : d_serverpassword;

			new ServerConfig(ServerEnabled, ServerHost, ServerPort, ServerPassword);

			int ServerId = 1;
			var xmlirclist = xmldoc.SelectNodes("Schumix/Irc");
			var IrcList = new Dictionary<string, IRCConfigBase>();

			if(xmlirclist.Count == 0)
			{
				string ServerName            = d_servername;
				string Server                = d_server;
				int Port                     = d_port;
				bool Ssl                     = d_ssl;
				string NickName              = d_nickname;
				string NickName2             = d_nickname2;
				string NickName3             = d_nickname3;
				string UserName              = d_username;
				string UserInfo              = d_userinfo;
				string MasterChannel         = d_masterchannel;
				string MasterChannelPassword = d_masterchannelpassword;
				string IgnoreChannels        = d_ignorechannels;
				string IgnoreNames           = d_ignorenames;
				bool UseNickServ             = d_usenickserv;
				string NickServPassword      = d_nickservpassword;
				bool UseHostServ             = d_usehostserv;
				bool HostServStatus          = d_hostservstatus;
				int MessageSending           = d_messagesending;
				string CommandPrefix         = d_commandprefix;
				string MessageType           = d_messagetype;

				IrcList.Add(ServerName.ToLower(), new IRCConfigBase(ServerId, Server, Port, Ssl, NickName, NickName2, NickName3, UserName, UserInfo, MasterChannel, MasterChannelPassword.Trim(), IgnoreChannels, IgnoreNames, UseNickServ, NickServPassword, UseHostServ, HostServStatus, MessageSending, CommandPrefix, MessageType));
			}
			else
			{
				foreach(XmlNode xn in xmlirclist)
				{
					string ServerName = !xn["ServerName"].IsNull() ? xn["ServerName"].InnerText : d_servername;
					string Server = !xn["Server"].IsNull() ? xn["Server"].InnerText : d_server;
					int Port = !xn["Port"].IsNull() ? Convert.ToInt32(xn["Port"].InnerText) : d_port;
					bool Ssl = !xn["Ssl"].IsNull() ? Convert.ToBoolean(xn["Ssl"].InnerText) : d_ssl;
					string NickName = !xn["NickName"].IsNull() ? xn["NickName"].InnerText : d_nickname;
					string NickName2 = !xn["NickName2"].IsNull() ? xn["NickName2"].InnerText : d_nickname2;
					string NickName3 = !xn["NickName3"].IsNull() ? xn["NickName3"].InnerText : d_nickname3;
					string UserName = !xn["UserName"].IsNull() ? xn["UserName"].InnerText : d_username;
					string UserInfo = !xn["UserInfo"].IsNull() ? xn["UserInfo"].InnerText : d_userinfo;
					string MasterChannel = !xn.SelectSingleNode("MasterChannel/Name").IsNull() ? xn.SelectSingleNode("MasterChannel/Name").InnerText : d_masterchannel;
					string MasterChannelPassword = !xn.SelectSingleNode("MasterChannel/Password").IsNull() ? xn.SelectSingleNode("MasterChannel/Password").InnerText : d_masterchannelpassword;
					string IgnoreChannels = !xn["IgnoreChannels"].IsNull() ? xn["IgnoreChannels"].InnerText : d_ignorechannels;
					string IgnoreNames = !xn["IgnoreNames"].IsNull() ? xn["IgnoreNames"].InnerText : d_ignorenames;
					bool UseNickServ = !xn.SelectSingleNode("NickServ/Enabled").IsNull() ? Convert.ToBoolean(xn.SelectSingleNode("NickServ/Enabled").InnerText) : d_usenickserv;
					string NickServPassword = !xn.SelectSingleNode("NickServ/Password").IsNull() ? xn.SelectSingleNode("NickServ/Password").InnerText : d_nickservpassword;
					bool UseHostServ = !xn.SelectSingleNode("HostServ/Enabled").IsNull() ? Convert.ToBoolean(xn.SelectSingleNode("HostServ/Enabled").InnerText) : d_usehostserv;
					bool HostServStatus = !xn.SelectSingleNode("HostServ/Vhost").IsNull() ? Convert.ToBoolean(xn.SelectSingleNode("HostServ/Vhost").InnerText) : d_hostservstatus;
					int MessageSending = !xn.SelectSingleNode("Wait/MessageSending").IsNull() ? Convert.ToInt32(xn.SelectSingleNode("Wait/MessageSending").InnerText) : d_messagesending;
					string CommandPrefix = !xn.SelectSingleNode("Command/Prefix").IsNull() ? xn.SelectSingleNode("Command/Prefix").InnerText : d_commandprefix;
					string MessageType = !xn["MessageType"].IsNull() ? xn["MessageType"].InnerText : d_messagetype;

					if(MasterChannel.Length >= 2 && MasterChannel.Trim().Length > 1 && MasterChannel.Substring(0, 1) != "#")
						MasterChannel = "#" + MasterChannel;
					else if(MasterChannel.Length < 2 && MasterChannel.Trim().Length <= 1)
						MasterChannel = d_masterchannel;

					if(IrcList.ContainsKey(ServerName.ToLower()))
						Log.Error("XmlConfig", sLConsole.Config("Text12"), ServerName);
					else
					{
						IrcList.Add(ServerName.ToLower(), new IRCConfigBase(ServerId, Server, Port, Ssl, NickName, NickName2, NickName3, UserName, UserInfo, MasterChannel, MasterChannelPassword.Trim(), IgnoreChannels, IgnoreNames, UseNickServ, NickServPassword, UseHostServ, HostServStatus, MessageSending, CommandPrefix, MessageType));
						ServerId++;
					}
				}

				new IRCConfig(IrcList);
			}

			bool Enabled = !xmldoc.SelectSingleNode("Schumix/MySql/Enabled").IsNull() ? Convert.ToBoolean(xmldoc.SelectSingleNode("Schumix/MySql/Enabled").InnerText) : d_mysqlenabled;
			string Host = !xmldoc.SelectSingleNode("Schumix/MySql/Host").IsNull() ? xmldoc.SelectSingleNode("Schumix/MySql/Host").InnerText : d_mysqlhost;
			string User = !xmldoc.SelectSingleNode("Schumix/MySql/User").IsNull() ? xmldoc.SelectSingleNode("Schumix/MySql/User").InnerText : d_mysqluser;
			string Password = !xmldoc.SelectSingleNode("Schumix/MySql/Password").IsNull() ? xmldoc.SelectSingleNode("Schumix/MySql/Password").InnerText : d_mysqlpassword;
			string Database = !xmldoc.SelectSingleNode("Schumix/MySql/Database").IsNull() ? xmldoc.SelectSingleNode("Schumix/MySql/Database").InnerText : d_mysqldatabase;
			string Charset = !xmldoc.SelectSingleNode("Schumix/MySql/Charset").IsNull() ? xmldoc.SelectSingleNode("Schumix/MySql/Charset").InnerText : d_mysqlcharset;

			new MySqlConfig(Enabled, Host, User, Password, Database, Charset);

			Enabled = !xmldoc.SelectSingleNode("Schumix/SQLite/Enabled").IsNull() ? Convert.ToBoolean(xmldoc.SelectSingleNode("Schumix/SQLite/Enabled").InnerText) : d_sqliteenabled;
			string FileName = !xmldoc.SelectSingleNode("Schumix/SQLite/FileName").IsNull() ? xmldoc.SelectSingleNode("Schumix/SQLite/FileName").InnerText : d_sqlitefilename;

			new SQLiteConfig(Enabled, sUtilities.GetSpecialDirectory(FileName));

			string Directory = !xmldoc.SelectSingleNode("Schumix/Crash/Directory").IsNull() ? xmldoc.SelectSingleNode("Schumix/Crash/Directory").InnerText : d_crashdirectory;

			new CrashConfig(sUtilities.GetSpecialDirectory(Directory));

			string Locale = !xmldoc.SelectSingleNode("Schumix/Localization/Locale").IsNull() ? xmldoc.SelectSingleNode("Schumix/Localization/Locale").InnerText : d_locale;

			new LocalizationConfig(Locale);

			int MaxMemory = !xmldoc.SelectSingleNode("Schumix/Shutdown/MaxMemory").IsNull() ? Convert.ToInt32(xmldoc.SelectSingleNode("Schumix/Shutdown/MaxMemory").InnerText) : d_shutdownmaxmemory;

			new ShutdownConfig(MaxMemory);

			bool Config = !xmldoc.SelectSingleNode("Schumix/Clean/Config").IsNull() ? Convert.ToBoolean(xmldoc.SelectSingleNode("Schumix/Clean/Config").InnerText) : d_cleanconfig;
			bool Database2 = !xmldoc.SelectSingleNode("Schumix/Clean/Database").IsNull() ? Convert.ToBoolean(xmldoc.SelectSingleNode("Schumix/Clean/Database").InnerText) : d_cleandatabase;

			new CleanConfig(Config, Database2);

			Log.Success("XmlConfig", sLConsole.Config("Text4"));
			Console.WriteLine();
		}

		~XmlConfig()
		{
		}

		public bool CreateConfig(string ConfigDirectory, string ConfigFile, bool ColorBindMode)
		{
			try
			{
				string filename = sUtilities.DirectoryToSpecial(ConfigDirectory, ConfigFile);

				if(File.Exists(filename))
					return true;
				else
				{
					new LogConfig(d_logfilename, d_logdatefilename, d_logmaxfilesize, 3, d_logdirectory, d_irclogdirectory, d_irclog);
					Log.Initialize(d_logfilename, ColorBindMode);
					Log.Error("XmlConfig", sLConsole.Config("Text5"));
					Log.Debug("XmlConfig", sLConsole.Config("Text6"));
					var w = new XmlTextWriter(filename, null);
					var xmldoc = new XmlDocument();
					string filename2 = sUtilities.DirectoryToSpecial(ConfigDirectory, "_" + ConfigFile);

					if(File.Exists(filename2))
						xmldoc.Load(filename2);

					try
					{
						w.Formatting = Formatting.Indented;
						w.Indentation = 4;
						w.Namespaces = false;
						w.WriteStartDocument();

						// <Schumix>
						w.WriteStartElement("Schumix");

						// <Server>
						w.WriteStartElement("Server");
						w.WriteElementString("Enabled",          (!xmldoc.SelectSingleNode("Schumix/Server/Enabled").IsNull() ? xmldoc.SelectSingleNode("Schumix/Server/Enabled").InnerText : d_serverenabled.ToString()));
						w.WriteElementString("Host",             (!xmldoc.SelectSingleNode("Schumix/Server/Host").IsNull() ? xmldoc.SelectSingleNode("Schumix/Server/Host").InnerText : d_serverhost));
						w.WriteElementString("Port",             (!xmldoc.SelectSingleNode("Schumix/Server/Port").IsNull() ? xmldoc.SelectSingleNode("Schumix/Server/Port").InnerText : d_serverport.ToString()));
						w.WriteElementString("Password",         (!xmldoc.SelectSingleNode("Schumix/Server/Password").IsNull() ? xmldoc.SelectSingleNode("Schumix/Server/Password").InnerText : d_serverpassword));

						// </Server>
						w.WriteEndElement();

						var xmlirclist = xmldoc.SelectNodes("Schumix/Irc");

						if(xmlirclist.Count == 0)
						{
							// <Irc>
							w.WriteStartElement("Irc");
							w.WriteElementString("ServerName",      d_servername);
							w.WriteElementString("Server",          d_server);
							w.WriteElementString("Port",            d_port.ToString());
							w.WriteElementString("Ssl",             d_ssl.ToString());
							w.WriteElementString("NickName",        d_nickname);
							w.WriteElementString("NickName2",       d_nickname2);
							w.WriteElementString("NickName3",       d_nickname3);
							w.WriteElementString("UserName",        d_username);
							w.WriteElementString("UserInfo",        d_userinfo);

							// <MasterChannel>
							w.WriteStartElement("MasterChannel");
							w.WriteElementString("Name",            d_masterchannel);
							w.WriteElementString("Password",        d_masterchannelpassword);

							// </MasterChannel>
							w.WriteEndElement();

							w.WriteElementString("IgnoreChannels",  d_ignorechannels);
							w.WriteElementString("IgnoreNames",     d_ignorenames);

							// <NickServ>
							w.WriteStartElement("NickServ");
							w.WriteElementString("Enabled",         d_usenickserv.ToString());
							w.WriteElementString("Password",        d_nickservpassword);

							// </NickServ>
							w.WriteEndElement();

							// <HostServ>
							w.WriteStartElement("HostServ");
							w.WriteElementString("Enabled",         d_usehostserv.ToString());
							w.WriteElementString("Vhost",           d_hostservstatus.ToString());

							// </HostServ>
							w.WriteEndElement();

							// <Wait>
							w.WriteStartElement("Wait");
							w.WriteElementString("MessageSending",  d_messagesending.ToString());

							// </Wait>
							w.WriteEndElement();

							// <Command>
							w.WriteStartElement("Command");
							w.WriteElementString("Prefix",          d_commandprefix);

							// </Command>
							w.WriteEndElement();

							w.WriteElementString("MessageType",     d_messagetype);

							// </Irc>
							w.WriteEndElement();
						}
						else
						{
							foreach(XmlNode xn in xmlirclist)
							{
								// <Irc>
								w.WriteStartElement("Irc");
								w.WriteElementString("ServerName",      (!xn["ServerName"].IsNull() ? xn["ServerName"].InnerText : d_servername));
								w.WriteElementString("Server",          (!xn["Server"].IsNull() ? xn["Server"].InnerText : d_server));
								w.WriteElementString("Port",            (!xn["Port"].IsNull() ? xn["Port"].InnerText : d_port.ToString()));
								w.WriteElementString("Ssl",             (!xn["Ssl"].IsNull() ? xn["Ssl"].InnerText : d_ssl.ToString()));
								w.WriteElementString("NickName",        (!xn["NickName"].IsNull() ? xn["NickName"].InnerText : d_nickname));
								w.WriteElementString("NickName2",       (!xn["NickName2"].IsNull() ? xn["NickName2"].InnerText : d_nickname2));
								w.WriteElementString("NickName3",       (!xn["NickName3"].IsNull() ? xn["NickName3"].InnerText : d_nickname3));
								w.WriteElementString("UserName",        (!xn["UserName"].IsNull() ? xn["UserName"].InnerText : d_username));
								w.WriteElementString("UserInfo",        (!xn["UserInfo"].IsNull() ? xn["UserInfo"].InnerText : d_userinfo));

								// <MasterChannel>
								w.WriteStartElement("MasterChannel");
								w.WriteElementString("Name",            (!xn.SelectSingleNode("MasterChannel/Name").IsNull() ? xn.SelectSingleNode("MasterChannel/Name").InnerText : d_masterchannel));
								w.WriteElementString("Password",        (!xn.SelectSingleNode("MasterChannel/Password").IsNull() ? xn.SelectSingleNode("MasterChannel/Password").InnerText : d_masterchannelpassword));

								// </MasterChannel>
								w.WriteEndElement();

								w.WriteElementString("IgnoreChannels",  (!xn["IgnoreChannels"].IsNull() ? xn["IgnoreChannels"].InnerText : d_ignorechannels));
								w.WriteElementString("IgnoreNames",     (!xn["IgnoreNames"].IsNull() ? xn["IgnoreNames"].InnerText : d_ignorenames));

								// <NickServ>
								w.WriteStartElement("NickServ");
								w.WriteElementString("Enabled",         (!xn.SelectSingleNode("NickServ/Enabled").IsNull() ? xn.SelectSingleNode("NickServ/Enabled").InnerText : d_usenickserv.ToString()));
								w.WriteElementString("Password",        (!xn.SelectSingleNode("NickServ/Password").IsNull() ? xn.SelectSingleNode("NickServ/Password").InnerText : d_nickservpassword));

								// </NickServ>
								w.WriteEndElement();

								// <HostServ>
								w.WriteStartElement("HostServ");
								w.WriteElementString("Enabled",         (!xn.SelectSingleNode("HostServ/Enabled").IsNull() ? xn.SelectSingleNode("HostServ/Enabled").InnerText : d_usehostserv.ToString()));
								w.WriteElementString("Vhost",           (!xn.SelectSingleNode("HostServ/Vhost").IsNull() ? xn.SelectSingleNode("HostServ/Vhost").InnerText : d_hostservstatus.ToString()));

								// </HostServ>
								w.WriteEndElement();

								// <Wait>
								w.WriteStartElement("Wait");
								w.WriteElementString("MessageSending",  (!xn.SelectSingleNode("Wait/MessageSending").IsNull() ? xn.SelectSingleNode("Wait/MessageSending").InnerText : d_messagesending.ToString()));

								// </Wait>
								w.WriteEndElement();

								// <Command>
								w.WriteStartElement("Command");
								w.WriteElementString("Prefix",          (!xn.SelectSingleNode("Command/Prefix").IsNull() ? xn.SelectSingleNode("Command/Prefix").InnerText : d_commandprefix));

								// </Command>
								w.WriteEndElement();

								w.WriteElementString("MessageType",     (!xn["MessageType"].IsNull() ? xn["MessageType"].InnerText : d_messagetype));

								// </Irc>
								w.WriteEndElement();
							}
						}

						// <Log>
						w.WriteStartElement("Log");
						w.WriteElementString("FileName",         (!xmldoc.SelectSingleNode("Schumix/Log/FileName").IsNull() ? xmldoc.SelectSingleNode("Schumix/Log/FileName").InnerText : d_logfilename));
						w.WriteElementString("DateFileName",     (!xmldoc.SelectSingleNode("Schumix/Log/DateFileName").IsNull() ? xmldoc.SelectSingleNode("Schumix/Log/DateFileName").InnerText : d_logdatefilename.ToString()));
						w.WriteElementString("MaxFileSize",      (!xmldoc.SelectSingleNode("Schumix/Log/MaxFileSize").IsNull() ? xmldoc.SelectSingleNode("Schumix/Log/MaxFileSize").InnerText : d_logmaxfilesize.ToString()));
						w.WriteElementString("LogLevel",         (!xmldoc.SelectSingleNode("Schumix/Log/LogLevel").IsNull() ? xmldoc.SelectSingleNode("Schumix/Log/LogLevel").InnerText : d_loglevel.ToString()));
						w.WriteElementString("LogDirectory",     (!xmldoc.SelectSingleNode("Schumix/Log/LogDirectory").IsNull() ? xmldoc.SelectSingleNode("Schumix/Log/LogDirectory").InnerText : d_logdirectory));
						w.WriteElementString("IrcLogDirectory",  (!xmldoc.SelectSingleNode("Schumix/Log/IrcLogDirectory").IsNull() ? xmldoc.SelectSingleNode("Schumix/Log/IrcLogDirectory").InnerText : d_irclogdirectory));
						w.WriteElementString("IrcLog",           (!xmldoc.SelectSingleNode("Schumix/Log/IrcLog").IsNull() ? xmldoc.SelectSingleNode("Schumix/Log/IrcLog").InnerText : d_irclog.ToString()));

						// </Log>
						w.WriteEndElement();

						// <MySql>
						w.WriteStartElement("MySql");
						w.WriteElementString("Enabled",          (!xmldoc.SelectSingleNode("Schumix/MySql/Enabled").IsNull() ? xmldoc.SelectSingleNode("Schumix/MySql/Enabled").InnerText : d_mysqlenabled.ToString()));
						w.WriteElementString("Host",             (!xmldoc.SelectSingleNode("Schumix/MySql/Host").IsNull() ? xmldoc.SelectSingleNode("Schumix/MySql/Host").InnerText : d_mysqlhost));
						w.WriteElementString("User",             (!xmldoc.SelectSingleNode("Schumix/MySql/User").IsNull() ? xmldoc.SelectSingleNode("Schumix/MySql/User").InnerText : d_mysqluser));
						w.WriteElementString("Password",         (!xmldoc.SelectSingleNode("Schumix/MySql/Password").IsNull() ? xmldoc.SelectSingleNode("Schumix/MySql/Password").InnerText : d_mysqlpassword));
						w.WriteElementString("Database",         (!xmldoc.SelectSingleNode("Schumix/MySql/Database").IsNull() ? xmldoc.SelectSingleNode("Schumix/MySql/Database").InnerText : d_mysqldatabase));
						w.WriteElementString("Charset",          (!xmldoc.SelectSingleNode("Schumix/MySql/Charset").IsNull() ? xmldoc.SelectSingleNode("Schumix/MySql/Charset").InnerText : d_mysqlcharset));

						// </MySql>
						w.WriteEndElement();

						// <SQLite>
						w.WriteStartElement("SQLite");
						w.WriteElementString("Enabled",          (!xmldoc.SelectSingleNode("Schumix/SQLite/Enabled").IsNull() ? xmldoc.SelectSingleNode("Schumix/SQLite/Enabled").InnerText : d_sqliteenabled.ToString()));
						w.WriteElementString("FileName",         (!xmldoc.SelectSingleNode("Schumix/SQLite/FileName").IsNull() ? xmldoc.SelectSingleNode("Schumix/SQLite/FileName").InnerText : d_sqlitefilename));

						// </SQLite>
						w.WriteEndElement();

						// <Crash>
						w.WriteStartElement("Crash");
						w.WriteElementString("Directory",        (!xmldoc.SelectSingleNode("Schumix/Crash/Directory").IsNull() ? xmldoc.SelectSingleNode("Schumix/Crash/Directory").InnerText : d_crashdirectory));

						// </Crash>
						w.WriteEndElement();

						// <Localization>
						w.WriteStartElement("Localization");
						w.WriteElementString("Locale",           (!xmldoc.SelectSingleNode("Schumix/Localization/Locale").IsNull() ? xmldoc.SelectSingleNode("Schumix/Localization/Locale").InnerText : d_locale));

						// </Localization>
						w.WriteEndElement();

						// <Shutdown>
						w.WriteStartElement("Shutdown");
						w.WriteElementString("MaxMemory",        (!xmldoc.SelectSingleNode("Schumix/Shutdown/MaxMemory").IsNull() ? xmldoc.SelectSingleNode("Schumix/Shutdown/MaxMemory").InnerText : d_shutdownmaxmemory.ToString()));

						// </Shutdown>
						w.WriteEndElement();

						// <Clean>
						w.WriteStartElement("Clean");
						w.WriteElementString("Config",           (!xmldoc.SelectSingleNode("Schumix/Clean/Config").IsNull() ? xmldoc.SelectSingleNode("Schumix/Clean/Config").InnerText : d_cleanconfig.ToString()));
						w.WriteElementString("Database",         (!xmldoc.SelectSingleNode("Schumix/Clean/Database").IsNull() ? xmldoc.SelectSingleNode("Schumix/Clean/Database").InnerText : d_cleandatabase.ToString()));

						// </Clean>
						w.WriteEndElement();

						// </Schumix>
						w.WriteEndElement();

						w.Flush();
						w.Close();

						if(File.Exists(filename2))
							File.Delete(filename2);

						Log.Success("XmlConfig", sLConsole.Config("Text7"));
					}
					catch(Exception e)
					{
						Log.Error("XmlConfig", sLConsole.Config("Text8"), e.Message);
						errors = true;
					}
				}
			}
			catch(DirectoryNotFoundException)
			{
				CreateConfig(ConfigDirectory, ConfigFile, ColorBindMode);
			}

			return false;
		}
	}
}