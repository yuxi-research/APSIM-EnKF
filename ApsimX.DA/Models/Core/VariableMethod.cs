﻿// -----------------------------------------------------------------------
// <copyright file="VariableProperty.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
//-----------------------------------------------------------------------
namespace Models.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Models.Soils;
    using System.Globalization;
    using APSIM.Shared.Utilities;
    using System.Collections;

    /// <summary>
    /// Encapsulates a discovered method of a model. 
    /// </summary>
    [Serializable]
    public class VariableMethod : IVariable
    {
        /// <summary>
        /// Gets or sets the PropertyInfo for this property.
        /// </summary>
        private MethodInfo method;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableMethod" /> class.
        /// </summary>
        /// <param name="model">The underlying model for the property</param>
        /// <param name="method">The PropertyInfo for this property</param>
        public VariableMethod(object model, MethodInfo method)
        {
            if (model == null || method == null)
                throw new ApsimXException(null, "Cannot create an instance of class VariableMethod with a null model or methodInfo");
            
            this.Object = model;
            this.method = method;
        }

        /// <summary>
        /// Gets or sets the underlying model that this property belongs to.
        /// </summary>
        public override object Object { get; set; }

        /// <summary>
        /// Return the name of the method.
        /// </summary>
        public override string Name 
        { 
            get 
            {
                return this.method.Name; 
            } 
        }

        /// <summary>
        /// Gets the description of the method
        /// </summary>
        public override string Description { get { return string.Empty; } }

        /// <summary>
        /// Gets the units of the method
        /// </summary>
        public override string Units { get { return string.Empty; } set { } }

        /// <summary>
        /// Gets the units of the property as formmatted for display (in parentheses) or null if not found.
        /// </summary>
        public override string UnitsLabel { get { return string.Empty; } }
        
        /// <summary>
        /// Gets a list of allowable units
        /// </summary>
        public string[] AllowableUnits { get { return null; } }

        /// <summary>
        /// Gets a value indicating whether the method is readonly.
        /// </summary>
        public bool IsReadOnly { get { return true; } }

        /// <summary>
        /// Gets the metadata for each layer. Returns new string[0] if none available.
        /// </summary>
        public string[] Metadata { get { return null; } }

        /// <summary>
        /// Gets the data type of the method
        /// </summary>
        public Type DataType { get { return null; } }

        /// <summary>
        /// Gets the values of the method
        /// </summary>
        public override object Value
        {
            get
            {
                return method.Invoke(Object, new object[] { -1 });
            }

            set
            {
            }
        }


    }
}
