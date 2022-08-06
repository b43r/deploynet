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
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace ch.deceed.deployNET.Commands
{
    /// <summary>
    /// This command starts another process.
    /// </summary>
    class Run : Command
    {
        private string file;
        private string arguments;

        /// <summary>
        /// Create a new run command from an xml node.
        /// </summary>
        /// <param name="node">xml node</param>
        /// <param name="logger">reference to logger instance</param>
        public Run(XmlNode node, ILog logger)
            : base(node, logger)
        {
            if ((node.LocalName != "run") ||
                !node.HasAttribute("file"))
            {
                throw new ArgumentException("No valid <run> node.");
            }
            file = node.GetAttribute("file");
            arguments = node.GetAttribute("arguments");
        }

        /// <summary>
        /// Execute a process.
        /// </summary>
        /// <returns>true if successful</returns>
        public override bool Execute()
        {
            try
            {
                Process.Start(file, arguments);
                return true;
            }
            catch (Exception ex)
            {
                LastException = ex;
                LastError = String.Format(Properties.Resources.ErrorExecute, Path.GetFileName(file), arguments);
                return false;
            }
        }
    }
}
