﻿using System;
using System.Collections.Generic;
using Glade;
using Gtk;

namespace UserInterface.Views
{
    public delegate void PositionChangedDelegate(string NewText);

    /// <summary>
    /// Describes an interface for an axis view.
    /// </summary>
    interface ILegendView
    {
        event PositionChangedDelegate OnPositionChanged;
        void Populate(string title, string[] values);

        void SetSeriesNames(string[] seriesNames);
        void SetDisabledSeriesNames(string[] seriesNames);
        string[] GetDisabledSeriesNames();
        event EventHandler DisabledSeriesChanged;
        
    }

    /// <summary>
    /// A Windows forms implementation of an AxisView
    /// </summary>
    public class LegendView : ViewBase, ILegendView
    {
        private string OriginalText;

        public event PositionChangedDelegate OnPositionChanged;
        public event EventHandler DisabledSeriesChanged;

        [Widget]
        private ComboBox combobox1 = null;
        [Widget]
        private HBox hbox1 = null;
        [Widget]
        private TreeView listview = null;

        private ListStore comboModel = new ListStore(typeof(string));
        private CellRendererText comboRender = new CellRendererText();

        private ListStore listModel = new ListStore(typeof(Boolean), typeof(string));
        private CellRendererText listRender = new CellRendererText();
        private CellRendererToggle listToggle = new CellRendererToggle();

        /// <summary>
        /// Construtor
        /// </summary>
        public LegendView(ViewBase owner) : base(owner)
        {
            Glade.XML gxml = new Glade.XML("ApsimNG.Resources.Glade.LegendView.glade", "hbox1");
            gxml.Autoconnect(this);
            _mainWidget = hbox1;
            combobox1.Model = comboModel;
            combobox1.PackStart(comboRender, false);
            combobox1.AddAttribute(comboRender, "text", 0);
            combobox1.Changed += OnPositionComboChanged;
            combobox1.Focused += OnTitleTextBoxEnter; 

            listview.Model = listModel;
            TreeViewColumn column = new TreeViewColumn();
            column.Title = "Series name";
            column.PackStart(listToggle, false);
            listRender.Editable = false;
            column.PackStart(listRender, true);
            column.SetAttributes(listToggle, "active", 0);
            column.SetAttributes(listRender, "text", 1);
            listview.AppendColumn(column);
            listToggle.Activatable = true;
            listToggle.Toggled += OnItemChecked;
            _mainWidget.Destroyed += _mainWidget_Destroyed;
        }

        private void _mainWidget_Destroyed(object sender, EventArgs e)
        {
            combobox1.Changed -= OnPositionComboChanged;
            combobox1.Focused -= OnTitleTextBoxEnter;
            listToggle.Toggled -= OnItemChecked;
        }

        private bool settingCombo = false;
        /// <summary>
        /// Populate the view with the specified title.
        /// </summary>
        public void Populate(string title, string[] values)
        {
            settingCombo = true;
            comboModel.Clear();
            foreach (string text in values)
                comboModel.AppendValues(text);
            TreeIter iter;
            if (comboModel.GetIterFirst(out iter))
            {
                string entry = (string)comboModel.GetValue(iter, 0);
                while (!entry.Equals(title, StringComparison.InvariantCultureIgnoreCase) && comboModel.IterNext(ref iter)) // Should the text matchin be case-insensitive?
                    entry = (string)comboModel.GetValue(iter, 0);
                if (entry == title)
                    combobox1.SetActiveIter(iter);
                else // Could not find a matching entry
                    combobox1.Active = 0;
            }
            OriginalText = title;
            settingCombo = false;
        }

        /// <summary>
        /// When the user 'enters' the position combo box, save the current text value for later.
        /// </summary>
        private void OnTitleTextBoxEnter(object sender, FocusedArgs e)
        {
            TreeIter iter;
            if (combobox1.GetActiveIter(out iter))
                OriginalText = (string)combobox1.Model.GetValue(iter, 0);
            else
                OriginalText = null;
        }

        /// <summary>
        /// When the user changes the combo box check to see if the text has changed. 
        /// If so then invoke the 'OnPositionChanged' event so that the presenter can pick it up.
        /// </summary>
        private void OnPositionComboChanged(object sender, EventArgs e)
        {
            if (settingCombo) return;
            TreeIter iter;
            string curText = null;
            if (combobox1.GetActiveIter(out iter))
                curText = (string)combobox1.Model.GetValue(iter, 0);
            if (OriginalText == null)
                OriginalText = curText;
            if (curText != OriginalText && OnPositionChanged != null)
            {
                OriginalText = curText;
                OnPositionChanged.Invoke(curText);
            }
        }

        /// <summary>Sets the series names.</summary>
        /// <param name="seriesNames">The series names.</param>
        public void SetSeriesNames(string[] seriesNames)
        {
            listModel.Clear();
            foreach (string seriesName in seriesNames)
                listModel.AppendValues(true, seriesName);
        }

  
        /// <summary>Sets the disabled series names.</summary>
        /// <param name="seriesNames">The series names.</param>
        public void SetDisabledSeriesNames(string[] seriesNames)
        {
            TreeIter iter;
            if (listModel.GetIterFirst(out iter))
            {
                do
                {
                    string entry = (string)listModel.GetValue(iter, 1);
                    if (Array.IndexOf(seriesNames, entry) >= 0)
                        listModel.SetValue(iter, 0, false);
                } while (listModel.IterNext(ref iter));
            }
        }

        /// <summary>Returns the index of an item.</summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public TreeIter IndexOf(string text)
        {
            TreeIter iter;
            if (listModel.GetIterFirst(out iter))
            {
                do
                {
                    if (text == (string)listModel.GetValue(iter, 1))
                        return iter;
                } while (listModel.IterNext(ref iter));
            }
            return TreeIter.Zero;
        }

        /// <summary>Gets the disabled series names.</summary>
        /// <returns></returns>
        public string[] GetDisabledSeriesNames()
        {
            List<string> disabledSeries = new List<string>();
            foreach (object[] row in listModel)
                if ((bool)row[0] == false)
                    disabledSeries.Add((string)row[1]);
            return disabledSeries.ToArray();
        }

        /// <summary>Called when user checks an item.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ItemCheckedEventArgs"/> instance containing the event data.</param>
        private void OnItemChecked(object sender, ToggledArgs e)
        {
            TreeIter iter;

            if (listModel.GetIter(out iter, new TreePath(e.Path)))
            {
                bool old = (bool)listModel.GetValue(iter, 0);
                listModel.SetValue(iter, 0, !old);
            }
            if (DisabledSeriesChanged != null)
                DisabledSeriesChanged.Invoke(this, new EventArgs());
        }
        
    }
}
