using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace APSIM.Shared.Soils
{
    /// <summary>
    /// A soil crop parameterisation
    /// </summary>
    [Serializable]
    public class SoilCrop
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [XmlAttribute("name")]
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the thickness.
        /// </summary>
        /// <value>
        /// The thickness.
        /// </value>
        public double[] Thickness { get; set; }
        /// <summary>
        /// Gets or sets the ll.
        /// </summary>
        /// <value>
        /// The ll.
        /// </value>
        public double[] LL { get; set; }
        /// <summary>
        /// Gets or sets the kl.
        /// </summary>
        /// <value>
        /// The kl.
        /// </value>
        public double[] KL { get; set; }
        /// <summary>
        /// Gets or sets the xf.
        /// </summary>
        /// <value>
        /// The xf.
        /// </value>
        public double[] XF { get; set; }

        /// <summary>
        /// Gets or sets the ll metadata.
        /// </summary>
        /// <value>
        /// The ll metadata.
        /// </value>
        public string[] LLMetadata { get; set; }
        /// <summary>
        /// Gets or sets the kl metadata.
        /// </summary>
        /// <value>
        /// The kl metadata.
        /// </value>
        public string[] KLMetadata { get; set; }
        /// <summary>
        /// Gets or sets the xf metadata.
        /// </summary>
        /// <value>
        /// The xf metadata.
        /// </value>
        public string[] XFMetadata { get; set; }

    }
}