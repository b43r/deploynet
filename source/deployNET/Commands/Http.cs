/*
 * deploy.NET
 * 
 * Copyright (C) 2013..2022 by deceed / Simon Baer
 *
 * This program is free software; you can redistribute it and/or modify it under the terms
 * of the GNU General Public License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with this program;
 * If not, see http://www.gnu.org/licenses/.
 * 
 */

using System;
using System.Xml;
using System.IO;

namespace ch.deceed.deployNET.Commands
{
    /// <summary>
    /// This command downlaods a file via http(s).
    /// </summary>
    class Http : Command
    {
        private string url;
        private string dst;
        private string user;
        private string password;

        private static System.Net.WebClient client = new System.Net.WebClient();

        /// <summary>
        /// Create a new http command from an xml node.
        /// </summary>
        /// <param name="node">xml node</param>
        /// <param name="logger">reference to logger instance</param>
        public Http(XmlNode node, ILog logger)
            : base(node, logger)
        {
            if ((node.LocalName != "http") ||
                !node.HasAttribute("url") || !node.HasAttribute("dst"))
            {
                throw new ArgumentException("No valid <http> node.");
            }
            url = node.GetAttribute("url");
            dst = Utilities.GetAbsolutePath(Utilities.ReplacePlaceholders(node.GetAttribute("dst")));
            user = node.GetAttribute("user");
            password = node.GetAttribute("password");
        }

        /// <summary>
        /// Download a file via http.
        /// </summary>
        /// <returns>true if successful</returns>
        public override bool Execute()
        {
            try
            {
                if (File.Exists(dst))
                {
                    File.Delete(dst);
                }

                if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(password))
                {
                    client.Credentials = new System.Net.NetworkCredential(user, password);
                }            
                else
                {
                    client.Credentials = null;
                }

                client.DownloadFile(url, dst);
                return true;
            }
            catch (Exception ex)
            {
                LastException = ex;
                LastError = String.Format(Properties.Resources.ErrorHttp, url, dst);
                return false;
            }
        }
    }
}

