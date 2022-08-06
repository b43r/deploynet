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
    /// This command copies a single file or directory.
    /// </summary>
    class Copy : Command
    {
        private string src;
        private string dst;

        /// <summary>
        /// Create a new copy command from an xml node.
        /// </summary>
        /// <param name="node">xml node</param>
        /// <param name="logger">reference to logger instance</param>
        public Copy(XmlNode node, ILog logger)
            : base(node, logger)
        {
            if ((node.LocalName != "copy") ||
                !node.HasAttribute("src") || !node.HasAttribute("dst"))
            {
                throw new ArgumentException("No valid <copy> node.");
            }
            src = Utilities.GetAbsolutePath(node.GetAttribute("src"));
            dst = Utilities.GetAbsolutePath(Utilities.ReplacePlaceholders(node.GetAttribute("dst")));
        }

        /// <summary>
        /// Recursively copy a directory with all files.
        /// </summary>
        /// <param name="srcDir">source path</param>
        /// <param name="dstDir">destination path</param>
        private void CopyDir(string srcDir, string dstDir)
        {
            if (!Directory.Exists(dstDir))
            {
                Directory.CreateDirectory(dstDir);
            }
            foreach (string file in Directory.GetFiles(srcDir))
            {
                File.Copy(file, Path.Combine(dstDir, Path.GetFileName(file)), true);
            }
            foreach (string directory in Directory.GetDirectories(srcDir))
            {
                CopyDir(directory, Path.Combine(dstDir, Path.GetFileName(directory)));
            }
        }

        /// <summary>
        /// Copy a file or directory.
        /// </summary>
        /// <returns>true if successful</returns>
        public override bool Execute()
        {
            if (File.Exists(src))
            {
                // copy a single file
                try
                {
                    File.Copy(src, dst, true);
                }
                catch (Exception ex)
                {
                    LastException = ex;
                    LastError = String.Format(Properties.Resources.ErrorCopy, src, dst);
                    return false;
                }
            }
            else if (Directory.Exists(src))
            {
                // recursively copy a directory
                try
                {
                    CopyDir(src, dst);
                }
                catch (Exception ex)
                {
                    LastException = ex;
                    LastError = String.Format(Properties.Resources.ErrorCopy, src, dst);
                    return false;
                }
            }
            else
            {
                LastException = null;
                LastError = String.Format(Properties.Resources.ErrorCopyNotFound, src);
                return false;
            }
            return true;
        }
    }
}

