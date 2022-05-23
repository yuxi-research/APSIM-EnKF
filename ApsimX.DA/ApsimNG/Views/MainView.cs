﻿// -----------------------------------------------------------------------
// <copyright file="TabbedExplorerView.cs"  company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
// -----------------------------------------------------------------------
namespace UserInterface.Views
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using Gtk;
    using Glade;
    using System.Reflection;

    /// <summary>An enum type for the AskQuestion method.</summary>
    public enum QuestionResponseEnum { Yes, No, Cancel }

    public interface IMainView
    {
        /// <summary>Get the start page 1 view</summary>
        IListButtonView StartPage1 { get; }

        /// <summary>Get the start page 2 view</summary>
        IListButtonView StartPage2 { get; }

        /// <summary>Add a tab form to the tab control. Optionally select the tab if SelectTab is true.</summary>
        /// <param name="text">Text for tab.</param>
        /// <param name="image">Image for tab.</param>
        /// <param name="control">Control for tab.</param>
        /// <param name="onLeftTabControl">If true a tab will be added to the left hand tab control.</param>
        void AddTab(string text, Gtk.Image image, Widget control, bool onLeftTabControl);

        /// <summary>Change the text of a tab.</summary>
        /// <param name="currentTabName">Current tab text.</param>
        /// <param name="newTabName">New text of the tab.</param>
        void ChangeTabText(object ownerView, string newTabName, string tooltip);

        Point WindowLocation { get; set; }

        /// <summary>Gets or set the main window size.</summary>
        Size WindowSize { get; set; }

        /// <summary>Gets or set the main window size.</summary>
        bool WindowMaximised { get; set; }

        /// <summary>Gets or set the main window size.</summary>
        string WindowCaption { get; set; }

        /// <summary>Turn split window on/off</summary>
        bool SplitWindowOn { get; set; }

        /// <summary>
        /// Returns true if the object is a control on the left side
        /// </summary>
        bool IsControlOnLeft(object control);

        /// <summary>Ask user for a filename to open.</summary>
        /// <param name="fileSpec">The file specification to use to filter the files.</param>
        /// <param name="initialDirectory">Optional Initial starting directory</param>
        string AskUserForOpenFileName(string fileSpec, string initialDirectory = "");

        /// <summary>
        /// A helper function that asks user for a SaveAs name and returns their new choice.
        /// </summary>
        /// <param name="fileSpec">The file specification to filter the files.</param>
        /// <param name="OldFilename">The current file name.</param>
        /// <returns>Returns the new file name or null if action cancelled by user.</returns>
        string AskUserForSaveFileName(string fileSpec, string OldFilename);

        /// <summary>Ask the user a question</summary>
        /// <param name="message">The message to show the user.</param>
        QuestionResponseEnum AskQuestion(string message);

        /// <summary>
        /// Add a status message. A message of null will clear the status message.
        /// </summary>
        /// <param name="Message"></param>
        void ShowMessage(string Message, Models.DataStore.ErrorLevel errorLevel);

        /// <summary>Show a message in a dialog box</summary>
        /// <param name="message">The message.</param>
        /// <param name="errorLevel">The error level.</param>
        int ShowMsgDialog(string message, string title, Gtk.MessageType msgType, Gtk.ButtonsType buttonType);

        /// <summary>
        /// Show progress bar with the specified percent.
        /// </summary>
        /// <param name="percent"></param>
        void ShowProgress(int percent);

        /// <summary>Set the wait cursor (or not)/</summary>
        /// <param name="wait">Shows wait cursor if true, normal cursor if false.</param>
        void ShowWaitCursor(bool wait);

        /// <summary>
        /// Display the window.
        /// </summary>
        void Show();

        /// <summary>Close the application.</summary>
        /// <param name="askToSave">If true, will ask user whether they want to save.</param>
        void Close(bool askToSave = true);

        /// <summary>Close a tab.</summary>
        /// <param name="o">A widget appearing on the tab</param>
        void CloseTabContaining(object o);

        /// <summary>Invoked when application tries to close</summary>
        event EventHandler<AllowCloseArgs> AllowClose;

        /// <summary>Invoked when a tab is closing.</summary>
        event EventHandler<TabClosingEventArgs> TabClosing;

        /// <summary>Invoked when application tries to close</summary>
        event EventHandler<EventArgs> StopSimulation;
    }

    /// <summary>
    /// TabbedExplorerView maintains multiple explorer views in a tabbed interface. It also
    /// has a StartPageView that is shown to the use when they open a new tab.
    /// </summary>
    public class MainView : ViewBase, IMainView
    {
        private static string indexTabText = "Home";

        /// <summary>Get the list and button view</summary>
        public IListButtonView StartPage1 { get { return listButtonView1; } }

        /// <summary>Get the list and button view</summary>
        public IListButtonView StartPage2 { get { return listButtonView2; } }

        /// <summary>Invoked when application tries to close</summary>
        public event EventHandler<AllowCloseArgs> AllowClose;

        /// <summary>Invoked when a tab is closing.</summary>
        public event EventHandler<TabClosingEventArgs> TabClosing;

        /// <summary>Invoked when application tries to close</summary>
        public event EventHandler<EventArgs> StopSimulation;

        private Views.ListButtonView listButtonView1;
        private Views.ListButtonView listButtonView2;

        [Widget]
        private Window window1 = null;
        [Widget]
        private ProgressBar progressBar = null;
        [Widget]
        private TextView StatusWindow = null;
        [Widget]
        private Button stopButton = null;
        [Widget]
        private Notebook notebook1 = null;
        [Widget]
        private Notebook notebook2 = null;
        [Widget]
        private VBox vbox1 = null;
        [Widget]
        private VBox vbox2 = null;
        [Widget]
        private HPaned hpaned1 = null;
        [Widget]
        private HBox hbox1 = null;

        /// <summary>Constructor</summary>
        public MainView(ViewBase owner = null) : base(owner)
        {
            Gtk.Rc.Parse(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), 
                                      ".gtkrc"));

            Glade.XML gxml = new Glade.XML("ApsimNG.Resources.Glade.MainView.glade", "window1");
            gxml.Autoconnect(this);
            _mainWidget = window1;
            window1.Icon = new Gdk.Pixbuf(null, "ApsimNG.Resources.apsim logo32.png");
            listButtonView1 = new ListButtonView(this);
            listButtonView1.ButtonsAreToolbar = true;
            vbox1.PackEnd(listButtonView1.MainWidget, true, true, 0);
            listButtonView2 = new ListButtonView(this);
            listButtonView2.ButtonsAreToolbar = true;
            vbox2.PackEnd(listButtonView2.MainWidget, true, true, 0);
            hpaned1.PositionSet = true;
            hpaned1.Child2.Hide();
            hpaned1.Child2.NoShowAll = true;
            notebook1.SetMenuLabel(vbox1, new Label(indexTabText));
            notebook2.SetMenuLabel(vbox2, new Label(indexTabText));
            hbox1.HeightRequest = 20;

            TextTag tag = new TextTag("error");
            tag.Foreground = "red";
            StatusWindow.Buffer.TagTable.Add(tag);
            tag = new TextTag("warning");
            tag.Foreground = "brown";
            StatusWindow.Buffer.TagTable.Add(tag);
            tag = new TextTag("normal");
            tag.Foreground = "blue";
            StatusWindow.ModifyBase(StateType.Normal, new Gdk.Color(0xff, 0xff, 0xf0));
            StatusWindow.Visible = false;
            stopButton.Image = new Gtk.Image(new Gdk.Pixbuf(null, "ApsimNG.Resources.MenuImages.Delete.png", 12, 12));
            stopButton.ImagePosition = PositionType.Right;
            stopButton.Image.Visible = true;
            stopButton.Clicked += OnStopClicked;
            window1.DeleteEvent += OnClosing;
            //window1.ShowAll();
            if (APSIM.Shared.Utilities.ProcessUtilities.CurrentOS.IsMac)
                InitMac();
        }

        /// <summary>
        /// Display the window.
        /// </summary>
        public void Show()
        {
            window1.ShowAll();
        }


        /// <summary>Add a tab form to the tab control. Optionally select the tab if SelectTab is true.</summary>
        /// <param name="text">Text for tab.</param>
        /// <param name="image">Image for tab.</param>
        /// <param name="control">Control for tab.</param>
        /// <param name="onLeftTabControl">If true a tab will be added to the left hand tab control.</param>
        public void AddTab(string text, Gtk.Image image, Widget control, bool onLeftTabControl)
        {
            Label tabLabel = new Label();
            // If the tab text passed in is a filename then only show the filename (no path)
            // on the tab. The ToolTipText will still have the full path and name.
            if (text.Contains(Path.DirectorySeparatorChar.ToString()))
                tabLabel.Text = Path.GetFileNameWithoutExtension(text);
            else
                tabLabel.Text = text;
            HBox headerBox = new HBox();
            Button closeBtn = new Button();
            Gtk.Image closeImg = new Gtk.Image(new Gdk.Pixbuf(null, "ApsimNG.Resources.Close.png", 12, 12));

            closeBtn.Image = closeImg;
            closeBtn.Relief = ReliefStyle.None;
            closeBtn.Clicked += OnCloseBtnClick;

            headerBox.PackStart(tabLabel);
            headerBox.PackEnd(closeBtn);

            // Wrap the whole thing inside an event box, so we can respond to a right-button or center-button click
            EventBox eventbox = new EventBox();
            eventbox.HasTooltip = text.Contains(Path.DirectorySeparatorChar.ToString());
            eventbox.TooltipText = text;
            eventbox.ButtonPressEvent += on_eventbox1_button_press_event;
            eventbox.Add(headerBox);
            eventbox.ShowAll();
            Notebook notebook = onLeftTabControl ? notebook1 : notebook2;
            notebook.CurrentPage = notebook.AppendPageMenu(control, eventbox, new Label(tabLabel.Text));
        }

        /// <summary>
        /// Inits code to allow us to use AppKit on Mac OSX
        /// This lets us use the native browser and native file open/save dialogs
        /// Thia initialisation must be done once and only once
        /// Keeping this in a separate function allows the Microsoft .Net Frameworks
        /// to run even when MonoDoc.dll is not present (that is, with Microsoft,
        /// checking for referenced DLLs seems to occur on a "method", rather than "class" basis.
        /// The Mono runtime works differently.
        /// </summary>
        private void InitMac()
        {
            MonoMac.AppKit.NSApplication.Init();
        }

        public void on_eventbox1_button_press_event(object o, ButtonPressEventArgs e)
        {
            if (e.Event.Button == 2) // Let a center-button click on a tab close that tab.
            {
                CloseTabContaining(o);
            }
        }

        /// <summary>Change the text of a tab.</summary>
        /// <param name="currentTabName">Current tab text.</param>
        /// <param name="newTabName">New text of the tab.</param>
        public void ChangeTabText(object ownerView, string newTabName, string tooltip)
        {
            if (ownerView is ExplorerView)
            {
                Widget tab = (ownerView as ExplorerView).MainWidget;
                Notebook notebook = tab.IsAncestor(notebook1) ? notebook1 : notebook2;
                // The top level of the "label" is an EventBox
                EventBox ebox = (EventBox)notebook.GetTabLabel(tab);
                ebox.TooltipText = tooltip;
                ebox.HasTooltip = !String.IsNullOrEmpty(tooltip);
                // The EventBox holds an HBox
                HBox hbox = (HBox)ebox.Child;
                // And the HBox has the actual label as its first child
                Label tabLabel = (Label)hbox.Children[0];
                tabLabel.Text = newTabName;
            }
        }

        /// <summary>Set the wait cursor (or not)/</summary>
        /// <param name="wait">Shows wait cursor if true, normal cursor if false.</param>
        public void ShowWaitCursor(bool wait)
        {
            WaitCursor = wait;
        }

        /// <summary>Close the application.</summary>
        /// <param name="askToSave">Flag to turn on the request to save</param>
        public void Close(bool askToSave)
        {
            _mainWidget.Destroy();
            Application.Quit();
        }

        /// <summary>
        /// Helper function for dealing with clicks on the tab labels, or whatever
        /// widgets the tab label might control. Tests to see which tab the 
        /// indicated objects is on. This lets us identify the tabs associated
        /// with click events, for example.
        /// </summary>
        /// <param name="o">The widget that we are seaching for</param>
        /// <returns>Page number of the tab, or -1 if not found</returns>
        private int GetTabOfWidget(object o, ref Notebook notebook, ref string tabName) // Is there a better way?
        {
            tabName = null;
            Widget widg = o as Widget;
            if (widg == null)
                return -1;
            notebook = IsControlOnLeft(o) ? notebook1 : notebook2;
            for (int i = 0; i < notebook.NPages; i++)
            {
                // First check the tab labels
                Widget testParent = notebook.GetTabLabel(notebook.GetNthPage(i));
                if (testParent == widg || widg.IsAncestor(testParent))
                {
                    tabName = notebook.GetTabLabelText(notebook.GetNthPage(i));
                    return i;
                }
                // If not found, check the tab contents
                testParent = notebook.GetNthPage(i);
                if (testParent == widg || widg.IsAncestor(testParent))
                {
                    tabName = notebook.GetTabLabelText(notebook.GetNthPage(i));
                    return i;
                }
            }
            return -1;
        }

        public void OnCloseBtnClick(object o, EventArgs e)
        {
            CloseTabContaining(o);
        }

        /// <summary>Close a tab.</summary>
        /// <param name="o">A widget appearing on the tab</param>
        public void CloseTabContaining(object o)
        {
            Notebook notebook = null;
            string tabText = null;
            int tabPage = GetTabOfWidget(o, ref notebook, ref tabText);
            if (tabPage > 0)
            {
                TabClosingEventArgs args = new TabClosingEventArgs();
                if (TabClosing != null)
                {
                    args.LeftTabControl = IsControlOnLeft(o);
                    args.Name = tabText;
                    args.Index = tabPage;
                    TabClosing.Invoke(this, args);
                }
            }
        }

        /// <summary>Gets or set the main window position.</summary>
        public Point WindowLocation
        {
            get
            {
                int x, y;
                window1.GetPosition(out x, out y);
                return new Point(x, y);
            }
            set
            {
                window1.Move(value.X, value.Y);
            }
        }

        /// <summary>Gets or set the main window size.</summary>
        public Size WindowSize
        {
            get
            {
                int width, height;
                window1.GetSize(out width, out height);
                return new Size(width, height);
            }
            set
            {
                window1.Resize(value.Width, value.Height);
            }
        }

        /// <summary>Gets or set the main window size.</summary>
        public bool WindowMaximised
        {
            get
            {
                if (window1.GdkWindow != null)
                    return window1.GdkWindow.State == Gdk.WindowState.Maximized;
                else
                    return false;
            }
            set
            {
                if (value)
                    window1.Maximize();
                else
                    window1.Unmaximize();
            }
        }

        /// <summary>Gets or set the main window size.</summary>
        public string WindowCaption
        {
            get { return window1.Title; }
            set { window1.Title = value; }
        }

        /// <summary>Turn split window on/off</summary>
        public bool SplitWindowOn
        {
            get { return hpaned1.Child2.Visible; }
            set
            {
                if (value == hpaned1.Child2.Visible)
                    return;
                if (value)
                {
                    hpaned1.Child2.Show();
                    hpaned1.Position = hpaned1.Allocation.Width / 2;
                }
                else
                    hpaned1.Child2.Hide();
            }
        }

        /// <summary>
        /// Returns true if the object is a control on the left side
        /// </summary>
        public bool IsControlOnLeft(object control)
        {
            if (control is Widget)
            {
                return (control as Widget).IsAncestor(notebook1);
            }
            else
                return false;
        }

        /// <summary>Ask user for a filename to open.</summary>
        /// <param name="fileSpec">The file specification to use to filter the files.</param>
        /// <param name="initialDirectory">Optional Initial starting directory</param>
        public string AskUserForOpenFileName(string fileSpec, string initialDirectory = "")
        {
            string fileName = AskUserForFileName("Choose a file to open", fileSpec, FileChooserAction.Open, initialDirectory);
            if (!String.IsNullOrEmpty(fileName))
            { 
                string dir = Path.GetDirectoryName(fileName);
                if (!dir.Contains(@"ApsimX\Examples"))
                    Utility.Configuration.Settings.PreviousFolder = dir;
            }
            return fileName;
        }

        /// <summary>
        /// A helper function that asks user for a SaveAs name and returns their new choice.
        /// </summary>
        /// <param name="fileSpec">The file specification to filter the files.</param>
        /// <param name="OldFilename">The current file name.</param>
        /// <returns>Returns the new file name or null if action cancelled by user.</returns>
        public string AskUserForSaveFileName(string fileSpec, string OldFilename)
        {
            string result = AskUserForFileName("Choose a file name for saving", fileSpec, FileChooserAction.Save, OldFilename);
            if (!String.IsNullOrEmpty(result))
            {
                string dir = Path.GetDirectoryName(result);
                if (!dir.Contains(@"ApsimX\Examples"))
                    Utility.Configuration.Settings.PreviousFolder = dir;
            }
            return result;
        }

        /// <summary>Ask the user a question</summary>
        /// <param name="message">The message to show the user.</param>
        public QuestionResponseEnum AskQuestion(string message)
        {
            MessageDialog md = new MessageDialog(MainWidget.Toplevel as Window, DialogFlags.Modal, MessageType.Question, ButtonsType.YesNo, message);
            md.Title = "Save changes";
            int result = md.Run();
            md.Destroy();
            switch ((Gtk.ResponseType)result)
            {
                case Gtk.ResponseType.Yes: return QuestionResponseEnum.Yes;
                case Gtk.ResponseType.No: return QuestionResponseEnum.No;
                default: return QuestionResponseEnum.Cancel;
            }
        }

        /// <summary>Add a status message to the explorer window</summary>
        /// <param name="message">The message.</param>
        /// <param name="errorLevel">The error level.</param>
        public void ShowMessage(string message, Models.DataStore.ErrorLevel errorLevel)
        {
            Gtk.Application.Invoke(delegate
            {
                StatusWindow.Visible = message != null;
                StatusWindow.Buffer.Clear();

                string tagName;
                // Output the message
                if (errorLevel == Models.DataStore.ErrorLevel.Error)
                {
                    tagName = "error";
                }
                else if (errorLevel == Models.DataStore.ErrorLevel.Warning)
                {
                    tagName = "warning";
                }
                else
                {
                    tagName = "normal";
                }
                message = message.TrimEnd("\n".ToCharArray());
                message = message.Replace("\n", "\n                      ");
                message += "\n";
                TextIter insertIter = StatusWindow.Buffer.StartIter;
                StatusWindow.Buffer.InsertWithTagsByName(ref insertIter, message, tagName);

                //this.toolTip1.SetToolTip(this.StatusWindow, message);
                progressBar.Visible = false;
                stopButton.Visible = false;
            });
        }

        /// <summary>Show a message in a dialog box</summary>
        /// <param name="message">The message.</param>
        /// <param name="errorLevel">The error level.</param>
        public int ShowMsgDialog(string message, string title, Gtk.MessageType msgType, Gtk.ButtonsType buttonType)
        {
            Gtk.MessageDialog md = new Gtk.MessageDialog(MainWidget.Toplevel as Window, Gtk.DialogFlags.Modal,
                msgType, buttonType, message);
            md.Title = title;
            int result = md.Run();
            md.Destroy();
            return result;
        }

        /// <summary>
        /// Show progress bar with the specified percent.
        /// </summary>
        /// <param name="percent"></param>
        public void ShowProgress(int percent)
        {
            // We need to use "Invoke" if the timer is running in a
            // different thread. That means we can use either
            // System.Timers.Timer or Windows.Forms.Timer in 
            // RunCommand.cs
            Gtk.Application.Invoke(delegate
            {
                progressBar.Visible = true;
                progressBar.Fraction = percent / 100.0;
                stopButton.Visible = true;
            });
        }

        /// <summary>User is trying to close the application - allow that to happen?</summary>
        /// <param name="e">Event arguments.</param>
        protected void OnClosing(object o, DeleteEventArgs e)
        {
            if (AllowClose != null)
            {
                AllowCloseArgs args = new AllowCloseArgs();
                AllowClose.Invoke(this, args);
                e.RetVal = !args.AllowClose;
            }
            else
                e.RetVal = false;
            if ((bool)e.RetVal == false)
            {
                Close(false);
            }
        }

        /// <summary>User is trying to stop all currently executing simulations.
        /// <param name="e">Event arguments.</param>
        protected void OnStopClicked(object o, EventArgs e)
        {
            if (StopSimulation != null)
            {
                EventArgs args = new EventArgs();
                StopSimulation.Invoke(this, args);
            }
        }

    }

    /// <summary>An event argument structure with a string.</summary>
    public class TabClosingEventArgs : EventArgs
    {
        public bool LeftTabControl;
        public string Name;
        public int Index;
        public bool AllowClose = true;
    }

    /// <summary>An event argument structure with a field for allow to close.</summary>
    public class AllowCloseArgs : EventArgs
    {
        public bool AllowClose;
    }

}
