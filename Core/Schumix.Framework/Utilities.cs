/*
 * This file is part of Schumix.
 * 
 * Copyright (C) 2010-2011 Twl
 * Copyright (C) 2010-2011 Megax <http://www.megaxx.info/>
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
using System.Web;
using System.Linq;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Schumix.Framework.Extensions;

namespace Schumix.Framework
{
	public sealed class Utilities
	{
		private Utilities() {}

		public string GetUrl(string url)
		{
			string kod;

			using(var client = new WebClient())
			{
				kod = client.DownloadString(url);
			}

			return kod;
		}

		public string GetUrl(string url, string args)
		{
			string kod;
			var u = new Uri(url + HttpUtility.UrlEncode(args));

			using(var client = new WebClient())
			{
				kod = client.DownloadString(u);
			}

			return kod;
		}

		public string GetUrl(string url, string args, string noencode)
		{
			string kod;
			var u = new Uri(url + HttpUtility.UrlEncode(args) + noencode);

			using(var client = new WebClient())
			{
				kod = client.DownloadString(u);
			}

			return kod;
		}

		/// <summary>
		/// Gets the URLs in the specified text.
		/// </summary>
		/// <param name = "text">
		/// The text to search in.
		/// </param>
		/// <returns>
		/// The list of urls.
		/// </returns>
		public List<string> GetUrls(string text)
		{
			var urls = new List<string>();

			try
			{
				var urlFind = new Regex(@"(?<url>(http://)?(www\.)?\S+\.\S{2,6}([/]*\S+))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

				if(urlFind.IsMatch(text))
				{
					var matches = urlFind.Matches(text);

					foreach(var url in from Match match in matches select match.Groups["url"].ToString())
					{
						var lurl = url;
						if(!lurl.StartsWith("http://") && !url.StartsWith("https://"))
							lurl = string.Format("http://{0}", url);

						Log.Debug("Utilities", "Checking: {0}", url);
						urls.Add(lurl);
					}
				}
			}
			catch(Exception e)
			{
				Log.Error("Utilities", "Hiba oka: {0}", e.Message);
			}

			return urls;
		}

		public string GetRandomString()
		{
			string path = Path.GetRandomFileName();
			path = path.Replace(".", "");
			return path;
		}

		public string Sha1(string value)
		{
			if(value.IsNull())
				throw new ArgumentNullException("value");

			var x = new SHA1CryptoServiceProvider();
			var data = Encoding.ASCII.GetBytes(value);
			data = x.ComputeHash(data);
#if !MONO
			x.Dispose();
#endif
			var ret = string.Empty;

			for(var i = 0; i < data.Length; i++)
				ret += data[i].ToString("x2").ToLower();

			return ret;
		}

		public string Md5(string value)
		{
			if(value.IsNull())
				throw new ArgumentNullException("value");

			var x = new MD5CryptoServiceProvider();
			var data = Encoding.ASCII.GetBytes(value);
			data = x.ComputeHash(data);
#if !MONO
			x.Dispose();
#endif
			var ret = string.Empty;

			for(var i = 0; i < data.Length; i++)
				ret += data[i].ToString("x2").ToLower();

			return ret;
		}

		public string MD5File(string fileName)
		{
			if(fileName.IsNull())
				throw new ArgumentNullException("fileName");

			byte[] retVal;

			using(var file = new FileStream(fileName, FileMode.Open))
			{
				var md5 = new MD5CryptoServiceProvider();
				retVal = md5.ComputeHash(file);
#if !MONO
				md5.Dispose();
#endif
			}

			var sb = new StringBuilder();

			if(!retVal.IsNull())
			{
				for(var i = 0; i < retVal.Length; i++)
					sb.Append(retVal[i].ToString("x2"));
			}

			return sb.ToString();
		}

		public bool IsPrime(long x)
		{
			x = Math.Abs(x);

			if(x == 1 || x == 0)
				return false;

			if(x == 2)
				return true;

			if(x % 2 == 0)
				return false;

			bool p = true;

			for(var i = 3; i <= Math.Floor(Math.Sqrt(x)); i += 2)
			{
				if(x % i == 0)
				{
					p = false;
					break;
				}
			}

			return p;
		}

		public string GetPlatform()
		{
			string Platform = string.Empty;
			var pid = Environment.OSVersion.Platform;

			switch(pid)
			{
				case PlatformID.Win32NT:
				case PlatformID.Win32S:
				case PlatformID.Win32Windows:
				case PlatformID.WinCE:
					Platform = "Windows";
					break;
				case PlatformID.Unix:
					Platform = "Linux";
					break;
				case PlatformID.MacOSX:
					Platform = "MacOSX";
					break;
				case PlatformID.Xbox:
					Platform = "Xbox";
					break;
				default:
					Platform = "Ismeretlen";
					break;
			}

			return Platform;
		}

		public string GetVersion()
		{
			return Schumix.Framework.Config.Consts.SchumixVersion;
		}
	}
}