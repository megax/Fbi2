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
using System.Collections.Generic;
using Schumix.API;
using Schumix.API.Irc;
using Schumix.Irc.Ctcp;
using Schumix.Framework;
using Schumix.Framework.Addon;
using Schumix.Framework.Config;
using Schumix.Framework.Extensions;
using Schumix.Framework.Localization;

namespace Schumix.Irc.Commands
{
	public abstract partial class CommandHandler : CommandInfo
	{
		protected readonly LocalizationConsole sLConsole = Singleton<LocalizationConsole>.Instance;
		protected readonly LocalizationManager sLManager = Singleton<LocalizationManager>.Instance;
		protected readonly AddonManager sAddonManager = Singleton<AddonManager>.Instance;
		protected readonly Utilities sUtilities = Singleton<Utilities>.Instance;
		protected readonly IrcBase sIrcBase = Singleton<IrcBase>.Instance;
		public ChannelNameList sChannelNameList { get; private set; }
		public SendMessage sSendMessage { get; private set; }
		public ChannelInfo sChannelInfo { get; private set; }
		public CtcpSender sCtcpSender { get; private set; }
		public NickInfo sNickInfo { get; private set; }
		public Sender sSender { get; private set; }
		protected string ChannelPrivmsg { get; set; }
		protected string NewNickPrivmsg { get; set; }
		protected string OnlinePrivmsg { get; set; }
		protected bool IsOnline { get; set; }
		private string _servername;

		protected CommandHandler(string ServerName) : base(ServerName)
		{
			_servername = ServerName;
		}

		public void InitializeCommandHandler()
		{
			sSendMessage = new SendMessage(_servername);
			sSender = new Sender(_servername);
			sNickInfo = new NickInfo(_servername);
			sChannelInfo = new ChannelInfo(_servername);
			sCtcpSender = new CtcpSender(_servername);
			sChannelNameList = new ChannelNameList(_servername);
		}
	}
}