﻿// -----------------------------------------------------------------------
// <copyright file="Apsim.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
// -----------------------------------------------------------------------
namespace Models.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Xml;
    using APSIM.Shared.Utilities;
    using PMF.Functions;
    using PMF;
    using Factorial;

    /// <summary>
    /// The API for models to discover other models, get and set variables in
    /// other models and send events and subscribe to events in other models.
    /// </summary>
    public static class Apsim
    {
        /// <summary>
        /// Gets the value of a variable or model.
        /// </summary>
        /// <param name="model">The reference model</param>
        /// <param name="namePath">The name of the object to return</param>
        /// <param name="ignoreCase">If true, ignore case when searching for the object or property</param>
        /// <returns>The found object or null if not found</returns>
        public static object Get(IModel model, string namePath, bool ignoreCase = false)
        {
            return Locator(model).Get(namePath, model as Model, ignoreCase);
        }

        /// <summary>
        /// Get the underlying variable object for the given path.
        /// </summary>
        /// <param name="model">The reference model</param>
        /// <param name="namePath">The name of the variable to return</param>
        /// <returns>The found object or null if not found</returns>
        public static IVariable GetVariableObject(IModel model, string namePath)
        {
            return Locator(model).GetInternal(namePath, model as Model);
        }

        /// <summary>
        /// Sets the value of a variable. Will throw if variable doesn't exist.
        /// </summary>
        /// <param name="model">The reference model</param>
        /// <param name="namePath">The name of the object to set</param>
        /// <param name="value">The value to set the property to</param>
        public static void Set(IModel model, string namePath, object value)
        {
            Locator(model).Set(namePath, model as Model, value);
        }

        /// <summary>
        /// Returns the full path of the specified model.
        /// </summary>
        /// <param name="model">The model to return the full path for</param>
        /// <returns>The path</returns>
        public static string FullPath(IModel model)
        {
            string fullPath = "." + model.Name;
            IModel parent = model.Parent;
            while (parent != null)
            {
                fullPath = fullPath.Insert(0, "." + parent.Name);
                parent = parent.Parent;
            }

            return fullPath;
        }

        /// <summary>
        /// Return a parent node of the specified type 'typeFilter'. Will throw if not found.
        /// </summary>
        /// <param name="model">The model to get the parent for</param>
        /// <param name="typeFilter">The name of the parent model to return</param>
        /// <returns>The parent of the specified type.</returns>
        public static IModel Parent(IModel model, Type typeFilter)
        {
            IModel obj = model;
            while (obj.Parent != null && !typeFilter.IsAssignableFrom(obj.GetType()))
            {
                obj = obj.Parent as IModel;
            }

            if (obj == null)
            {
                throw new ApsimXException(model, "Cannot find a parent of type: " + typeFilter.Name);
            }

            return obj;
        }

        /// <summary>
        /// Locates and returns a model with the specified name that is in scope.
        /// </summary>
        /// <param name="model">The reference model</param>
        /// <param name="namePath">The name of the model to return</param>
        /// <returns>The found model or null if not found</returns>
        public static IModel Find(IModel model, string namePath)
        {
            List<IModel> matches = FindAll(model);
            return matches.Find(match => StringUtilities.StringsAreEqual(match.Name, namePath));
        }

        /// <summary>
        /// Locates and returns a model with the specified type that is in scope.
        /// </summary>
        /// <param name="model">The reference model</param>
        /// <param name="type">The type of the model to return</param>
        /// <returns>The found model or null if not found</returns>
        public static IModel Find(IModel model, Type type)
        {
            List<IModel> matches = FindAll(model, type);
            if (matches.Count > 0)
                return matches[0];
            else
                return null;
        }

        /// <summary>
        /// Locates and returns all models in scope.
        /// </summary>
        /// <param name="model">The reference model</param>
        /// <returns>The found models or an empty array if not found.</returns>
        public static List<IModel> FindAll(IModel model)
        {
            var simulation = Apsim.Parent(model, typeof(Simulation)) as Simulation;
            if (simulation == null || simulation.Scope == null)
            {
                ScopingRules scope = new ScopingRules();
                return scope.FindAll(model).ToList();
            }
            return simulation.Scope.FindAll(model).ToList();
        }

        /// <summary>
        /// Locates and returns all models in scope of the specified type.
        /// </summary>
        /// <param name="model">The reference model</param>
        /// <param name="typeFilter">The type of the models to return</param>
        /// <returns>The found models or an empty array if not found.</returns>
        public static List<IModel> FindAll(IModel model, Type typeFilter)
        {
            List<IModel> matches = FindAll(model);
            matches.RemoveAll(match => !typeFilter.IsAssignableFrom(match.GetType()));
            return matches;
        }

        /// <summary>
        /// Perform a deep Copy of the this model.
        /// </summary>
        /// <param name="model">The model to clone</param>
        /// <returns>The clone of the model</returns>
        public static IModel Clone(IModel model)
        {
            // Get rid of our parent temporarily as we don't want to serialise that.
            IModel parent = model.Parent;
            model.Parent = null;

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, model);
                stream.Seek(0, SeekOrigin.Begin);
                IModel returnObject = (IModel)formatter.Deserialize(stream);

                // Reinstate parent
                model.Parent = parent;

                return returnObject;
            }
        }

        /// <summary>
        /// Perform a deep serialise of the model.
        /// </summary>
        /// <param name="model">The model to clone</param>
        /// <returns>The model serialised to a stream.</returns>
        public static Stream SerialiseToStream(IModel model)
        {
            // Get rid of our parent temporarily as we don't want to serialise that.
            IModel parent = model.Parent;
            model.Parent = null;
            Stream stream = new MemoryStream();
            try
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, model);
            }
            finally
            {
                model.Parent = parent;
            }
            return stream;
        }

        /// <summary>
        /// Deserialise a model from a stream.
        /// </summary>
        /// <param name="stream">The stream to deserialise from.</param>
        /// <returns>The newly created model</returns>
        public static IModel DeserialiseFromStream(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);

            IFormatter formatter = new BinaryFormatter();
            IModel model = (IModel)formatter.Deserialize(stream);
            return model;
        }

        /// <summary>Adds a new model (as specified by the xml node) to the specified parent.</summary>
        /// <param name="parent">The parent to add the model to</param>
        /// <param name="node">The XML representing the new model</param>
        /// <returns>The newly created model.</returns>
        public static IModel Add(IModel parent, XmlNode node)
        {
            IModel modelToAdd = XmlUtilities.Deserialise(node, Assembly.GetExecutingAssembly()) as Model;

            // Call deserialised
            Events events = new Events(null);
            events.AddModelEvents(modelToAdd);
            object[] args = new object[] { true };
            events.CallEventHandler(modelToAdd, "Deserialised", args);

            // Correctly parent all models.
            Add(parent, modelToAdd);

            // Ensure the model name is valid.
            Apsim.EnsureNameIsUnique(modelToAdd);

            // Call OnLoaded
            events.CallEventHandler(modelToAdd, "Loaded", null);

            Locator(parent).Clear();

            return modelToAdd;
        }

        /// <summary>Add the specified model to the parent.</summary>
        /// <param name="parent">The parent model</param>
        /// <param name="modelToAdd">The child model.</param>
        public static void Add(IModel parent, IModel modelToAdd)
        {
            modelToAdd.Parent = parent;
            Apsim.ParentAllChildren(modelToAdd);
            parent.Children.Add(modelToAdd as Model);
        }

        /// <summary>Deletes the specified model.</summary>
        /// <param name="model">The model.</param>
        public static bool Delete(IModel model)
        {
            Locator(model.Parent).Clear();
            return model.Parent.Children.Remove(model as Model);
        }

        /// <summary>Clears the cache</summary>
        public static void ClearCache(IModel model)
        {
            Locator(model as Model).Clear();
        }

        /// <summary>
        /// Serialize the model to a string and return the string.
        /// </summary>
        /// <param name="model">The model to serialize</param>
        /// <returns>The string version of the model</returns>
        public static string Serialise(IModel model)
        {
            Events events = new Events(null);
            events.AddModelEvents(model);

            // Let all models know that we're about to serialise.
            object[] args = new object[] { true };
            events.CallEventHandler(model, "Serialising", args);

            // Do the serialisation
            StringWriter writer = new StringWriter();
            writer.Write(XmlUtilities.Serialise(model, true));

            // Let all models know that we have completed serialisation.
            events.CallEventHandler(model, "Serialised", args);

            // Set the clipboard text.
            return writer.ToString();
        }

        /// <summary>
        /// Return a child model that matches the specified 'modelType'. Returns 
        /// an empty list if not found.
        /// </summary>
        /// <param name="model">The parent model</param>
        /// <param name="typeFilter">The type of children to return</param>
        /// <returns>A list of all children</returns>
        public static IModel Child(IModel model, Type typeFilter)
        {
            return model.Children.Find(m => typeFilter.IsAssignableFrom(m.GetType()));
        }

        /// <summary>
        /// Return a child model that matches the specified 'name'. Returns 
        /// null if not found.
        /// </summary>
        /// <param name="model">The parent model</param>
        /// <param name="name">The name of the child to return</param>
        /// <returns>A list of all children</returns>
        public static IModel Child(IModel model, string name)
        {
            return model.Children.Find(m => m.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
        }
        
        /// <summary>
        /// Return children that match the specified 'typeFilter'. Never returns 
        /// null. Can return empty List.
        /// </summary>
        /// <param name="model">The parent model</param>
        /// <param name="typeFilter">The type of children to return</param>
        /// <returns>A list of all children</returns>
        public static List<IModel> Children(IModel model, Type typeFilter)
        {
            return model.Children.FindAll(m => typeFilter.IsAssignableFrom(m.GetType())).ToList<IModel>();
        }
        
        /// <summary>
        /// Return a list of all child models recursively. Never returns
        /// null. Can return an empty list.
        /// </summary>
        /// <param name="model">The parent model</param>
        /// <returns>A list of all children</returns>
        public static List<IModel> ChildrenRecursively(IModel model)
        {
            List<IModel> models = new List<IModel>();

            foreach (Model child in model.Children)
            {
                models.Add(child);
                models.AddRange(ChildrenRecursively(child));
            }
            return models;
        }

        /// <summary>
        /// Return a list of all child models recursively. Only models of 
        /// the specified 'typeFilter' will be returned. Never returns
        /// null. Can return an empty list.
        /// </summary>
        /// <param name="model">The parent model</param>
        /// <param name="typeFilter">The type of children to return</param>
        /// <returns>A list of all children</returns>
        public static List<IModel> ChildrenRecursively(IModel model, Type typeFilter)
        {
            return ChildrenRecursively(model).FindAll(m => typeFilter.IsAssignableFrom(m.GetType()));
        }
        
        /// <summary>
        /// Return a list of all child models recursively. Never returns
        /// null. Can return an empty list.
        /// </summary>
        /// <param name="model">The parent model</param>
        /// <returns>A list of all children</returns>
        public static List<IModel> ChildrenRecursivelyVisible(IModel model)
        {
            return ChildrenRecursively(model).FindAll(m => !m.IsHidden);
        }

        /// <summary>
        /// Give the specified model a unique name
        /// </summary>
        /// <param name="modelToCheck">The model to check the name of</param>
        public static void EnsureNameIsUnique(IModel modelToCheck)
        {
            string originalName = modelToCheck.Name;
            string newName = originalName;
            int counter = 0;
            List<IModel> siblings = Apsim.Siblings(modelToCheck);
            IModel child = siblings.Find(m => m.Name == newName);
            while (child != null && child != modelToCheck && counter < 10000)
            {
                counter++;
                newName = originalName + counter.ToString();
                child = siblings.Find(m => m.Name == newName);
            }

            if (counter == 1000)
            {
                throw new Exception("Cannot create a unique name for model: " + originalName);
            }

            modelToCheck.Name = newName;
            Locator(modelToCheck).Clear();
        }

        /// <summary>
        /// Return all siblings of the specified model.
        /// </summary>
        /// <param name="model">The parent model</param>
        /// <returns>The found siblings or an empty array if not found.</returns>
        public static List<IModel> Siblings(IModel model)
        {
            if (model != null && model.Parent != null)
            {
                return model.Parent.Children.FindAll(m => m != model).ToList<IModel>();
            }
            else
            {
                return new List<IModel>();
            }
        }

        /// <summary>
        /// Parent all children of 'model'.
        /// </summary>
        /// <param name="model">The model to parent</param>
        public static void ParentAllChildren(IModel model)
        {
            foreach (IModel child in model.Children)
            {
                child.Parent = model;
                ParentAllChildren(child);
            }
        }

        /// <summary>
        /// Subscribe to an event. Will throw if namePath doesn't point to a event publisher.
        /// </summary>
        /// <param name="model">The model containing the handler</param>
        /// <param name="eventNameAndPath">The name of the event to subscribe to</param>
        /// <param name="handler">The event handler</param>
        public static void Subscribe(IModel model, string eventNameAndPath, EventHandler handler)
        {
            // Get the name of the component and event.
            string componentName = StringUtilities.ParentName(eventNameAndPath, '.');
            if (componentName == null)
                throw new Exception("Invalid syntax for event: " + eventNameAndPath);
            string eventName = StringUtilities.ChildName(eventNameAndPath, '.');

            // Get the component.
            object component = Apsim.Get(model, componentName);
            if (component == null)
                throw new Exception(Apsim.FullPath(model) + " can not find the component: " + componentName);

            // Get the EventInfo for the published event.
            EventInfo componentEvent = component.GetType().GetEvent(eventName);
            if (componentEvent == null)
                throw new Exception("Cannot find event: " + eventName + " in model: " + componentName);

            // Subscribe to the event.
            componentEvent.AddEventHandler(component, handler);
        }

        /// <summary>
        /// Unsubscribe an event. Throws if not found.
        /// </summary>
        /// <param name="model">The model containing the handler</param>
        /// <param name="eventNameAndPath">The name of the event to subscribe to</param>
        /// <param name="handler">The event handler</param>
        public static void Unsubscribe(IModel model, string eventNameAndPath, EventHandler handler)
        {
            // Get the name of the component and event.
            string componentName = StringUtilities.ParentName(eventNameAndPath, '.');
            if (componentName == null)
                throw new Exception("Invalid syntax for event: " + eventNameAndPath);
            string eventName = StringUtilities.ChildName(eventNameAndPath, '.');

            // Get the component.
            object component = Apsim.Get(model, componentName);
            if (component == null)
                throw new Exception(Apsim.FullPath(model) + " can not find the component: " + componentName);

            // Get the EventInfo for the published event.
            EventInfo componentEvent = component.GetType().GetEvent(eventName);
            if (componentEvent == null)
                throw new Exception("Cannot find event: " + eventName + " in model: " + componentName);

            // Unsubscribe to the event.
            componentEvent.RemoveEventHandler(component, handler);
        }

        /// <summary>
        /// Return a list of all parameters (that are not references to child models). Never returns null. Can
        /// return an empty array. A parameter is a class property that is public and read/write
        /// </summary>
        /// <param name="model">The model to search</param>
        /// <param name="flags">The reflection tags to use in the search</param>
        /// <returns>The array of variables.</returns>
        public static IVariable[] FieldsAndProperties(object model, BindingFlags flags)
        {
            List<IVariable> allProperties = new List<IVariable>();
            foreach (PropertyInfo property in model.GetType().UnderlyingSystemType.GetProperties(flags))
            {
                if (property.CanRead)
                    allProperties.Add(new VariableProperty(model, property));
            }
            foreach (FieldInfo field in model.GetType().UnderlyingSystemType.GetFields(flags))
                allProperties.Add(new VariableField(model, field));
            return allProperties.ToArray();
        }

        /// <summary>Return true if the child can be added to the parent.</summary>
        /// <param name="parent">The parent model.</param>
        /// <param name="childType">The child type.</param>
        /// <returns>True if child can be added.</returns>
        public static bool IsChildAllowable(object parent, Type childType)
        {
            if (childType == typeof(Simulations))
                return false;

            if (parent.GetType() == typeof(Folder) ||
                parent.GetType() == typeof(Factor) ||
                parent.GetType() == typeof(Replacements))
                return true;

            // Functions are currently allowable anywhere
            if (childType.GetInterface("IFunction") != null)
                return true;

            // Is allowable if one of the valid parents of this type (t) matches the parent type.
            foreach (ValidParentAttribute validParent in ReflectionUtilities.GetAttributes(childType, typeof(ValidParentAttribute), true))
            {
                if (validParent != null)
                {
                    if (validParent.DropAnywhere)
                        return true;

                    if (validParent.ParentType.IsAssignableFrom(parent.GetType()))
                        return true;
                }
            }
            return false;
        }

        /// <summary>Get a list of allowable child models for the specified parent.</summary>
        /// <param name="parent">The parent model.</param>
        /// <returns>A list of allowable child models.</returns>
        public static List<Type> GetAllowableChildModels(object parent)
        {
            List<Type> allowableModels = new List<Type>();
            foreach (Type t in ReflectionUtilities.GetTypesThatHaveInterface(typeof(IModel)))
            {
                bool isAllowable = IsChildAllowable(parent, t);
                if (isAllowable && allowableModels.Find(m => m.Name == t.Name) == null)
                    allowableModels.Add(t);
            }

            allowableModels.Sort(new ReflectionUtilities.TypeComparer());
            return allowableModels;
        }

        /// <summary>Get a list of allowable child functions for the specified parent.</summary>
        /// <param name="parent">The parent model.</param>
        /// <returns>A list of allowable child functions.</returns>
        public static List<Type> GetAllowableChildFunctions(object parent)
        {
            // For now, we allow all functions to be added anywhere
            List<Type> allowableFunctions = new List<Type>();
            foreach (Type t in ReflectionUtilities.GetTypesThatHaveInterface(typeof(IFunction)))
            {
                allowableFunctions.Add(t);
            }

            allowableFunctions.Sort(new ReflectionUtilities.TypeComparer());
            return allowableFunctions;
        }
        /// <summary>
        /// Gets the locater model for the specified model.
        /// </summary>
        /// <param name="model">The model to find the locator for</param>
        /// <returns>The an instance of a locater class for the specified model. Never returns null.</returns>
        private static Locater Locator(IModel model)
        {
            var simulation = Apsim.Parent(model, typeof(Simulation)) as Simulation;
            if (simulation == null)
            {
                // Simulation can be null if this model is not under a simulation e.g. DataStore.
                return new Locater();
            }
            else
            {
                return simulation.Locater;
            }
        }

    }
}
