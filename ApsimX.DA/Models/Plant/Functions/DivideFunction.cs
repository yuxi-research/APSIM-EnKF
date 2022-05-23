﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Models.Core;

namespace Models.PMF.Functions
{
    /// <summary>
    /// Starting with the first child function, recursively divide by the values of the subsequent child functions
    /// </summary>
    [Serializable]
    [Description("Starting with the first child function, recursively divide by the values of the subsequent child functions")]
    public class DivideFunction : Model, IFunction
    {
        /// <summary>The child functions</summary>
        private List<IModel> ChildFunctions;

        /// <summary>Gets the value.</summary>
        /// <value>The value.</value>
        public double Value(int arrayIndex = -1)
        {
            if (ChildFunctions == null)
                ChildFunctions = Apsim.Children(this, typeof(IFunction));

            double returnValue = 0.0;
            if (ChildFunctions.Count > 0)
            {
                IFunction F = ChildFunctions[0] as IFunction;
                returnValue = F.Value(arrayIndex);

                if (ChildFunctions.Count > 1)
                    for (int i = 1; i < ChildFunctions.Count; i++)
                    {
                        F = ChildFunctions[i] as IFunction;
                        returnValue = returnValue / F.Value(arrayIndex);
                    }

            }
            return returnValue;
        }

        /// <summary>Writes documentation for this function by adding to the list of documentation tags.</summary>
        /// <param name="tags">The list of tags to add to.</param>
        /// <param name="headingLevel">The level (e.g. H2) of the headings.</param>
        /// <param name="indent">The level of indentation 1, 2, 3 etc.</param>
        public override void Document(List<AutoDocumentation.ITag> tags, int headingLevel, int indent)
        {
            SubtractFunction.DocumentMathFunction(this, '/', tags, headingLevel, indent);
        }

    }
}