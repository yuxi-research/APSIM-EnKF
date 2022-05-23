﻿// -----------------------------------------------------------------------
// <copyright file="ReportColumnForFactorValue.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
//-----------------------------------------------------------------------
namespace Models.Report
{
    using System;
    using System.Collections.Generic;

    /// <summary>A class for outputting a constant value in a report column.</summary>
    [Serializable]
    public class ReportColumnConstantValue : IReportColumn
    {
        /// <summary>The column name for the constant</summary>
        public string Name { get; private set; }

        /// <summary>The constant value</summary>
        public List<object> Values { get; set; }

        /// <summary>
        /// Constructor for a plain report variable.
        /// </summary>
        /// <param name="columnName">The column name to write to the output</param>
        /// <param name="constantValue">The constant value</param>
        public ReportColumnConstantValue(string columnName, object constantValue)
        {
            Name = columnName;
            Values = new List<object>();
            Values.Add(constantValue);
        }
    }
}
