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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows.Forms;
using System.Xml;

using ch.deceed.deployNET.Commands;

namespace ch.deceed.deployNET
{
    public partial class Form1 : Form, ILog
    {
        /// <summary>
        /// List of commands
        /// </summary>
        private List<Command> commands = new List<Command>();

        private int x;
        private int y;
        private bool requiresAdmin = false;
        private string defaultScript;

        [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int SendMessage(IntPtr hWnd, UInt32 Msg, int wParam, IntPtr lParam);

        const UInt32 BCM_SETSHIELD = 0x160C;

        /// <summary>
        /// Initialize form.
        /// </summary>
        public Form1()
        {
            InitializeComponent();

            panel1.Left = 0;
            panel1.Top = 0;
            panel1.Width = this.Width;
            panel1.Height = this.Height;

            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            lblVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            // register .deploy file extension
            SetFileAssociation();

            // set default script
            defaultScript = Path.Combine(
                Path.GetDirectoryName(Application.ExecutablePath),
                "script.deploy");

            // get script file from command line (/script:xxx)
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                for (int i = 1; i < args.Length; i++)
                {
                    if (args[i].ToLower().StartsWith("/script:"))
                    {
                        defaultScript = args[i].Substring("/script:".Length);
                    }
                }
            }

            // set working directory to script directory
            if (File.Exists(defaultScript))
            {
                if (!String.IsNullOrEmpty(Path.GetDirectoryName(defaultScript)))
                {
                    Environment.CurrentDirectory = Path.GetDirectoryName(defaultScript);
                }
            }

            // load commands
            try
            {
                LoadScript(defaultScript);
            }
            catch (Exception ex)
            {
                richTextBox1.SelectionColor = Color.Red;
                richTextBox1.AppendText("Failed to load script: " + ex.Message + "\r\n");
                btnStart.Enabled = false;
            }

            if (requiresAdmin && !IsAdministrator())
            {
                btnStart.FlatStyle = FlatStyle.System;
                SendMessage(btnStart.Handle, BCM_SETSHIELD, 0, (IntPtr)1);
            }
        }

        /// <summary>
        /// Load commands from an XML file.
        /// </summary>
        public void LoadScript(string scriptFile)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(scriptFile);

            XmlNode root = doc.SelectSingleNode("deployNET");
            requiresAdmin = root.GetAttribute("requiresAdmin").ToLower() == "true";

            foreach (XmlNode node in doc.SelectNodes("deployNET/*"))
            {
                if (node.LocalName == "title")
                {
                    label1.Text = node.InnerText;
                }
                else
                {
                    commands.Add(Command.Create(node, this));
                }
            }
        }

        /// <summary>
        /// Start executing commands.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, EventArgs e)
        {
            if (requiresAdmin && !IsAdministrator())
            {
                // launch itself as administrator
                ProcessStartInfo proc = new ProcessStartInfo();
                proc.UseShellExecute = true;
                proc.WorkingDirectory = Environment.CurrentDirectory;
                proc.FileName = Application.ExecutablePath;
                proc.Arguments = "/script:\"" + defaultScript + "\"";
                proc.Verb = "runas";

                try
                {
                    Process.Start(proc);
                }
                catch
                {
                    richTextBox1.ResetText();
                    richTextBox1.SelectionColor = Color.Red;
                    richTextBox1.AppendText("No administration rights granted.");
                    label1.ForeColor = Color.Red;
                    label1.Text = "Error!";
                    return;
                }

                Application.Exit();
                return;
            }


            btnExit.Enabled = false;
            btnStart.Enabled = false;
            label1.ForeColor = Color.Black;
            label1.Text = "Please wait...";
            richTextBox1.ResetText();
            Application.DoEvents();

            bool success = true;
            foreach (Command cmd in commands)
            {
                bool cmdResult = false;
                while (!cmdResult)
                {
                    richTextBox1.SelectionColor = Color.Black;
                    richTextBox1.AppendText(cmd.Description);
                    Application.DoEvents();

                    progressBar.Value = 0;
                    progressBar.Visible = cmd.HasProgress;
                    if (cmd.HasProgress)
                    {
                        cmd.Progress += cmd_Progress;
                    }

                    cmdResult = cmd.Execute();
                    progressBar.Visible = false;
                    if (cmd.HasProgress)
                    {
                        cmd.Progress -= cmd_Progress;
                    }


                    if (!cmdResult)
                    {
                        // display error and exception
                        richTextBox1.AppendText("\r\n");
                        richTextBox1.SelectionColor = Color.Red;
                        richTextBox1.AppendText(cmd.LastError);
                        if (cmd.LastException != null)
                        {
                            richTextBox1.AppendText("\r\n");
                            richTextBox1.SelectionColor = Color.Red;
                            richTextBox1.AppendText(cmd.LastException.Message);
                        }
                        richTextBox1.AppendText("\r\n");

                        // abort, retry or ignore?
                        DialogResult dr = MessageBox.Show(
                            "An error occured in the last command:\r\n\r\n" +
                            cmd.LastError +
                            ((cmd.LastException != null) ? "\r\n" + cmd.LastException.Message : ""),
                            "deployNET",
                            MessageBoxButtons.AbortRetryIgnore,
                            MessageBoxIcon.Error);
                        if (dr == DialogResult.Abort)
                        {
                            // abort
                            break;
                        }
                        else if (dr == DialogResult.Ignore)
                        {
                            // ignore
                            cmdResult = true;
                        }
                    }
                }

                if (!cmdResult)
                {
                    // abort script
                    success = false;
                    Application.DoEvents();
                    break;
                }
                richTextBox1.SelectionColor = Color.Green;
                richTextBox1.AppendText("        Done!\r\n");
                Application.DoEvents();
            }

            label1.ForeColor = success ? Color.Green : Color.Red;
            label1.Text = success ? "Done!" : "Error!";
            btnExit.Enabled = true;
            btnStart.Enabled = true;
            Application.DoEvents();
        }

        /// <summary>
        /// Update command progress.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmd_Progress(object sender, ProgressEventArgs e)
        {
            if (progressBar.Value != e.Percent)
            {
                progressBar.Value = e.Percent;
            }
        }

        /// <summary>
        /// Log a message in the main window.
        /// </summary>
        /// <param name="msg"></param>
        public void Log(string msg)
        {
            richTextBox1.SelectionColor = Color.Black;
            richTextBox1.AppendText(msg);
            Application.DoEvents();
        }

        /// <summary>
        /// Exit application.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
            Application.Exit();
        }

        /// <summary>
        /// Suppress keypress.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// Suppress keypress.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void richTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// Create a file association for the extension ".deploy" with "deployNET.exe".
        /// </summary>
        private void SetFileAssociation()
        {
            try
            {
                // check whether a file extension has been registered for local machine
                RegistrySettings reg = new RegistrySettings(Microsoft.Win32.Registry.LocalMachine, "Software\\classes");
                if (reg.ReadValue(".deploy", "", string.Empty) != "deployNET")
                {
                    // check whether a file extension has been registered for the current user
                    reg = new RegistrySettings(Microsoft.Win32.Registry.CurrentUser, "Software\\classes");
                    if (reg.ReadValue(".deploy", "", string.Empty) != "deployNET")
                    {
                        // register a new file extension
                        reg.WriteValue(".deploy", "", "deployNET");

                        reg = new RegistrySettings(Microsoft.Win32.Registry.CurrentUser, "Software\\classes");
                        reg.WriteValue("deployNET", "", "Job for deployNET");
                        reg = new RegistrySettings(Microsoft.Win32.Registry.CurrentUser, "Software\\classes\\deployNET");
                        reg.WriteValue("DefaultIcon", "", Application.ExecutablePath + ",0");

                        // open = open in deployNET
                        RegistrySettings reg2 = new RegistrySettings(Microsoft.Win32.Registry.CurrentUser, "Software\\classes\\deployNET\\shell\\open");
                        reg2.WriteValue("", "MuiVerb", "Deploy with deploy.NET");
                        reg2.WriteValue("command", "", Application.ExecutablePath + " /script:\"%1\"");

                        // edit = edit in notepad
                        RegistrySettings reg3 = new RegistrySettings(Microsoft.Win32.Registry.CurrentUser, "Software\\classes\\deployNET\\shell\\edit");
                        reg3.WriteValue("", "MuiVerb", "Edit");
                        reg3.WriteExpandableString("command", "", "%SystemRoot%\\system32\\NOTEPAD.EXE %1");
                    }
                }
            }
            catch
            { }
        }

        /// <summary>
        /// Save mouse location within control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            x = e.X;
            y = e.Y;
        }

        /// <summary>
        /// Move form according to movement of mouse cursor within control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                this.Left += (e.X - x);
                this.Top += (e.Y - y);
            }
        }

        /// <summary>
        /// Highlight version number.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblVersion_MouseEnter(object sender, EventArgs e)
        {
            lblVersion.ForeColor = System.Drawing.Color.Red;
        }

        /// <summary>
        /// Show version number in normal color again.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblVersion_MouseLeave(object sender, EventArgs e)
        {
            lblVersion.ForeColor = System.Drawing.Color.Black;
        }

        /// <summary>
        /// Show copyright info.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblVersion_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Copyright (c) 2013..2021 by Simon Baer\r\rhttps://github.com/b43r/deploynet", "About deploy.NET", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Returns a flag whether the application runs under an administrator account.
        /// </summary>
        /// <returns></returns>
        private bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// Open GitHub URL with online help.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pbHelp_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/b43r/deploynet");
        }
    }
}

