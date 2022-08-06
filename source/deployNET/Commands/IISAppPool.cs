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
using System.Linq;
using System.Xml;

using Microsoft.Web.Administration;

namespace ch.deceed.deployNET.Commands
{
    /// <summary>
    /// This command controls an IIS application pool.
    /// </summary>
    class IISAppPool : Command
    {
        private enum Action
        {
            start,
            stop,
            recycle
        }

        private string name;
        private Action action;

        private const int SleepInterval = 100;
        private const int Timeout = 100;

        /// <summary>
        /// Create a new service command from an xml node.
        /// </summary>
        /// <param name="node">xml node</param>
        /// <param name="logger">reference to logger instance</param>
        public IISAppPool(XmlNode node, ILog logger)
            : base(node, logger)
        {
            if ((node.LocalName != "iisapppool") ||
                !node.HasAttribute("name") || !node.HasAttribute("action"))
            {
                throw new ArgumentException("No valid <iisapppool> node.");
            }

            name = node.GetAttribute("name");
            if (!Enum.TryParse(node.GetAttribute("action"), out action))
            {
                throw new ArgumentException("Invalid value for 'action' attribute in <iisapppool> node.");
            }
        }

        /// <summary>
        /// Start or stop an IIS application pool.
        /// </summary>
        /// <returns>true if successful</returns>
        public override bool Execute()
        {
            try
            {
                using (var serverMgr = new ServerManager())
                {
                    var appPool = serverMgr.ApplicationPools.SingleOrDefault(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                    if (appPool != null)
                    {
                        if (action == Action.stop)
                        {
                            if (appPool.State != ObjectState.Stopped)
                            {
                                appPool.Stop();
                                int timeout = Timeout;
                                while (appPool.State != ObjectState.Stopped && timeout > 0)
                                {
                                    System.Threading.Thread.Sleep(SleepInterval);
                                    timeout--;
                                }
                                if (appPool.State != ObjectState.Stopped)
                                {
                                    LastException = null;
                                    LastError = String.Format(Properties.Resources.ErrorStopAppPool, name);
                                    return false;
                                }
                            }
                        }
                        else if (action == Action.start)
                        {
                            if (appPool.State != ObjectState.Started)
                            {
                                appPool.Start();
                                int timeout = Timeout;
                                while (appPool.State != ObjectState.Started && timeout > 0)
                                {
                                    System.Threading.Thread.Sleep(SleepInterval);
                                    timeout--;
                                }
                                if (appPool.State != ObjectState.Started)
                                {
                                    LastException = null;
                                    LastError = String.Format(Properties.Resources.ErrorStartAppPool, name);
                                    return false;
                                }
                            }
                        }
                        else if (action == Action.recycle)
                        {
                            appPool.Recycle();
                        }
                    }
                    else
                    {
                        LastException = null;
                        LastError = String.Format(Properties.Resources.ErrorNoAppPool, name);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                LastException = ex;
                LastError = String.Format(Properties.Resources.ErrorAppPool, action, name);
                return false;
            }

            return true;
        }
    }
}
