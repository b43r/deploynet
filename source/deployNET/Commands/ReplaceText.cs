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
using System.IO;
using System.Text.RegularExpressions;

namespace ch.deceed.deployNET.Commands
{
    class ReplaceText : Command
    {
        private string file;
        private string src;
        private string dst;
        private string encoding;

        /// <summary>
        /// Create a new replace text command from an xml node.
        /// </summary>
        /// <param name="node">xml node</param>
        /// <param name="logger">reference to logger instance</param>
        public ReplaceText(XmlNode node, ILog logger)
            : base(node, logger)
        {
            if ((node.LocalName != "replacetext") ||
                !node.HasAttribute("src") || !node.HasAttribute("file"))
            {
                throw new ArgumentException("No valid <replacetext> node.");
            }
            file = Utilities.GetAbsolutePath(node.GetAttribute("file"));
            src = node.GetAttribute("src");
            dst = node.GetAttribute("dst");
            encoding = node.GetAttribute("encoding");
        }

        /// <summary>
        /// Replace a line in a text file.
        /// </summary>
        /// <returns>true if successful</returns>
        public override bool Execute()
        {
            FileInfo fi = new FileInfo(file);
            if (fi.Exists)
            {
                try
                {
                    bool fileUpdated = false;
                    string destination = Utilities.ReplacePlaceholders(dst);
                    
                    System.Text.Encoding enc = System.Text.Encoding.Default;
                    if (!String.IsNullOrEmpty(encoding))
                    {
                        try
                        {
                            enc = System.Text.Encoding.GetEncoding(encoding);
                        }
                        catch
                        { }
                    }

                    // read all lines
                    Regex regex = new Regex(src);
                    string[] lines = File.ReadAllLines(file, enc);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        Match m = regex.Match(lines[i]);
                        if (m.Success)
                        {
                            lines[i] = lines[i].Replace(m.Value, destination);
                            fileUpdated = true;
                        }
                    }

                    // save file if changed
                    if (fileUpdated)
                    {
                        // remove read-only attribute if neccessary
                        bool readOnly = fi.IsReadOnly;
                        if (readOnly)
                        {
                            fi.IsReadOnly = false;
                        }
                        File.WriteAllLines(file, lines, enc);
                        if (readOnly)
                        {
                            fi.IsReadOnly = true;
                        }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    LastException = ex;
                    LastError = String.Format(Properties.Resources.ErrorReplace, file);
                    return false;
                }
            }
            else
            {
                LastException = null;
                LastError = String.Format(Properties.Resources.ErrorReplaceNotFound, file);
                return false;
            }
        }
    }
}

