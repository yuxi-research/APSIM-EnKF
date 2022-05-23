﻿// -----------------------------------------------------------------------
// <copyright file="LinkAttribute.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
// -----------------------------------------------------------------------
namespace Models.Core
{
    using PMF;
    using PMF.Functions;
    using System;
    using System.Reflection;
    using System.Xml.Serialization;

    /// <summary>
    /// When applied to a field, the infrastructure will locate an object that matches the 
    /// related field and store a reference to it in the field (dependency injection). 
    /// If no matching model is found (and IsOptional is not specified or is false), then an 
    /// exception will be thrown. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class LinkAttribute : XmlIgnoreAttribute
    {
        /// <summary>Stores the value of IsOptional if specified.</summary>
        private bool isOptional = false;

        /// <summary>When true, the infrastructure will not throw an exception when an object cannot be found.</summary>
        public bool IsOptional { get { return this.isOptional; } set { this.isOptional = value; } }

        /// <summary>Is this link a scoped link</summary>
        public virtual bool IsScoped(FieldInfo field)
        {
            if (typeof(IFunction).IsAssignableFrom(field.FieldType) ||
                typeof(Biomass).IsAssignableFrom(field.FieldType) ||
                field.FieldType.Name == "Object")
                return false;
            else
                return true;
        }

        /// <summary>Should the fields name be used when matching?</summary>
        public virtual bool UseNameToMatch(FieldInfo field)
        {
            if (IsScoped(field))
                return false;
            else
                return true;
        }
    }


    /// <summary>
    /// When applied to a field, the infrastructure will locate an object in scope of the 
    /// related field and store a reference to it in the field. If no matching
    /// model is found (and IsOptional is not specified or is false), then an 
    /// exception will be thrown. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ScopedLinkByNameAttribute : LinkAttribute
    {
        /// <summary>Is this link a scoped link</summary>
        public override bool IsScoped(FieldInfo fieldInfo) { return true; }

        /// <summary>Should the fields name be used when matching?</summary>
        public override bool UseNameToMatch(FieldInfo field) { return true; }
    }

    /// <summary>
    /// When applied to a field, the infrastructure will locate an object in scope of the 
    /// related field and store a reference to it in the field. If no matching
    /// model is found (and IsOptional is not specified or is false), then an 
    /// exception will be thrown. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ScopedLinkAttribute : LinkAttribute
    {
        /// <summary>Is this link a scoped link</summary>
        public override bool IsScoped(FieldInfo fieldInfo) { return true; }

        /// <summary>Should the fields name be used when matching?</summary>
        public override bool UseNameToMatch(FieldInfo field) { return false; }
    }

    /// <summary>
    /// When applied to a field, the infrastructure will locate a child object of the 
    /// related fields type and store a reference to it in the field. If no matching
    /// model is found (and IsOptional is not specified or is false), then an 
    /// exception will be thrown. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ChildLinkAttribute : LinkAttribute
    {
        /// <summary>Is this link a scoped link</summary>
        public override bool IsScoped(FieldInfo fieldInfo) { return false; }

        /// <summary>Should the fields name be used when matching?</summary>
        public override bool UseNameToMatch(FieldInfo field) { return false; }
    }

    /// <summary>
    /// When applied to a field, the infrastructure will locate a child object of the 
    /// related fields type and name and store a reference to it in the field. If no matching
    /// model is found (and IsOptional is not specified or is false), then an 
    /// exception will be thrown. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ChildLinkByNameAttribute : LinkAttribute
    {
        /// <summary>Is this link a scoped link</summary>
        public override bool IsScoped(FieldInfo fieldInfo) { return false; }

        /// <summary>Should the fields name be used when matching?</summary>
        public override bool UseNameToMatch(FieldInfo field) { return true; }
    }

    /// <summary>
    /// When applied to a field, the infrastructure will locate a parent object of the 
    /// related fields type and store a reference to it in the field. If no matching
    /// model is found (and IsOptional is not specified or is false), then an 
    /// exception will be thrown. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ParentLinkAttribute : LinkAttribute
    {
        /// <summary>Is this link a scoped link</summary>
        public override bool IsScoped(FieldInfo fieldInfo) { return false; }

        /// <summary>Should the fields name be used when matching?</summary>
        public override bool UseNameToMatch(FieldInfo field) { return false; }
    }
}
