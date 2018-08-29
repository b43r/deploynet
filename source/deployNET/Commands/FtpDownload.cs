/*
 * deploy.NET
 * 
 * Copyright (C) 2013..2018 by deceed / Simon Baer
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
using System.Net.FtpClient;
using System.Windows.Forms;

namespace ch.deceed.deployNET.Commands
{
    class FtpDownload : FtpCommand
    {
        private string src;
        private string dst;

        /// <summary>
        /// Create a new FTP download command from an xml node.
        /// </summary>
        /// <param name="node">xml node</param>
        public FtpDownload(XmlNode node)
            : base(node)
        {
            if ((node.LocalName != "download") ||
                !node.HasAttribute("src") || !node.HasAttribute("dst"))
            {
                throw new ArgumentException("No valid <download> node.");
            }

            src = node.GetAttribute("src");
            dst = Utilities.GetAbsolutePath(node.GetAttribute("dst"));
        }

        /// <summary>
        /// Download a file via FTP.
        /// </summary>
        /// <param name="client">FtpClient</param>
        /// <returns>true if successful</returns>
        public override bool Execute(FtpClient client)
        {
            try
            {
                if (File.Exists(dst))
                {
                    File.Delete(dst);
                }

                using (Stream s = client.OpenRead(src))
                {
                    using (FileStream fs = new FileStream(dst, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        long bytesTotal = s.Length;
                        long bytesCopied = 0;
                        int percent = 0;
                        int oldPercent = 0;
                        byte[] buffer = new byte[32768];
                        int read;
                        while ((read = s.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fs.Write(buffer, 0, read);
                            bytesCopied += read;
                            percent = (int)Math.Round(bytesCopied * 100d / bytesTotal);
                            if (percent > oldPercent)
                            {
                                oldPercent = percent;
                                OnProgress(percent);
                            }
                            Application.DoEvents();
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                LastException = ex;
                LastError = String.Format(Properties.Resources.ErrorFtpDownload, src);
                return false;
            }
        }
    }
}
