using System;


namespace APSIM.Shared.Soils
{

    /// <summary>
    /// These classes define the attributes used to provide metadata for the
    /// APSIM Component properties and events.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class EventHandler : Attribute
    {
        /// <summary>
        /// The _ event name
        /// </summary>
        private string _EventName;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventHandler"/> class.
        /// </summary>
        public EventHandler()
        {
            _EventName = "";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventHandler"/> class.
        /// </summary>
        /// <param name="Name">The name.</param>
        public EventHandler(string Name)
        {
            _EventName = Name;
        }
        /// <summary>
        /// Gets or sets the name of the event.
        /// </summary>
        /// <value>
        /// The name of the event.
        /// </value>
        public string EventName
        {
            get { return _EventName; }
            set { _EventName = EventName; }
        }
    }

    /// <summary>
    /// An attribute for specifying units.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class Units : Attribute
    {
        /// <summary>
        /// The st
        /// </summary>
        private String St;
        /// <summary>
        /// Initializes a new instance of the <see cref="Units"/> class.
        /// </summary>
        /// <param name="Units">The units.</param>
        public Units(String Units)
        {
            St = Units;
        }
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override String ToString()
        {
            return St;
        }
    }

    /// <summary>
    /// An attribute for specifying a description
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class Description : Attribute
    {
        /// <summary>
        /// The st
        /// </summary>
        private String St;

        /// <summary>
        /// Initializes a new instance of the <see cref="Description"/> class.
        /// </summary>
        /// <param name="Description">The description.</param>
        public Description(String Description)
        {
            St = Description;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override String ToString()
        {
            return St;
        }
    }

    /// <summary>
    /// This attribute is used in DotNetProxies.cs to provide a map between the
    /// proxy class and the component type.
    /// Usage: [ComponentType("Plant2")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ComponentTypeAttribute : Attribute
    {
        /// <summary>
        /// The component class
        /// </summary>
        public String ComponentClass;
        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentTypeAttribute"/> class.
        /// </summary>
        /// <param name="CompClass">The comp class.</param>
        public ComponentTypeAttribute(String CompClass)
        {
            ComponentClass = CompClass;
        }

    }

    /// <summary>
    /// A user interface attribute to instruct the UI to allow for a large amount of text.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class UILargeText : Attribute
    {
    }

    /// <summary>
    /// A user interface attribute to instruct the UI to ignore this field/property
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class UIIgnore : Attribute
    {
    }

}
