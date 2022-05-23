﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using APSIM.Shared;
using Models.Core;
using APSIM.Shared.Utilities;

namespace Models.PMF.Functions
{
    /// <summary>
    /// Return the value of a nominated internal \ref Models.PMF.Plant "Plant" numerical variable
    /// </summary>
    /// \warning You have to specify the full path of numerical variable, which starts from the child of \ref Models.PMF.Plant "Plant".
    /// For example,  <b>[Phenology].ThermalTime.Value</b> refers to value of ThermalTime under phenology function.
    [Serializable]
    [ViewName("UserInterface.Views.GridView")]
    [PresenterName("UserInterface.Presenters.PropertyPresenter")]
    [Description("Returns the value of a nominated internal Plant numerical variable")]
    public class VariableReference : Model, IFunction
    {
        /// <summary>The variable name</summary>
        [Description("Specify an internal Plant variable")]
        public string VariableName { get; set; }


        /// <summary>Gets the value.</summary>
        /// <value>The value.</value>
        public double Value(int arrayIndex = -1)
        {
            object o = Apsim.Get(this, VariableName.Trim());
            if (o is IFunction)
                return (o as IFunction).Value(arrayIndex);
            else if (o is Array)
                return Convert.ToDouble((o as Array).GetValue(arrayIndex));
            else
                return Convert.ToDouble(o);
        }

        /// <summary>Writes documentation for this function by adding to the list of documentation tags.</summary>
        /// <param name="tags">The list of tags to add to.</param>
        /// <param name="headingLevel">The level (e.g. H2) of the headings.</param>
        /// <param name="indent">The level of indentation 1, 2, 3 etc.</param>
        public override void Document(List<AutoDocumentation.ITag> tags, int headingLevel, int indent)
        {
            // write memos.
            foreach (IModel memo in Apsim.Children(this, typeof(Memo)))
                memo.Document(tags, -1, indent);


            tags.Add(new AutoDocumentation.Paragraph("<i>" + Name + " = " + StringUtilities.RemoveTrailingString(VariableName,".Value()") + "</i>", indent));
        
        }

    }
}