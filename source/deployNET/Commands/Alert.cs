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
using System.Windows.Forms;

namespace ch.deceed.deployNET.Commands
{
    class Alert : Command
    {
        private string title;
        private string msg;

        /// <summary>
        /// Create a new copy command from an xml node.
        /// </summary>
        /// <param name="node">xml node</param>
        /// <param name="logger">reference to logger instance</param>
        public Alert(XmlNode node, ILog logger)
            : base(node, logger)
        {
            if ((node.LocalName != "alert") ||
                !node.HasAttribute("msg"))
            {
                throw new ArgumentException("No valid <alert> node.");
            }
            title = node.GetAttribute("title");
            msg = node.GetAttribute("msg");
        }

        /// <summary>
        /// Show a message.
        /// </summary>
        /// <returns>always true</returns>
        public override bool Execute()
        {
            MessageBox.Show(msg, title);
            return true;
        }
    }
}
