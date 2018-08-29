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

namespace ch.deceed.deployNET.Commands
{
    class Error : Command
    {
        private string msg;

        /// <summary>
        /// Create a new copy command from an xml node.
        /// </summary>
        /// <param name="node">xml node</param>
        /// <param name="logger">reference to logger instance</param>
        public Error(XmlNode node, ILog logger)
            : base(node, logger)
        {
            if (node.LocalName != "error")
            {
                throw new ArgumentException("No valid <error> node.");
            }
            msg = node.InnerText;
        }

        /// <summary>
        /// Stop script execution with an error.
        /// </summary>
        /// <returns>false</returns>
        public override bool Execute()
        {
            LastException = null;
            LastError = msg;
            return false;
        }
    }
}
