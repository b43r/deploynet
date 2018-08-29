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
using System.Collections.Generic;
using System.Linq;

using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Locator;
using Microsoft.Build.Logging;

namespace ch.deceed.deployNET.Commands
{
    class Build : Command
    {
        private string project;
        private string platform;
        private string configuration;
        private string output;
        private string target;
        private string logfile;

        private static bool msBuildRegistered = false;

        /// <summary>
        /// Create a new msbuild command from an xml node.
        /// </summary>
        /// <param name="node">xml node</param>
        /// <param name="logger">reference to logger instance</param>
        public Build(XmlNode node, ILog logger)
            : base(node, logger)
        {
            if ((node.LocalName != "build") ||
                !node.HasAttribute("project"))
            {
                throw new ArgumentException("No valid <build> node.");
            }
            project = Utilities.GetAbsolutePath(node.GetAttribute("project"));
            platform = node.GetAttribute("platform");
            configuration = node.GetAttribute("configuration");
            output = Utilities.GetAbsolutePath(node.GetAttribute("output"));
            target = node.GetAttribute("target");
            logfile = node.GetAttribute("logfile");

            if (!msBuildRegistered)
            {
                msBuildRegistered = true;

                var instances = MSBuildLocator.QueryVisualStudioInstances().ToList();
                foreach (var inst in instances)
                {
                    logger.Log($"VisualStudio {inst.Version} detected ({inst.VisualStudioRootPath}).\r\n");
                }
                if (!instances.Any())
                {
                    logger.Log("Warning: No MSBuild found!\r\n");
                }
                else
                {
                    Environment.SetEnvironmentVariable("VSINSTALLDIR", instances.First().VisualStudioRootPath);
                    Environment.SetEnvironmentVariable("VisualStudioVersion", $"{instances.First().Version.Major}.0");
                }
            }
        }

        /// <summary>
        /// Build a project.
        /// </summary>
        /// <returns>true if successful</returns>
        public override bool Execute()
        {
            try
            {
                Dictionary<string, string> globalProperties = new Dictionary<string, string>();
                if (!String.IsNullOrEmpty(configuration))
                {
                    globalProperties.Add("Configuration", configuration);
                }

                if (!String.IsNullOrEmpty(platform))
                {
                    globalProperties.Add("Platform", platform);
                }
                if (!String.IsNullOrEmpty(output))
                {
                    globalProperties.Add("OutputPath", output);
                    if (!Directory.Exists(output))
                    {
                        Directory.CreateDirectory(output);
                    }
                }
                
                if (String.IsNullOrEmpty(target))
                {
                    target = "Build";
                }

                ProjectCollection pc = new ProjectCollection();
                BuildRequestData brd = new BuildRequestData(project, globalProperties, null, new string[] { target }, null);
                BuildParameters bp = new BuildParameters(pc);
                
                if (!String.IsNullOrEmpty(logfile))
                {
                    FileLogger logger = new FileLogger();
                    logger.Parameters = "logfile=" + logfile;
                    bp.Loggers = new List<Microsoft.Build.Framework.ILogger> { logger }.AsEnumerable();
                }

                BuildResult br = BuildManager.DefaultBuildManager.Build(bp, brd);
                pc.UnregisterAllLoggers();

                if (br.OverallResult != BuildResultCode.Success)
                {
                    LastException = br.Exception;
                    LastError = String.Format(Properties.Resources.ErrorBuild, Path.GetFileName(project));
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                LastException = ex;
                LastError = String.Format(Properties.Resources.ErrorBuild, Path.GetFileName(project));
                return false;
            }
        }
    }
}
