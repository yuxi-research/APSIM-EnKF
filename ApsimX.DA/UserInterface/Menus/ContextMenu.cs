﻿// -----------------------------------------------------------------------
// <copyright file="ContextMenu.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
// -----------------------------------------------------------------------
namespace UserInterface.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.IO;
    using System.Windows.Forms;
    using Commands;
    using Models;
    using Models.Core;
    using Models.Factorial;
    using Models.Soils;
    using APSIM.Shared.Utilities;
    /// <summary>
    /// This class contains methods for all context menu items that the ExplorerView exposes to the user.
    /// </summary>
    public class ContextMenu
    {
        /// <summary>
        /// Reference to the ExplorerPresenter.
        /// </summary>
        private ExplorerPresenter explorerPresenter;

        /// <summary>
        /// The command that is currently being run.
        /// </summary>
        private RunCommand command = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextMenu" /> class.
        /// </summary>
        /// <param name="explorerPresenter">The explorer presenter to work with</param>
        public ContextMenu(ExplorerPresenter explorerPresenter)
        {
            this.explorerPresenter = explorerPresenter;
        }

        /// <summary>
        /// User has clicked rename
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        [ContextMenu(MenuName = "Rename", ShortcutKey = "F2")]
        public void OnRename(object sender, EventArgs e)
        {
            this.explorerPresenter.Rename();
        }

        /// <summary>
        /// User has clicked Copy
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        [ContextMenu(MenuName = "Copy", ShortcutKey = "Ctrl+C")]
        public void OnCopyClick(object sender, EventArgs e)
        {
            Model model = Apsim.Get(this.explorerPresenter.ApsimXFile, this.explorerPresenter.CurrentNodePath) as Model;
            if (model != null)
            {
                // Set the clipboard text.
                this.explorerPresenter.SetClipboardText(Apsim.Serialise(model));
            }
        }

        /// <summary>
        /// User has clicked Paste
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        [ContextMenu(MenuName = "Paste", ShortcutKey = "Ctrl+V")]
        public void OnPasteClick(object sender, EventArgs e)
        {
            this.explorerPresenter.Add(this.explorerPresenter.GetClipboardText(), this.explorerPresenter.CurrentNodePath);
        }

        /// <summary>
        /// User has clicked Delete
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        [ContextMenu(MenuName = "Delete", ShortcutKey = "Del")]
        public void OnDeleteClick(object sender, EventArgs e)
        {
            IModel model = Apsim.Get(this.explorerPresenter.ApsimXFile, this.explorerPresenter.CurrentNodePath) as IModel;
            if (model != null && model.GetType().Name != "Simulations")
                this.explorerPresenter.Delete(model);
        }

        /// <summary>
        /// Move up
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        [ContextMenu(MenuName = "Move up", ShortcutKey = "Ctrl+Up")]
        public void OnMoveUpClick(object sender, EventArgs e)
        {
            IModel model = Apsim.Get(this.explorerPresenter.ApsimXFile, this.explorerPresenter.CurrentNodePath) as IModel;
            if (model != null && model.GetType().Name != "Simulations")
                this.explorerPresenter.MoveUp(model);
        }

        /// <summary>
        /// Move down
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        [ContextMenu(MenuName = "Move down", ShortcutKey = "Ctrl+Down")]
        public void OnMoveDownClick(object sender, EventArgs e)
        {
            IModel model = Apsim.Get(this.explorerPresenter.ApsimXFile, this.explorerPresenter.CurrentNodePath) as IModel;
            if (model != null && model.GetType().Name != "Simulations")
                this.explorerPresenter.MoveDown(model);
        }

        /// <summary>
        /// Event handler for a User interface "Run APSIM" action
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        [ContextMenu(MenuName = "Run APSIM",
                     AppliesTo = new Type[] { typeof(Simulation),
                                              typeof(Simulations),
                                              typeof(Experiment),
                                              typeof(Folder) },
                     ShortcutKey = "F5")]
        public void RunAPSIM(object sender, EventArgs e)
        {
            RunAPSIMInternal(multiProcessRunner:false);
        }

        /// <summary>
        /// Event handler for a User interface "Run APSIM multi-process (experimental)" action
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        [ContextMenu(MenuName = "Run APSIM multi-process (experimental)",
                     AppliesTo = new Type[] { typeof(Simulation),
                                              typeof(Simulations),
                                              typeof(Experiment),
                                              typeof(Folder) },
                     ShortcutKey = "F6")]
        public void RunAPSIMMultiProcess(object sender, EventArgs e)
        {
            RunAPSIMInternal(multiProcessRunner: true);
        }

        /// <summary>Run APSIM.</summary>
        /// <param name="multiProcessRunner">Use the multi-process runner?</param>
        private void RunAPSIMInternal(bool multiProcessRunner)
        {
            if (this.explorerPresenter.Save())
            {
                List<string> duplicates = this.explorerPresenter.ApsimXFile.FindDuplicateSimulationNames();
                if (duplicates.Count > 0)
                {
                    string errorMessage = "Duplicate simulation names found " + StringUtilities.BuildString(duplicates.ToArray(), ", ");
                    explorerPresenter.MainPresenter.ShowMessage(errorMessage, Models.DataStore.ErrorLevel.Error);
                }
                else
                {
                    Model model = Apsim.Get(this.explorerPresenter.ApsimXFile, this.explorerPresenter.CurrentNodePath) as Model;

                    List<JobManager.IRunnable> jobs = new List<JobManager.IRunnable>();
                    jobs.Add(Runner.ForSimulations(this.explorerPresenter.ApsimXFile, model, false));

                    this.command = new Commands.RunCommand(jobs, model.Name, this.explorerPresenter, multiProcessRunner);
                    this.command.Do(null);
                }
            }
        }

        /// <summary>
        /// A run has completed so re-enable the run button.
        /// </summary>
        /// <returns>True when APSIM is not running</returns>
        public bool RunAPSIMEnabled()
        {
            bool isRunning = this.command != null && this.command.IsRunning;
            if (!isRunning)
            {
                this.command = null;
            }

            return !isRunning;
        }

        /// <summary>
        /// Event handler for a User interface "Run APSIM" action
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        [ContextMenu(MenuName = "Check Soil", AppliesTo = new Type[] { typeof(Soil) })]
        public void CheckSoil(object sender, EventArgs e)
        {
            Soil currentSoil = Apsim.Get(this.explorerPresenter.ApsimXFile, this.explorerPresenter.CurrentNodePath) as Soil;
            if (currentSoil != null)
            {
                string errorMessages = currentSoil.Check(false);
                if (errorMessages != string.Empty)
                {
                    this.explorerPresenter.MainPresenter.ShowMessage(errorMessages, DataStore.ErrorLevel.Error);
                }
            }
        }

        /// <summary>
        /// Accept the current test output as the official baseline for future comparison. 
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        [ContextMenu(MenuName = "Accept Tests", AppliesTo = new Type[] { typeof(Tests) })]
        public void AcceptTests(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("You are about to change the officially accepted stats for this model. Are you sure?", "Replace official stats?", MessageBoxButtons.YesNo);
            if(result != DialogResult.Yes)
            {
                return;
            }

            Tests test = Apsim.Get(this.explorerPresenter.ApsimXFile, this.explorerPresenter.CurrentNodePath) as Tests;
            try
            {
                test.Test(true);
            }
            catch (ApsimXException ex)
            {
                this.explorerPresenter.MainPresenter.ShowMessage(ex.Message, DataStore.ErrorLevel.Error);
            }
            finally
            {
                this.explorerPresenter.HideRightHandPanel();
                this.explorerPresenter.ShowRightHandPanel();
            }
        }

        /// <summary>
        /// Event handler for adding a factor
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        [ContextMenu(MenuName = "Add factor", AppliesTo = new Type[] { typeof(Factors) })]
        public void AddFactor(object sender, EventArgs e)
        {
            Model factors = Apsim.Get(this.explorerPresenter.ApsimXFile, this.explorerPresenter.CurrentNodePath) as Model;
            if (factors != null)
                this.explorerPresenter.Add("<Factor/>", this.explorerPresenter.CurrentNodePath);
        }

        /// <summary>
        /// Run post simulation models.
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        [ContextMenu(MenuName = "Refresh",
                     AppliesTo = new Type[] { typeof(DataStore) })]
        public void RunPostSimulationModels(object sender, EventArgs e)
        {
            DataStore dataStore = Apsim.Get(this.explorerPresenter.ApsimXFile, this.explorerPresenter.CurrentNodePath) as DataStore;
            if (dataStore != null)
            {
                try
                {
                    // Run all child model post processors.
                    dataStore.RunPostProcessingTools();
                    this.explorerPresenter.MainPresenter.ShowMessage("Post processing models have successfully completed", Models.DataStore.ErrorLevel.Information);
                }
                catch (Exception err)
                {
                    this.explorerPresenter.MainPresenter.ShowMessage("Error: " + err.Message, Models.DataStore.ErrorLevel.Error);
                }
            }
        }

        /// <summary>
        /// Empty the data store
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        [ContextMenu(MenuName = "Empty the data store",
                     AppliesTo = new Type[] { typeof(DataStore) })]
        public void EmptyDataStore(object sender, EventArgs e)
        {
            DataStore dataStore = Apsim.Get(this.explorerPresenter.ApsimXFile, this.explorerPresenter.CurrentNodePath) as DataStore;
            if (dataStore != null)
            {
                dataStore.DeleteAllTables(false);
            }
        }

        /// <summary>
        /// Export the data store to EXCEL format
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        [ContextMenu(MenuName = "Export to EXCEL",
                     AppliesTo = new Type[] { typeof(DataStore) })]
        public void ExportDataStoreToEXCEL(object sender, EventArgs e)
        {
            DataStore dataStore = Apsim.Get(this.explorerPresenter.ApsimXFile, this.explorerPresenter.CurrentNodePath) as DataStore;
            if (dataStore != null)
            {
                Cursor.Current = Cursors.WaitCursor;
                List<DataTable> tables = new List<DataTable>();
                foreach (string tableName in dataStore.TableNames)
                {
                    if (tableName != "Simulations" && tableName != "Messages" && tableName != "InitialConditions")
                    {
                        DataTable table = dataStore.GetData("*", tableName, true);
                        table.TableName = tableName;
                        tables.Add(table);
                    }
                }
                string fileName = Path.ChangeExtension(dataStore.Filename, ".xlsx");
                Utility.Excel.WriteToEXCEL(tables.ToArray(), fileName);
                explorerPresenter.MainPresenter.ShowMessage("Excel successfully created: " + fileName, DataStore.ErrorLevel.Information);
                Cursor.Current = Cursors.Default; 
            }
        }


        /// <summary>
        /// Export output in the data store to text files
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        [ContextMenu(MenuName = "Export output to text files",
                     AppliesTo = new Type[] { typeof(DataStore) })]
        public void ExportOutputToTextFiles(object sender, EventArgs e)
        {
            DataStore dataStore = Apsim.Get(this.explorerPresenter.ApsimXFile, this.explorerPresenter.CurrentNodePath) as DataStore;
            if (dataStore != null)
            {
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    dataStore.WriteOutputToTextFiles();
                    string folder = Path.GetDirectoryName(explorerPresenter.ApsimXFile.FileName);
                    explorerPresenter.MainPresenter.ShowMessage("Text files have been written to " + folder, DataStore.ErrorLevel.Information);
                }
                catch (Exception err)
                {
                    explorerPresenter.MainPresenter.ShowMessage(err.ToString(), DataStore.ErrorLevel.Error);
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        /// <summary>
        /// Export summary in the data store to text files
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        [ContextMenu(MenuName = "Export summary to text files",
                     AppliesTo = new Type[] { typeof(DataStore) })]
        public void ExportSummaryToTextFiles(object sender, EventArgs e)
        {
            DataStore dataStore = Apsim.Get(this.explorerPresenter.ApsimXFile, this.explorerPresenter.CurrentNodePath) as DataStore;
            if (dataStore != null)
            {
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    dataStore.WriteSummaryToTextFiles();
                    string folder = Path.GetDirectoryName(explorerPresenter.ApsimXFile.FileName);
                    explorerPresenter.MainPresenter.ShowMessage("Text files have been written to " + folder, DataStore.ErrorLevel.Information);
                }
                catch (Exception err)
                {
                    explorerPresenter.MainPresenter.ShowMessage(err.ToString(), DataStore.ErrorLevel.Error);
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        /// <summary>
        /// Event handler for a User interface "Create documentation" action
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        [ContextMenu(MenuName = "Create documentation")]
        public void CreateDocumentation(object sender, EventArgs e)
        {
            if (this.explorerPresenter.Save())
            {
                string destinationFolder = Path.Combine(Path.GetDirectoryName(this.explorerPresenter.ApsimXFile.FileName), "Doc");
                if (destinationFolder != null)
                {
                    explorerPresenter.MainPresenter.ShowMessage("Creating documentation...", DataStore.ErrorLevel.Information);
                    Cursor.Current = Cursors.WaitCursor;

                    try
                    {
                        ExportNodeCommand command = new ExportNodeCommand(this.explorerPresenter, this.explorerPresenter.CurrentNodePath);
                        this.explorerPresenter.CommandHistory.Add(command, true);
                        explorerPresenter.MainPresenter.ShowMessage("Finished creating documentation", DataStore.ErrorLevel.Information);
                        Process.Start(command.FileNameWritten);
                    }
                    catch (Exception err)
                    {
                        explorerPresenter.MainPresenter.ShowMessage(err.Message, DataStore.ErrorLevel.Error);
                    }

                    Cursor.Current = Cursors.Default;
                }
            }
        }

        /// <summary>
        /// Event handler for a Add model action
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        [ContextMenu(MenuName = "Add model...")]
        public void AddModel(object sender, EventArgs e)
        {
            object model = Apsim.Get(explorerPresenter.ApsimXFile, explorerPresenter.CurrentNodePath);
            explorerPresenter.ShowInRightHandPanel(model,
                                                   "UserInterface.Views.ListButtonView",
                                                   "UserInterface.Presenters.AddModelPresenter");
        }

        /// <summary>
        /// Event handler for a Add function action
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        [ContextMenu(MenuName = "Add function...")]
        public void AddFunction(object sender, EventArgs e)
        {
            object model = Apsim.Get(explorerPresenter.ApsimXFile, explorerPresenter.CurrentNodePath);
            explorerPresenter.ShowInRightHandPanel(model,
                                                   "UserInterface.Views.ListButtonView",
                                                   "UserInterface.Presenters.AddFunctionPresenter");
        }

        /// <summary>
        /// Event handler for a write debug document
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        [ContextMenu(MenuName = "Write debug document",
                     AppliesTo = new Type[] { typeof(Simulation) })]
        public void WriteDebugDocument(object sender, EventArgs e)
        {
            try
            {
                Simulation model = Apsim.Get(explorerPresenter.ApsimXFile, explorerPresenter.CurrentNodePath) as Simulation;
                WriteDebugDoc writeDocument = new WriteDebugDoc(explorerPresenter, model);
                writeDocument.Do(null);
            }
            catch (Exception err)
            {
                explorerPresenter.MainPresenter.ShowMessage(err.ToString(), Models.DataStore.ErrorLevel.Error);
            }
        }

    }
}