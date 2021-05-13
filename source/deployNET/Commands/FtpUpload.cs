/*
 * deploy.NET
 * 
 * Copyright (C) 2013..2021 by deceed / Simon Baer
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
using System.Windows.Forms;

using FluentFTP;

namespace ch.deceed.deployNET.Commands
{
    class FtpUpload : FtpCommand
    {
        private string src;
        private string dst;

        /// <summary>
        /// Create a new FTP download command from an xml node.
        /// </summary>
        /// <param name="node">xml node</param>
        public FtpUpload(XmlNode node)
            : base(node)
        {
            if ((node.LocalName != "upload") ||
                !node.HasAttribute("src") || !node.HasAttribute("dst"))
            {
                throw new ArgumentException("No valid <upload> node.");
            }

            src = Utilities.GetAbsolutePath(node.GetAttribute("src"));
            dst = node.GetAttribute("dst");
        }

        /// <summary>
        /// Upload a file via FTP.
        /// </summary>
        /// <param name="client">FtpClient</param>
        /// <returns>true if successful</returns>
        public override bool Execute(FtpClient client)
        {
            if (File.Exists(src))
            {
                try
                {
                    int oldPercent = 0;
                    Action<FtpProgress> progress = new Action<FtpProgress>(x => {
                        if (x.Progress > 0)
                        {
                            if ((int)Math.Round(x.Progress) > oldPercent)
                            {
                                oldPercent = (int)Math.Round(x.Progress);
                                OnProgress(oldPercent);
                                Application.DoEvents();
                            }
                        }
                    });

                    var status = client.UploadFile(src, dst, FtpRemoteExists.Overwrite, false, FtpVerify.None, progress);
                    return status == FtpStatus.Success;
                }
                catch (Exception ex)
                {
                    LastException = ex;
                    LastError = String.Format(Properties.Resources.ErrorFtpUpload, src);
                    return false;
                }
            }
            else
            {
                LastException = null;
                LastError = String.Format(Properties.Resources.ErrorFtpUploadNotFound, src);
                return false;
            }
        }
    }
}
