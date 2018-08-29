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
using System.IO;
using System.Threading;
using System.Xml;

namespace ch.deceed.deployNET.Commands
{
    /// <summary>
    /// This command deletes a single file or directory.
    /// </summary>
    class Delete : Command
    {
        private string file;
        private string folder;
        private bool recursive;

        private const int MaxRetries = 5;

        /// <summary>
        /// Create a new delete command from an xml node.
        /// </summary>
        /// <param name="node">xml node</param>
        /// <param name="logger">reference to logger instance</param>
        public Delete(XmlNode node, ILog logger)
            : base(node, logger)
        {
            if ((node.LocalName != "delete") ||
                (!node.HasAttribute("file") && !node.HasAttribute("folder")))
            {
                throw new ArgumentException("No valid <delete> node.");
            }

            file = Utilities.GetAbsolutePath(node.GetAttribute("file"));
            folder = Utilities.GetAbsolutePath(node.GetAttribute("folder"));
            recursive = node.GetAttribute("recursive") == "true";
        }

        /// <summary>
        /// Delete a file or directory.
        /// </summary>
        /// <returns>true if successful</returns>
        public override bool Execute()
        {
            if (!String.IsNullOrEmpty(file))
            {
                if (File.Exists(file))
                {
                    // delete a single file
                    try
                    {
                        File.SetAttributes(file, FileAttributes.Normal);
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        LastException = ex;
                        LastError = String.Format(Properties.Resources.ErrorDeleteFile, file);
                        return false;
                    }
                }
                else if (Path.GetFileName(file).Contains("*") || Path.GetFileName(file).Contains("?"))
                {
                    // delete multiple files
                    string path = Path.GetDirectoryName(file);
                    string mask = Path.GetFileName(file);
                    bool result = true;
                    foreach (string f in Directory.GetFiles(path, mask, SearchOption.TopDirectoryOnly))
                    {
                        try
                        {
                            File.SetAttributes(f, FileAttributes.Normal);
                            File.Delete(f);
                        }
                        catch (Exception ex)
                        {
                            result = false;
                            LastException = ex;
                        }
                    }

                    if (!result)
                    {
                        LastError = String.Format(Properties.Resources.ErrorDeleteFile, file);
                        return false;
                    }
                }
            }
            if (!String.IsNullOrEmpty(folder) && Directory.Exists(folder))
            {
                try
                {
                    int retry = MaxRetries;
                    bool success = false;
                    while (!success)
                    {
                        try
                        {
                            foreach (string f in Directory.GetFiles(folder, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                            {
                                File.SetAttributes(f, FileAttributes.Normal);
                            }
                            Directory.Delete(folder, recursive);
                            success = true;
                        }
                        catch
                        {
                            if (retry > 0)
                            {
                                Thread.Sleep(100);
                                retry--;
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LastException = ex;
                    LastError = String.Format(Properties.Resources.ErrorDeleteFolder, folder);
                    return false;
                }
            }
            return true;
        }
    }
}
