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
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;

using ICSharpCode.SharpZipLib.Zip;

using SevenZip;

namespace ch.deceed.deployNET.Commands
{
    /// <summary>
    /// This command creates a compressed ZIP file from a file or directory.
    /// </summary>
    class Zip : Command
    {
        private string src;
        private string dst;
        private string fileMask;
        private bool recursive;
        private bool use7zip;
        private volatile bool finished;

        /// <summary>
        /// Create a new ZIP command from an xml node.
        /// </summary>
        /// <param name="node">xml node</param>
        /// <param name="logger">reference to logger instance</param>
        public Zip(XmlNode node, ILog logger)
            : base(node, logger)
        {
            if ((node.LocalName != "zip") ||
                !node.HasAttribute("src") || !node.HasAttribute("dst"))
            {
                throw new ArgumentException("No valid <zip> node.");
            }
            src = Utilities.GetAbsolutePath(node.GetAttribute("src"));
            dst = Utilities.GetAbsolutePath(Utilities.ReplacePlaceholders(node.GetAttribute("dst")));
            fileMask = node.HasAttribute("fileMask") ? node.GetAttribute("fileMask") : string.Empty;
            recursive = node.GetAttribute("recursive") == "true";
            use7zip = dst.ToLower().EndsWith(".7z");

            if (use7zip && Environment.Is64BitProcess)
            {
                string dll = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "7z_x64.dll");
                SevenZipCompressor.SetLibraryPath(dll);
            }

            HasProgress = true;
        }

        /// <summary>
        /// Create a ZIP file.
        /// </summary>
        /// <returns>true if successful</returns>
        public override bool Execute()
        {
            if (Directory.Exists(src))
            {
                try
                {
                    if (File.Exists(dst))
                    {
                        File.Delete(dst);
                    }

                    if (use7zip)
                    {
                        SevenZipCompressor compressor = new SevenZipCompressor();

                        List<Regex> neg = new List<Regex>();
                        List<Regex> pos = new List<Regex>();
                        if (!String.IsNullOrEmpty(fileMask))
                        {
                            foreach (string mask in fileMask.Split(';'))
                            {
                                if (mask.StartsWith("-"))
                                {
                                    neg.Add(new Regex(mask.Substring(1)));
                                }
                                else
                                {
                                    pos.Add(new Regex(mask));
                                }
                            }
                        }

                        List<string> files = new List<string>();
                        foreach (string file in Directory.GetFiles(src, "*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                        {
                            bool include = true;
                            foreach (Regex re in neg)
                            {
                                if (re.IsMatch(file))
                                {
                                    include = false;
                                    break;
                                }
                            }
                            foreach (Regex re in pos)
                            {
                                if (!re.IsMatch(file))
                                {
                                    include = false;
                                    break;
                                }
                            }
                            if (include)
                            {
                                files.Add(file);
                            }
                        }

                        compressor.Compressing += compressor_Compressing;
                        compressor.CompressionFinished += compressor_CompressionFinished;
                        finished = false;
                        compressor.BeginCompressFiles(dst, files.ToArray());
                        
                        // wait until the command has ended
                        while (!finished)
                        {
                            System.Windows.Forms.Application.DoEvents();
                        }

                        compressor.Compressing -= compressor_Compressing;
                        compressor.CompressionFinished -= compressor_CompressionFinished;
                    }
                    else
                    {
                        FastZip zip = new FastZip();
                        zip.CreateZip(dst, src, recursive, fileMask);
                    }
                }
                catch (Exception ex)
                {
                    LastException = ex;
                    LastError = String.Format(Properties.Resources.ErrorZip, src);
                    return false;
                }
            }
            else
            {
                LastException = null;
                LastError = String.Format(Properties.Resources.ErrorZipSrcNotFound, src);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Indicate end of execution.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void compressor_CompressionFinished(object sender, EventArgs e)
        {
            finished = true;
        }

        /// <summary>
        /// Provide progress of file compression.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void compressor_Compressing(object sender, SevenZip.ProgressEventArgs e)
        {
            OnProgress(e.PercentDone);
        }
    }
}
