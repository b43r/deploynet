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
using System.Text;
using System.Xml;

namespace ch.deceed.deployNET.Commands
{
    public class ProgressEventArgs : EventArgs
    {
        public int Percent { get; private set; }

        public ProgressEventArgs(int percent)
        {
            Percent = percent;
        }
    }
    /// <summary>
    /// Abstract base class for all commands.
    /// </summary>
    abstract class Command
    {
        /// <summary>
        /// Gets a flag whether the command provides progress information.
        /// </summary>
        public bool HasProgress { get; protected set; }

        /// <summary>
        /// Gets the last exception that has occured.
        /// </summary>
        public Exception LastException { get; protected set; }

        /// <summary>
        /// Gets the last error that has occured.
        /// </summary>
        public string LastError { get; protected set; }

        /// <summary>
        /// Gets the command description.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <returns>true if successful</returns>
        public abstract bool Execute();

        /// <summary>
        /// Reference to logger class.
        /// </summary>
        private ILog logger;

        /// <summary>
        /// Event that provides a progress indicator.
        /// </summary>
        public event EventHandler<ProgressEventArgs> Progress;

        /// <summary>
        /// Create a new Command instance.
        /// </summary>
        /// <param name="node">XmlNode</param>
        /// <param name="logger">reference to logger instance</param>
        public Command(XmlNode node, ILog logger)
        {
            this.logger = logger;
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
            HasProgress = false;
        }

        /// <summary>
        /// Create a new Command instance from the given xml node.
        /// </summary>
        /// <param name="node">xml node</param>
        /// <param name="logger">reference to logger instance</param>
        /// <returns>Command instance</returns>
        public static Command Create(XmlNode node, ILog logger)
        {
            switch (node.LocalName)
            {
                case "copy":
                    return new Copy(node, logger);
                case "delete":
                    return new Delete(node, logger);
                case "ifexists":
                    return new IfExists(node, logger);
                case "ifnotexists":
                    return new IfNotExists(node, logger);
                case "rename":
                    return new Rename(node, logger);
                case "move":
                    return new Move(node, logger);
                case "unzip":
                    return new Unzip(node, logger);
                case "zip":
                    return new Zip(node, logger);
                case "ftp":
                    return new Ftp(node, logger);
                case "alert":
                    return new Alert(node, logger);
                case "error":
                    return new Error(node, logger);
                case "service":
                    return new Service(node, logger);
                case "replacetext":
                    return new ReplaceText(node, logger);
                case "delay":
                    return new Delay(node, logger);
                case "run":
                    return new Run(node, logger);
                case "iisapppool":
                    return new IISAppPool(node, logger);
                case "http":
                    return new Http(node, logger);
                default:
                    return new Nop(node, logger);
            }
        }

        /// <summary>
        /// Log a message in the main window.
        /// </summary>
        /// <param name="msg">text to log</param>
        /// <param name="args">parameters</param>
        protected void Log(string msg, params object[] args)
        {
            logger.Log(String.Format(msg, args));
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
