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
    /// This command renames a single file or directory.
    /// </summary>
    class Rename : Command
    {
        private string src;
        private string dst;

        /// <summary>
        /// Create a new rename command from an xml node.
        /// </summary>
        /// <param name="node">xml node</param>
        /// <param name="logger">reference to logger instance</param>
        public Rename(XmlNode node, ILog logger)
            : base(node, logger)
        {
            if ((node.LocalName != "rename") ||
                !node.HasAttribute("src") || !node.HasAttribute("dst"))
            {
                throw new ArgumentException("No valid <rename> node.");
            }
            src = Utilities.GetAbsolutePath(node.GetAttribute("src"));
            dst = Utilities.ReplacePlaceholders(node.GetAttribute("dst"));
        }

        /// <summary>
        /// Rename a file or directory.
        /// </summary>
        /// <returns>true if successful</returns>
        public override bool Execute()
        {
            if (File.Exists(src))
            {
                // rename a file
                string destination = Path.Combine(Path.GetDirectoryName(src), Path.GetFileName(dst));
                try
                {
                    if (File.Exists(destination))
                    {
                        File.Delete(destination);
                    }
                    File.Move(src, destination);
                }
                catch (Exception ex)
                {
                    LastException = ex;
                    LastError = String.Format(Properties.Resources.ErrorRename, src, Path.GetFileName(dst));
                    return false;
                }
            }
            else if (Directory.Exists(src))
            {
                // rename a directory
                try
                {
                    Directory.Move(src, Path.Combine(Path.GetDirectoryName(src), Path.GetFileName(dst)));
                }
                catch (Exception ex)
                {
                    LastException = ex;
                    LastError = String.Format(Properties.Resources.ErrorRename, src, Path.GetFileName(dst));
                    return false;
                }
            }
            else
            {
                LastException = null;
                LastError = String.Format(Properties.Resources.ErrorRenameNotFound, src);
                return false;
            }
            return true;
        }
    }
}
