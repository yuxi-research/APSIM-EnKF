﻿// -----------------------------------------------------------------------
// <copyright file="UpgradeForm.cs"  company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
// -----------------------------------------------------------------------
namespace UserInterface.Forms
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using APSIM.Shared.Utilities;
    using Glade;
    using Gtk;
    using Views;

    /// <summary>
    /// An upgrade form.
    /// </summary>
    public class UpgradeForm
    {
        public class Upgrade
        {
            public DateTime ReleaseDate { get; set; }
            public int issueNumber { get; set; }
            public string IssueTitle { get; set; }
            public string IssueURL { get; set; }
            public string ReleaseURL { get; set; }
        }

        /// <summary>
        /// A list of potential upgrades available.
        /// </summary>
        private Upgrade[] upgrades = new Upgrade[0];

        private bool loadFailure = false;

        /// <summary>
        /// Our explorer presenter.
        /// </summary>
        private IMainView tabbedExplorerView;
        [Widget]
        private Window window1 = null;
        [Widget]
        private Button button1 = null;
        [Widget]
        private Button button2 = null;
        [Widget]
        private Table table1 = null;
        [Widget]
        private Table table2 = null;
        [Widget]
        private Entry firstNameBox = null;
        [Widget]
        private Entry lastNameBox = null;
        [Widget]
        private Entry organisationBox = null;
        [Widget]
        private Entry emailBox = null;
        [Widget]
        private Entry address1Box = null;
        [Widget]
        private Entry address2Box = null;
        [Widget]
        private Entry cityBox = null;
        [Widget]
        private Entry stateBox = null;
        [Widget]
        private Entry countryBox = null;
        [Widget]
        private Entry postcodeBox = null;
        [Widget]
        private Label label1 = null;
        [Widget]
        private Alignment HTMLalign = null;
        [Widget]
        private CheckButton checkbutton1 = null;
        [Widget]
        private TreeView listview1 = null;
        [Widget]
        private Alignment alignment3 = null;
        [Widget]
        private Alignment alignment4 = null;
        [Widget]
        private Alignment alignment5 = null;
        [Widget]
        private Alignment alignment6 = null;
        [Widget]
        private Alignment alignment7 = null;
        private ListStore listmodel = new ListStore(typeof(string), typeof(string), typeof(string));
        private Views.HTMLView HTMLview;

        /// <summary>
        /// Constructor
        /// </summary>
        public UpgradeForm(IMainView explorerPresenter)
        {
            Glade.XML gxml = new Glade.XML("ApsimNG.Resources.Glade.UpgradeForm.glade", "window1");
            gxml.Autoconnect(this);
            listview1.Model = listmodel;

            CellRendererText textRender = new Gtk.CellRendererText();
            textRender.Editable = false;

            TreeViewColumn column0 = new TreeViewColumn("Version", textRender, "text", 0);
            listview1.AppendColumn(column0);
            column0.Sizing = TreeViewColumnSizing.Autosize;
            column0.Resizable = true;

            TreeViewColumn column1 = new TreeViewColumn("Description", textRender, "text", 1);
            listview1.AppendColumn(column1);
            column1.Sizing = TreeViewColumnSizing.Autosize;
            column1.Resizable = true;

            // Make the tab order a little more sensible than the defaults
            table1.FocusChain = new Widget[] { alignment7, button1, button2 };
            table2.FocusChain = new Widget[] { firstNameBox, lastNameBox, organisationBox, emailBox,
                          alignment3, alignment4, cityBox, alignment5, countryBox, alignment6 };

            HTMLview = new HTMLView(new ViewBase(null));
            HTMLalign.Add(HTMLview.MainWidget);
            this.tabbedExplorerView = explorerPresenter;
            button1.Clicked += OnUpgrade;
            button2.Clicked += OnViewMoreDetail;
            window1.Destroyed += OnFormClosing;
            window1.MapEvent += OnShown;
        }

        public void Show()
        {
            window1.ShowAll();
        }

        /// <summary>
        /// Form has loaded. Populate the form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnShown(object sender, EventArgs e)
        {
            window1.GdkWindow.Cursor = new Gdk.Cursor(Gdk.CursorType.Watch);
            while (Gtk.Application.EventsPending())
                Gtk.Application.RunIteration();
            PopulateForm();
            window1.GdkWindow.Cursor = null;
            if (loadFailure)
                window1.Destroy();
        }

        /// <summary>
        /// Populates all controls on the form.
        /// </summary>
        private void PopulateForm()
        {
            listmodel.Clear();
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            if (version.Major == 0)
                label1.Text = "You are currently using a custom build of APSIM. You cannot upgrade this to a newer version.";
            else
            {
                try
                {
                    PopulateUpgradeList();
                }
                catch (Exception)
                {
                    ShowMsgDialog("Cannot download the upgrade list.\nEither the server is down or your network connection is broken.", "Error", MessageType.Error, ButtonsType.Ok);
                    loadFailure = true;
                    return;
                }
                if (upgrades.Length > 0)
                {
                    label1.Text = "You are currently using version " + version.ToString() + ". Newer versions are listed below.";
                    label1.Text = label1.Text + Environment.NewLine + "Select an upgrade below.";
                }
                else
                    label1.Text = "You are currently using version " + version.ToString() + ". You are using the latest version.";
            }

            firstNameBox.Text = Utility.Configuration.Settings.FirstName;
            lastNameBox.Text = Utility.Configuration.Settings.LastName;
            organisationBox.Text = Utility.Configuration.Settings.Organisation;
            address1Box.Text = Utility.Configuration.Settings.Address1;
            address2Box.Text = Utility.Configuration.Settings.Address2;
            cityBox.Text = Utility.Configuration.Settings.City;
            stateBox.Text = Utility.Configuration.Settings.State;
            postcodeBox.Text = Utility.Configuration.Settings.Postcode;
            countryBox.Text = Utility.Configuration.Settings.Country;
            emailBox.Text = Utility.Configuration.Settings.Email;

            WebClient web = new WebClient();

            string tempLicenseFileName = Path.Combine(Path.GetTempPath(), "APSIM_NonCommercial_RD_licence.htm");
            if (File.Exists(tempLicenseFileName))
                File.Delete(tempLicenseFileName);

            try
            {
                // web.DownloadFile(@"https://www.apsim.info/APSIM.Registration.Portal/APSIM_NonCommercial_RD_licence.htm", tempLicenseFileName);
                // HTMLview.SetContents(File.ReadAllText(tempLicenseFileName), false, true);
                HTMLview.SetContents(@"https://www.apsim.info/APSIM.Registration.Portal/APSIM_NonCommercial_RD_licence.htm", false, true);
            }
            catch (Exception)
            {
                ShowMsgDialog("Cannot download the license.", "Error", MessageType.Error, ButtonsType.Ok);
                loadFailure = true;
            }

        }

        /// <summary>
        /// Populate the upgrade list.
        /// </summary>
        private void PopulateUpgradeList()
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            // version = new Version(0, 0, 0, 652);  
            upgrades = WebUtilities.CallRESTService<Upgrade[]>("http://www.apsim.info/APSIM.Builds.Service/Builds.svc/GetUpgradesSinceIssue?issueID=" + version.Revision);
            foreach (Upgrade upgrade in upgrades)
            {
                string versionNumber = upgrade.ReleaseDate.ToString("yyyy.MM.dd.") + upgrade.issueNumber;
                listmodel.AppendValues(versionNumber, upgrade.IssueTitle, "");
            }
            if (listmodel.IterNChildren() > 0)
                listview1.SetCursor(new TreePath("0"), null, false);
        }

        private int GetSelIndex()
        {
            TreePath selPath;
            TreeViewColumn selCol;
            listview1.GetCursor(out selPath, out selCol);
            return selPath != null ? selPath.Indices[0] : -1;
        }

        /// <summary>
        /// User is requesting more detail about a release.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnViewMoreDetail(object sender, EventArgs e)
        {
            int selIndex = GetSelIndex();
            if (selIndex >= 0)
                Process.Start(upgrades[selIndex].IssueURL);
        }

        /// <summary>
        /// User has requested an upgrade.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUpgrade(object sender, EventArgs e)
        {
            int selIndex = GetSelIndex();
            if (selIndex >= 0)
            {
                try
                {
                    if (!checkbutton1.Active)
                        throw new Exception("You must agree to the license terms before upgrading.");

                    if (String.IsNullOrWhiteSpace(firstNameBox.Text) || String.IsNullOrWhiteSpace(lastNameBox.Text) ||
                        String.IsNullOrWhiteSpace(emailBox.Text) || String.IsNullOrWhiteSpace(countryBox.Text))
                        throw new Exception("The mandatory details at the bottom of the screen (denoted with an asterisk) must be completed.");

                    Upgrade upgrade = upgrades[selIndex];
                    string versionNumber = upgrade.ReleaseDate.ToString("yyyy.MM.dd.") + upgrade.issueNumber;

                    if ((Gtk.ResponseType)ShowMsgDialog("Are you sure you want to upgrade to version " + versionNumber + "?",
                                            "Are you sure?", MessageType.Question, ButtonsType.YesNo) == Gtk.ResponseType.Yes)
                    {
                        window1.GdkWindow.Cursor = new Gdk.Cursor(Gdk.CursorType.Watch);

                        WebClient web = new WebClient();

                        string tempSetupFileName = Path.Combine(Path.GetTempPath(), "APSIMSetup.exe");

                        string sourceURL;
                        if (ProcessUtilities.CurrentOS.IsMac)
                        {
                            sourceURL = Path.ChangeExtension(upgrade.ReleaseURL, "dmg");
                            tempSetupFileName = Path.ChangeExtension(tempSetupFileName, "dmg");
                        }
                        else if (ProcessUtilities.CurrentOS.IsUnix)
                        {
                            sourceURL = System.IO.Path.ChangeExtension(upgrade.ReleaseURL, "deb");
                            tempSetupFileName = System.IO.Path.ChangeExtension(tempSetupFileName, "deb");
                        }
                        else
                            sourceURL = upgrade.ReleaseURL;

                        if (File.Exists(tempSetupFileName))
                            File.Delete(tempSetupFileName);

                        try
                        {
                            web.DownloadFile(sourceURL, tempSetupFileName);
                        }
                        catch (Exception err)
                        {
                            ShowMsgDialog("Cannot download this release. Error message is: \r\n" + err.Message, "Error", MessageType.Error, ButtonsType.Ok);
                        }

                        // Write to the registration database.
                        WriteUpgradeRegistration(versionNumber);

                        if (File.Exists(tempSetupFileName))
                        {
                            // Copy the separate upgrader executable to the temp directory.
                            string sourceUpgraderFileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Updater.exe");
                            string upgraderFileName = Path.Combine(Path.GetTempPath(), "Updater.exe");

                            // Check to see if upgrader is already running for whatever reason.
                            // Kill them if found.
                            foreach (Process process in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(upgraderFileName)))
                                process.Kill();

                            // Delete the old upgrader.
                            if (File.Exists(upgraderFileName))
                                File.Delete(upgraderFileName);
                            // Copy in the new upgrader.
                            File.Copy(sourceUpgraderFileName, upgraderFileName, true);

                            // Run the upgrader.
                            string binDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                            string ourDirectory = Path.GetFullPath(Path.Combine(binDirectory, ".."));
                            string newDirectory = Path.GetFullPath(Path.Combine(ourDirectory, "..", "APSIM" + versionNumber));
                            string arguments = StringUtilities.DQuote(ourDirectory) + " " + 
                                               StringUtilities.DQuote(newDirectory);

                            ProcessStartInfo info = new ProcessStartInfo();
                            if (ProcessUtilities.CurrentOS.IsMac)
                            {
                                info.FileName = "mono";
                                info.Arguments = upgraderFileName + " " + arguments;
                            }
                            else
                            {
                                info.FileName = upgraderFileName;
                                info.Arguments = arguments;
                            }
                            info.WorkingDirectory = Path.GetTempPath();
                            Process.Start(info);

                            window1.GdkWindow.Cursor = null;

                            // Shutdown the user interface
                            window1.Destroy();
                            tabbedExplorerView.Close();
                        }
                    }
                }
                catch (Exception err)
                {
                    window1.GdkWindow.Cursor = null;
                    ShowMsgDialog(err.Message, "Error", MessageType.Error, ButtonsType.Ok);
                }
            }
        }

        /// <summary>
        /// Write to the registration database.
        /// </summary>
        private void WriteUpgradeRegistration(string version)
        {
            string url = "http://www.apsim.info/APSIM.Registration.Service/Registration.svc/Add";
            url += "?firstName=" + firstNameBox.Text;

            url = addToURL(url, "lastName", lastNameBox.Text);
            url = addToURL(url, "organisation", organisationBox.Text);
            url = addToURL(url, "address1", address1Box.Text);
            url = addToURL(url, "address2", address2Box.Text);
            url = addToURL(url, "city", cityBox.Text);
            url = addToURL(url, "state", stateBox.Text);
            url = addToURL(url, "postcode", postcodeBox.Text);
            url = addToURL(url, "country", countryBox.Text);
            url = addToURL(url, "email", emailBox.Text);
            url = addToURL(url, "product", "APSIM Next Generation " + version);

            WebUtilities.CallRESTService<object>(url);
        }

        /// <summary>Add a key / value pair to url if not empty</summary>
        private string addToURL(string url, string key, string value)
        {
            if (value == null || value == string.Empty)
                value = "-";
            return url + "&" + key + "=" + value;
        }

        /// <summary>
        /// Form is closing - save personal details.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFormClosing(object sender, EventArgs e)
        {
            Utility.Configuration.Settings.FirstName = firstNameBox.Text;
            Utility.Configuration.Settings.LastName = lastNameBox.Text;
            Utility.Configuration.Settings.Organisation = organisationBox.Text;
            Utility.Configuration.Settings.Address1 = address1Box.Text;
            Utility.Configuration.Settings.Address2 = address2Box.Text;
            Utility.Configuration.Settings.City = cityBox.Text;
            Utility.Configuration.Settings.State = stateBox.Text;
            Utility.Configuration.Settings.Postcode = postcodeBox.Text;
            Utility.Configuration.Settings.Country = countryBox.Text;
            Utility.Configuration.Settings.Email = emailBox.Text;
        }

        /// <summary>Show a message in a dialog box</summary>
        /// <param name="message">The message.</param>
        /// <param name="errorLevel">The error level.</param>
        public int ShowMsgDialog(string message, string title, Gtk.MessageType msgType, Gtk.ButtonsType buttonType)
        {
            Gtk.MessageDialog md = new Gtk.MessageDialog(window1, Gtk.DialogFlags.Modal,
                msgType, buttonType, message);
            md.Title = title;
            int result = md.Run();
            md.Destroy();
            return result;
        }

    }
}
