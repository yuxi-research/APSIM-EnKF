﻿// -----------------------------------------------------------------------
// <copyright file="MainMenu.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
// -----------------------------------------------------------------------
namespace UserInterface.Presenters
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using Models;
    using Models.Core;
    using global::UserInterface.Forms;

    /// <summary>
    /// This class contains methods for all main menu items that the ExplorerView exposes to the user.
    /// </summary>
    public class MainMenu
    {
        /// <summary>
        /// Reference to the ExplorerPresenter.
        /// </summary>
        private ExplorerPresenter explorerPresenter;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainMenu" /> class.
        /// </summary>
        /// <param name="explorerPresenter">The explorer presenter to work with</param>
        public MainMenu(ExplorerPresenter explorerPresenter)
        {
            this.explorerPresenter = explorerPresenter;
        }

        /// <summary>
        /// User has clicked on Save
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        [MainMenu(MenuName = "Save")]
        public void OnSaveClick(object sender, EventArgs e)
        {
            this.explorerPresenter.Save();
        }

        /// <summary>
        /// User has clicked on SaveAs
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        [MainMenu(MenuName = "Save As")]
        public void OnSaveAsClick(object sender, EventArgs e)
        {
            this.explorerPresenter.SaveAs();
        }

        /// <summary>
        /// User has clicked on Undo
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        [MainMenu(MenuName = "Undo")]
        public void OnUndoClick(object sender, EventArgs e)
        {
            this.explorerPresenter.CommandHistory.Undo();
        }

        /// <summary>
        /// User has clicked on Redo
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        [MainMenu(MenuName = "Redo")]
        public void OnRedoClick(object sender, EventArgs e)
        {
            this.explorerPresenter.CommandHistory.Redo();
        }

        /// <summary>
        /// User has clicked on Redo
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        [MainMenu(MenuName = "Split screen")]
        public void ToggleSecondExplorerViewVisible(object sender, EventArgs e)
        {
            this.explorerPresenter.MainPresenter.ToggleSecondExplorerViewVisible();
        }

        /// <summary>
        /// User has clicked on Help
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        [MainMenu(MenuName = "Help")]
        public void OnHelp(object sender, EventArgs e)
        {
            Process process = new Process();
            process.StartInfo.FileName = "http://www.apsim.info/Documentation/ApsimX/Overview.aspx";
            process.Start();
        }


    }
}