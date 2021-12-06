// -----------------------------------------------------------------------
// <copyright file="Unit.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
// -----------------------------------------------------------------------
namespace APSIM.Shared.Soils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using APSIM.Shared.Utilities;

    /// <summary>Convert soil units to APSIM standard.</summary>
    public class Unit
    {
        /// <summary>Convert soil units to APSIM standard.</summary>
        /// <param name="soil">The soil.</param>
        public static void Convert(Soil soil)
        {
            // Convert soil organic matter OC to total %
            if (soil.SoilOrganicMatter != null)
            {
                soil.SoilOrganicMatter.OC = OCTotalPercent(soil.SoilOrganicMatter.OC, soil.SoilOrganicMatter.OCUnits);
                soil.SoilOrganicMatter.OCUnits = SoilOrganicMatter.OCUnitsEnum.Total;
            }

            // Convert nitrogen to ppm.
            if (soil.Nitrogen != null)
            {
                double[] bd = LayerStructure.BDMapped(soil, soil.Nitrogen.Thickness);
                soil.Nitrogen.NO3 = Nppm(soil.Nitrogen.NO3, soil.Nitrogen.Thickness, soil.Nitrogen.NO3Units, bd);
                soil.Nitrogen.NO3Units = Nitrogen.NUnitsEnum.ppm;

                soil.Nitrogen.NH4 = Nppm(soil.Nitrogen.NH4, soil.Nitrogen.Thickness, soil.Nitrogen.NH4Units, bd);
                soil.Nitrogen.NH4Units = Nitrogen.NUnitsEnum.ppm;
            }

            // Convert analysis.
            if (soil.Analysis != null)
            {
                soil.Analysis.PH = PHWater(soil.Analysis.PH, soil.Analysis.PHUnits);
                soil.Analysis.PHUnits = Analysis.PHUnitsEnum.Water;
            }

            // Convert all samples.
            if (soil.Samples != null)
            {
                foreach (Sample sample in soil.Samples)
                {
                    // Convert sw units to volumetric.
                    if (sample.SW != null)
                        sample.SW = SWVolumetric(sample, soil);
                    sample.SWUnits = Sample.SWUnitsEnum.Volumetric;

                    // Convert no3 units to ppm.
                    if (sample.NO3 != null)
                    {
                        double[] bd = LayerStructure.BDMapped(soil, sample.Thickness);
                        sample.NO3 = Nppm(sample.NO3, sample.Thickness, sample.NO3Units, bd);
                    }
                    sample.NO3Units = Nitrogen.NUnitsEnum.ppm;

                    // Convert nh4 units to ppm.
                    if (sample.NH4 != null)
                    {
                        double[] bd = LayerStructure.BDMapped(soil, sample.Thickness);
                        sample.NH4 = Nppm(sample.NH4, sample.Thickness, sample.NH4Units, bd);
                    }
                    sample.NH4Units = Nitrogen.NUnitsEnum.ppm;

                    // Convert OC to total (%)
                    if (sample.OC != null)
                        sample.OC = OCTotalPercent(sample.OC, sample.OCUnits);
                    sample.OCUnits = SoilOrganicMatter.OCUnitsEnum.Total;

                    // Convert PH to water.
                    if (sample.PH != null)
                        sample.PH = PHWater(sample.PH, sample.PHUnits);
                    sample.PHUnits = Analysis.PHUnitsEnum.Water;
                }
            }

        }

        /// <summary>Calculates volumetric soil water for the given sample.</summary>
        /// <param name="sample">The sample.</param>
        /// <param name="soil">The soil.</param>
        /// <returns>Volumetric water (mm/mm)</returns>
        private static double[] SWVolumetric(Sample sample, Soil soil)
        {
            if (sample.SWUnits == Sample.SWUnitsEnum.Volumetric || sample.SW == null)
                return sample.SW;
            else
            {
                // convert the numbers
                if (sample.SWUnits == Sample.SWUnitsEnum.Gravimetric)
                {
                    double[] bd = LayerStructure.BDMapped(soil, sample.Thickness);

                    return MathUtilities.Multiply(sample.SW, bd);
                }
                else
                    return MathUtilities.Divide(sample.SW, sample.Thickness); // from mm to mm/mm
            }
        }

        /// <summary>Converts n values to ppm.</summary>
        /// <param name="n">The n values to convert.</param>
        /// <param name="thickness">The thickness of the values..</param>
        /// <param name="nunits">The current units of n.</param>
        /// <param name="bd">The related bulk density.</param>
        /// <returns>ppm values.</returns>
        private static double[] Nppm(double[] n, double[] thickness, Nitrogen.NUnitsEnum nunits, double[] bd)
        {
            if (nunits == Nitrogen.NUnitsEnum.ppm || n == null)
                return n;

            // kg/ha to ppm
            double[] newN = new double[n.Length];
            for (int i = 0; i < n.Length; i++)
            {
                if (n[i] == double.NaN)
                    newN[i] = double.NaN;
                else
                    newN[i] = n[i] * 100 / (bd[i] * thickness[i]);
            }
            
            return newN;
        }

        /// <summary>Converts OC to total %</summary>
        /// <param name="oc">The oc.</param>
        /// <param name="units">The current units.</param>
        /// <returns>The converted values</returns>
        private static double[] OCTotalPercent(double[] oc, SoilOrganicMatter.OCUnitsEnum units)
        {
            if (units == SoilOrganicMatter.OCUnitsEnum.Total || oc == null)
                return oc;
            
            // convert the numbers
            return MathUtilities.Multiply_Value(oc, 1.3);
        }

        /// <summary>Converst PH to water units.</summary>
        /// <param name="ph">The ph.</param>
        /// <param name="units">The current units.</param>
        /// <returns>The converted values</returns>
        private static double[] PHWater(double[] ph, Analysis.PHUnitsEnum units)
        {
            if (units == Analysis.PHUnitsEnum.Water || ph == null)
                return ph;

            // pH in water = (pH in CaCl X 1.1045) - 0.1375
            return MathUtilities.Subtract_Value(MathUtilities.Multiply_Value(ph, 1.1045), 0.1375);
        }
    }
}
