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
using System.IO;

namespace ch.deceed.deployNET.Commands
{
    /// <summary>
    /// This command moves a single file or directory.
    /// </summary>
    class Move : Command
    {
        private string src;
        private string dst;

        /// <summary>
        /// Create a new move command from an xml node.
        /// </summary>
        /// <param name="node">xml node</param>
        /// <param name="logger">reference to logger instance</param>
        public Move(XmlNode node, ILog logger)
            : base(node, logger)
        {
            if ((node.LocalName != "move") ||
                !node.HasAttribute("src") || !node.HasAttribute("dst"))
            {
                throw new ArgumentException("No valid <move> node.");
            }
            src = Utilities.GetAbsolutePath(node.GetAttribute("src"));
            dst = Utilities.GetAbsolutePath(Utilities.ReplacePlaceholders(node.GetAttribute("dst")));
        }

        /// <summary>
        /// Move a file or directory.
        /// </summary>
        /// <returns>true if successful</returns>
        public override bool Execute()
        {
            if (File.Exists(src))
            {
                // move a single file
                try
                {
                    if (File.Exists(dst))
                    {
                        File.Delete(dst);
                    }
                    File.Move(src, dst);
                }
                catch (Exception ex)
                {
                    LastException = ex;
                    LastError = String.Format(Properties.Resources.ErrorMove, src, dst);
                    return false;
                }
            }
            else if (Directory.Exists(src))
            {
                // move a directory
                try
                {
                    Directory.Move(src, dst);
                }
                catch (Exception ex)
                {
                    LastException = ex;
                    LastError = String.Format(Properties.Resources.ErrorMove, src, dst);
                    return false;
                }
            }
            else
            {
                LastException = null;
                LastError = String.Format(Properties.Resources.ErrorMoveNotFound, src);
                return false;
            }
            return true;
        }
    }
}
