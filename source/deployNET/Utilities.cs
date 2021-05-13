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
using System.IO;

namespace ch.deceed.deployNET
{
    /// <summary>
    /// Various helper methods.
    /// </summary>
    static class Utilities
    {
        /// <summary>
        /// Replace placeholders in a filename.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ReplacePlaceholders(string input)
        {
            int p = input.IndexOf("{DATE");
            int p2 = input.IndexOf("}", p < 0 ? 0 : p);
            while ((p >= 0) && (p2 >= 0))
            {
                if (p2 >= 0)
                {
                    string placeholder = input.Substring(p, p2 - p + 1);
                    string format = placeholder.Substring(5, placeholder.Length - 6).TrimStart(':');
                    input = input.Replace(placeholder, String.IsNullOrEmpty(format) ? DateTime.Now.ToShortDateString() : DateTime.Now.ToString(format));
                }
                p = input.IndexOf("{DATE");
                p2 = input.IndexOf("}", p < 0 ? 0 : p);
            }

            return input;
        }

        /// <summary>
        /// Returns the absolute path of a relative path.
        /// </summary>
        /// <param name="input">path</param>
        /// <returns>absolute path</returns>
        public static string GetAbsolutePath(string input)
        {
            if (!String.IsNullOrEmpty(input))
            {
                try
                {
                    return Path.GetFullPath(input);
                }
                catch
                {
                    // ignore
                }
            }
            return input;
        }
    }
}
