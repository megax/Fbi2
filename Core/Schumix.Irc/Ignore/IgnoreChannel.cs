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
using Schumix.Framework;
using Schumix.Framework.Config;
using Schumix.Framework.Extensions;
using Schumix.Framework.Localization;

namespace Schumix.Irc.Ignore
{
	public sealed class IgnoreChannel
	{
		private readonly Utilities sUtilities = Singleton<Utilities>.Instance;
		private IgnoreChannel() {}

		public bool IsIgnore(string Name)
		{
			var db = SchumixBase.DManager.QueryFirstRow("SELECT* FROM ignore_channels WHERE Channel = '{0}'", sUtilities.SqlEscape(Name.ToLower()));
			return !db.IsNull() ? true : false;
		}

		public void AddConfig()
		{
			string[] ignore = IRCConfig.IgnoreChannels.Split(SchumixBase.Comma);

			if(ignore.Length > 1)
			{
				foreach(var name in ignore)
				{
					if(name.ToLower() == IRCConfig.MasterChannel.ToLower())
						continue;

					Add(name.ToLower());
				}
			}
			else
			{
				if(IRCConfig.IgnoreChannels.ToLower() != IRCConfig.MasterChannel.ToLower())
					Add(IRCConfig.IgnoreChannels.ToLower());
			}
		}

		public void Add(string Name)
		{
			if(Name.Trim() == string.Empty)
				return;

			var db = SchumixBase.DManager.QueryFirstRow("SELECT* FROM ignore_channels WHERE Channel = '{0}'", sUtilities.SqlEscape(Name.ToLower()));
			if(!db.IsNull())
				return;

			SchumixBase.DManager.Insert("`ignore_channels`(Channel)", sUtilities.SqlEscape(Name.ToLower()));
		}

		public void Remove(string Name)
		{
			if(Name.Trim() == string.Empty)
				return;

			var db = SchumixBase.DManager.QueryFirstRow("SELECT* FROM ignore_channels WHERE Channel = '{0}'", sUtilities.SqlEscape(Name.ToLower()));
			if(db.IsNull())
				return;

			SchumixBase.DManager.Delete("ignore_channels", string.Format("Channel = '{0}'", sUtilities.SqlEscape(Name.ToLower())));
		}

		public void RemoveConfig()
		{
			string[] ignore = IRCConfig.IgnoreChannels.Split(SchumixBase.Comma);

			if(ignore.Length > 1)
			{
				foreach(var name in ignore)
					Remove(name.ToLower());
			}
			else
				Remove(IRCConfig.IgnoreChannels.ToLower());
		}
	}
}