﻿// -----------------------------------------------------------------------
// <copyright file="ISupplementView.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
// -----------------------------------------------------------------------

namespace UserInterface.Interfaces
{
    using System;
    using Models.GrazPlan;

    public class TSuppAttrArgs : EventArgs
    {
        public int attr;
        public double attrVal;
    }

    public class TStringArgs : EventArgs
    {
        public string name;
    }

    public class TIntArgs : EventArgs
    {
        public int value;
    }

    /// <summary>
    /// Interface for a supplement view.
    /// </summary>
    public interface ISupplementView
    {
        /// <summary>
        /// Invoked when a supplement has been selected by user.
        /// </summary>
        event EventHandler<TIntArgs> SupplementSelected;

        /// <summary>
        /// Invoked when a new supplement is added.
        /// </summary>
        event EventHandler<TStringArgs> SupplementAdded;

        /// <summary>
        /// Invoked when a supplement is deleted.
        /// </summary>
        event EventHandler SupplementDeleted;

        /// <summary>
        /// Invoked when a supplement is reset to default values.
        /// </summary>
        event EventHandler SupplementReset;

        /// <summary>
        /// Invoked when all supplements are reset.
        /// </summary>
        event EventHandler AllSupplementsReset;

        event EventHandler<TSuppAttrArgs> SuppAttrChanged;

        event EventHandler<TStringArgs> SuppNameChanged;

        string[] SupplementNames { get; set; }

        string[] DefaultSuppNames { set; }

        TSupplementItem SelectedSupplementValues { set; }

        string SelectedSupplementName { get; set; }

        int SelectedSupplementIndex { set; get;  }
     }
}
