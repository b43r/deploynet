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
using System.IO;
using System.Reflection;

using ICSharpCode.SharpZipLib.Zip;

using SevenZip;

namespace ch.deceed.deployNET.Commands
{
    /// <summary>
    /// This command extracts a ZIP file.
    /// </summary>
    class Unzip : Command
    {
        private string src;
        private string dst;
        private bool checkIntegrity;
        private bool use7zip;

        /// <summary>
        /// Create a new unzip command from an xml node.
        /// </summary>
        /// <param name="node">xml node</param>
        /// <param name="logger">reference to logger instance</param>
        public Unzip(XmlNode node, ILog logger)
            : base(node, logger)
        {
            if ((node.LocalName != "unzip") ||
                !node.HasAttribute("src") || !node.HasAttribute("dst"))
            {
                throw new ArgumentException("No valid <unzip> node.");
            }
            src = Utilities.GetAbsolutePath(node.GetAttribute("src"));
            dst = Utilities.GetAbsolutePath(node.GetAttribute("dst"));
            checkIntegrity = node.GetAttribute("check") == "true";
            use7zip = src.ToLower().EndsWith(".7z");

            if (use7zip && Environment.Is64BitProcess)
            {
                string dll = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "7z_x64.dll");
                SevenZipExtractor.SetLibraryPath(dll);
            }

            HasProgress = true;
        }

        /// <summary>
        /// Extract a ZIP file.
        /// </summary>
        /// <returns>true if successful</returns>
        public override bool Execute()
        {
            if (File.Exists(src))
            {
                try
                {
                    if (use7zip)
                    {
                        using (SevenZipExtractor extractor = new SevenZipExtractor(src))
                        {
                            if (checkIntegrity && !extractor.Check())
                            {
                                LastException = null;
                                LastError = String.Format(Properties.Resources.ErrorUnZipInvalid, src);
                                return false;
                            }

                            extractor.Extracting += extractor_Extracting;
                            extractor.ExtractArchive(dst);
                            extractor.Extracting -= extractor_Extracting;
                        }
                    }
                    else
                    {
                        
                        FastZip zip = new FastZip();
                        zip.ExtractZip(src, dst, null);
                    }
                }
                catch (Exception ex)
                {
                    LastException = ex;
                    LastError = String.Format(Properties.Resources.ErrorUnZip, src);
                    return false;
                }
            }
            else
            {
                LastException = null;
                LastError = String.Format(Properties.Resources.ErrorUnZipSrcNotFound, src);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Provide progress of file extration.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void extractor_Extracting(object sender, SevenZip.ProgressEventArgs e)
        {
            OnProgress(e.PercentDone);
        }
    }
}
