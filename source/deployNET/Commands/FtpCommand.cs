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
using System.Text;

using FluentFTP;

namespace ch.deceed.deployNET.Commands
{
    /// <summary>
    /// Abstract base class for all FTP commands.
    /// </summary>
    abstract class FtpCommand
    {
        /// <summary>
        /// Event that provides a progress indicator.
        /// </summary>
        public event EventHandler<ProgressEventArgs> Progress;

        /// <summary>
        /// Gets the last exception that has occured.
        /// </summary>
        public Exception LastException { get; protected set; }

        /// <summary>
        /// Gets the last error that has occured.
        /// </summary>
        public string LastError { get; protected set; }

        /// <summary>
        /// Execute the FTP command.
        /// </summary>
        /// <param name="client">FtpClient</param>
        /// <returns>true if successful</returns>
        public abstract bool Execute(FtpClient client);

        /// <summary>
        /// Gets the command description.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Create a new FtpCommand instance.
        /// </summary>
        /// <param name="node">XmlNode</param>
        public FtpCommand(XmlNode node)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(node.LocalName);
            if (node.Attributes != null)
            {
                foreach (XmlAttribute attr in node.Attributes)
                {
                    sb.Append(", ");
                    sb.Append(attr.Name);
                    sb.Append("=");
                    sb.Append(attr.Value);
                }
            }
            Description = sb.ToString();
        }

        /// <summary>
        /// Create a new FtpCommand instance from the given xml node.
        /// </summary>
        /// <param name="node">xml node</param>
        /// <returns>FtpCommand instance</returns>
        public static FtpCommand Create(XmlNode node)
        {
            switch (node.LocalName)
            {
                case "upload":
                    return new FtpUpload(node);
                case "download":
                    return new FtpDownload(node);
                default:
                    throw new ArgumentException("Invalid FTP command \"" + node.LocalName + "\".");
            }
        }

        /// <summary>
        /// Raises the 'Progress' event.
        /// </summary>
        /// <param name="percent"></param>
        protected void OnProgress(int percent)
        {
            if (Progress != null)
            {
                Progress(this, new ProgressEventArgs(percent));
            }
        }
    }
}
