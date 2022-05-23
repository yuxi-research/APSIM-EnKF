﻿// -----------------------------------------------------------------------
// <copyright file="ReportColumnForFactorValue.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
//-----------------------------------------------------------------------
namespace Models.Report
{
    using System;
    using System.Collections.Generic;

    /// <summary>A class for containing values for a report column</summary>
    [Serializable]
    public class ReportColumnWithValues : IReportColumn
    {
        /// <summary>Name of column</summary>
        public string Name { get; private set; }

        /// <summary>The values</summary>
        public List<object> Values { get; set; }

        /// <summary>Constructor for a report column that has simple values.</summary>
        /// <param name="columnName">The column name to write to the output</param>
        public ReportColumnWithValues(string columnName)
        {
            Name = columnName;
            Values = new List<object>();
        }

        /// <summary>Constructor for a report column that has simple values.</summary>
        /// <param name="columnName">The column name to write to the output</param>
        /// <param name="initialValues">Values for column - used for testing.</param>
        public ReportColumnWithValues(string columnName, object[] initialValues)
        {
            Name = columnName;
            Values = new List<object>();
            Values.AddRange(initialValues);
        }

        /// <summary>Add a value.</summary>
        /// <param name="value">The value to add</param>
        public void Add(object value)
        {
            Values.Add(value);
        }
    }
}
