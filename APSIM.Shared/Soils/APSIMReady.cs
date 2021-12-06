// -----------------------------------------------------------------------
// <copyright file="APSIMReady.cs" company="APSIM Initiative">
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

    /// <summary>Methods for creating an APSIM ready soil.</summary>
    public class APSIMReadySoil
    {
        /// <summary>Creates an apsim ready soil.</summary>
        /// <param name="soil">The soil.</param>
        /// <returns>A newly created soil ready to be run in APSIM.</returns>
        public static Soil Create(Soil soil)
        {
            Unit.Convert(soil);
            RemoveInitialWater(soil);
            LayerStructure.Standardise(soil);
            Defaults.FillInMissingValues(soil);
            RemoveSamples(soil);

            return soil;
        }

        /// <summary>Removes the samples from the specified soil, copying data to other soil components.</summary>
        /// <param name="soil">The soil.</param>
        /// <exception cref="System.Exception">Cannot fold a sample into the Water component. Thicknesses are different.</exception>
        private static void RemoveSamples(Soil soil)
        {
            foreach (Sample sample in soil.Samples)
            {
                // Make sure the thicknesses are the same.
                if (!MathUtilities.AreEqual(sample.Thickness, soil.Water.Thickness))
                    throw new Exception("Cannot fold a sample into the Water component. Thicknesses are different.");

                if (sample.SW != null)
                    soil.Water.SW = sample.SW;

                if (sample.NO3 != null)
                {
                    if (soil.Nitrogen == null)
                    {
                        soil.Nitrogen = new Nitrogen();
                        soil.Nitrogen.Thickness = sample.Thickness;
                    }
                    soil.Nitrogen.NO3 = sample.NO3;
                    MathUtilities.ReplaceMissingValues(soil.Nitrogen.NO3, 0.01);
                }

                if (sample.NH4 != null)
                {
                    if (soil.Nitrogen == null)
                    {
                        soil.Nitrogen = new Nitrogen();
                        soil.Nitrogen.Thickness = sample.Thickness;
                    }
                    soil.Nitrogen.NH4 = sample.NH4;
                    MathUtilities.ReplaceMissingValues(soil.Nitrogen.NH4, 0.01);
                }

                if (sample.OC != null)
                {
                    double[] values = soil.SoilOrganicMatter.OC;
                    double[] thickness = soil.SoilOrganicMatter.Thickness;
                    OverlaySampleOnTo(sample.OC, sample.Thickness, ref values, ref thickness, 
                                      MathUtilities.LastValue(sample.OC));
                    soil.SoilOrganicMatter.OC = values;
                    soil.SoilOrganicMatter.Thickness = thickness;
                    MathUtilities.ReplaceMissingValues(soil.SoilOrganicMatter.OC, 0.01);
                }
                if (sample.PH != null)
                {
                    double[] values = soil.Analysis.PH;
                    double[] thickness = soil.Analysis.Thickness;
                    OverlaySampleOnTo(sample.PH, sample.Thickness, ref values, ref thickness,
                                      MathUtilities.LastValue(sample.PH));
                    soil.Analysis.PH = values;
                    soil.Analysis.Thickness = thickness;
                    MathUtilities.ReplaceMissingValues(soil.Analysis.PH, 7);
                }

                if (sample.ESP != null)
                {
                    double[] values = soil.Analysis.ESP;
                    double[] thickness = soil.Analysis.Thickness;
                    OverlaySampleOnTo(sample.ESP, sample.Thickness, ref values, ref thickness,
                                      MathUtilities.LastValue(sample.ESP));
                    soil.Analysis.ESP = values;
                    soil.Analysis.Thickness = thickness;
                    MathUtilities.ReplaceMissingValues(soil.Analysis.ESP, 0);
                }

                if (sample.EC != null)
                {
                    double[] values = soil.Analysis.EC;
                    double[] thickness = soil.Analysis.Thickness;
                    OverlaySampleOnTo(sample.EC, sample.Thickness, ref values, ref thickness,
                                      MathUtilities.LastValue(sample.EC));
                    soil.Analysis.EC = values;
                    soil.Analysis.Thickness = thickness;
                    MathUtilities.ReplaceMissingValues(soil.Analysis.EC, 0);
                }

                if (sample.CL != null)
                {
                    double[] values = soil.Analysis.CL;
                    double[] thickness = soil.Analysis.Thickness;
                    OverlaySampleOnTo(sample.CL, sample.Thickness, ref values, ref thickness,
                                      0.0);
                    soil.Analysis.CL = values;
                    soil.Analysis.Thickness = thickness;
                    MathUtilities.ReplaceMissingValues(soil.Analysis.CL, 0);
                }
            }

            soil.Samples.Clear();
        }

        /// <summary>Overlays samples on to soil values. Missing values (double.NaN) are replaced with a default value.</summary>
        /// <param name="sampleValues">The sample values.</param>
        /// <param name="sampleThickness">The sample thickness.</param>
        /// <param name="soilValues">The soil values.</param>
        /// <param name="soilThickness">The soil thickness.</param>
        /// <param name="defaultValue">The default value to use for any missing values.</param>
        /// <returns></returns>
        private static bool OverlaySampleOnTo(double[] sampleValues, double[] sampleThickness,
                                              ref double[] soilValues, ref double[] soilThickness,
                                              double defaultValue)
        {
            if (MathUtilities.ValuesInArray(sampleValues))
            {
                double[] Values = (double[])sampleValues.Clone();
                double[] Thicknesses = (double[])sampleThickness.Clone();
                InFillValues(ref Values, ref Thicknesses, soilValues, soilThickness);
                soilValues = Values;
                soilThickness = Thicknesses;

                for (int i = 0; i < soilValues.Length; i++)
                    if (double.IsNaN(soilValues[i]))
                        soilValues[i] = defaultValue;

                return true;
            }
            return false;
        }

        /// <summary>Takes values from SoilValues and puts them at the bottom of SampleValues.</summary>
        private static void InFillValues(ref double[] SampleValues, ref double[] SampleThickness,
                                         double[] SoilValues, double[] SoilThickness)
        {
            //-------------------------------------------------------------------------
            //  e.g. IF             SoilThickness  Values   SampleThickness	SampleValues
            //                           0-100		2         0-100				10
            //                         100-250	    3	     100-600			11
            //                         250-500		4		
            //                         500-750		5
            //                         750-900		6
            //						  900-1200		7
            //                        1200-1500		8
            //                        1500-1800		9
            //
            // will produce:		SampleThickness	        Values
            //						     0-100				  10
            //						   100-600				  11
            //						   600-750				   5
            //						   750-900				   6
            //						   900-1200				   7
            //						  1200-1500				   8
            //						  1500-1800				   9
            //
            //-------------------------------------------------------------------------
            if (SoilValues == null || SoilThickness == null) return;

            // remove missing layers.
            for (int i = 0; i < SampleValues.Length; i++)
            {
                if (double.IsNaN(SampleValues[i]) || double.IsNaN(SampleThickness[i]))
                {
                    SampleValues[i] = double.NaN;
                    SampleThickness[i] = double.NaN;
                }
            }
            SampleValues = MathUtilities.RemoveMissingValuesFromBottom(SampleValues);
            SampleThickness = MathUtilities.RemoveMissingValuesFromBottom(SampleThickness);

            double CumSampleDepth = MathUtilities.Sum(SampleThickness);

            // Work out if we need to create a dummy layer so that the sample depths line up 
            // with the soil depths
            double CumSoilDepth = 0.0;
            for (int SoilLayer = 0; SoilLayer < SoilThickness.Length; SoilLayer++)
            {
                CumSoilDepth += SoilThickness[SoilLayer];
                if (CumSoilDepth > CumSampleDepth)
                {
                    Array.Resize(ref SampleThickness, SampleThickness.Length + 1);
                    Array.Resize(ref SampleValues, SampleValues.Length + 1);
                    int i = SampleThickness.Length - 1;
                    SampleThickness[i] = CumSoilDepth - CumSampleDepth;
                    if (SoilValues[SoilLayer] == MathUtilities.MissingValue)
                        SampleValues[i] = 0.0;
                    else
                        SampleValues[i] = SoilValues[SoilLayer];
                    CumSampleDepth = CumSoilDepth;
                }
            }
        }

        /// <summary>Removes the initial water.</summary>
        /// <param name="soil">The soil.</param>
        private static void RemoveInitialWater(Soil soil)
        {
            if (soil.InitialWater != null)
            {
                soil.Water.SW = soil.InitialWater.SW(soil);

                // If any sample also has SW then get rid of it. Initial Water takes
                // priority.
                foreach (Sample sample in soil.Samples)
                    sample.SW = null;
            }
            soil.InitialWater = null;            
        }
    }
}
