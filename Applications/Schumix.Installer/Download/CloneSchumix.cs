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
using System.IO;
using System.Net;
using NGit;
using NGit.Api;

namespace Schumix.Installer.Download
{
	sealed class CloneSchumix
	{
		public CloneSchumix(string Url, string Dir)
		{
			var clone = Git.CloneRepository();
			var repo = clone.SetURI(Url).SetDirectory(Path.Combine(Environment.CurrentDirectory, Dir)).Call();
			repo.Checkout().SetCreateBranch(true).SetUpstreamMode(CreateBranchCommand.SetupUpstreamMode.SET_UPSTREAM).SetName("origin/stable").Call();
			repo.SubmoduleInit().Call();
			repo.SubmoduleUpdate().Call();
		}
	}
}