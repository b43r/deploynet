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

namespace ch.deceed.deployNET
{
    /// <summary>
    /// Various extension methods.
    /// </summary>
    static class Extensions
    {
        /// <summary>
        /// Check whether an XmlNode contains an attribute.
        /// </summary>
        /// <param name="node">XmlNode</param>
        /// <param name="attribute">attribute name</param>
        /// <returns>true if attribute exists and is not empty</returns>
        public static bool HasAttribute(this XmlNode node, string attribute)
        {
            return (node.Attributes[attribute] != null) && !String.IsNullOrEmpty(node.Attributes[attribute].Value);
        }

        /// <summary>
        /// Returns the value of an XmlNode's attribute. If the attribute does not exist, an empty string is returned.
        /// </summary>
        /// <param name="node">XmlNode</param>
        /// <param name="attribute">attribute name</param>
        /// <returns>attribute value</returns>
        public static string GetAttribute(this XmlNode node, string attribute)
        {
            return (node.Attributes[attribute] != null) ? node.Attributes[attribute].Value : string.Empty;
        }  
    }
}
