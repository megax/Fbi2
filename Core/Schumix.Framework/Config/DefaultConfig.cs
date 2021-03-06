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

namespace Schumix.Framework.Config
{
	public abstract class DefaultConfig
	{
		protected const string d_logfilename           = "Schumix.log";
		protected const bool d_logdatefilename         = false;
		protected const int d_logmaxfilesize           = 100;
		protected const int d_loglevel                 = 2;
		protected const string d_logdirectory          = "Logs";
		protected const string d_irclogdirectory       = "Channels";
		protected const bool d_irclog                  = false;
		protected const bool d_serverenabled           = false;
		protected const string d_serverhost            = "127.0.0.1";
		protected const int d_serverport               = 35220;
		protected const string d_serverpassword        = "schumix";
		protected const string d_servername            = "Default";
		protected const string d_server                = "localhost";
		protected const int d_port                     = 6667;
		protected const bool d_ssl                     = false;
		protected const string d_nickname              = "FBI-0";
		protected const string d_nickname2             = "_FBI-0";
		protected const string d_nickname3             = "__FBI-0";
		protected const string d_username              = "FBI";
		protected const string d_userinfo              = "FBI IRC Bot";
		protected const string d_masterchannel         = "#fbi";
		protected const string d_masterchannelpassword = " ";
		protected const string d_ignorechannels        = " ";
		protected const string d_ignorenames           = " ";
		protected const bool d_usenickserv             = false;
		protected const string d_nickservpassword      = "password";
		protected const bool d_usehostserv             = false;
		protected const bool d_hostservstatus          = false;
		protected const int d_messagesending           = 400;
		protected const string d_commandprefix         = "$";
		protected const string d_messagetype           = "Privmsg";
		protected const bool d_mysqlenabled            = false;
		protected const string d_mysqlhost             = "localhost";
		protected const string d_mysqluser             = "root";
		protected const string d_mysqlpassword         = "password";
		protected const string d_mysqldatabase         = "database";
		protected const string d_mysqlcharset          = "utf8";
		protected const bool d_sqliteenabled           = true;
		protected const string d_sqlitefilename        = "Schumix.db3";
		protected const string d_crashdirectory        = "Dumps";
		protected const string d_locale                = "enUS";
		protected const int d_shutdownmaxmemory        = 100;
		protected const bool d_cleanconfig             = false;
		protected const bool d_cleandatabase           = false;
		protected bool errors                          = false;
	}
}