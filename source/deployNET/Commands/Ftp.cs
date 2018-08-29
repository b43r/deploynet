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
using System.Collections.Generic;
using System.Xml;
using System.Net.FtpClient;

namespace ch.deceed.deployNET.Commands
{
    /// <summary>
    /// This command downloads or uploads files from/to an FTP server.
    /// </summary>
    class Ftp : Command
    {
        private string userName;
        private string password;
        private string server;

        private List<FtpCommand> commands = new List<FtpCommand>();

        /// <summary>
        /// Create a new unzip command from an xml node.
        /// </summary>
        /// <param name="node">xml node</param>
        /// <param name="logger">reference to logger instance</param>
        public Ftp(XmlNode node, ILog logger)
            : base(node, logger)
        {
            if ((node.LocalName != "ftp") ||
                !node.HasAttribute("user") || !node.HasAttribute("password") || !node.HasAttribute("server"))
            {
                throw new ArgumentException("No valid <ftp> node.");
            }

            userName = node.GetAttribute("user");
            password = node.GetAttribute("password");
            server = node.GetAttribute("server");

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                {
                    FtpCommand cmd = FtpCommand.Create(child);
                    cmd.Progress += cmd_Progress;
                    commands.Add(cmd);
                }
            }

            HasProgress = true;
        }

        /// <summary>
        /// Forward the progress event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void cmd_Progress(object sender, ProgressEventArgs e)
        {
            OnProgress(e.Percent);
        }

        /// <summary>
        /// Up- or download a file.
        /// </summary>
        /// <returns>true if successful</returns>
        public override bool Execute()
        {
            try
            {
                using (FtpClient client = new FtpClient())
                {
                    client.Credentials = new System.Net.NetworkCredential(userName, password);
                    client.Host = server;
                    client.Connect();
                    //client.DataConnectionType = FtpDataConnectionType.AutoPassive;

                    // execute all commands
                    bool success = true;
                    if (client.IsConnected)
                    {
                        client.SetDataType(FtpDataType.Binary);

                        foreach (FtpCommand cmd in commands)
                        {
                            Log("\r\n" + cmd.Description);
                            success &= cmd.Execute(client);
                            if (!success)
                            {
                                LastError = cmd.LastError;
                                LastException = cmd.LastException;
                                break;
                            }
                        }
                    }
                    client.Disconnect();
                    return success;
                }
            }
            catch (Exception ex)
            {
                LastException = ex;
                LastError = Properties.Resources.ErrorFtp;
                return false;
            }
        }
    }
}
