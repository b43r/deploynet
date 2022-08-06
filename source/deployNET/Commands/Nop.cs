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

using System.Xml;

namespace ch.deceed.deployNET.Commands
{
    /// <summary>
    /// This command does nothing.
    /// </summary>
    class Nop : Command
    {
        /// <summary>
        /// Create a new command that does nothing.
        /// </summary>
        /// <param name="node">xml node</param>
        /// <param name="logger">reference to logger instance</param>
        public Nop(XmlNode node, ILog logger)
            : base(node, logger)
        { }

        /// <summary>
        /// Do nothing.
        /// </summary>
        /// <returns>true</returns>
        public override bool Execute()
        {
            return true;
        }
    }
}
