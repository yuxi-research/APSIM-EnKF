// -----------------------------------------------------------------------
// <copyright file="SoilUtilities.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
// -----------------------------------------------------------------------
namespace APSIM.Shared.Soils
{
    using System;
    using System.IO;
    using System.Xml.Serialization;

    /// <summary>Various soil utilities.</summary>
    public class SoilUtilities
    {
        /// <summary>Create a soil object from the XML passed in.</summary>
        /// <param name="Xml">The XML.</param>
        /// <returns></returns>
        public static Soil FromXML(string Xml)
        {
            XmlSerializer x = new XmlSerializer(typeof(Soil));
            StringReader F = new StringReader(Xml);
            return x.Deserialize(F) as Soil;
        }

        /// <summary>Write soil to XML</summary>
        /// <param name="soil">The soil.</param>
        /// <returns></returns>
        public static string ToXML(Soil soil)
        {
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            XmlSerializer x = new XmlSerializer(typeof(Soil));

            StringWriter Out = new StringWriter();
            x.Serialize(Out, soil, ns);
            string st = Out.ToString();
            if (st.Length > 5 && st.Substring(0, 5) == "<?xml")
            {
                // remove the first line: <?xml version="1.0"?>/n
                int posEol = st.IndexOf("\n");
                if (posEol != -1)
                    return st.Substring(posEol + 1);
            }
            return st;
        }

        /// <summary>Convert the specified thicknesses to mid points for plotting.</summary>
        /// <param name="Thickness">The thicknesses.</param>
        static public double[] ToMidPoints(double[] Thickness)
        {
            double[] CumThickness = ToCumThickness(Thickness);
            double[] MidPoints = new double[CumThickness.Length];
            for (int Layer = 0; Layer != CumThickness.Length; Layer++)
            {
                if (Layer == 0)
                    MidPoints[Layer] = CumThickness[Layer] / 2.0;
                else
                    MidPoints[Layer] = (CumThickness[Layer] + CumThickness[Layer - 1]) / 2.0;
            }
            return MidPoints;
        }

        /// <summary>Returns a cumulative thickness based on the specified thickness.</summary>
        /// <param name="Thickness">The thickness.</param>
        static public double[] ToCumThickness(double[] Thickness)
        {
            // ------------------------------------------------
            // Return cumulative thickness for each layer - mm
            // ------------------------------------------------
            double[] CumThickness = new double[Thickness.Length];
            if (Thickness.Length > 0)
            {
                CumThickness[0] = Thickness[0];
                for (int Layer = 1; Layer != Thickness.Length; Layer++)
                    CumThickness[Layer] = Thickness[Layer] + CumThickness[Layer - 1];
            }
            return CumThickness;
        }

        /// <summary>Return the index of the layer that contains the specified depth.</summary>
        /// <param name="soil">The soil</param>
        /// <param name="depth">The depth to search for.</param>
        /// <returns></returns>
        static public int FindLayerIndex(Soil soil, double depth)
        {
            return Array.FindIndex(ToCumThickness(soil.Water.Thickness), d => d > depth);
        }

    }
}
