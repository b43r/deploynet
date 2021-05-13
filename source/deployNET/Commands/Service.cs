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
using System.ServiceProcess;

namespace ch.deceed.deployNET.Commands
{
    /// <summary>
    /// This command controls a Windows service.
    /// </summary>
    class Service : Command
    {
        private enum Action
        {
            start,
            stop
        }

        private string name;
        private Action action;

        private const int Timeout = 30;

        /// <summary>
        /// Create a new service command from an xml node.
        /// </summary>
        /// <param name="node">xml node</param>
        /// <param name="logger">reference to logger instance</param>
        public Service(XmlNode node, ILog logger)
            : base(node, logger)
        {
            if ((node.LocalName != "service") ||
                !node.HasAttribute("name") || !node.HasAttribute("action"))
            {
                throw new ArgumentException("No valid <service> node.");
            }

            name = node.GetAttribute("name");
            if (!Enum.TryParse(node.GetAttribute("action"), out action))
            {
                throw new ArgumentException("Invalid value for 'action' attribute in <service> node.");
            }
        }

        /// <summary>
        /// Start or stop a Windows service.
        /// </summary>
        /// <returns>true if successful</returns>
        public override bool Execute()
        {
            try
            {
                using (ServiceController sc = new ServiceController(name))
                {
                    switch (action)
                    {
                        case Action.start:
                            if (sc.Status == ServiceControllerStatus.Stopped)
                            {
                                sc.Start();
                            }
                            sc.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, Timeout));
                            break;
                        case Action.stop:
                            if (sc.Status == ServiceControllerStatus.Running)
                            {
                                sc.Stop();
                            }
                            sc.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, Timeout));
                            break;
                    }
                    Log("\r\nService status: {0}", sc.Status);
                }
            }
            catch (Exception ex)
            {
                LastException = ex;
                LastError = String.Format(Properties.Resources.ErrorService, action, name);
                return false;
            }

            return true;
        }
    }
}
