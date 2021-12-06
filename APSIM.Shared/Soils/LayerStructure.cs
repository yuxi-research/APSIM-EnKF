// -----------------------------------------------------------------------
// <copyright file="LayerStructure.cs" company="APSIM Initiative">
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

    /// <summary>Methods to manipulate the layer structure of a soil.</summary>
    [Serializable]
    public class LayerStructure
    {
        /// <summary>Gets or sets the thickness.</summary>
        public double[] Thickness { get; set; }

        /// <summary>Standardise the specified soil with a uniform thickness.</summary>
        /// <param name="soil">The soil.</param>
        /// <returns>A standardised soil.</returns>
        public static void Standardise(Soil soil)
        {
            double[] toThickness = soil.Water.Thickness;
            if (soil.LayerStructure != null)
                toThickness = soil.LayerStructure.Thickness;

            SetWaterThickness(soil.Water, toThickness, soil);
            SetSoilWaterThickness(soil.SoilWater, toThickness);
            SetAnalysisThickness(soil.Analysis, toThickness);
            SetSoilOrganicMatterThickness(soil.SoilOrganicMatter, toThickness);
            SetPhosphorus(soil.Phosphorus, toThickness);
            SetSWIM(soil.Swim, toThickness);
            SetSoilTemperature(soil.SoilTemperature, toThickness);

            foreach (Sample sample in soil.Samples)
                SetSampleThickness(sample, toThickness, soil);
        }

        /// <summary>Sets the water thickness.</summary>
        /// <param name="water">The water.</param>
        /// <param name="toThickness">To thickness.</param>
        /// <param name="soil">Soil</param>
        private static void SetWaterThickness(Water water, double[] toThickness, Soil soil)
        {
            if (!MathUtilities.AreEqual(toThickness, water.Thickness))
            {
                water.BD = MapConcentration(water.BD, water.Thickness, toThickness, MathUtilities.LastValue(water.BD));
                water.SW = MapSW(water.SW, water.Thickness, toThickness, soil);
                water.AirDry = MapConcentration(water.AirDry, water.Thickness, toThickness, MathUtilities.LastValue(water.AirDry));
                water.LL15 = MapConcentration(water.LL15, water.Thickness, toThickness, MathUtilities.LastValue(water.LL15));
                water.DUL = MapConcentration(water.DUL, water.Thickness, toThickness, MathUtilities.LastValue(water.DUL));
                water.SAT = MapConcentration(water.SAT, water.Thickness, toThickness, MathUtilities.LastValue(water.SAT));
                water.KS = MapConcentration(water.KS, water.Thickness, toThickness, MathUtilities.LastValue(water.KS));
                water.Thickness = toThickness;
            }

            if (water.Crops != null)
            {
                foreach (SoilCrop crop in water.Crops)
                {
                    if (!MathUtilities.AreEqual(toThickness, crop.Thickness))
                    {
                        crop.LL = MapConcentration(crop.LL, crop.Thickness, toThickness, MathUtilities.LastValue(crop.LL));
                        crop.KL = MapConcentration(crop.KL, crop.Thickness, toThickness, MathUtilities.LastValue(crop.KL));
                        crop.XF = MapConcentration(crop.XF, crop.Thickness, toThickness, MathUtilities.LastValue(crop.XF));
                        crop.Thickness = toThickness;

                        // Ensure crop LL are between Airdry and DUL.
                        for (int i = 0; i < crop.LL.Length; i++)
                            crop.LL = MathUtilities.Constrain(crop.LL, soil.Water.AirDry, soil.Water.DUL);
                    }
                }
            }
        }

        /// <summary>Sets the soil water thickness.</summary>
        /// <param name="soilWater">The soil water.</param>
        /// <param name="thickness">Thickness to change soil water to.</param>
        private static void SetSoilWaterThickness(SoilWater soilWater, double[] thickness)
        {
            if (soilWater != null)
            {
                if (!MathUtilities.AreEqual(thickness, soilWater.Thickness))
                {
                    soilWater.KLAT = MapConcentration(soilWater.KLAT, soilWater.Thickness, thickness, MathUtilities.LastValue(soilWater.KLAT));
                    soilWater.MWCON = MapConcentration(soilWater.MWCON, soilWater.Thickness, thickness, MathUtilities.LastValue(soilWater.MWCON));
                    soilWater.SWCON = MapConcentration(soilWater.SWCON, soilWater.Thickness, thickness, 0.0);

                    soilWater.Thickness = thickness;
                }

                MathUtilities.ReplaceMissingValues(soilWater.SWCON, 0.0);
            }
        }

        /// <summary>Sets the soil organic matter thickness.</summary>
        /// <param name="soilOrganicMatter">The soil organic matter.</param>
        /// <param name="thickness">Thickness to change soil water to.</param>
        private static void SetSoilOrganicMatterThickness(SoilOrganicMatter soilOrganicMatter, double[] thickness)
        {
            if (soilOrganicMatter != null)
            {
                if (!MathUtilities.AreEqual(thickness, soilOrganicMatter.Thickness))
                {
                    soilOrganicMatter.FBiom = MapConcentration(soilOrganicMatter.FBiom, soilOrganicMatter.Thickness, thickness, MathUtilities.LastValue(soilOrganicMatter.FBiom));
                    soilOrganicMatter.FInert = MapConcentration(soilOrganicMatter.FInert, soilOrganicMatter.Thickness, thickness, MathUtilities.LastValue(soilOrganicMatter.FInert));
                    soilOrganicMatter.OC = MapConcentration(soilOrganicMatter.OC, soilOrganicMatter.Thickness, thickness, MathUtilities.LastValue(soilOrganicMatter.OC));
                    soilOrganicMatter.Thickness = thickness;

                    soilOrganicMatter.OCMetadata = StringUtilities.CreateStringArray("Mapped", thickness.Length); ;
                }

                if (soilOrganicMatter.FBiom != null)
                    MathUtilities.ReplaceMissingValues(soilOrganicMatter.FBiom, MathUtilities.LastValue(soilOrganicMatter.FBiom));
                if (soilOrganicMatter.FInert != null)
                    MathUtilities.ReplaceMissingValues(soilOrganicMatter.FInert, MathUtilities.LastValue(soilOrganicMatter.FInert));
                if (soilOrganicMatter.OC != null)
                    MathUtilities.ReplaceMissingValues(soilOrganicMatter.OC, MathUtilities.LastValue(soilOrganicMatter.OC));
            }
        }

        /// <summary>Sets the analysis thickness.</summary>
        /// <param name="analysis">The analysis.</param>
        /// <param name="thickness">The thickness to change the analysis to.</param>
        private static void SetAnalysisThickness(Analysis analysis, double[] thickness)
        {
            if (analysis != null && !MathUtilities.AreEqual(thickness, analysis.Thickness))
            {

                string[] metadata = StringUtilities.CreateStringArray("Mapped", thickness.Length);

                analysis.Al = MapConcentration(analysis.Al, analysis.Thickness, thickness, MathUtilities.LastValue(analysis.Al));
                analysis.AlMetadata = metadata;
                analysis.Ca = MapConcentration(analysis.Ca, analysis.Thickness, thickness, MathUtilities.LastValue(analysis.Ca));
                analysis.CaMetadata = metadata;
                analysis.CEC = MapConcentration(analysis.CEC, analysis.Thickness, thickness, MathUtilities.LastValue(analysis.CEC));
                analysis.CECMetadata = metadata;
                analysis.CL = MapConcentration(analysis.CL, analysis.Thickness, thickness, MathUtilities.LastValue(analysis.CL));
                analysis.CLMetadata = metadata;
                analysis.EC = MapConcentration(analysis.EC, analysis.Thickness, thickness, MathUtilities.LastValue(analysis.EC));
                analysis.ECMetadata = metadata;
                analysis.ESP = MapConcentration(analysis.ESP, analysis.Thickness, thickness, MathUtilities.LastValue(analysis.ESP));
                analysis.ESPMetadata = metadata;
                analysis.K = MapConcentration(analysis.K, analysis.Thickness, thickness, MathUtilities.LastValue(analysis.K));
                analysis.KMetadata = metadata;
                analysis.Mg = MapConcentration(analysis.Mg, analysis.Thickness, thickness, MathUtilities.LastValue(analysis.Mg));
                analysis.MgMetadata = metadata;
                analysis.Mn = MapConcentration(analysis.Mn, analysis.Thickness, thickness, MathUtilities.LastValue(analysis.Mn));
                analysis.MnMetadata = metadata;
                analysis.MunsellColour = null;
                analysis.Na = MapConcentration(analysis.Na, analysis.Thickness, thickness, MathUtilities.LastValue(analysis.Na));
                analysis.NaMetadata = metadata;
                analysis.ParticleSizeClay = MapConcentration(analysis.ParticleSizeClay, analysis.Thickness, thickness, MathUtilities.LastValue(analysis.ParticleSizeClay));
                analysis.ParticleSizeClayMetadata = metadata;
                analysis.ParticleSizeSand = MapConcentration(analysis.ParticleSizeSand, analysis.Thickness, thickness, MathUtilities.LastValue(analysis.ParticleSizeSand));
                analysis.ParticleSizeSandMetadata = metadata;
                analysis.ParticleSizeSilt = MapConcentration(analysis.ParticleSizeSilt, analysis.Thickness, thickness, MathUtilities.LastValue(analysis.ParticleSizeSilt));
                analysis.ParticleSizeSiltMetadata = metadata;
                analysis.PH = MapConcentration(analysis.PH, analysis.Thickness, thickness, MathUtilities.LastValue(analysis.PH));
                analysis.PHMetadata = metadata;
                analysis.Rocks = MapConcentration(analysis.Rocks, analysis.Thickness, thickness, MathUtilities.LastValue(analysis.Rocks));
                analysis.RocksMetadata = metadata;
                analysis.Texture = null;
                analysis.Thickness = thickness;

            }
        }

        /// <summary>Sets the sample thickness.</summary>
        /// <param name="sample">The sample.</param>
        /// <param name="thickness">The thickness to change the sample to.</param>
        /// <param name="soil">The soil</param>
        private static void SetSampleThickness(Sample sample, double[] thickness, Soil soil)
        {
            if (!MathUtilities.AreEqual(thickness, sample.Thickness))
            {
                sample.Name = sample.Name;
                sample.Date = sample.Date;

                if (sample.SW != null)
                    sample.SW = MapSW(sample.SW, sample.Thickness, thickness, soil);
                if (sample.NH4 != null)
                    sample.NH4 = MapConcentration(sample.NH4, sample.Thickness, thickness, 0.01);
                if (sample.NO3 != null)
                    sample.NO3 = MapConcentration(sample.NO3, sample.Thickness, thickness, 0.01);

                // The elements below will be overlaid over other arrays of values so we want 
                // to have missing values (double.NaN) used at the bottom of the profile.
                
                if (sample.CL != null)
                    sample.CL = MapConcentration(sample.CL, sample.Thickness, thickness, double.NaN, allowMissingValues:true);
                if (sample.EC != null)
                    sample.EC = MapConcentration(sample.EC, sample.Thickness, thickness, double.NaN, allowMissingValues: true);
                if (sample.ESP != null)
                    sample.ESP = MapConcentration(sample.ESP, sample.Thickness, thickness, double.NaN, allowMissingValues: true);
                if (sample.OC != null)
                    sample.OC = MapConcentration(sample.OC, sample.Thickness, thickness, double.NaN, allowMissingValues: true);
                if (sample.PH != null)
                    sample.PH = MapConcentration(sample.PH, sample.Thickness, thickness, double.NaN, allowMissingValues: true);
                sample.Thickness = thickness;
            }
        }

        /// <summary>Convert the crop to the specified thickness. Ensures LL is between AirDry and DUL.</summary>
        /// <param name="crop">The crop to convert</param>
        /// <param name="thickness">The thicknesses to convert the crop to.</param>
        /// <param name="soil">The soil the crop belongs to.</param>
        private static void SetCropThickness(SoilCrop crop, double[] thickness, Soil soil)
        {
            if (!MathUtilities.AreEqual(thickness, crop.Thickness))
            {
                crop.LL = MapConcentration(crop.LL, crop.Thickness, thickness, MathUtilities.LastValue(crop.LL));
                crop.KL = MapConcentration(crop.KL, crop.Thickness, thickness, MathUtilities.LastValue(crop.KL));
                crop.XF = MapConcentration(crop.XF, crop.Thickness, thickness, MathUtilities.LastValue(crop.XF));
                crop.Thickness = thickness;
                
                crop.LL = MathUtilities.Constrain(crop.LL, AirDryMapped(soil, thickness), DULMapped(soil, thickness));
            }
        }

        /// <summary>Convert the phosphorus component to the specified thicknesses.</summary>
        /// <param name="phosphorus">The phosphorus.</param>
        /// <param name="thickness">The thickness to convert to.</param>
        private static void SetPhosphorus(Phosphorus phosphorus, double[] thickness)
        {
            if (phosphorus != null && !MathUtilities.AreEqual(thickness, phosphorus.Thickness))
            {
                phosphorus.BandedP = MapConcentration(phosphorus.BandedP, phosphorus.Thickness, thickness, MathUtilities.LastValue(phosphorus.BandedP));
                phosphorus.LabileP = MapConcentration(phosphorus.LabileP, phosphorus.Thickness, thickness, MathUtilities.LastValue(phosphorus.LabileP));
                phosphorus.RockP = MapConcentration(phosphorus.RockP, phosphorus.Thickness, thickness, MathUtilities.LastValue(phosphorus.RockP));
                phosphorus.Sorption = MapConcentration(phosphorus.Sorption, phosphorus.Thickness, thickness, MathUtilities.LastValue(phosphorus.Sorption));
                phosphorus.Thickness = thickness;
            }
        }

        /// <summary>Convert the SWIM component to the specified thicknesses.</summary>
        /// <param name="swim">The swim component.</param>
        /// <param name="thickness">The thickness to convert to.</param>
        private static void SetSWIM(Swim swim, double[] thickness)
        {
            if (swim != null && swim.SwimSoluteParameters != null && !MathUtilities.AreEqual(thickness, swim.SwimSoluteParameters.Thickness))
            {
                swim.SwimSoluteParameters.NO3Exco = MapConcentration(swim.SwimSoluteParameters.NO3Exco, swim.SwimSoluteParameters.Thickness, thickness, MathUtilities.LastValue(swim.SwimSoluteParameters.NO3Exco));
                swim.SwimSoluteParameters.NO3FIP = MapConcentration(swim.SwimSoluteParameters.NO3FIP, swim.SwimSoluteParameters.Thickness, thickness, 0.0);      // Making these use the LastValue causes C:/Apsim/Tests/UserRuns/WinteringSystems/BaseSimulation.sum to fail
                swim.SwimSoluteParameters.NH4Exco = MapConcentration(swim.SwimSoluteParameters.NH4Exco, swim.SwimSoluteParameters.Thickness, thickness, 0.0); 
                swim.SwimSoluteParameters.NH4FIP = MapConcentration(swim.SwimSoluteParameters.NH4FIP, swim.SwimSoluteParameters.Thickness, thickness, 0.0);  
                swim.SwimSoluteParameters.UreaExco = MapConcentration(swim.SwimSoluteParameters.UreaExco, swim.SwimSoluteParameters.Thickness, thickness, MathUtilities.LastValue(swim.SwimSoluteParameters.UreaExco));
                swim.SwimSoluteParameters.UreaFIP = MapConcentration(swim.SwimSoluteParameters.UreaFIP, swim.SwimSoluteParameters.Thickness, thickness, 0.0);   
                swim.SwimSoluteParameters.ClExco = MapConcentration(swim.SwimSoluteParameters.ClExco, swim.SwimSoluteParameters.Thickness, thickness, MathUtilities.LastValue(swim.SwimSoluteParameters.ClExco));
                swim.SwimSoluteParameters.ClFIP = MapConcentration(swim.SwimSoluteParameters.ClFIP, swim.SwimSoluteParameters.Thickness, thickness, MathUtilities.LastValue(swim.SwimSoluteParameters.ClFIP));
                swim.SwimSoluteParameters.TracerExco = MapConcentration(swim.SwimSoluteParameters.TracerExco, swim.SwimSoluteParameters.Thickness, thickness, MathUtilities.LastValue(swim.SwimSoluteParameters.TracerExco));
                swim.SwimSoluteParameters.TracerFIP = MapConcentration(swim.SwimSoluteParameters.TracerFIP, swim.SwimSoluteParameters.Thickness, thickness, MathUtilities.LastValue(swim.SwimSoluteParameters.TracerFIP));
                swim.SwimSoluteParameters.MineralisationInhibitorExco = MapConcentration(swim.SwimSoluteParameters.MineralisationInhibitorExco, swim.SwimSoluteParameters.Thickness, thickness, MathUtilities.LastValue(swim.SwimSoluteParameters.MineralisationInhibitorExco));
                swim.SwimSoluteParameters.MineralisationInhibitorFIP = MapConcentration(swim.SwimSoluteParameters.MineralisationInhibitorFIP, swim.SwimSoluteParameters.Thickness, thickness, MathUtilities.LastValue(swim.SwimSoluteParameters.MineralisationInhibitorFIP));
                swim.SwimSoluteParameters.UreaseInhibitorExco = MapConcentration(swim.SwimSoluteParameters.UreaseInhibitorExco, swim.SwimSoluteParameters.Thickness, thickness, MathUtilities.LastValue(swim.SwimSoluteParameters.UreaseInhibitorExco));
                swim.SwimSoluteParameters.UreaseInhibitorFIP = MapConcentration(swim.SwimSoluteParameters.UreaseInhibitorFIP, swim.SwimSoluteParameters.Thickness, thickness, MathUtilities.LastValue(swim.SwimSoluteParameters.UreaseInhibitorFIP));
                swim.SwimSoluteParameters.NitrificationInhibitorExco = MapConcentration(swim.SwimSoluteParameters.NitrificationInhibitorExco, swim.SwimSoluteParameters.Thickness, thickness, MathUtilities.LastValue(swim.SwimSoluteParameters.NitrificationInhibitorExco));
                swim.SwimSoluteParameters.NitrificationInhibitorFIP = MapConcentration(swim.SwimSoluteParameters.NitrificationInhibitorFIP, swim.SwimSoluteParameters.Thickness, thickness, MathUtilities.LastValue(swim.SwimSoluteParameters.NitrificationInhibitorFIP));
                swim.SwimSoluteParameters.DenitrificationInhibitorExco = MapConcentration(swim.SwimSoluteParameters.DenitrificationInhibitorExco, swim.SwimSoluteParameters.Thickness, thickness, MathUtilities.LastValue(swim.SwimSoluteParameters.DenitrificationInhibitorExco));
                swim.SwimSoluteParameters.DenitrificationInhibitorFIP = MapConcentration(swim.SwimSoluteParameters.DenitrificationInhibitorFIP, swim.SwimSoluteParameters.Thickness, thickness, MathUtilities.LastValue(swim.SwimSoluteParameters.DenitrificationInhibitorFIP));
                swim.SwimSoluteParameters.Thickness = thickness;
            }
        }

        /// <summary>Convert the soil temperature component to the specified thicknesses.</summary>
        /// <param name="soilTemperature">The soil temperature.</param>
        /// <param name="thickness">The thickness to convert to.</param>
        private static void SetSoilTemperature(SoilTemperature soilTemperature, double[] thickness)
        {
            if (soilTemperature != null && !MathUtilities.AreEqual(thickness, soilTemperature.Thickness))
            {
                soilTemperature.InitialSoilTemperature = MapConcentration(soilTemperature.InitialSoilTemperature, soilTemperature.Thickness, thickness, MathUtilities.LastValue(soilTemperature.InitialSoilTemperature));
                soilTemperature.Thickness = thickness;
            }
        }

        /// <summary>The type of mapping.</summary>
        private enum MapType { Mass, Concentration, UseBD }

        /// <summary>Map soil variables (using concentration) from one layer structure to another.</summary>
        /// <param name="fromValues">The from values.</param>
        /// <param name="fromThickness">The from thickness.</param>
        /// <param name="toThickness">To thickness.</param>
        /// <param name="defaultValueForBelowProfile">The default value for below profile.</param>
        /// <param name="allowMissingValues">Tolerate missing values (double.NaN)?</param>
        /// <returns></returns>
        internal static double[] MapConcentration(double[] fromValues, double[] fromThickness,
                                                  double[] toThickness,
                                                  double defaultValueForBelowProfile,
                                                  bool allowMissingValues = false)
        {
            if (fromValues != null)
            {
                if (fromValues.Length != fromThickness.Length)
                    throw new Exception("In MapConcentraction, the number of values doesn't match the number of thicknesses.");
                if (fromValues == null || fromThickness == null)
                    return null;

                // convert from values to a mass basis with a dummy bottom layer.
                List<double> values = new List<double>();
                List<double> thickness = new List<double>();
                for (int i = 0; i < fromValues.Length; i++)
                {
                    if (!allowMissingValues && double.IsNaN(fromValues[i]))
                        break;

                    values.Add(fromValues[i]);
                    thickness.Add(fromThickness[i]);
                }

                values.Add(defaultValueForBelowProfile);
                thickness.Add(3000);
                double[] massValues = MathUtilities.Multiply(values.ToArray(), thickness.ToArray());

                double[] newValues = MapMass(massValues, thickness.ToArray(), toThickness, allowMissingValues);

                // Convert mass back to concentration and return
                return MathUtilities.Divide(newValues, toThickness);
            }
            return null;
        }

        /// <summary>Map soil variables (using BD) from one layer structure to another.</summary>
        /// <param name="fromValues">The from values.</param>
        /// <param name="fromThickness">The from thickness.</param>
        /// <param name="toThickness">To thickness.</param>
        /// <param name="soil">The soil.</param>
        /// <param name="defaultValueForBelowProfile">The default value for below profile.</param>
        /// <returns></returns>
        private static double[] MapUsingBD(double[] fromValues, double[] fromThickness,
                                           double[] toThickness,
                                           Soil soil,
                                           double defaultValueForBelowProfile)
        {
            if (fromValues == null || fromThickness == null)
                return null;

            // create an array of values with a dummy bottom layer.
            List<double> values = new List<double>();
            values.AddRange(fromValues);
            values.Add(defaultValueForBelowProfile);
            List<double> thickness = new List<double>();
            thickness.AddRange(fromThickness);
            thickness.Add(3000);

            // convert fromValues to a mass basis
            double[] BD = BDMapped(soil, fromThickness);
            for (int Layer = 0; Layer < values.Count; Layer++)
                values[Layer] = values[Layer] * BD[Layer] * fromThickness[Layer] / 100;

            // change layer structure
            double[] newValues = MapMass(values.ToArray(), thickness.ToArray(), toThickness);

            // convert newValues back to original units and return
            BD = BDMapped(soil, toThickness);
            for (int Layer = 0; Layer < newValues.Length; Layer++)
                newValues[Layer] = newValues[Layer] * 100.0 / BD[Layer] / toThickness[Layer];
            return newValues;
        }

        /// <summary>Map soil water from one layer structure to another.</summary>
        /// <param name="fromValues">The from values.</param>
        /// <param name="fromThickness">The from thickness.</param>
        /// <param name="toThickness">To thickness.</param>
        /// <param name="soil">The soil.</param>
        /// <returns></returns>
        private static double[] MapSW(double[] fromValues, double[] fromThickness, double[] toThickness, Soil soil)
        {
            if (fromValues == null || fromThickness == null)
                return null;

            // convert from values to a mass basis with a dummy bottom layer.
            List<double> values = new List<double>();
            values.AddRange(fromValues);
            values.Add(MathUtilities.LastValue(fromValues) * 0.8);
            values.Add(MathUtilities.LastValue(fromValues) * 0.4);
            values.Add(0.0);
            List<double> thickness = new List<double>();
            thickness.AddRange(fromThickness);
            thickness.Add(MathUtilities.LastValue(fromThickness));
            thickness.Add(MathUtilities.LastValue(fromThickness));
            thickness.Add(3000);

            // Get the first crop ll or ll15.
            double[] LowerBound;
            if (soil.Water.Crops.Count > 0)
                LowerBound = LLMapped(soil.Water.Crops[0], thickness.ToArray());
            else
                LowerBound = LL15Mapped(soil, thickness.ToArray());
            if (LowerBound == null)
                throw new Exception("Cannot find crop lower limit or LL15 in soil");

            // Make sure all SW values below LastIndex don't go below CLL.
            int bottomLayer = fromThickness.Length - 1;
            for (int i = bottomLayer + 1; i < thickness.Count; i++)
                values[i] = Math.Max(values[i], LowerBound[i]);

            double[] massValues = MathUtilities.Multiply(values.ToArray(), thickness.ToArray());

            // Convert mass back to concentration and return
            double[] newValues = MathUtilities.Divide(MapMass(massValues, thickness.ToArray(), toThickness), toThickness);



            return newValues;
        }

        /// <summary>Map soil variables from one layer structure to another.</summary>
        /// <param name="fromValues">The f values.</param>
        /// <param name="fromThickness">The f thickness.</param>
        /// <param name="toThickness">To thickness.</param>
        /// <param name="allowMissingValues">Tolerate missing values (double.NaN)?</param>
        /// <returns>The from values mapped to the specified thickness</returns>
        private static double[] MapMass(double[] fromValues, double[] fromThickness, double[] toThickness,
                                        bool allowMissingValues = false)
        {
            if (fromValues == null || fromThickness == null)
                return null;

            double[] FromThickness = MathUtilities.RemoveMissingValuesFromBottom((double[])fromThickness.Clone());
            double[] FromValues = (double[])fromValues.Clone();

            if (FromValues == null)
                return null;

            if (!allowMissingValues)
            {
                // remove missing layers.
                for (int i = 0; i < FromValues.Length; i++)
                {
                    if (double.IsNaN(FromValues[i]) || i >= FromThickness.Length || double.IsNaN(FromThickness[i]))
                    {
                        FromValues[i] = double.NaN;
                        if (i == FromThickness.Length)
                            Array.Resize(ref FromThickness, i + 1);
                        FromThickness[i] = double.NaN;
                    }
                }
                FromValues = MathUtilities.RemoveMissingValuesFromBottom(FromValues);
                FromThickness = MathUtilities.RemoveMissingValuesFromBottom(FromThickness);
            }

            if (MathUtilities.AreEqual(FromThickness, toThickness))
                return FromValues;

            if (FromValues.Length != FromThickness.Length)
                return null;

            // Remapping is achieved by first constructing a map of
            // cumulative mass vs depth
            // The new values of mass per layer can be linearly
            // interpolated back from this shape taking into account
            // the rescaling of the profile.

            double[] CumDepth = new double[FromValues.Length + 1];
            double[] CumMass = new double[FromValues.Length + 1];
            CumDepth[0] = 0.0;
            CumMass[0] = 0.0;
            for (int Layer = 0; Layer < FromThickness.Length; Layer++)
            {
                CumDepth[Layer + 1] = CumDepth[Layer] + FromThickness[Layer];
                CumMass[Layer + 1] = CumMass[Layer] + FromValues[Layer];
            }

            //look up new mass from interpolation pairs
            double[] ToMass = new double[toThickness.Length];
            for (int Layer = 1; Layer <= toThickness.Length; Layer++)
            {
                double LayerBottom = MathUtilities.Sum(toThickness, 0, Layer, 0.0);
                double LayerTop = LayerBottom - toThickness[Layer - 1];
                bool DidInterpolate;
                double CumMassTop = MathUtilities.LinearInterpReal(LayerTop, CumDepth,
                    CumMass, out DidInterpolate);
                double CumMassBottom = MathUtilities.LinearInterpReal(LayerBottom, CumDepth,
                    CumMass, out DidInterpolate);
                ToMass[Layer - 1] = CumMassBottom - CumMassTop;
            }

            if (!allowMissingValues)
            {
                for (int i = 0; i < ToMass.Length; i++)
                    if (double.IsNaN(ToMass[i]))
                        ToMass[i] = 0.0;
            }

            return ToMass;
        }

        /// <summary>AirDry - mapped to the specified layer structure. Units: mm/mm        /// </summary>
        /// <param name="soil">The soil.</param>
        /// <param name="ToThickness">To thickness.</param>
        /// <returns></returns>
        private static double[] AirDryMapped(Soil soil, double[] ToThickness)
        {
            return MapConcentration(soil.Water.AirDry, soil.Water.Thickness, ToThickness, soil.Water.AirDry.Last());
        }

        /// <summary>Crop lower limit - mapped to the specified layer structure. Units: mm/mm        /// </summary>
        /// <param name="crop">The crop.</param>
        /// <param name="ToThickness">To thickness.</param>
        /// <returns></returns>
        private static double[] LLMapped(SoilCrop crop, double[] ToThickness)
        {
            return MapConcentration(crop.LL, crop.Thickness, ToThickness, MathUtilities.LastValue(crop.LL));
        }

        /// <summary>Bulk density - mapped to the specified layer structure. Units: mm/mm</summary>
        /// <param name="soil">The soil.</param>
        /// <param name="ToThickness">To thickness.</param>
        /// <returns></returns>
        internal static double[] BDMapped(Soil soil, double[] ToThickness)
        {
            return MapConcentration(soil.Water.BD, soil.Water.Thickness, ToThickness, soil.Water.BD.Last());
        }

        /// <summary>Lower limit 15 bar - mapped to the specified layer structure. Units: mm/mm        /// </summary>
        /// <param name="soil">The soil.</param>
        /// <param name="ToThickness">To thickness.</param>
        /// <returns></returns>
        internal static double[] LL15Mapped(Soil soil, double[] ToThickness)
        {
            return MapConcentration(soil.Water.LL15, soil.Water.Thickness, ToThickness, soil.Water.LL15.Last());
        }

        /// <summary>Drained upper limit - mapped to the specified layer structure. Units: mm/mm        /// </summary>
        /// <param name="soil">The soil.</param>
        /// <param name="ToThickness">To thickness.</param>
        /// <returns></returns>
        internal static double[] DULMapped(Soil soil, double[] ToThickness)
        {
            return MapConcentration(soil.Water.DUL, soil.Water.Thickness, ToThickness, soil.Water.DUL.Last());
        }

    }
}
