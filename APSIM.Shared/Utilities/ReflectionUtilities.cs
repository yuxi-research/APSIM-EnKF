// -----------------------------------------------------------------------
// <copyright file="ReflectionUtilities.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
//-----------------------------------------------------------------------
namespace APSIM.Shared.Utilities
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;

    /// <summary>
    /// Utility class with reflection functions
    /// </summary>
    public class ReflectionUtilities
    {
        /// <summary>
        /// Returns true if the specified type T is of type TypeName
        /// </summary>
        public static bool IsOfType(Type t, string typeName)
        {
            while (t != null)
            {
                if (t.ToString() == typeName)
                    return true;

                if (t.GetInterface(typeName) != null)
                    return true;

                t = t.BaseType;
            }
            return false;
        }

        /// <summary>
        /// Return all fields. The normal .NET reflection doesn't return private fields in base classes.
        /// This function does.
        /// </summary>
        public static List<FieldInfo> GetAllFields(Type type, BindingFlags flags)
        {
            if (type == typeof(Object)) return new List<FieldInfo>();

            var list = GetAllFields(type.BaseType, flags);
            // in order to avoid duplicates, force BindingFlags.DeclaredOnly
            list.AddRange(type.GetFields(flags | BindingFlags.DeclaredOnly));
            return list;
        }

        /// <summary>
        /// Get the value of a field or property.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object GetValueOfFieldOrProperty(string name, object obj)
        {
            int Pos = name.IndexOf('.');
            if (Pos > -1)
            {
                string FieldName = name.Substring(0, Pos);
                obj = GetValueOfFieldOrProperty(FieldName, obj);
                if (obj == null)
                    return null;
                else
                    return GetValueOfFieldOrProperty(name.Substring(Pos + 1), obj);
            }
            else
            {
                BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase;
                FieldInfo F = obj.GetType().GetField(name, Flags);
                if (F != null)
                    return F.GetValue(obj);

                PropertyInfo P = obj.GetType().GetProperty(name, Flags);
                if (P != null)
                    return P.GetValue(obj, null);

                return null;
            }
        }

        /// <summary>
        /// Trys to set the value of a public or private field or property. Name can have '.' characters. Will
        /// return true if successfull. Will throw if Value is the wrong type for the field
        /// or property. Supports strings/double/int conversion or direct setting.
        /// </summary>
        public static bool SetValueOfFieldOrProperty(string name, object obj, object value)
        {
            if (name.Contains("."))
            {
                int Pos = name.IndexOf('.');
                string FieldName = name.Substring(0, Pos);
                obj = SetValueOfFieldOrProperty(FieldName, obj, value);
                if (obj == null)
                    return false;
                else
                    return SetValueOfFieldOrProperty(name.Substring(Pos + 1), obj, value);
            }
            else
            {
                BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase;
                FieldInfo F = obj.GetType().GetField(name, Flags);
                if (F != null)
                {
                    if (F.FieldType == typeof(string))
                        F.SetValue(obj, value.ToString());
                    else if (F.FieldType == typeof(double))
                        F.SetValue(obj, Convert.ToDouble(value));
                    else if (F.FieldType == typeof(int))
                        F.SetValue(obj, Convert.ToInt32(value));
                    else
                        F.SetValue(obj, value);
                    return true;
                }

                return SetValueOfProperty(name, obj, value);
            }
        }

        /// <summary>
        /// Set the value of a object property using reflection. Property must be public.
        /// </summary>
        /// <param name="name">Name of the property</param>
        /// <param name="obj">Object to probe</param>
        /// <param name="value">The value to set the property to</param>
        /// <returns>True if value set successfully</returns>
        public static bool SetValueOfProperty(string name, object obj, object value)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase;
            PropertyInfo P = obj.GetType().GetProperty(name, flags);
            if (P != null)
            {
                if (value == null)
                    P.SetValue(obj, value, null);
                else if (P.PropertyType == typeof(string))
                    P.SetValue(obj, value.ToString(), null);
                else if (P.PropertyType == typeof(double))
                    P.SetValue(obj, Convert.ToDouble(value), null);
                else if (P.PropertyType == typeof(int))
                    P.SetValue(obj, Convert.ToInt32(value), null);
                else
                    P.SetValue(obj, value, null);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets all Type instances matching the specified class name with no namespace qualified class name.
        /// Will not throw. May return empty array.
        /// </summary>
        public static Type[] GetTypeWithoutNameSpace(string className, Assembly assembly)
        {
            List<Type> returnVal = new List<Type>();

            Type[] assemblyTypes = assembly.GetTypes();
            for (int j = 0; j < assemblyTypes.Length; j++)
            {
                if (assemblyTypes[j].Name == className)
                {
                    returnVal.Add(assemblyTypes[j]);
                }
            }

            return returnVal.ToArray();
        }

        /// <summary>
        /// Gets the specified attribute type.
        /// </summary>
        /// <returns>Returns the attribute or null if not found.</returns>
        public static Attribute GetAttribute(Type t, Type attributeTypeToFind, bool lookInBaseClasses)
        {
            foreach (Attribute A in t.GetCustomAttributes(lookInBaseClasses))
            {
                if (A.GetType() == attributeTypeToFind)
                    return A;
            }
            return null;
        }

        /// <summary>
        /// Gets the specified attribute type.
        /// </summary>
        /// <returns>Returns the attribute or null if not found.</returns>
        public static Attribute GetAttribute(MemberInfo t, Type attributeTypeToFind, bool lookInBaseClasses)
        {
            foreach (Attribute A in t.GetCustomAttributes(lookInBaseClasses))
            {
                if (A.GetType() == attributeTypeToFind)
                    return A;
            }
            return null;
        }

        /// <summary>
        /// Gets 0 or more attributes of the specified type.
        /// </summary>
        /// <returns>Returns the attributes or string[0] if none found.</returns>
        public static Attribute[] GetAttributes(Type t, Type attributeTypeToFind, bool lookInBaseClasses)
        {
            List<Attribute> Attributes = new List<Attribute>();
            foreach (Attribute A in t.GetCustomAttributes(lookInBaseClasses))
            {
                if (A.GetType() == attributeTypeToFind)
                    Attributes.Add(A);
            }
            return Attributes.ToArray();
        }

        /// <summary>
        /// Gets 0 or more attributes of the specified type.
        /// </summary>
        /// <returns>Returns the attributes or string[0] if none found.</returns>
        public static Attribute[] GetAttributes(MemberInfo t, Type attributeTypeToFind, bool lookInBaseClasses)
        {
            List<Attribute> Attributes = new List<Attribute>();
            foreach (Attribute A in t.GetCustomAttributes(lookInBaseClasses))
            {
                if (A.GetType() == attributeTypeToFind)
                    Attributes.Add(A);
            }
            return Attributes.ToArray();
        }

        /// <summary>
        /// Returns the name of the specified object if it has a public name property
        /// or it returns the name of the type if no name property is present.
        /// </summary>
        public static string Name(object obj)
        {
            if (obj != null)
            {
                PropertyInfo NameProperty = obj.GetType().GetProperty("Name");
                if (NameProperty == null)
                    return obj.GetType().Name;
                else
                    return NameProperty.GetValue(obj, null) as string;
            }
            return null;
        }

        /// <summary>
        /// Sets the name of the specified object if it has a public name property that is settable.
        /// Will throw if cannot set the name.
        /// </summary>
        public static void SetName(object obj, string newName)
        {
            PropertyInfo NameProperty = obj.GetType().GetProperty("Name");
            if (NameProperty == null || !NameProperty.CanWrite)
                throw new Exception("Cannot set the name of object with type: " + obj.GetType().Name + 
                                    ". It does not have a public, settable, name property");
            else
                NameProperty.SetValue(obj, newName, null);
        }

        /// <summary>
        /// Returns true if the specified object has a name property with a public setter.
        /// </summary>
        public static bool NameIsSettable(object obj)
        {
            PropertyInfo NameProperty = obj.GetType().GetProperty("Name");
            return NameProperty != null && NameProperty.CanWrite;
        }

        /// <summary>
        /// Return a type from the specified unqualified (no namespaces) type name.
        /// </summary>
        public static Type GetTypeFromUnqualifiedName(string typeName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.Name == typeName)
                        return type;
                }
            }
            return null;
        }

        /// <summary>
        /// An assembly cache.
        /// </summary>
        private static Dictionary<string, Assembly> AssemblyCache = new Dictionary<string, Assembly>();

        /// <summary>
        /// Compile the specified 'code' into an executable assembly. If 'assemblyFileName'
        /// is null then compile to an in-memory assembly.
        /// </summary>
        public static Assembly CompileTextToAssembly(string code, string assemblyFileName)
        {
            // See if we've already compiled this code. If so then return the assembly.
            if (AssemblyCache.ContainsKey(code))
                return AssemblyCache[code];

            lock (AssemblyCache)
            {
                if (AssemblyCache.ContainsKey(code))
                    return AssemblyCache[code];
                bool VB = code.IndexOf("Imports System") != -1;
                string Language;
                if (VB)
                    Language = CodeDomProvider.GetLanguageFromExtension(".vb");
                else
                    Language = CodeDomProvider.GetLanguageFromExtension(".cs");

                if (Language != null && CodeDomProvider.IsDefinedLanguage(Language))
                {
                    CodeDomProvider Provider = CodeDomProvider.CreateProvider(Language);
                    if (Provider != null)
                    {
                        CompilerParameters Params = new CompilerParameters();

                        string[] source = new string[1];
                        if (assemblyFileName == null)
                        {
                            Params.GenerateInMemory = true;
                            source[0] = code;
                        }
                        else
                        {
                            Params.GenerateInMemory = false;
                            Params.OutputAssembly = assemblyFileName;
                            string sourceFileName;
                            if (VB)
                                sourceFileName = Path.ChangeExtension(assemblyFileName, ".vb");
                            else
                                sourceFileName = Path.ChangeExtension(assemblyFileName, ".cs");
                            File.WriteAllText(sourceFileName, code);
                            source[0] = sourceFileName;
                        }
                        Params.TreatWarningsAsErrors = false;
                        Params.IncludeDebugInformation = true;
                        Params.WarningLevel = 2;
                        Params.ReferencedAssemblies.Add("System.dll");
                        Params.ReferencedAssemblies.Add("System.Xml.dll");
                        Params.ReferencedAssemblies.Add("System.Windows.Forms.dll");
                        Params.ReferencedAssemblies.Add("System.Data.dll");
                        //Params.ReferencedAssemblies.Add("APSIM.Shared.dll");
                        Params.ReferencedAssemblies.Add(System.IO.Path.Combine(Assembly.GetExecutingAssembly().Location));

                        if (Assembly.GetCallingAssembly() != Assembly.GetExecutingAssembly())
                            Params.ReferencedAssemblies.Add(System.IO.Path.Combine(Assembly.GetCallingAssembly().Location));
                        Params.TempFiles = new TempFileCollection(Path.GetTempPath());  // ensure that any temp files are in a writeable area
                        Params.TempFiles.KeepFiles = false;
                        CompilerResults results;
                        if (assemblyFileName == null)
                            results = Provider.CompileAssemblyFromSource(Params, source);
                        else
                            results = Provider.CompileAssemblyFromFile(Params, source);
                        string Errors = "";
                        foreach (CompilerError err in results.Errors)
                        {
                            if (Errors != "")
                                Errors += "\r\n";

                            Errors += err.ErrorText + ". Line number: " + err.Line.ToString();
                        }
                        if (Errors != "")
                            throw new Exception(Errors);

                        AssemblyCache.Add(code, results.CompiledAssembly);
                        return results.CompiledAssembly;
                    }
                }
                throw new Exception("Cannot compile manager script to an assembly");
            }
        }

        /// <summary>
        /// Binary serialise the object and return the resulting stream.
        /// </summary>
        public static Stream BinarySerialise(object source)
        {
            if (source == null)
                return null;

            if (!source.GetType().IsSerializable)
                throw new ArgumentException("The type must be serializable.", "source");

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            formatter.Serialize(stream, source);
            return stream;
        }

        /// <summary>
        /// Binary deserialise the specified stream and return the resulting object
        /// </summary>
        public static object BinaryDeserialise(Stream stream)
        {
            if (stream == null)
                return null;

            IFormatter formatter = new BinaryFormatter();
            return formatter.Deserialize(stream);
        }

        /// <summary>
        /// Convert the specified 'stringValue' into an object of the specified 'type'.
        /// Will throw if cannot convert type.
        /// </summary>
        public static object StringToObject(Type type, string stringValue)
        {
            if (type.IsArray)
            {
                string[] stringValues = stringValue.ToString().Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (type == typeof(double[]))
                    return MathUtilities.StringsToDoubles(stringValues);
                else if (type == typeof(int[]))
                    return MathUtilities.StringsToDoubles(stringValues);
                else if (type == typeof(string[]))
                    return stringValues;
                else
                    throw new Exception("Cannot convert '" + stringValue + "' into an object of type '" + type.ToString() + "'");
            }
            else if (type == typeof(double))
                return Convert.ToDouble(stringValue, CultureInfo.InvariantCulture);
            else if (type == typeof(float))
                return Convert.ToSingle(stringValue, CultureInfo.InvariantCulture);
            else if (type == typeof(int))
                return Convert.ToInt32(stringValue, CultureInfo.InvariantCulture);
            else if (type == typeof(DateTime))
                return Convert.ToDateTime(stringValue, CultureInfo.InvariantCulture);
            else if (type == typeof(string))
                return stringValue;
            else if (type == typeof(bool))
                return Boolean.Parse(stringValue);
            else if (type.IsEnum)
                return Enum.Parse(type, stringValue, true);
            else
                return null;
        }

        /// <summary>
        /// Convert the specified 'obj' into a string.
        /// </summary>
        public static string ObjectToString(object obj)
        {
            if (obj.GetType().IsArray)
            {
                string stringValue = "";
                Array arr = obj as Array;
                for (int j = 0; j < arr.Length; j++)
                {
                    if (j > 0)
                        stringValue += ",";
                    stringValue += arr.GetValue(j).ToString();
                }
                return stringValue;
            }
            else if (obj.GetType() == typeof(DateTime))
            {
                return ((DateTime) obj).ToString("yyyy-MM-dd");
            }
            else
            {
                return Convert.ToString(obj, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Perform a deep Copy of the specified object
        /// </summary>
        public static object Clone(object sourceObj)
        {
            Stream stream = BinarySerialise(sourceObj);
            stream.Seek(0, SeekOrigin.Begin);
            return BinaryDeserialise(stream);
        }

        /// <summary>
        /// Return a list of sorted properties.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static PropertyInfo[] GetPropertiesSorted(Type type, BindingFlags flags)
        {
            List<PropertyInfo> properties = new List<PropertyInfo>();

            properties.AddRange(type.GetProperties(flags));
            properties.Sort(new PropertyInfoComparer());

            return properties.ToArray();

        }

        /// <summary>
        /// A private property comparer.
        /// </summary>
        private class PropertyInfoComparer : IComparer<PropertyInfo>
        {
            // Calls CaseInsensitiveComparer.Compare with the parameters reversed.
            public int Compare(PropertyInfo x, PropertyInfo y)
            {
                return x.Name.CompareTo(y.Name);
            }
        }

        /// <summary>
        /// A type comparer.
        /// </summary>
        public class TypeComparer : IComparer<Type>
        {
            /// <summary>A type comparer that uses names.</summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public int Compare(Type x, Type y)
            {
                return x.Name.CompareTo(y.Name);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        public static List<Type> GetTypesThatHaveInterface(Type interfaceType)
        {
            List<Type> types = new List<Type>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type t in assembly.GetTypes())
                {
                    if (interfaceType.IsAssignableFrom(t) && t.Name != interfaceType.Name && t.IsPublic)
                        types.Add(t);
                }
            }

            return types;
        }
    }
}
