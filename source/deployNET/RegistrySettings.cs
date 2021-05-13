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

using Microsoft.Win32;

namespace ch.deceed.deployNET
{
    /// <summary>
    /// This class encapsulates the access to the windows registry.
    /// </summary>
    public class RegistrySettings
    {
        private RegistryKey rootKey;
        private string[] subKeys;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="rootKey">registry root key</param>
        /// <param name="subKey">one or more subkeys</param>
        public RegistrySettings(RegistryKey rootKey, string subKey)
        {
            this.rootKey = rootKey;

            if (subKey.StartsWith("/"))
            {
                subKey = subKey.Remove(0, 1);
            }
            if (subKey.EndsWith("/"))
            {
                subKey = subKey.Remove(subKey.Length - 1, 1);
            }

            this.subKeys = subKey.Split('/');
        }

        /// <summary>
        /// Read an object from the registry.
        /// </summary>
        /// <param name="section">section to read from</param>
        /// <param name="key">key to read</param>
        /// <param name="defaultValue">default value returned if key does not exist</param>
        /// <returns>object from registry or null</returns>
        private object ReadObject(string section, string key)
        {
            object result = null;

            // open key
            RegistryKey workKey = rootKey;
            foreach (string subKey in subKeys)
            {
                if (workKey != null)
                {
                    workKey = workKey.OpenSubKey(subKey);
                }
            }

            if (workKey != null)
            {
                // if a "section" if given open it...
                if (section != "")
                {
                    workKey = workKey.OpenSubKey(section);
                }
                if (workKey != null)
                {
                    // read value
                    result = workKey.GetValue(key);
                }
            }

            return result;
        }

        /// <summary>
        /// Read an integer value from the registry.
        /// </summary>
        /// <param name="section">section to read from</param>
        /// <param name="key">key to read</param>
        /// <param name="defaultValue">default value returned if key does not exist</param>
        /// <returns>value from registry or default value</returns>
        public int ReadValue(string section, string key, int defaultValue)
        {
            int result = defaultValue;

            try
            {
                object obj = ReadObject(section, key);
                if (obj != null)
                {
                    result = Int32.Parse(obj.ToString());
                }
            }
            catch (System.Exception)
            { }

            return result;
        }

        /// <summary>
        /// Read a string value from the registry.
        /// </summary>
        /// <param name="section">section to read from</param>
        /// <param name="key">key to read</param>
        /// <param name="defaultValue">default value returned if key does not exist</param>
        /// <returns>value from registry or default value</returns>
        public string ReadValue(string section, string key, string defaultValue)
        {
            string result = defaultValue;

            try
            {
                object obj = ReadObject(section, key);
                if (obj != null)
                {
                    result = obj.ToString();
                }
            }
            catch (System.Exception)
            { }

            return result;
        }

        /// <summary>
        /// Read a bool value from the registry.
        /// </summary>
        /// <param name="section">section to read from</param>
        /// <param name="key">key to read</param>
        /// <param name="defaultValue">default value returned if key does not exist</param>
        /// <returns>value from registry or default value</returns>
        public bool ReadValue(string section, string key, bool defaultValue)
        {
            bool result = defaultValue;

            try
            {
                object obj = ReadObject(section, key);
                if (obj != null)
                {
                    result = bool.Parse(obj.ToString());
                }
            }
            catch (System.Exception)
            { }

            return result;
        }

        /// <summary>
        /// Write an object into the registry.
        /// </summary>
        /// <param name="section">section to write into</param>
        /// <param name="key">key to write</param>
        /// <param name="objectValue">value to write</param>
        /// <param name="valueKind">optionally the type of the registry key</param>
        /// <returns>true if successful</returns>
        private bool WriteObject(string section, string key, object objectValue, RegistryValueKind valueKind = RegistryValueKind.Unknown)
        {
            bool result = false;
            
            try
            {
                // open key and create missing subkeys...
                RegistryKey workKey = rootKey;
                foreach (string subKey in subKeys)
                {
                    if (workKey != null)
                    {
                        workKey = workKey.CreateSubKey(subKey);
                    }
                }

                if (workKey != null)
                {
                    // if a "section" is given try to open or create it
                    if (section != "")
                    {
                        workKey = workKey.CreateSubKey(section);
                    }
                    if (workKey != null)
                    {
                        if (valueKind != RegistryValueKind.Unknown)
                        {
                            workKey.SetValue(key, objectValue, valueKind);
                        }
                        else
                        {
                            workKey.SetValue(key, objectValue);
                        }
                        result = true;
                    }
                }
            }
            catch (System.Exception)
            { }

            return result;
        }

        /// <summary>
        /// Write an integer value to the registry.
        /// </summary>
        /// <param name="section">section to write into</param>
        /// <param name="key">key to write</param>
        /// <param name="intValue">value to write</param>
        /// <returns>true if successful</returns>
        public bool WriteValue(string section, string key, int intValue)
        {
            return WriteObject(section, key, intValue);
        }

        /// <summary>
        /// Write a string value to the registry.
        /// </summary>
        /// <param name="section">section to write into</param>
        /// <param name="key">key to write</param>
        /// <param name="stringValue">value to write</param>
        /// <returns>true if successful</returns>
        public bool WriteValue(string section, string key, string stringValue)
        {
            return WriteObject(section, key, stringValue);
        }

        /// <summary>
        /// Write an expandable string value to the registry.
        /// </summary>
        /// <param name="section">section to write into</param>
        /// <param name="key">key to write</param>
        /// <param name="stringValue">value to write</param>
        /// <returns>true if successful</returns>
        public bool WriteExpandableString(string section, string key, string stringValue)
        {
            return WriteObject(section, key, stringValue, RegistryValueKind.ExpandString);
        }

        /// <summary>
        /// Write a bool value to the registry.
        /// </summary>
        /// <param name="section">section to write into</param>
        /// <param name="key">key to write</param>
        /// <param name="boolValue">value to write</param>
        /// <returns>true if successful</returns>
        public bool WriteValue(string section, string key, bool boolValue)
        {
            return WriteObject(section, key, boolValue);
        }
    }
}
