﻿// -----------------------------------------------------------------------
// <copyright file="IGridCell.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
// -----------------------------------------------------------------------
using System;
namespace UserInterface.Interfaces
{
    /// <summary>
    /// An enumeration that specifies the allowed editor types for the cell.
    /// </summary>
    public enum EditorTypeEnum
    {
        /// <summary>
        /// A normal text box editor
        /// </summary>
        TextBox,

        /// <summary>
        /// A date time editor
        /// </summary>
        DateTime,

        /// <summary>
        /// A boolean checkbox editor
        /// </summary>
        Boolean,

        /// <summary>
        /// A color editor
        /// </summary>
        Colour,

        /// <summary>
        /// A drop down editor
        /// </summary>
        DropDown,

        /// <summary>
        /// A button editor
        /// </summary>
        Button
    }

    /// <summary>
    /// An interface for a grid cell.
    /// </summary>
    public interface IGridCell
    {
        /// <summary>
        /// Gets the column index of the cell.
        /// </summary>
        int ColumnIndex { get; }

        /// <summary>
        /// Gets the row index of the cell.
        /// </summary>
        int RowIndex { get; }

        /// <summary>
        /// Gets or sets the editor type for the cell
        /// </summary>
        EditorTypeEnum EditorType { get; set; }

        /// <summary>
        /// Gets or sets the strings to be used in the drop down editor for this cell
        /// </summary>
        string[] DropDownStrings { get; set; }

        /// <summary>
        /// Gets or sets a cell's tooltip.
        /// </summary>
        string ToolTip { get; set; }

        /// <summary>
        /// Gets or sets the cell value
        /// </summary>
        object Value { get; set; }
    }
}
