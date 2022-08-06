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
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace ch.deceed.deployNET.Commands
{
    /// <summary>
    /// This command checks if a file or folder exists and executes a serie of other commands if not.
    /// </summary>
    class IfNotExists : Command
    {
        private List<Command> commands = new List<Command>();
        private string file;
        private string folder;

        /// <summary>
        /// Create a new ifnotexists command from an xml node.
        /// </summary>
        /// <param name="node">xml node</param>
        /// <param name="logger">reference to logger instance</param>
        public IfNotExists(XmlNode node, ILog logger)
            : base(node, logger)
        {
            if ((node.LocalName != "ifnotexists") ||
                (!node.HasAttribute("file") && !node.HasAttribute("folder")))
            {
                throw new ArgumentException("No valid <ifnotexists> node.");
            }

            file = Utilities.GetAbsolutePath(node.GetAttribute("file"));
            folder = Utilities.GetAbsolutePath(node.GetAttribute("folder"));

            foreach (XmlNode childNode in node.ChildNodes)
            {
                commands.Add(Command.Create(childNode, logger));
            }
        }

        /// <summary>
        /// Execute all child commands if the file or folder does not exists.
        /// </summary>
        /// <returns>always true</returns>
        public override bool Execute()
        {
            bool? result = null;
            if (!String.IsNullOrEmpty(file))
            {
                result = File.Exists(Environment.ExpandEnvironmentVariables(file));
            }
            if (!String.IsNullOrEmpty(folder))
            {
                result = (result ?? true) & Directory.Exists(Environment.ExpandEnvironmentVariables(folder));
            }

            if (!result ?? false)
            {
                foreach (Command cmd in commands)
                {
                    if (!cmd.Execute())
                    {
                        LastException = cmd.LastException;
                        LastError = cmd.LastError;
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
