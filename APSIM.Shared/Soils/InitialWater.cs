// -----------------------------------------------------------------------
// <copyright file="InitialWater.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
// -----------------------------------------------------------------------
namespace APSIM.Shared.Soils
{
    using APSIM.Shared.Utilities;
    using System;
    using System.Linq;
    using System.Collections.Generic;

    /// <summary>Class for holding information about the initial water state for a soil.</summary>
    [Serializable]
    public class InitialWater
    {
        /// <summary>Gets or sets the name.</summary>
        public string Name { get; set; }

        /// <summary>Two different mechanisms for specifying percent of water.</summary>
        public enum PercentMethodEnum 
        {
            /// <summary>Water should fill the profile from the top.</summary>
            FilledFromTop,

            /// <summary>Water should be evenly distributed down the profile.</summary>
            EvenlyDistributed 
        };

        /// <summary>Gets or sets the percent method.</summary>
        public PercentMethodEnum PercentMethod { get; set; }
        
        /// <summary>The fraction full (0-1)</summary>
        public double FractionFull = double.NaN;
        
        /// <summary>The depth wet soil (mm)</summary>
        public double DepthWetSoil = double.NaN;
        
        /// <summary>Gets or sets the crop name that the water should be relative to.</summary>
        public string RelativeTo { get; set; }

        /// <summary>Calculate a layered soil water. Units: mm/mm</summary>
        public double[] SW(Soil soil)
        {
            string[] cropNames = soil.Water.Crops.Select(c => c.Name).ToArray();

            // Get the correct LL and XF
            int cropIndex = -1;
            if (RelativeTo != null)
                cropIndex = StringUtilities.IndexOfCaseInsensitive(cropNames, RelativeTo);
            double[] ll;
            double[] xf = null;
            double[] PAWCmm;
            if (cropIndex == -1)
            {
                ll = soil.Water.LL15;
                PAWCmm = PAWC.OfSoilmm(soil);
            }
            else 
            {
                SoilCrop crop = soil.Water.Crops[cropIndex];
                ll = crop.LL;
                xf = crop.XF;
                PAWCmm = PAWC.OfCropmm(soil, crop);
            }

            if (double.IsNaN(DepthWetSoil))
            {
                if (PercentMethod == InitialWater.PercentMethodEnum.FilledFromTop)
                    return SWFilledFromTop(PAWCmm, ll, soil.Water.DUL, xf);
                else
                    return SWEvenlyDistributed(ll, soil.Water.DUL);
            }
            else
                return SWDepthWetSoil(soil.Water.Thickness, ll, soil.Water.DUL);
        }

        /// <summary>Calculate a layered soil water using a FractionFull and filled from the top. Units: mm/mm</summary>
        private double[] SWFilledFromTop(double[] PAWCmm, double[] LL, double[] DUL, double[] XF)
        {
            double[] SW = new double[PAWCmm.Length];

            double AmountWater = MathUtilities.Sum(PAWCmm) * FractionFull;
            for (int Layer = 0; Layer < LL.Length; Layer++)
            {
                if (AmountWater >= 0 && XF != null && XF[Layer] == 0)
                    SW[Layer] = LL[Layer];
                else if (AmountWater >= PAWCmm[Layer])
                {
                    SW[Layer] = DUL[Layer];
                    AmountWater = AmountWater - PAWCmm[Layer];
                }
                else
                {
                    double Prop = AmountWater / PAWCmm[Layer];
                    SW[Layer] = Prop * (DUL[Layer] - LL[Layer]) + LL[Layer];
                    AmountWater = 0;
                }
            }
            return SW;
        }

        /// <summary>Calculate a layered soil water using a FractionFull and evenly distributed. Units: mm/mm</summary>
        private double[] SWEvenlyDistributed(double[] LL, double[] DUL)
        {
            double[] SW = new double[LL.Length];
            for (int Layer = 0; Layer < LL.Length; Layer++)
                SW[Layer] = FractionFull * (DUL[Layer] - LL[Layer]) + LL[Layer];
            return SW;
        }

        /// <summary>Calculate a layered soil water using a depth of wet soil. Units: mm/mm</summary>
        private double[] SWDepthWetSoil(double[] Thickness, double[] LL, double[] DUL)
        {
            double[] SW = new double[LL.Length];
            double DepthSoFar = 0;
            for (int Layer = 0; Layer < Thickness.Length; Layer++)
            {
                if (DepthWetSoil > DepthSoFar + Thickness[Layer])
                    SW[Layer] = DUL[Layer];
                else
                {
                    double Prop = Math.Max(DepthWetSoil - DepthSoFar, 0) / Thickness[Layer];
                    SW[Layer] = Prop * (DUL[Layer] - LL[Layer]) + LL[Layer];
                }
                DepthSoFar += Thickness[Layer];
            }
            return SW;
        }
    }

}