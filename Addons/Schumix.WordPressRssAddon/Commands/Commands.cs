/*
 * This file is part of Schumix.
 * 
 * Copyright (C) 2010-2013 Megax <http://megax.yeahunter.hu/>
 * Copyright (C) 2013 Schumix Team <http://schumix.eu/>
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
using System.Collections.Generic;
using Schumix.Api.Irc;
using Schumix.Irc;
using Schumix.Irc.Util;
using Schumix.Irc.Commands;
using Schumix.Framework;
using Schumix.Framework.Extensions;
using Schumix.Framework.Localization;

namespace Schumix.WordPressRssAddon.Commands
{
	class RssCommand : CommandInfo
	{
		private readonly LocalizationConsole sLConsole = Singleton<LocalizationConsole>.Instance;
		private readonly LocalizationManager sLManager = Singleton<LocalizationManager>.Instance;
		private readonly Utilities sUtilities = Singleton<Utilities>.Instance;
		public readonly List<WordPressRss> RssList = new List<WordPressRss>();
		private readonly IrcBase sIrcBase = Singleton<IrcBase>.Instance;

		public RssCommand(string ServerName) : base(ServerName)
		{
		}

		public void HandleWordPress(IRCMessage sIRCMessage)
		{
			if(!IsAdmin(sIRCMessage.Nick, sIRCMessage.Host, AdminFlag.Operator))
				return;

			var sSendMessage = sIrcBase.Networks[sIRCMessage.ServerName].sSendMessage;

			if(sIRCMessage.Info.Length < 5)
			{
				sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("NoValue", sIRCMessage.Channel, sIRCMessage.ServerName));
				return;
			}

			if(sIRCMessage.Info[4].ToLower() == "info")
			{
				var db = SchumixBase.DManager.Query("SELECT Name, Channel FROM wordpressinfo WHERE ServerName = '{0}'", sIRCMessage.ServerName);
				if(!db.IsNull())
				{
					foreach(DataRow row in db.Rows)
					{
						string name = row["Name"].ToString();
						string[] channel = row["Channel"].ToString().Split(SchumixBase.Comma);
						sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetCommandText("wordpress/info", sIRCMessage.Channel, sIRCMessage.ServerName), name, channel.SplitToString(SchumixBase.Space));
					}
				}
				else
					sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("FaultyQuery", sIRCMessage.Channel, sIRCMessage.ServerName));
			}
			else if(sIRCMessage.Info[4].ToLower() == "list")
			{
				var db = SchumixBase.DManager.Query("SELECT Name FROM wordpressinfo WHERE ServerName = '{0}'", sIRCMessage.ServerName);
				if(!db.IsNull())
				{
					string list = string.Empty;

					foreach(DataRow row in db.Rows)
						list += SchumixBase.Comma + SchumixBase.Space + row["Name"].ToString();

					if(list.IsEmpty())
						sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetCommandText("wordpress/list", sIRCMessage.Channel, sIRCMessage.ServerName), SchumixBase.Space + sLConsole.Other("Nothing"));
					else
						sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetCommandText("wordpress/list", sIRCMessage.Channel, sIRCMessage.ServerName), list.Remove(0, 1, SchumixBase.Comma));
				}
				else
					sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("FaultyQuery", sIRCMessage.Channel, sIRCMessage.ServerName));
			}
			else if(sIRCMessage.Info[4].ToLower() == "start")
			{
				var text = sLManager.GetCommandTexts("wordpress/start", sIRCMessage.Channel, sIRCMessage.ServerName);
				if(text.Length < 3)
				{
					sSendMessage.SendChatMessage(sIRCMessage, sLConsole.Translations("NoFound2", sLManager.GetChannelLocalization(sIRCMessage.Channel, sIRCMessage.ServerName)));
					return;
				}

				if(sIRCMessage.Info.Length < 6)
				{
					sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("NoName", sIRCMessage.Channel, sIRCMessage.ServerName));
					return;
				}

				foreach(var list in RssList)
				{
					if(sIRCMessage.Info[5].ToLower() == list.Name.ToLower())
					{
						if(list.Started)
						{
							sSendMessage.SendChatMessage(sIRCMessage, text[0], list.Name);
							return;
						}

						list.Start();
						sSendMessage.SendChatMessage(sIRCMessage, text[1], list.Name);
						return;
					}
				}

				sSendMessage.SendChatMessage(sIRCMessage, text[2], sIRCMessage.Info[5]);
			}
			else if(sIRCMessage.Info[4].ToLower() == "stop")
			{
				var text = sLManager.GetCommandTexts("wordpress/stop", sIRCMessage.Channel, sIRCMessage.ServerName);
				if(text.Length < 3)
				{
					sSendMessage.SendChatMessage(sIRCMessage, sLConsole.Translations("NoFound2", sLManager.GetChannelLocalization(sIRCMessage.Channel, sIRCMessage.ServerName)));
					return;
				}

				if(sIRCMessage.Info.Length < 6)
				{
					sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("NoName", sIRCMessage.Channel, sIRCMessage.ServerName));
					return;
				}

				foreach(var list in RssList)
				{
					if(sIRCMessage.Info[5].ToLower() == list.Name.ToLower())
					{
						if(!list.Started)
						{
							sSendMessage.SendChatMessage(sIRCMessage, text[0], list.Name);
							return;
						}

						list.Stop();
						sSendMessage.SendChatMessage(sIRCMessage, text[1], list.Name);
						return;
					}
				}

				sSendMessage.SendChatMessage(sIRCMessage, text[2], sIRCMessage.Info[5]);
			}
			else if(sIRCMessage.Info[4].ToLower() == "reload")
			{
				if(sIRCMessage.Info.Length < 6)
				{
					sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("No1Value", sIRCMessage.Channel, sIRCMessage.ServerName));
					return;
				}

				if(sIRCMessage.Info[5].ToLower() == "all")
				{
					foreach(var list in RssList)
						list.Reload();

					sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetCommandText("wordpress/reload/all", sIRCMessage.Channel, sIRCMessage.ServerName));
				}
				else
				{
					var text = sLManager.GetCommandTexts("wordpress/reload", sIRCMessage.Channel, sIRCMessage.ServerName);
					if(text.Length < 2)
					{
						sSendMessage.SendChatMessage(sIRCMessage, sLConsole.Translations("NoFound2", sLManager.GetChannelLocalization(sIRCMessage.Channel, sIRCMessage.ServerName)));
						return;
					}

					if(sIRCMessage.Info.Length < 6)
					{
						sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("NoName", sIRCMessage.Channel, sIRCMessage.ServerName));
						return;
					}

					foreach(var list in RssList)
					{
						if(sIRCMessage.Info[5].ToLower() == list.Name.ToLower())
						{
							list.Reload();
							sSendMessage.SendChatMessage(sIRCMessage, text[0], list.Name);
							return;
						}
					}

					sSendMessage.SendChatMessage(sIRCMessage, text[1], sIRCMessage.Info[5]);
				}
			}
			else if(sIRCMessage.Info[4].ToLower() == "channel")
			{
				if(sIRCMessage.Info.Length < 6)
				{
					sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("NoCommand", sIRCMessage.Channel, sIRCMessage.ServerName));
					return;
				}

				if(sIRCMessage.Info[5].ToLower() == "add")
				{
					var text = sLManager.GetCommandTexts("wordpress/channel/add", sIRCMessage.Channel, sIRCMessage.ServerName);
					if(text.Length < 3)
					{
						sSendMessage.SendChatMessage(sIRCMessage, sLConsole.Translations("NoFound2", sLManager.GetChannelLocalization(sIRCMessage.Channel, sIRCMessage.ServerName)));
						return;
					}

					if(sIRCMessage.Info.Length < 7)
					{
						sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("NoName", sIRCMessage.Channel, sIRCMessage.ServerName));
						return;
					}

					if(sIRCMessage.Info.Length < 8)
					{
						sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("NoChannelName", sIRCMessage.Channel, sIRCMessage.ServerName));
						return;
					}

					if(!Rfc2812Util.IsValidChannelName(sIRCMessage.Info[7]))
					{
						sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("NotaChannelHasBeenSet", sIRCMessage.Channel, sIRCMessage.ServerName));
						return;
					}

					var db = SchumixBase.DManager.QueryFirstRow("SELECT Channel FROM wordpressinfo WHERE LOWER(Name) = '{0}' And ServerName = '{1}'", sUtilities.SqlEscape(sIRCMessage.Info[6].ToLower()), sIRCMessage.ServerName);
					if(!db.IsNull())
					{
						string[] channel = db["Channel"].ToString().Split(SchumixBase.Comma);
						string data = channel.SplitToString(SchumixBase.Comma);

						if(channel.ToList().Contains(sIRCMessage.Info[7].ToLower()))
						{
							sSendMessage.SendChatMessage(sIRCMessage, text[2]);
							return;
						}

						if(channel.Length == 1 && data.IsEmpty())
							data += sIRCMessage.Info[7].ToLower();
						else
							data += SchumixBase.Comma + sIRCMessage.Info[7].ToLower();

						SchumixBase.DManager.Update("wordpressinfo", string.Format("Channel = '{0}'", data), string.Format("LOWER(Name) = '{0}' And ServerName = '{1}'", sUtilities.SqlEscape(sIRCMessage.Info[6].ToLower()), sIRCMessage.ServerName));
						sSendMessage.SendChatMessage(sIRCMessage, text[0]);
					}
					else
						sSendMessage.SendChatMessage(sIRCMessage, text[1]);
				}
				else if(sIRCMessage.Info[5].ToLower() == "remove")
				{
					var text = sLManager.GetCommandTexts("wordpress/channel/remove", sIRCMessage.Channel, sIRCMessage.ServerName);
					if(text.Length < 3)
					{
						sSendMessage.SendChatMessage(sIRCMessage, sLConsole.Translations("NoFound2", sLManager.GetChannelLocalization(sIRCMessage.Channel, sIRCMessage.ServerName)));
						return;
					}

					if(sIRCMessage.Info.Length < 7)
					{
						sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("NoName", sIRCMessage.Channel, sIRCMessage.ServerName));
						return;
					}

					if(sIRCMessage.Info.Length < 8)
					{
						sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("NoChannelName", sIRCMessage.Channel, sIRCMessage.ServerName));
						return;
					}

					if(!Rfc2812Util.IsValidChannelName(sIRCMessage.Info[7]))
					{
						sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("NotaChannelHasBeenSet", sIRCMessage.Channel, sIRCMessage.ServerName));
						return;
					}

					var db = SchumixBase.DManager.QueryFirstRow("SELECT Channel FROM wordpressinfo WHERE LOWER(Name) = '{0}' And ServerName = '{1}'", sUtilities.SqlEscape(sIRCMessage.Info[6].ToLower()), sIRCMessage.ServerName);
					if(!db.IsNull())
					{
						string[] channel = db["Channel"].ToString().Split(SchumixBase.Comma);
						string data = string.Empty;

						if(!channel.ToList().Contains(sIRCMessage.Info[7].ToLower()))
						{
							sSendMessage.SendChatMessage(sIRCMessage, text[2]);
							return;
						}

						for(int x = 0; x < channel.Length; x++)
						{
							if(channel[x] == sIRCMessage.Info[7].ToLower())
								continue;

							data += SchumixBase.Comma + channel[x];
						}

						SchumixBase.DManager.Update("wordpressinfo", string.Format("Channel = '{0}'", data.Remove(0, 1, SchumixBase.Comma)), string.Format("LOWER(Name) = '{0}' And ServerName = '{1}'", sUtilities.SqlEscape(sIRCMessage.Info[6].ToLower()), sIRCMessage.ServerName));
						sSendMessage.SendChatMessage(sIRCMessage, text[0]);
					}
					else
						sSendMessage.SendChatMessage(sIRCMessage, text[1]);
				}
			}
			else if(sIRCMessage.Info[4].ToLower() == "add")
			{
				var text = sLManager.GetCommandTexts("wordpress/add", sIRCMessage.Channel, sIRCMessage.ServerName);
				if(text.Length < 2)
				{
					sSendMessage.SendChatMessage(sIRCMessage, sLConsole.Translations("NoFound2", sLManager.GetChannelLocalization(sIRCMessage.Channel, sIRCMessage.ServerName)));
					return;
				}
				
				if(sIRCMessage.Info.Length < 6)
				{
					sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("NoName", sIRCMessage.Channel, sIRCMessage.ServerName));
					return;
				}
				
				if(sIRCMessage.Info.Length < 7)
				{
					sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("UrlMissing", sIRCMessage.Channel, sIRCMessage.ServerName));
					return;
				}

				var db = SchumixBase.DManager.QueryFirstRow("SELECT * FROM wordpressinfo WHERE LOWER(Name) = '{0}' And ServerName = '{1}'", sUtilities.SqlEscape(sIRCMessage.Info[5].ToLower()), sIRCMessage.ServerName);
				if(db.IsNull())
				{
					bool started = false;
					
					foreach(var list in RssList)
					{
						if(sIRCMessage.Info[5].ToLower() == list.Name.ToLower())
						{
							if(list.Started)
							{
								started = true;
								continue;
							}
							
							list.Start();
							started = true;
						}
					}
					
					if(!started)
					{
						var rss = new WordPressRss(sIRCMessage.ServerName, sUtilities.SqlEscape(sIRCMessage.Info[5]), sUtilities.SqlEscape(sIRCMessage.Info[6]));
						RssList.Add(rss);
						rss.Start();
					}
					
					SchumixBase.DManager.Insert("`wordpressinfo`(ServerId, ServerName, Name, Link)", sIRCMessage.ServerId, sIRCMessage.ServerName, sUtilities.SqlEscape(sIRCMessage.Info[5]), sUtilities.SqlEscape(sIRCMessage.Info[6]));
					sSendMessage.SendChatMessage(sIRCMessage, text[0]);
				}
				else
					sSendMessage.SendChatMessage(sIRCMessage, text[1]);
			}
			else if(sIRCMessage.Info[4].ToLower() == "remove")
			{
				var text = sLManager.GetCommandTexts("wordpress/remove", sIRCMessage.Channel, sIRCMessage.ServerName);
				if(text.Length < 2)
				{
					sSendMessage.SendChatMessage(sIRCMessage, sLConsole.Translations("NoFound2", sLManager.GetChannelLocalization(sIRCMessage.Channel, sIRCMessage.ServerName)));
					return;
				}
				
				if(sIRCMessage.Info.Length < 6)
				{
					sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("NoName", sIRCMessage.Channel, sIRCMessage.ServerName));
					return;
				}
				
				var db = SchumixBase.DManager.QueryFirstRow("SELECT * FROM wordpressinfo WHERE LOWER(Name) = '{0}' And ServerName = '{1}'", sUtilities.SqlEscape(sIRCMessage.Info[5].ToLower()), sIRCMessage.ServerName);
				if(!db.IsNull())
				{
					WordPressRss gitr = null;
					bool isstop = false;
					
					foreach(var list in RssList)
					{
						if(sIRCMessage.Info[5].ToLower() == list.Name.ToLower())
						{
							if(!list.Started)
							{
								isstop = true;
								continue;
							}
							
							list.Stop();
							gitr = list;
							isstop = true;
						}
					}
					
					if(isstop && !gitr.IsNull())
						RssList.Remove(gitr);
					
					SchumixBase.DManager.Delete("wordpressinfo", string.Format("Name = '{0}' And ServerName = '{1}'", sUtilities.SqlEscape(sIRCMessage.Info[5]), sIRCMessage.ServerName));
					sSendMessage.SendChatMessage(sIRCMessage, text[0]);
				}
				else
					sSendMessage.SendChatMessage(sIRCMessage, text[1]);
			}
			else if(sIRCMessage.Info[4].ToLower() == "change")
			{
				if(sIRCMessage.Info.Length < 6)
				{
					sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("NoCommand", sIRCMessage.Channel, sIRCMessage.ServerName));
					return;
				}
				
				if(sIRCMessage.Info[5].ToLower() == "colors")
				{
					var text = sLManager.GetCommandTexts("wordpress/change/colors", sIRCMessage.Channel, sIRCMessage.ServerName);
					if(text.Length < 2)
					{
						sSendMessage.SendChatMessage(sIRCMessage, sLConsole.Translations("NoFound2", sLManager.GetChannelLocalization(sIRCMessage.Channel, sIRCMessage.ServerName)));
						return;
					}
					
					if(sIRCMessage.Info.Length < 7)
					{
						sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("NoName", sIRCMessage.Channel, sIRCMessage.ServerName));
						return;
					}
					
					if(sIRCMessage.Info.Length < 8)
					{
						sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("StatusIsMissing", sIRCMessage.Channel, sIRCMessage.ServerName));
						return;
					}
					
					if(sIRCMessage.Info[7].ToLower() != "true" && sIRCMessage.Info[7].ToLower() != "false")
					{
						sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("ValueIsNotTrueOrFalse", sIRCMessage.Channel, sIRCMessage.ServerName));
						return;
					}
					
					var db = SchumixBase.DManager.QueryFirstRow("SELECT Colors FROM wordpressinfo WHERE LOWER(Name) = '{0}' And ServerName = '{1}'", sUtilities.SqlEscape(sIRCMessage.Info[6].ToLower()), sIRCMessage.ServerName);
					if(!db.IsNull())
					{
						bool enabled = Convert.ToBoolean(db["Colors"].ToString());
						
						if(Convert.ToBoolean(sIRCMessage.Info[7].ToLower()) == enabled)
						{
							if(enabled)
							{
								sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("FunctionAlreadyTurnedOn", sIRCMessage.Channel, sIRCMessage.ServerName));
								return;
							}
							else
							{
								sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("FunctionAlreadyTurnedOff", sIRCMessage.Channel, sIRCMessage.ServerName));
								return;
							}
						}
						
						SchumixBase.DManager.Update("wordpressinfo", string.Format("Colors = '{0}'", sIRCMessage.Info[7].ToLower()), string.Format("LOWER(Name) = '{0}' And ServerName = '{1}'", sUtilities.SqlEscape(sIRCMessage.Info[6].ToLower()), sIRCMessage.ServerName));
						sSendMessage.SendChatMessage(sIRCMessage, text[0]);
					}
					else
						sSendMessage.SendChatMessage(sIRCMessage, text[1]);
				}
				else if(sIRCMessage.Info[5].ToLower() == "shorturl")
				{
					var text = sLManager.GetCommandTexts("wordpress/change/shorturl", sIRCMessage.Channel, sIRCMessage.ServerName);
					if(text.Length < 2)
					{
						sSendMessage.SendChatMessage(sIRCMessage, sLConsole.Translations("NoFound2", sLManager.GetChannelLocalization(sIRCMessage.Channel, sIRCMessage.ServerName)));
						return;
					}
					
					if(sIRCMessage.Info.Length < 7)
					{
						sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("NoName", sIRCMessage.Channel, sIRCMessage.ServerName));
						return;
					}
					
					if(sIRCMessage.Info.Length < 8)
					{
						sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("StatusIsMissing", sIRCMessage.Channel, sIRCMessage.ServerName));
						return;
					}
					
					if(sIRCMessage.Info[7].ToLower() != "true" && sIRCMessage.Info[7].ToLower() != "false")
					{
						sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("ValueIsNotTrueOrFalse", sIRCMessage.Channel, sIRCMessage.ServerName));
						return;
					}
					
					var db = SchumixBase.DManager.QueryFirstRow("SELECT ShortUrl FROM wordpressinfo WHERE LOWER(Name) = '{0}' And ServerName = '{1}'", sUtilities.SqlEscape(sIRCMessage.Info[6].ToLower()), sIRCMessage.ServerName);
					if(!db.IsNull())
					{
						bool enabled = Convert.ToBoolean(db["ShortUrl"].ToString());
						
						if(Convert.ToBoolean(sIRCMessage.Info[7].ToLower()) == enabled)
						{
							if(enabled)
							{
								sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("FunctionAlreadyTurnedOn", sIRCMessage.Channel, sIRCMessage.ServerName));
								return;
							}
							else
							{
								sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("FunctionAlreadyTurnedOff", sIRCMessage.Channel, sIRCMessage.ServerName));
								return;
							}
						}
						
						SchumixBase.DManager.Update("wordpressinfo", string.Format("ShortUrl = '{0}'", sIRCMessage.Info[7].ToLower()), string.Format("LOWER(Name) = '{0}' And ServerName = '{1}'", sUtilities.SqlEscape(sIRCMessage.Info[6].ToLower()), sIRCMessage.ServerName));
						sSendMessage.SendChatMessage(sIRCMessage, text[0]);
					}
					else
						sSendMessage.SendChatMessage(sIRCMessage, text[1]);
				}
				else if(sIRCMessage.Info[5].ToLower() == "url")
				{
					var text = sLManager.GetCommandTexts("wordpress/change/url", sIRCMessage.Channel, sIRCMessage.ServerName);
					if(text.Length < 2)
					{
						sSendMessage.SendChatMessage(sIRCMessage, sLConsole.Translations("NoFound2", sLManager.GetChannelLocalization(sIRCMessage.Channel, sIRCMessage.ServerName)));
						return;
					}
					
					if(sIRCMessage.Info.Length < 7)
					{
						sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("NoName", sIRCMessage.Channel, sIRCMessage.ServerName));
						return;
					}
					
					if(sIRCMessage.Info.Length < 8)
					{
						sSendMessage.SendChatMessage(sIRCMessage, sLManager.GetWarningText("UrlMissing", sIRCMessage.Channel, sIRCMessage.ServerName));
						return;
					}
					
					var db = SchumixBase.DManager.QueryFirstRow("SELECT * FROM wordpressinfo WHERE LOWER(Name) = '{0}' And ServerName = '{1}'", sUtilities.SqlEscape(sIRCMessage.Info[6].ToLower()), sIRCMessage.ServerName);
					if(!db.IsNull())
					{
						sSendMessage.SendChatMessage(sIRCMessage, text[0]);
						return;
					}
					
					SchumixBase.DManager.Update("wordpressinfo", string.Format("Link = '{0}'", sUtilities.SqlEscape(sIRCMessage.Info[7])), string.Format("LOWER(Name) = '{0}' And ServerName = '{1}'", sUtilities.SqlEscape(sIRCMessage.Info[6].ToLower()), sIRCMessage.ServerName));
					WordPressRss gitr = null;
					bool isstop = false;
					
					foreach(var list in RssList)
					{
						if(sIRCMessage.Info[6].ToLower() == list.Name.ToLower())
						{
							if(!list.Started)
							{
								isstop = true;
								continue;
							}
							
							list.Stop();
							gitr = list;
							isstop = true;
						}
					}
					
					if(isstop && !gitr.IsNull())
						RssList.Remove(gitr);
					
					var db1 = SchumixBase.DManager.QueryFirstRow("SELECT Link FROM wordpressinfo WHERE LOWER(Name) = '{0}' And ServerName = '{1}'", sIRCMessage.Info[6].ToLower(), sIRCMessage.ServerName);
					if(!db1.IsNull())
					{
						var rss = new WordPressRss(sIRCMessage.ServerName, sUtilities.SqlEscape(sIRCMessage.Info[6]), db1["Link"].ToString());
						RssList.Add(rss);
						rss.Start();
					}
					
					sSendMessage.SendChatMessage(sIRCMessage, text[1]);
				}
			}
		}
	}
}