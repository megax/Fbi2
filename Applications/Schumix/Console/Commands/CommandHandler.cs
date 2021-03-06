/*
 * This file is part of Schumix.
 * 
 * Copyright (C) 2010-2012 Megax <http://www.megaxx.info/>
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
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using Schumix.API.Delegate;
using Schumix.Irc;
using Schumix.Framework;
using Schumix.Framework.Config;
using Schumix.Framework.Extensions;
using Schumix.Framework.Localization;

namespace Schumix.Console.Commands
{
	/// <summary>
	///     CommandHandler class.
	/// </summary>
	partial class CommandHandler : ConsoleLog
	{
		/// <summary>
		///     Hozzáférést biztosít singleton-on keresztül a megadott class-hoz.
		///     LocalizationConsole segítségével állíthatók be a konzol nyelvi tulajdonságai.
		/// </summary>
		private readonly LocalizationConsole sLConsole = Singleton<LocalizationConsole>.Instance;
		/// <summary>
		///     Hozzáférést biztosít singleton-on keresztül a megadott class-hoz.
		///     LocalizationManager segítségével állítható be az irc szerver felé menő tárolt üzenetek nyelvezete.
		/// </summary>
		private readonly LocalizationManager sLManager = Singleton<LocalizationManager>.Instance;
		/// <summary>
		///     Hozzáférést biztosít singleton-on keresztül a megadott class-hoz.
		///     Utilities sokféle függvényt tartalmaz melyek hasznosak lehetnek.
		/// </summary>
		private readonly Utilities sUtilities = Singleton<Utilities>.Instance;
		private readonly IrcBase sIrcBase = Singleton<IrcBase>.Instance;
		/// <summary>
		///     A szétdarabolt információkat tárolja.
		/// </summary>
		protected string[] Info;
		/// <summary>
		///     Csatorna nevét tárolja.
		/// </summary>
		protected string _channel;
		protected string _servername;

		/// <summary>
		///     Indulási függvény.
		/// </summary>
		protected CommandHandler() : base(LogConfig.IrcLog)
		{

		}

		/// <summary>
		///     Megállapítja hogy az adot szöveg csatorna-e.
		/// </summary>
		private bool IsChannel(string Name)
		{
			return (Name.Length >= 2 && Name.Trim().Length > 1 && Name.Substring(0, 1) == "#");
		}

		/// <summary>
		///     Consolelog parancs függvénye.
		/// </summary>
		protected void HandleConsoleLog()
		{
			if(Info.Length < 2)
			{
				Log.Error("Console", sLManager.GetConsoleWarningText("NoValue"));
				return;
			}

			var text = sLManager.GetConsoleCommandTexts("consolelog");
			if(text.Length < 2)
			{
				Log.Error("Console", sLConsole.Translations("NoFound2"));
				return;
			}

			if(Info[1].ToLower() == SchumixBase.On)
			{
				Log.Notice("Console", text[0]);
				ChangeLog(true);
			}
			else if(Info[1].ToLower() == SchumixBase.Off)
			{
				Log.Notice("Console", text[1]);
				ChangeLog(false);
			}
		}

		/// <summary>
		///     Sys parancs függvénye.
		/// </summary>
		protected void HandleSys()
		{
			var text = sLManager.GetConsoleCommandTexts("sys");
			if(text.Length < 7)
			{
				Log.Error("Console", sLConsole.Translations("NoFound2"));
				return;
			}

			var memory = Process.GetCurrentProcess().WorkingSet64/1024/1024;
			Log.Notice("Console", text[0], sUtilities.GetVersion());
			Log.Notice("Console", text[1], sUtilities.GetPlatform());
			Log.Notice("Console", text[2], Environment.OSVersion.ToString());
			Log.Notice("Console", text[3]);
			Log.Notice("Console", text[4], memory);
			Log.Notice("Console", text[5], Process.GetCurrentProcess().Threads.Count);
			Log.Notice("Console", text[6], SchumixBase.timer.Uptime());
		}

		/// <summary>
		///     Csatorna parancs függvénye.
		/// </summary>
		protected void HandleConsoleToChannel()
		{
			if(Info.Length < 2)
			{
				Log.Error("Console", sLManager.GetConsoleWarningText("NoChannelName"));
				return;
			}

			if(_channel == Info[1].ToLower())
			{
				Log.Warning("Console", sLManager.GetConsoleWarningText("ChannelAlreadyBeenUsed"));
				return;
			}

			_channel = Info[1].ToLower();
			Log.Notice("Console", sLManager.GetConsoleCommandText("cchannel"), Info[1]);
			System.Console.Title = SchumixBase.Title + " || Console Writing Channel: " + _servername + SchumixBase.Colon + Info[1];
		}

		protected void HandleOldServerToNewServer()
		{
			if(Info.Length < 2)
			{
				Log.Error("Console", sLManager.GetConsoleWarningText("NoServerName"));
				return;
			}

			if(!sIrcBase.Networks.ContainsKey(Info[1].ToLower()))
			{
				Log.Error("Console", sLManager.GetConsoleWarningText("ThereIsNoSuchAServerName"));
				return;
			}

			if(_servername == Info[1].ToLower())
			{
				Log.Warning("Console", sLManager.GetConsoleWarningText("ServerAlreadyBeenUsed"));
				return;
			}

			_servername = Info[1].ToLower();
			Log.Notice("Console", sLManager.GetConsoleCommandText("cserver"), Info[1]);
			System.Console.Title = SchumixBase.Title + " || Console Writing Channel: " + Info[1] + SchumixBase.Colon + _channel;
		}

		/// <summary>
		///     Function parancs függvénye.
		/// </summary>
		protected void HandleFunction()
		{
			if(Info.Length < 2)
			{
				Log.Error("Console", sLManager.GetConsoleWarningText("NoValue1"));
				return;
			}

			if(Info[1].ToLower() == "channel")
			{
				var text = sLManager.GetConsoleCommandTexts("function/channel");
				if(text.Length < 3)
				{
					Log.Error("Console", sLConsole.Translations("NoFound2"));
					return;
				}

				if(Info.Length < 3)
				{
					Log.Error("Console", sLManager.GetConsoleWarningText("NoChannelName"));
					return;
				}

				if(!IsChannel(Info[2]))
				{
					Log.Error("Console", sLManager.GetConsoleWarningText("NotaChannelHasBeenSet"));
					return;
				}

				var db0 = SchumixBase.DManager.QueryFirstRow("SELECT* FROM channels WHERE Channel = '{0}' And ServerName = '{1}'", Info[2].ToLower(), _servername);
				if(db0.IsNull())
				{
					Log.Error("Console", text[2]);
					return;
				}

				if(Info.Length < 4)
				{
					Log.Error("Console", sLManager.GetConsoleWarningText("NoValue1"));
					return;
				}
			
				string channel = Info[2].ToLower();
				string status = Info[3].ToLower();
			
				if(Info[3].ToLower() == "info")
				{
					var text2 = sLManager.GetConsoleCommandTexts("function/channel/info");
					if(text2.Length < 2)
					{
						Log.Error("Console", sLConsole.Translations("NoFound2"));
						return;
					}

					string[] ChannelInfo = sIrcBase.Networks[_servername].sChannelInfo.ChannelFunctionsInfo(channel).Split('|');
					if(ChannelInfo.Length < 2)
						return;

					Log.Notice("Console", text2[0], ChannelInfo[0]);
					Log.Notice("Console", text2[1], ChannelInfo[1]);
				}
				else if(status == SchumixBase.On || status == SchumixBase.Off)
				{
					if(Info.Length < 5)
					{
						Log.Error("Console", sLManager.GetConsoleWarningText("NoFunctionName"));
						return;
					}

					if(Info.Length >= 6)
					{
						string args = string.Empty;
						string onfunction = string.Empty;
						string offfunction = string.Empty;
						string nosuchfunction = string.Empty;

						for(int i = 4; i < Info.Length; i++)
						{
							if(!sIrcBase.Networks[_servername].sChannelInfo.SearchChannelFunction(Info[i]))
							{
								nosuchfunction += ", " + Info[i].ToLower();
								continue;
							}

							if(sIrcBase.Networks[_servername].sChannelInfo.FSelect(Info[i], channel) && status == SchumixBase.On)
							{
								onfunction += ", " + Info[i].ToLower();
								continue;
							}
							else if(!sIrcBase.Networks[_servername].sChannelInfo.FSelect(Info[i], channel) && status == SchumixBase.Off)
							{
								offfunction += ", " + Info[i].ToLower();
								continue;
							}

							if(sIrcBase.Networks[_servername].sChannelInfo.SearchFunction(Info[i]))
							{
								if(!sIrcBase.Networks[_servername].sChannelInfo.FSelect(Info[i]) && status == SchumixBase.On)
								{
									SchumixBase.DManager.Update("schumix", "FunctionStatus = 'on'", string.Format("FunctionName = '{0}' And ServerName = '{1}'", Info[i].ToLower(), _servername));
									sIrcBase.Networks[_servername].sChannelInfo.FunctionsReload();
								}
							}

							args += ", " + Info[i].ToLower();
							SchumixBase.DManager.Update("channels", string.Format("Functions = '{0}'", sIrcBase.Networks[_servername].sChannelInfo.ChannelFunctions(Info[i].ToLower(), status, channel)), string.Format("Channel = '{0}' And ServerName = '{1}'", channel, _servername));
							sIrcBase.Networks[_servername].sChannelInfo.ChannelFunctionsReload();
						}

						if(onfunction != string.Empty)
							Log.Warning("Console", sLManager.GetConsoleWarningText("FunctionAlreadyTurnedOn2"), onfunction.Remove(0, 2, ", "));
			
						if(offfunction != string.Empty)
							Log.Warning("Console", sLManager.GetConsoleWarningText("FunctionAlreadyTurnedOff2"), offfunction.Remove(0, 2, ", "));

						if(nosuchfunction != string.Empty)
							Log.Error("Console", sLConsole.Other("NoSuchFunctions2"), nosuchfunction.Remove(0, 2, ", "));

						if(args.Length == 0)
							return;

						if(status == SchumixBase.On)
							Log.Notice("Console", text[0],  args.Remove(0, 2, ", "));
						else
							Log.Notice("Console", text[1],  args.Remove(0, 2, ", "));
					}
					else
					{
						if(!sIrcBase.Networks[_servername].sChannelInfo.SearchChannelFunction(Info[4]))
						{
							Log.Error("Console", sLConsole.Other("NoSuchFunctions"));
							return;
						}

						if(sIrcBase.Networks[_servername].sChannelInfo.FSelect(Info[4], channel) && status == SchumixBase.On)
						{
							Log.Warning("Console", sLManager.GetConsoleWarningText("FunctionAlreadyTurnedOn"));
							return;
						}
						else if(!sIrcBase.Networks[_servername].sChannelInfo.FSelect(Info[4], channel) && status == SchumixBase.Off)
						{
							Log.Warning("Console", sLManager.GetConsoleWarningText("FunctionAlreadyTurnedOff"));
							return;
						}

						if(sIrcBase.Networks[_servername].sChannelInfo.SearchFunction(Info[4]))
						{
							if(!sIrcBase.Networks[_servername].sChannelInfo.FSelect(Info[4]) && status == SchumixBase.On)
							{
								SchumixBase.DManager.Update("schumix", "FunctionStatus = 'on'", string.Format("FunctionName = '{0}' And ServerName = '{1}'", Info[4].ToLower(), _servername));
								sIrcBase.Networks[_servername].sChannelInfo.FunctionsReload();
							}
						}

						if(status == SchumixBase.On)
							Log.Notice("Console", text[0], Info[4].ToLower());
						else
							Log.Notice("Console", text[1], Info[4].ToLower());

						SchumixBase.DManager.Update("channels", string.Format("Functions = '{0}'", sIrcBase.Networks[_servername].sChannelInfo.ChannelFunctions(Info[4].ToLower(), status, channel)), string.Format("Channel = '{0}' And ServerName = '{1}'", channel, _servername));
						sIrcBase.Networks[_servername].sChannelInfo.ChannelFunctionsReload();
					}
				}
				else
					Log.Error("Console", sLManager.GetConsoleWarningText("WrongSwitch"));
			}
			else if(Info[1].ToLower() == "update")
			{
				if(Info.Length < 3)
				{
					Log.Error("Console", sLManager.GetConsoleWarningText("NoValue1"));
					return;
				}

				if(Info[2].ToLower() == "all")
				{
					var db = SchumixBase.DManager.Query("SELECT Channel FROM channels WHERE ServerName = '{0}'", _servername);
					if(!db.IsNull())
					{
						foreach(DataRow row in db.Rows)
						{
							string channel = row["Channel"].ToString();
							SchumixBase.DManager.Update("channels", string.Format("Functions = '{0}'", sUtilities.GetFunctionUpdate()), string.Format("Channel = '{0}' And ServerName = '{1}'", channel, _servername));
						}

						sIrcBase.Networks[_servername].sChannelInfo.ChannelFunctionsReload();
						Log.Notice("Console", sLManager.GetConsoleCommandText("function/update/all"));
					}
					else
						Log.Error("Console", sLManager.GetConsoleWarningText("FaultyQuery"));
				}
				else
				{
					if(!IsChannel(Info[2]))
					{
						Log.Error("Console", sLManager.GetConsoleWarningText("NotaChannelHasBeenSet"));
						return;
					}

					Log.Notice("Console", sLManager.GetConsoleCommandText("function/update"), Info[2].ToLower());
					SchumixBase.DManager.Update("channels", string.Format("Functions = '{0}'", sUtilities.GetFunctionUpdate()), string.Format("Channel = '{0}' And ServerName = '{1}'", Info[2].ToLower(), _servername));
					sIrcBase.Networks[_servername].sChannelInfo.ChannelFunctionsReload();
				}
			}
			else if(Info[1].ToLower() == "info")
			{
				var text = sLManager.GetConsoleCommandTexts("function/info");
				if(text.Length < 2)
				{
					Log.Error("Console", sLConsole.Translations("NoFound2"));
					return;
				}

				string f = sIrcBase.Networks[_servername].sChannelInfo.FunctionsInfo();
				if(f == string.Empty)
				{
					Log.Error("Console", sLManager.GetConsoleWarningText("FaultyQuery"));
					return;
				}

				string[] FunkcioInfo = f.Split('|');
				if(FunkcioInfo.Length < 2)
					return;
	
				Log.Notice("Console", text[0], FunkcioInfo[0]);
				Log.Notice("Console", text[1], FunkcioInfo[1]);
			}
			else
			{
				if(Info.Length < 3)
				{
					Log.Error("Console", sLManager.GetConsoleWarningText("NoFunctionName"));
					return;
				}

				var text = sLManager.GetConsoleCommandTexts("function");
				if(text.Length < 2)
				{
					Log.Error("Console", sLConsole.Translations("NoFound2"));
					return;
				}

				if(Info[1].ToLower() == SchumixBase.On || Info[1].ToLower() == SchumixBase.Off)
				{
					if(Info.Length >= 4)
					{
						string args = string.Empty;
						string onfunction = string.Empty;
						string offfunction = string.Empty;
						string nosuchfunction = string.Empty;

						for(int i = 2; i < Info.Length; i++)
						{
							if(!sIrcBase.Networks[_servername].sChannelInfo.SearchFunction(Info[i]))
							{
								nosuchfunction += ", " + Info[i].ToLower();
								continue;
							}

							if(sIrcBase.Networks[_servername].sChannelInfo.FSelect(Info[i]) && Info[1].ToLower() == SchumixBase.On)
							{
								onfunction += ", " + Info[i].ToLower();
								continue;
							}
							else if(!sIrcBase.Networks[_servername].sChannelInfo.FSelect(Info[i]) && Info[1].ToLower() == SchumixBase.Off)
							{
								offfunction += ", " + Info[i].ToLower();
								continue;
							}

							args += ", " + Info[i].ToLower();
							SchumixBase.DManager.Update("schumix", string.Format("FunctionStatus = '{0}'", Info[1].ToLower()), string.Format("FunctionName = '{0}' And ServerName = '{1}'", Info[i].ToLower(), _servername));
							sIrcBase.Networks[_servername].sChannelInfo.FunctionsReload();
						}

						if(onfunction != string.Empty)
							Log.Warning("Console", sLManager.GetConsoleWarningText("FunctionAlreadyTurnedOn2"), onfunction.Remove(0, 2, ", "));
			
						if(offfunction != string.Empty)
							Log.Warning("Console", sLManager.GetConsoleWarningText("FunctionAlreadyTurnedOff2"), offfunction.Remove(0, 2, ", "));

						if(nosuchfunction != string.Empty)
							Log.Error("Console", sLConsole.Other("NoSuchFunctions2"), nosuchfunction.Remove(0, 2, ", "));

						if(args.Length == 0)
							return;

						if(Info[1].ToLower() == SchumixBase.On)
							Log.Notice("Console", text[0],  args.Remove(0, 2, ", "));
						else
							Log.Notice("Console", text[1],  args.Remove(0, 2, ", "));
					}
					else
					{
						if(!sIrcBase.Networks[_servername].sChannelInfo.SearchFunction(Info[2]))
						{
							Log.Error("Console", sLConsole.Other("NoSuchFunctions"));
							return;
						}

						if(sIrcBase.Networks[_servername].sChannelInfo.FSelect(Info[2]) && Info[1].ToLower() == SchumixBase.On)
						{
							Log.Warning("Console", sLManager.GetConsoleWarningText("FunctionAlreadyTurnedOn"));
							return;
						}
						else if(!sIrcBase.Networks[_servername].sChannelInfo.FSelect(Info[2]) && Info[1].ToLower() == SchumixBase.Off)
						{
							Log.Warning("Console", sLManager.GetConsoleWarningText("FunctionAlreadyTurnedOff"));
							return;
						}

						if(Info[1].ToLower() == SchumixBase.On)
							Log.Notice("Console", text[0], Info[2].ToLower());
						else
							Log.Notice("Console", text[1], Info[2].ToLower());

						SchumixBase.DManager.Update("schumix", string.Format("FunctionStatus = '{0}'", Info[1].ToLower()), string.Format("FunctionName = '{0}' And ServerName = '{1}'", Info[2].ToLower(), _servername));
						sIrcBase.Networks[_servername].sChannelInfo.FunctionsReload();
					}
				}
				else
					Log.Error("Console", sLManager.GetConsoleWarningText("WrongSwitch"));
			}
		}

		/// <summary>
		///     Channel parancs függvénye.
		/// </summary>
		protected void HandleChannel()
		{
			if(Info.Length < 2)
			{
				Log.Notice("Console", sLManager.GetConsoleCommandText("channel"));
				return;
			}

			if(Info[1].ToLower() == "add")
			{
				if(Info.Length < 3)
				{
					Log.Error("Console", sLManager.GetConsoleWarningText("NoChannelName"));
					return;
				}

				var text = sLManager.GetConsoleCommandTexts("channel/add");
				if(text.Length < 2)
				{
					Log.Error("Console", sLConsole.Translations("NoFound2"));
					return;
				}

				string channel = Info[2].ToLower();

				if(!IsChannel(channel))
				{
					Log.Error("Console", sLManager.GetConsoleWarningText("NotaChannelHasBeenSet"));
					return;
				}

				var db = SchumixBase.DManager.QueryFirstRow("SELECT* FROM channels WHERE Channel = '{0}' And ServerName = '{1}'", channel, _servername);
				if(!db.IsNull())
				{
					Log.Warning("Console", text[0]);
					return;
				}

				if(Info.Length == 4)
				{
					string pass = Info[3];
					sIrcBase.Networks[_servername].sSender.Join(channel, pass);
					SchumixBase.DManager.Insert("`channels`(ServerId, ServerName, Channel, Password, Language)", IRCConfig.List[_servername].ServerId, _servername, channel, pass, sLManager.Locale);
					SchumixBase.DManager.Update("channels", "Enabled = 'true'", string.Format("Channel = '{0}' And ServerName = '{1}'", channel, _servername));
				}
				else
				{
					sIrcBase.Networks[_servername].sSender.Join(channel);
					SchumixBase.DManager.Insert("`channels`(ServerId, ServerName, Channel, Password, Language)", IRCConfig.List[_servername].ServerId, _servername, channel, string.Empty, sLManager.Locale);
					SchumixBase.DManager.Update("channels", "Enabled = 'true'", string.Format("Channel = '{0}' And ServerName = '{1}'", channel, _servername));
				}

				Log.Notice("Console", text[1], channel);
				sIrcBase.Networks[_servername].sChannelInfo.ChannelListReload();
				sIrcBase.Networks[_servername].sChannelInfo.ChannelFunctionsReload();
			}
			else if(Info[1].ToLower() == "remove")
			{
				if(Info.Length < 3)
				{
					Log.Error("Console", sLManager.GetConsoleWarningText("NoChannelName"));
					return;
				}

				var text = sLManager.GetConsoleCommandTexts("channel/remove");
				if(text.Length < 3)
				{
					Log.Error("Console", sLConsole.Translations("NoFound2"));
					return;
				}

				string channel = Info[2].ToLower();

				if(!IsChannel(channel))
				{
					Log.Error("Console", sLManager.GetConsoleWarningText("NotaChannelHasBeenSet"));
					return;
				}

				if(channel == IRCConfig.List[_servername].MasterChannel.ToLower())
				{
					Log.Warning("Console", text[0]);
					return;
				}

				var db = SchumixBase.DManager.QueryFirstRow("SELECT* FROM channels WHERE Channel = '{0}' And ServerName = '{1}'", channel, _servername);
				if(db.IsNull())
				{
					Log.Warning("Console", text[1]);
					return;
				}

				sIrcBase.Networks[_servername].sSender.Part(channel);
				SchumixBase.DManager.Delete("channels", string.Format("Channel = '{0}' And ServerName = '{1}'", channel, _servername));
				Log.Notice("Console", text[2], channel);

				sIrcBase.Networks[_servername].sChannelInfo.ChannelListReload();
				sIrcBase.Networks[_servername].sChannelInfo.ChannelFunctionsReload();
			}
			else if(Info[1].ToLower() == "update")
			{
				sIrcBase.Networks[_servername].sChannelInfo.ChannelListReload();
				sIrcBase.Networks[_servername].sChannelInfo.ChannelFunctionsReload();
				Log.Notice("Console", sLManager.GetConsoleCommandText("channel/update"));
			}
			else if(Info[1].ToLower() == "info")
			{
				var text = sLManager.GetConsoleCommandTexts("channel/info");
				if(text.Length < 3)
				{
					Log.Error("Console", sLConsole.Translations("NoFound2"));
					return;
				}

				var db = SchumixBase.DManager.Query("SELECT Channel, Enabled, Error FROM channels WHERE ServerName = '{0}'", _servername);
				if(!db.IsNull())
				{
					string ActiveChannels = string.Empty, InActiveChannels = string.Empty;

					foreach(DataRow row in db.Rows)
					{
						string channel = row["Channel"].ToString();
						bool enabled = Convert.ToBoolean(row["Enabled"].ToString());

						if(enabled)
							ActiveChannels += ", " + channel;
						else if(!enabled)
							InActiveChannels += ", " + channel + ":" + row["Error"].ToString();
					}

					if(ActiveChannels.Length > 0)
						Log.Notice("Console", text[0], ActiveChannels.Remove(0, 2, ", "));
					else
						Log.Notice("Console", text[1]);

					if(InActiveChannels.Length > 0)
						Log.Notice("Console", text[2], InActiveChannels.Remove(0, 2, ", "));
					else
						Log.Notice("Console", text[3]);
				}
				else
					Log.Error("Console", sLManager.GetConsoleWarningText("FaultyQuery"));
			}
			else if(Info[1].ToLower() == "language")
			{
				var text = sLManager.GetConsoleCommandTexts("channel/language");
				if(text.Length < 3)
				{
					Log.Error("Console", sLConsole.Translations("NoFound2"));
					return;
				}

				if(Info.Length < 3)
				{
					Log.Error("Console", sLManager.GetConsoleWarningText("NoChannelName"));
					return;
				}

				if(!IsChannel(Info[2]))
				{
					Log.Error("Console", sLManager.GetConsoleWarningText("NotaChannelHasBeenSet"));
					return;
				}

				var db = SchumixBase.DManager.QueryFirstRow("SELECT* FROM channels WHERE Channel = '{0}' And ServerName = '{1}'", Info[2].ToLower(), _servername);
				if(db.IsNull())
				{
					Log.Warning("Console", text[1]);
					return;
				}

				if(Info.Length < 4)
				{
					Log.Error("Console", sLManager.GetConsoleWarningText("NoChannelLanguage"));
					return;
				}

				db = SchumixBase.DManager.QueryFirstRow("SELECT Language FROM channels WHERE Channel = '{0}' And ServerName = '{1}'", Info[2].ToLower(), _servername);
				if(!db.IsNull())
				{
					if(db["Language"].ToString().ToLower() == Info[3].ToLower())
					{
						Log.Warning("Console", text[2], Info[3]);
						return;
					}
				}

				SchumixBase.DManager.Update("channels", string.Format("Language = '{0}'", Info[3]), string.Format("Channel = '{0}' And ServerName = '{1}'", Info[2].ToLower(), _servername));
				Log.Notice("Console", text[0], Info[3]);
				SchumixBase.sCacheDB.ReLoad("channels");
			}
			else if(Info[1].ToLower() == "password")
			{
				if(Info.Length < 3)
				{
					Log.Error("Console", sLManager.GetConsoleWarningText("NoValue"));
					return;
				}

				if(Info[2].ToLower() == "add")
				{
					var text = sLManager.GetConsoleCommandTexts("channel/password/add");
					if(text.Length < 3)
					{
						Log.Error("Console", sLConsole.Translations("NoFound2"));
						return;
					}

					if(Info.Length < 4)
					{
						Log.Error("Console", sLManager.GetConsoleWarningText("NoChannelName"));
						return;
					}
	
					if(!IsChannel(Info[3]))
					{
						Log.Error("Console", sLManager.GetConsoleWarningText("NotaChannelHasBeenSet"));
						return;
					}

					if(Info.Length < 5)
					{
						Log.Error("Console", sLManager.GetConsoleWarningText("NoPassword"));
						return;
					}

					var db = SchumixBase.DManager.QueryFirstRow("SELECT* FROM channels WHERE Channel = '{0}' And ServerName = '{1}'", Info[3].ToLower(), _servername);
					if(db.IsNull())
					{
						Log.Warning("Console", text[0]);
						return;
					}

					db = SchumixBase.DManager.QueryFirstRow("SELECT Password FROM channels WHERE Channel = '{0}' And ServerName = '{1}'", Info[3].ToLower(), _servername);
					if(!db.IsNull())
					{
						if(db["Password"].ToString().Trim() != string.Empty)
						{
							Log.Notice("Console", text[1]);
							return;
						}
					}

					SchumixBase.DManager.Update("channels", string.Format("Password = '{0}'", Info[4]), string.Format("Channel = '{0}' And ServerName = '{1}'", Info[3].ToLower(), _servername));
					Log.Notice("Console", text[2], Info[3]);
					SchumixBase.sCacheDB.ReLoad("channels");
				}
				else if(Info[2].ToLower() == "remove")
				{
					var text = sLManager.GetConsoleCommandTexts("channel/password/remove");
					if(text.Length < 3)
					{
						Log.Error("Console", sLConsole.Translations("NoFound2"));
						return;
					}

					if(Info.Length < 4)
					{
						Log.Error("Console", sLManager.GetConsoleWarningText("NoChannelName"));
						return;
					}
	
					if(!IsChannel(Info[3]))
					{
						Log.Error("Console", sLManager.GetConsoleWarningText("NotaChannelHasBeenSet"));
						return;
					}

					var db = SchumixBase.DManager.QueryFirstRow("SELECT* FROM channels WHERE Channel = '{0}' And ServerName = '{1}'", Info[3].ToLower(), _servername);
					if(db.IsNull())
					{
						Log.Warning("Console", text[0]);
						return;
					}

					db = SchumixBase.DManager.QueryFirstRow("SELECT Password FROM channels WHERE Channel = '{0}' And ServerName = '{1}'", Info[3].ToLower(), _servername);
					if(!db.IsNull())
					{
						if(db["Password"].ToString().Trim() == string.Empty)
						{
							Log.Notice("Console", text[1]);
							return;
						}
					}

					SchumixBase.DManager.Update("channels", "Password = ''", string.Format("Channel = '{0}' And ServerName = '{1}'", Info[3].ToLower(), _servername));
					Log.Notice("Console", text[2]);
					SchumixBase.sCacheDB.ReLoad("channels");
				}
				else if(Info[2].ToLower() == "update")
				{
					var text = sLManager.GetConsoleCommandTexts("channel/password/update");
					if(text.Length < 3)
					{
						Log.Error("Console", sLConsole.Translations("NoFound2"));
						return;
					}

					if(Info.Length < 4)
					{
						Log.Error("Console", sLManager.GetConsoleWarningText("NoChannelName"));
						return;
					}
	
					if(!IsChannel(Info[3]))
					{
						Log.Error("Console", sLManager.GetConsoleWarningText("NotaChannelHasBeenSet"));
						return;
					}

					if(Info.Length < 5)
					{
						Log.Error("Console", sLManager.GetConsoleWarningText("NoPassword"));
						return;
					}

					var db = SchumixBase.DManager.QueryFirstRow("SELECT* FROM channels WHERE Channel = '{0}' And ServerName = '{1}'", Info[3].ToLower(), _servername);
					if(db.IsNull())
					{
						Log.Warning("Console", text[0]);
						return;
					}

					db = SchumixBase.DManager.QueryFirstRow("SELECT Password FROM channels WHERE Channel = '{0}' And ServerName = '{1}'", Info[3].ToLower(), _servername);
					if(!db.IsNull())
					{
						if(db["Password"].ToString().Trim() == string.Empty)
						{
							Log.Notice("Console", text[1]);
							return;
						}
					}

					SchumixBase.DManager.Update("channels", string.Format("Password = '{0}'", Info[4]), string.Format("Channel = '{0}' And ServerName = '{1}'", Info[3].ToLower(), _servername));
					Log.Notice("Console", text[2], Info[4]);
					SchumixBase.sCacheDB.ReLoad("channels");
				}
				else if(Info[2].ToLower() == "info")
				{
					var text = sLManager.GetConsoleCommandTexts("channel/password/info");
					if(text.Length < 3)
					{
						Log.Error("Console", sLConsole.Translations("NoFound2"));
						return;
					}

					if(Info.Length < 4)
					{
						Log.Error("Console", sLManager.GetConsoleWarningText("NoChannelName"));
						return;
					}
	
					if(!IsChannel(Info[3]))
					{
						Log.Error("Console", sLManager.GetConsoleWarningText("NotaChannelHasBeenSet"));
						return;
					}

					var db = SchumixBase.DManager.QueryFirstRow("SELECT* FROM channels WHERE Channel = '{0}' And ServerName = '{1}'", Info[3].ToLower(), _servername);
					if(db.IsNull())
					{
						Log.Warning("Console", text[0]);
						return;
					}

					db = SchumixBase.DManager.QueryFirstRow("SELECT Password FROM channels WHERE Channel = '{0}' And ServerName = '{1}'", Info[3].ToLower(), _servername);
					if(!db.IsNull())
					{
						if(db["Password"].ToString().Trim() == string.Empty)
							Log.Notice("Console", text[1]);
						else
							Log.Notice("Console", text[2]);
					}
				}
			}
		}

		/// <summary>
		///     Connect parancs függvénye.
		/// </summary>
		protected void HandleConnect()
		{
			sIrcBase.Networks[_servername].Connect();
		}

		/// <summary>
		///     Disconnect parancs függvénye.
		/// </summary>
		protected void HandleDisConnect()
		{
			sIrcBase.Networks[_servername].sSender.Quit("Console: Disconnect.");
			sIrcBase.Networks[_servername].DisConnect();
		}

		/// <summary>
		///     Reconnect parancs függvénye.
		/// </summary>
		protected void HandleReConnect()
		{
			sIrcBase.Networks[_servername].sSender.Quit("Console: Reconnect.");
			sIrcBase.Networks[_servername].ReConnect();
		}

		/// <summary>
		///     Nick parancs függvénye.
		/// </summary>
		protected void HandleNick()
		{
			if(Info.Length < 2)
			{
				Log.Error("Console", sLManager.GetConsoleWarningText("NoName"));
				return;
			}

			SchumixBase.NewNick = true;
			string nick = Info[1];
			sIrcBase.Networks[_servername].sNickInfo.ChangeNick(nick);
			sIrcBase.Networks[_servername].sSender.Nick(nick);
			Log.Notice("Console", sLManager.GetConsoleCommandText("nick"), nick);
		}

		/// <summary>
		///     Join parancs függvénye.
		/// </summary>
		protected void HandleJoin()
		{
			if(Info.Length < 2)
			{
				Log.Error("Console", sLManager.GetConsoleWarningText("NoChannelName"));
				return;
			}

			if(!IsChannel(Info[1]))
			{
				Log.Error("Console", sLManager.GetConsoleWarningText("NotaChannelHasBeenSet"));
				return;
			}

			if(Info.Length == 2)
				sIrcBase.Networks[_servername].sSender.Join(Info[1]);
			else if(Info.Length == 3)
				sIrcBase.Networks[_servername].sSender.Join(Info[1], Info[2]);

			Log.Notice("Console", sLManager.GetConsoleCommandText("join"), Info[1]);
		}

		/// <summary>
		///     Left parancs függvénye.
		/// </summary>
		protected void HandleLeave()
		{
			if(Info.Length < 2)
			{
				Log.Error("Console", sLManager.GetConsoleWarningText("NoChannelName"));
				return;
			}

			if(!IsChannel(Info[1]))
			{
				Log.Error("Console", sLManager.GetConsoleWarningText("NotaChannelHasBeenSet"));
				return;
			}

			sIrcBase.Networks[_servername].sSender.Part(Info[1]);
			Log.Notice("Console", sLManager.GetConsoleCommandText("leave"), Info[1]);
		}

		/// <summary>
		///     Reload parancs függvénye.
		/// </summary>
		protected void HandleReload()
		{
			if(Info.Length < 3)
			{
				Log.Error("Console", sLManager.GetConsoleWarningText("NoName"));
				return;
			}

			var text = sLManager.GetConsoleCommandTexts("reload");
			if(text.Length < 2)
			{
				Log.Error("Console", sLConsole.Translations("NoFound2"));
				return;
			}

			int i = -1;

			switch(Info[1].ToLower())
			{
				case "config":
					new Config(SchumixConfig.ConfigDirectory, SchumixConfig.ConfigFile, SchumixConfig.ColorBindMode);
					sLConsole.Locale = LocalizationConfig.Locale;
					i = 1;
					break;
				case "cachedb":
					SchumixBase.sCacheDB.ReLoad();
					i = 1;
					break;
			}

			if(i == -1)
				Log.Error("Console", text[0]);
			else if(i == 0)
				Log.Error("Console", text[1]);
			else if(i == 1)
				Log.Notice("Console", text[2], Info[1]);
		}

		/// <summary>
		///     Quit parancs függvénye.
		/// </summary>
		protected void HandleQuit()
		{
			var text = sLManager.GetConsoleCommandTexts("quit");
			if(text.Length < 2)
			{
				Log.Error("Console", sLConsole.Translations("NoFound2"));
				return;
			}

			Log.Notice("Console", text[0]);
			SchumixBase.Quit();
			sIrcBase.Shutdown(text[1]);
		}
	}
}