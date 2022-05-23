﻿// -----------------------------------------------------------------------
// <copyright file="Runoff.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
// -----------------------------------------------------------------------
namespace Models.WaterModel
{
    using APSIM.Shared.Soils;
    using APSIM.Shared.Utilities;
    using Core;
    using Interfaces;
    using PMF.Functions;
    using SurfaceOM;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    /// Runoff from rainfall is calculated using the USDA-Soil Conservation Service procedure known as the curve number technique. 
    /// The procedure uses total precipitation from one or more storms occurring on a given day to estimate runoff.
    /// The relation excludes duration of rainfall as an explicit variable, and so rainfall intensity is ignored.
    /// When irrigation is applied it can optionally be included in the runoff calculation. This flag (willRunoff) can be set
    /// when applying irrigation.
    /// 
    /// ![Alt Text](..\\..\\Documentation\\Images\\RunoffRainfallCurves.png)
    /// Figure: Runoff response curves (ie runoff as a function of total daily rainfall) are specified by numbers from 0 (no runoff) to 100 (all runoff). 
    /// Response curves for three runoff curve numbers for rainfall varying between 0 and 100 mm per day.
    /// 
    /// The user supplies a curve number for average antecedent rainfall conditions (CN2Bare). 
    /// From this value the wet (high runoff potential) response curve and the dry (low runoff potential) 
    /// response curve are calculated. The SoilWater module will then use the family of curves between these 
    /// two extremes for calculation of runoff depending on the daily moisture status of the soil. 
    /// The effect of soil moisture on runoff is confined to the effective hydraulic depth as specified in the 
    /// module's ini file and is calculated to give extra weighting to layers closer to the soil surface.
    /// ![Alt Text](..\\..\\Documentation\\Images\\RunoffResponseCurve.png)
    /// Figure: Runoff response curves (ie runoff as a function of total daily rainfall) are specified by numbers from 0 (no runoff) to 100 (all runoff). 
    ///
    /// ![Alt Text](..\\..\\Documentation\\Images\\CurveNumberCover.png) 
    /// Figure: Residue cover effect on runoff curve number where bare soil curve number is 75 and total reduction in 
    /// curve number is 20 at 80% cover. 
    /// 
    /// Surface residues inhibit the transport of water across the soil surface during runoff events and so different 
    /// families of response curves are used according to the amount of crop and residue cover.The extent of the effect 
    /// on runoff is specified by a threshold surface cover (CNCov), above which there is no effect, and the corresponding 
    /// curve number reduction (CNRed). 
    ///
    /// Tillage of the soil surface also reduces runoff potential, and a similar modification of Curve Number is used to 
    /// represent this process. A tillage event is directed to the module, specifying cn_red, the CN reduction, and cn_rain, 
    /// the rainfall amount required to remove the tillage roughness. CN2 is immediately reduced and increases linearly with 
    /// cumulative rain, ie.roughness is smoothed out by rain. 
    /// </summary>
    [Serializable]
    public class RunoffModel : Model, IFunction
    {
        // --- Links -------------------------------------------------------------------------

        /// <summary>The water movement model.</summary>
        [Link]
        private SoilModel soil = null;

        /// <summary>A function for reducing CN due to cover.</summary>
        [Link]
        private IFunction reductionForCover = null;

        /// <summary>A function for reducing CN due to tillage.</summary>
        [Link]
        private IFunction reductionForTillage = null;

        // --- Privates ----------------------------------------------------------------------

        /// <summary>Effective hydraulic depth (mm)</summary>
        private double hydrolEffectiveDepth = 450;

        // --- Settable properties -----------------------------------------------------------

        /// <summary>Gets or sets the c n2 bare.</summary>
        [DescriptionAttribute("Bare soil runoff curve number")]
        public double CN2Bare { get; set; }

        // --- Outputs -----------------------------------------------------------------------

        /// <summary>Calculate and return the runoff (mm).</summary>
        public double Value(int arrayIndex = -1)
        {
            double runoff = 0.0;

            if (soil.PotentialRunoff > 0.0)
            {
                double cn2New = CN2Bare - reductionForCover.Value(arrayIndex) - reductionForTillage.Value(arrayIndex);

                // cut off response to cover at high covers
                cn2New = MathUtilities.Bound(cn2New, 0.0, 100.0);

                // Calculate CN proportional in dry range (dul to ll15)
                double[] runoff_wf = RunoffWeightingFactor();
                double[] SW = soil.Water;
                double[] LL15 = MathUtilities.Multiply(soil.Properties.Water.LL15, soil.Properties.Water.Thickness);
                double[] DUL = MathUtilities.Multiply(soil.Properties.Water.DUL, soil.Properties.Water.Thickness);
                double cnpd = 0.0;
                for (int i = 0; i < soil.Properties.Water.Thickness.Length; i++)
                {
                    double DULFraction = MathUtilities.Divide((SW[i] - LL15[i]), (DUL[i] - LL15[i]), 0.0);
                    cnpd = cnpd + DULFraction * runoff_wf[i];
                }
                cnpd = MathUtilities.Bound(cnpd, 0.0, 1.0);

                // curve no. for dry soil (antecedant) moisture
                double cn1 = MathUtilities.Divide(cn2New, (2.334 - 0.01334 * cn2New), 0.0);

                // curve no. for wet soil (antecedant) moisture
                double cn3 = MathUtilities.Divide(cn2New, (0.4036 + 0.005964 * cn2New), 0.0);

                // scs curve number
                double cn = cn1 + (cn3 - cn1) * cnpd;

                // curve number will be decided from scs curve number table ??dms
                // s is potential max retention (surface ponding + infiltration)
                double s = 254.0 * (MathUtilities.Divide(100.0, cn, 1000000.0) - 1.0);
                double xpb = soil.PotentialRunoff - 0.2 * s;
                xpb = Math.Max(xpb, 0.0);

                // assign the output variable
                runoff = MathUtilities.Divide(xpb * xpb, (soil.PotentialRunoff + 0.8 * s), 0.0);

                // bound check the ouput variable
                return MathUtilities.Bound(runoff, 0.0, soil.PotentialRunoff);
            }

            return 0.0;
        }

        // --- Private methods ---------------------------------------------------------------

        /// <summary>
        /// Calculate the weighting factor hydraulic effectiveness used
        /// to weight the effect of soil moisture on runoff.
        /// </summary>
        /// <returns>Weighting factor for runoff</returns>
        private double[] RunoffWeightingFactor()
        {
            double cumRunoffWeightingFactor = 0.0;

            // Get sumulative depth (mm)
            double[] cumThickness = SoilUtilities.ToCumThickness(soil.Properties.Water.Thickness);

            // Ensure hydro effective depth doesn't go below bottom of soil.
            hydrolEffectiveDepth = Math.Min(hydrolEffectiveDepth, MathUtilities.Sum(soil.Properties.Water.Thickness));

            // Scaling factor for wf function to sum to 1
            double scaleFactor = 1.0 / (1.0 - Math.Exp(-4.16));

            // layer number that the effective depth occurs
            int hydrolEffectiveLayer = SoilUtilities.FindLayerIndex(soil.Properties, hydrolEffectiveDepth);

            double[] runoffWeightingFactor = new double[soil.Properties.Water.Thickness.Length];
            for (int i = 0; i <= hydrolEffectiveLayer; i++)
            {
                double cumDepth = cumThickness[i];
                cumDepth = Math.Min(cumDepth, hydrolEffectiveDepth);

                // assume water content to hydrol_effective_depth affects runoff
                // sum of wf should = 1 - may need to be bounded? <dms 7-7-95>
                runoffWeightingFactor[i] = scaleFactor * (1.0 - Math.Exp(-4.16 * MathUtilities.Divide(cumDepth, hydrolEffectiveDepth, 0.0)));
                runoffWeightingFactor[i] = runoffWeightingFactor[i] - cumRunoffWeightingFactor;  
                cumRunoffWeightingFactor += runoffWeightingFactor[i];
            }

            // Ensure total runoff weighting factor equals 1.
            if (!MathUtilities.FloatsAreEqual(MathUtilities.Sum(runoffWeightingFactor), 1.0))
                throw new Exception("Internal error. Total runoff weighting factor must be equal to one.");

            return runoffWeightingFactor;
        }
    }
}
