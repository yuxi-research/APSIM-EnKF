﻿using APSIM.Shared.Utilities;
using Models.Core;
using Models.PMF;
using Models.PMF.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Models.PMF
{
    /// <summary>
    /// Relative allocation class
    /// </summary>
    [Serializable]
    public class RelativeAllocationSinglePass : Model,IArbitrationMethod
    {
        /// <summary>Relatives the allocation.</summary>
        /// <param name="Organs">The organs.</param>
        /// <param name="TotalSupply">The total supply.</param>
        /// <param name="TotalAllocated">The total allocated.</param>
        /// <param name="BAT">The bat.</param>
        public void DoAllocation(IArbitration[] Organs, double TotalSupply, ref double TotalAllocated, BiomassArbitrationType BAT)
        {
            double NotAllocated = TotalSupply;
            ////allocate to all pools based on their relative demands
            for (int i = 0; i < Organs.Length; i++)
            {
                double StructuralRequirement = Math.Max(0, BAT.StructuralDemand[i] - BAT.StructuralAllocation[i]); //N needed to get to Minimum N conc and satisfy structural and metabolic N demands
                double MetabolicRequirement = Math.Max(0, BAT.MetabolicDemand[i] - BAT.MetabolicAllocation[i]);
                double NonStructuralRequirement = Math.Max(0, BAT.NonStructuralDemand[i] - BAT.NonStructuralAllocation[i]);
                if ((StructuralRequirement + MetabolicRequirement + NonStructuralRequirement) > 0.0)
                {
                    double StructuralFraction = BAT.TotalStructuralDemand / (BAT.TotalStructuralDemand + BAT.TotalMetabolicDemand + BAT.TotalNonStructuralDemand);
                    double MetabolicFraction = BAT.TotalMetabolicDemand / (BAT.TotalStructuralDemand + BAT.TotalMetabolicDemand + BAT.TotalNonStructuralDemand);
                    double NonStructuralFraction = BAT.TotalNonStructuralDemand / (BAT.TotalStructuralDemand + BAT.TotalMetabolicDemand + BAT.TotalNonStructuralDemand);

                    double StructuralAllocation = Math.Min(StructuralRequirement, TotalSupply * StructuralFraction * BAT.StructuralDemand[i] / BAT.TotalStructuralDemand);
                    double MetabolicAllocation = Math.Min(MetabolicRequirement, TotalSupply * MetabolicFraction * MathUtilities.Divide(BAT.MetabolicDemand[i], BAT.TotalMetabolicDemand, 0));
                    double NonStructuralAllocation = Math.Min(NonStructuralRequirement, TotalSupply * NonStructuralFraction * MathUtilities.Divide(BAT.NonStructuralDemand[i], BAT.TotalNonStructuralDemand, 0));

                    BAT.StructuralAllocation[i] += StructuralAllocation;
                    BAT.MetabolicAllocation[i] += MetabolicAllocation;
                    BAT.NonStructuralAllocation[i] += Math.Max(0, NonStructuralAllocation);
                    NotAllocated -= (StructuralAllocation + MetabolicAllocation + NonStructuralAllocation);
                    TotalAllocated += (StructuralAllocation + MetabolicAllocation + NonStructuralAllocation);
                }
            }
        }

        /// <summary>Writes documentation for this function by adding to the list of documentation tags.</summary>
        /// <param name="tags">The list of tags to add to.</param>
        /// <param name="headingLevel">The level (e.g. H2) of the headings.</param>
        /// <param name="indent">The level of indentation 1, 2, 3 etc.</param>
        public override void Document(List<AutoDocumentation.ITag> tags, int headingLevel, int indent)
        {
            // add a heading.
            tags.Add(new AutoDocumentation.Heading(Name, headingLevel));

            // write memos.
            foreach (IModel memo in Apsim.Children(this, typeof(Memo)))
                memo.Document(tags, -1, indent);

            // write description of this class.
            AutoDocumentation.GetClassDescription(this, tags, indent);

            string RelativeDocString = "Arbitration is performed in two passes for each of the biomass supply sources.  On the first pass, structural and metabolic biomass is allocated to each organ based on their demand relative to the demand from all organs.  On the second pass any remaining biomass is allocated to non-structural demands based on the organ's relative demand.";
            //string RelativeThenPriorityDocStirng = "Arbitration is performed in two passes for each of the biomass supply sources.  On the first pass, structural and metabolic biomass is allocated to each organ based on their order of priority with higher priority organs recieving their full demand first. On the second pass any remaining biomass is allocated to non-structural demands based on the relative demand from all organs.";
            //string PriorityDocString = "Arbitration is performed in two passes for each of the biomass supply sources.  On the first pass, structural and metabolic biomass is allocated to each organ based on their order of priority with higher priority organs recieving their full demand first.  On the second pass any remaining biomass is allocated to non-structural demands based on the same order of priority.";
            //string SinglePassDocString = "Arbitration is performed in a single pass for each of the biomass supply sources.  Biomass is partitioned between organs based on their relative demand in a single pass so non-structural demands compete dirrectly with structural demands.";

            tags.Add(new AutoDocumentation.Paragraph(RelativeDocString, indent));
        }
    }
}
