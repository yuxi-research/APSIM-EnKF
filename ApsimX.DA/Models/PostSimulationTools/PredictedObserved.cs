﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Models.Core;
using System.IO;
using System.Data;
using System.Xml.Serialization;

namespace Models.PostSimulationTools
{


    /// <summary>
    /// Reads the contents of a file (in apsim format) and stores into the DataStore.
    /// If the file has a column name of 'SimulationName' then this model will only input data for those rows
    /// where the data in column 'SimulationName' matches the name of the simulation under which
    /// this input model sits.
    /// If the file does NOT have a 'SimulationName' column then all data will be input.
    /// </summary>
    [Serializable]
    [ViewName("UserInterface.Views.GridView")]
    [PresenterName("UserInterface.Presenters.PropertyPresenter")]
    [ValidParent(ParentType=typeof(DataStore))]
    public class PredictedObserved : Model, IPostSimulationTool
    {
        /// <summary>Gets or sets the name of the predicted table.</summary>
        [Description("Predicted table")]
        [Display(DisplayType = DisplayAttribute.DisplayTypeEnum.TableName)]
        public string PredictedTableName { get; set; }

        /// <summary>Gets or sets the name of the observed table.</summary>
        [Description("Observed table")]
        [Display(DisplayType = DisplayAttribute.DisplayTypeEnum.TableName)]
        public string ObservedTableName { get; set; }
        
        /// <summary>Gets or sets the field name used for match.</summary>
        [Description("Field name to use for matching predicted with observed data")]
        [Display(DisplayType = DisplayAttribute.DisplayTypeEnum.FieldName)]
        public string FieldNameUsedForMatch { get; set; }

        /// <summary>Gets or sets the second field name used for match.</summary>
        [Description("Second field name to use for matching predicted with observed data (optional)")]
        [Display(DisplayType = DisplayAttribute.DisplayTypeEnum.FieldName)]
        public string FieldName2UsedForMatch { get; set; }

        /// <summary>Gets or sets the third field name used for match.</summary>
        [Description("Third field name to use for matching predicted with observed data (optional)")]
        [Display(DisplayType = DisplayAttribute.DisplayTypeEnum.FieldName)]
        public string FieldName3UsedForMatch { get; set; }

        /// <summary>Main run method for performing our calculations and storing data.</summary>
        /// <param name="dataStore">The data store.</param>
        /// <exception cref="ApsimXException">
        /// Could not find model data table:  + ObservedTableName
        /// or
        /// Could not find observed data table:  + ObservedTableName
        /// </exception>
        public void Run(DataStore dataStore)
        {
            if (PredictedTableName != null && ObservedTableName != null)
            {
                dataStore.DeleteTable(this.Name);
                
                DataTable predictedDataNames = dataStore.RunQuery("PRAGMA table_info(" + PredictedTableName + ")");
                DataTable observedDataNames  = dataStore.RunQuery("PRAGMA table_info(" + ObservedTableName + ")");

                if (predictedDataNames == null)
                    throw new ApsimXException(this, "Could not find model data table: " + ObservedTableName);
                
                if (observedDataNames == null)
                    throw new ApsimXException(this, "Could not find observed data table: " + ObservedTableName);

                IEnumerable<string> commonCols = from p in predictedDataNames.AsEnumerable()
                                               join o in observedDataNames.AsEnumerable() on p["name"] equals o["name"]
                                               select p["name"] as string;

                StringBuilder query = new StringBuilder("SELECT ");
                foreach (string s in commonCols)
                {
                    if (s == FieldNameUsedForMatch || s == FieldName2UsedForMatch || s == FieldName3UsedForMatch)
                        query.Append("I.'@field', ");
                    else
                        query.Append("I.'@field' AS 'Observed.@field', R.'@field' AS 'Predicted.@field', ");

                    query.Replace("@field", s);
                }

                query.Append("FROM " + ObservedTableName + " I INNER JOIN " + PredictedTableName + " R USING (SimulationID) WHERE I.'@match1' = R.'@match1'");
                if (FieldName2UsedForMatch != null)
                    query.Append(" AND I.'@match2' = R.'@match2'");
                if (FieldName3UsedForMatch != null)
                    query.Append(" AND I.'@match3' = R.'@match3'");
                query.Replace(", FROM", " FROM"); // get rid of the last comma
                query.Replace("I.'SimulationID' AS 'Observed.SimulationID', R.'SimulationID' AS 'Predicted.SimulationID'", "I.'SimulationID' AS 'SimulationID'");

                query = query.Replace("@match1", FieldNameUsedForMatch);
                query = query.Replace("@match2", FieldName2UsedForMatch);
                query = query.Replace("@match3", FieldName3UsedForMatch);

                DataTable predictedObservedData = dataStore.RunQuery(query.ToString());

                if (predictedObservedData != null)
                    dataStore.WriteTable(null, this.Name, predictedObservedData);
                dataStore.Disconnect();
            }
        }
    }
}
