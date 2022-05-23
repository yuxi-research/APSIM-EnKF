﻿using System;
using System.Reflection;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using Models.Core;
using Models.SurfaceOM;
using APSIM.Shared.Utilities;
using Models.Interfaces;

namespace Models.Soils
{

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class FOMType
    {
        /// <summary>The amount</summary>
        public double amount;
        /// <summary>The c</summary>
        public double C;
        /// <summary>The n</summary>
        public double N;
        /// <summary>The p</summary>
        public double P;
        /// <summary>The ash alk</summary>
        public double AshAlk;
    }
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class FOMPoolType
    {
        /// <summary>The layer</summary>
        public FOMPoolLayerType[] Layer;
    }
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class FOMPoolLayerType
    {
        /// <summary>The thickness</summary>
        public double thickness;
        /// <summary>The no3</summary>
        public double no3;
        /// <summary>The NH4</summary>
        public double nh4;
        /// <summary>The po4</summary>
        public double po4;
        /// <summary>The pool</summary>
        public FOMType[] Pool;
    }
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class FOMLayerType
    {
        /// <summary>The type</summary>
        public string Type = "";
        /// <summary>The layer</summary>
        public FOMLayerLayerType[] Layer;
    }
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class SurfaceOrganicMatterDecompPoolType
    {
        /// <summary>The name</summary>
        public string Name = "";
        /// <summary>The organic matter type</summary>
        public string OrganicMatterType = "";
        /// <summary>The fom</summary>
        public FOMType FOM;
    }
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class SurfaceOrganicMatterDecompType
    {

        /// <summary>The pool</summary>
        public SurfaceOrganicMatterDecompPoolType[] Pool;
    }


    /// <summary>
    /// 
    /// </summary>
    public struct FOMdecompData
    {
        // lists with values from FOM decompostion
        /// <summary>The dlt_c_hum</summary>
        public double[] dlt_c_hum;
        /// <summary>The dlt_c_biom</summary>
        public double[] dlt_c_biom;
        /// <summary>The dlt_c_atm</summary>
        public double[] dlt_c_atm;
        /// <summary>The dlt_fom_n</summary>
        public double[] dlt_fom_n;
        /// <summary>The dlt_n_min</summary>
        public double dlt_n_min;
    }
    /// <summary>
    /// 
    /// </summary>
    public class ExternalMassFlowType
    {
        /// <summary>The pool class</summary>
        public string PoolClass = "";
        /// <summary>The flow type</summary>
        public string FlowType = "";
        /// <summary>The c</summary>
        public double C;
        /// <summary>The n</summary>
        public double N;
        /// <summary>The p</summary>
        public double P;
        /// <summary>The dm</summary>
        public double DM;
        /// <summary>The sw</summary>
        public double SW;
    }
    /// <summary>
    /// 
    /// </summary>
    public class MergeSoilCNPatchType
    {
        /// <summary>The sender</summary>
        public string Sender = "";
        /// <summary>The affected patches_nm</summary>
        public string[] AffectedPatches_nm;
        /// <summary>The affected patches_id</summary>
        public int[] AffectedPatches_id;
        /// <summary>The merge all</summary>
        public bool MergeAll;
    }
    /// <summary>
    /// 
    /// </summary>
    public class FOMLayerLayerType
    {
        /// <summary>The fom</summary>
        public FOMType FOM;
        /// <summary>The CNR</summary>
        public double CNR;
        /// <summary>The labile p</summary>
        public double LabileP;
    }
    /// <summary>
    /// 
    /// </summary>
    public class AddUrineType
    {
        /// <summary>The urinations</summary>
        public double Urinations;
        /// <summary>The volume per urination</summary>
        public double VolumePerUrination;
        /// <summary>The area per urination</summary>
        public double AreaPerUrination;
        /// <summary>The eccentricity</summary>
        public double Eccentricity;
        /// <summary>The urea</summary>
        public double Urea;
        /// <summary>The pox</summary>
        public double POX;
        /// <summary>The s o4</summary>
        public double SO4;
        /// <summary>The ash alk</summary>
        public double AshAlk;
    }


    /// <summary>
    /// Initially ported from Fortran SoilN model by Eric Zurcher Sept/Oct-2010.
    /// Code tidied up by RCichota on Aug/Sep-2012: mostly modifying how some variables are handled (substitute 'get's by [input]), added regions
    /// to ease access, updated error messages, moved all soilTemp code to a separate class (the idea is to eliminate it in the future), also added
    /// some of the constants to xml.
    /// Changes on Sep/Oct-2012 by RCichota, add patch capability: move all code for soil C and N to a separate class (SoilCNPatch), allow several
    /// instances to be initialised, modified inputs to handle the partitioning of incoming N, also modified outputs to sum up the pools from the
    /// several instances (patches)
    /// </summary>
    [Serializable]
    [ValidParent(ParentType=typeof(Soil))]
    public partial class SoilNitrogen : Model, ISolute
    {
        /// <summary>The surface organic matter</summary>
        [Link]
        private SurfaceOrganicMatter SurfaceOrganicMatter = null;

        /// <summary>Initializes a new instance of the <see cref="SoilNitrogen"/> class.</summary>
        public SoilNitrogen()
        {
            Patch = new List<soilCNPatch>();
            soilCNPatch newPatch = new soilCNPatch(this);
            Patch.Add(newPatch);
            Patch[0].RelativeArea = 1.0;
            Patch[0].PatchName = "base";
            wfpsN2N2O_x = new double[] { 22, 88 };
            wfpsN2N2O_y = new double[] { 0.1, 1 };
        }

        #region Events which we publish

        /// <summary>
        /// Event to communicate other modules of C and/or N changes to/from outside the simulation
        /// </summary>
        /// <param name="Data">The data.</param>
        public delegate void ExternalMassFlowDelegate(ExternalMassFlowType Data);
        /// <summary>Occurs when [external mass flow].</summary>
        public event ExternalMassFlowDelegate ExternalMassFlow;

        /// <summary>Event to comunicate other modules (SurfaceOM) that residues have been decomposed</summary>
        /// <param name="Data">The data.</param>
        public delegate void SurfaceOrganicMatterDecompDelegate(SurfaceOrganicMatterDecompType Data);

        #endregion

        #region Setup events handlers and methods


        /// <summary>Performs the initial checks and setup</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("Commencing")]
        private void OnSimulationCommencing(object sender, EventArgs e)
        {
            Reset();
        }

        /// <summary>Reset the state values to those set during the initialisation</summary>
        public void Reset()
        {

            Patch = new List<soilCNPatch>();
            soilCNPatch newPatch = new soilCNPatch(this);
            Patch.Add(newPatch);
            Patch[0].RelativeArea = 1.0;
            Patch[0].PatchName = "base";

            // Variable handling when using APSIMX
            initDone = false;
            dlayer = Soil.Thickness;
            bd = Soil.BD;
            sat_dep = MathUtilities.Multiply(Soil.SAT, Soil.Thickness);
            dul_dep = MathUtilities.Multiply(Soil.DUL, Soil.Thickness);
            ll15_dep = MathUtilities.Multiply(Soil.LL15, Soil.Thickness);
            sw_dep = MathUtilities.Multiply(Soil.InitialWaterVolumetric, Soil.Thickness);
            oc = Soil.OC;
            ph = Soil.PH;
            salb = Soil.SoilWater.Salb;
            NO3ppm = Soil.InitialNO3N;
            NH4ppm = Soil.InitialNH4N;
            ureappm = new double[Soil.Thickness.Length];
            num_residues = 0;

            fbiom = Soil.FBiom;
            finert = Soil.FInert;
            soil_cn = SoilOrganicMatter.SoilCN;
            root_wt = SoilOrganicMatter.RootWt;
            root_cn = SoilOrganicMatter.RootCN;
            enr_a_coeff = SoilOrganicMatter.EnrACoeff;
            enr_b_coeff = SoilOrganicMatter.EnrBCoeff;

            if (Soil.SoilType != null && Soil.SoilType.Equals("Sand", StringComparison.CurrentCultureIgnoreCase))
            {
                rd_biom = new double[] { 0.0324, 0.015 };
                wfmin_values = new double[] { 0.05, 1.0, 1.0, 0.5 };
            }
            else
            {
                rd_biom = new double[] { 0.0081, 0.004 };
                wfmin_values = new double[] { 0.0, 1.0, 1.0, 0.5 };
            }

            initDone = true;

            // set the size of arrays
            ResizeLayerArrays(dlayer.Length);
            foreach (soilCNPatch aPatch in Patch)
                aPatch.ResizeLayerArrays(dlayer.Length);

            // check few initialisation parameters
            CheckParams();

            // perform initial calculations and setup
            InitCalc();
        }

        /// <summary>Check general initialisation parameters, and let user know of some settings</summary>
        /// <exception cref="System.Exception">
        /// Number of \fract_carb\ different to \fom_type\
        /// or
        /// Number of \fract_cell\ different to \fom_type\
        /// or
        /// Number of \fract_lign\ different to \fom_type\
        /// </exception>
        private void CheckParams()
        {

            SoilCNParameterSet = SoilCNParameterSet.Trim();
            NPartitionApproach = NPartitionApproach.Trim();

            // check whether ph is supplied, use a default if not - might be better to throw an exception?
            use_external_ph = (ph != null);
            if (!use_external_ph)
            {
                for (int layer = 0; layer < dlayer.Length; ++layer)
                    ph[layer] = 6.0; // ph_ini
            }

            // convert minimum values for nh4 and no3 from ppm to kg/ha
            double convFact = 0;
            for (int layer = 0; layer < dlayer.Length; ++layer)
            {
                convFact = convFactor_kgha2ppm(layer);
                urea_min[layer] = MathUtilities.Divide(ureappm_min, convFact, 0.0);
                nh4_min[layer] = MathUtilities.Divide(nh4ppm_min, convFact, 0.0);
                no3_min[layer] = MathUtilities.Divide(no3ppm_min, convFact, 0.0);
            }

            // Check if all fom values have been supplied
            if (num_fom_types != fract_carb.Length)
                throw new Exception("Number of \"fract_carb\" different to \"fom_type\"");
            if (num_fom_types != fract_cell.Length)
                throw new Exception("Number of \"fract_cell\" different to \"fom_type\"");
            if (num_fom_types != fract_lign.Length)
                throw new Exception("Number of \"fract_lign\" different to \"fom_type\"");

            // Check if all C:N values have been supplied. If not use average C:N ratio in all pools
            if (fomPoolsCNratio == null || fomPoolsCNratio.Length < 3)
            {
                fomPoolsCNratio = new double[3];
                for (int i = 0; i < 3; i++)
                    fomPoolsCNratio[i] = iniFomCNratio;
            }

            // Check if initial fom depth has been supplied, if not assume that initial fom is distributed over the whole profile
            if (iniFomDepth == 0.0)
            {
                for (int i = 0; i < dlayer.Length; ++i)
                    iniFomDepth += dlayer[i];
            }
        }

        /// <summary>Do the initial setup and calculations - this is also used onReset</summary>
        private void InitCalc()
        {

            int nLayers = dlayer.Length;

            // Factor to distribute fom over the soil profile. Uses a exponential function and goes till the especified depth
            double[] fom_FracLayer = new double[nLayers];
            double cum_depth = 0.0;
            int deepest_layer = getCumulativeIndex(iniFomDepth, dlayer);
            for (int layer = 0; layer <= deepest_layer; layer++)
            {
                fom_FracLayer[layer] = Math.Exp(-3.0 * Math.Min(1.0, MathUtilities.Divide(cum_depth + dlayer[layer], iniFomDepth, 0.0))) *
                    Math.Min(1.0, MathUtilities.Divide(iniFomDepth - cum_depth, dlayer[layer], 0.0));
                cum_depth += dlayer[layer];
            }
            double fom_FracLayer_tot = SumDoubleArray(fom_FracLayer);

            // ensure initial OC has a value for each layer
            Array.Resize(ref OC_reset, nLayers);

            // Distribute an convert C an N values over the profile
            double convFact = 0.0;
            double newValue = 0.0;
            for (int layer = 0; layer < nLayers; layer++)
            {
                convFact = convFactor_kgha2ppm(layer);
                // check and distribute the mineral nitrogen
                if (ureappm_reset != null)
                {
                    newValue = MathUtilities.Divide(ureappm_reset[layer], convFact, 0.0);       //Convert from ppm to kg/ha
                    for (int k = 0; k < Patch.Count; k++)
                        Patch[k].urea[layer] = newValue;
                }
                newValue = MathUtilities.Divide(nh4ppm_reset[layer], convFact, 0.0);       //Convert from ppm to kg/ha
                for (int k = 0; k < Patch.Count; k++)
                    Patch[k].nh4[layer] = newValue;
                newValue = MathUtilities.Divide(no3ppm_reset[layer], convFact, 0.0);       //Convert from ppm to kg/ha
                for (int k = 0; k < Patch.Count; k++)
                    Patch[k].no3[layer] = newValue;

                // calculate total soil C
                double Soil_OC = OC_reset[layer] * 10000;					// = (oc/100)*1000000 - convert from % to ppm
                Soil_OC = MathUtilities.Divide(Soil_OC, convFact, 0.0);		//Convert from ppm to kg/ha

                // calculate inert soil C
                double InertC = finert[layer] * Soil_OC;

                // calculate microbial biomass C and N
                double BiomassC = MathUtilities.Divide((Soil_OC - InertC) * fbiom[layer], 1.0 + fbiom[layer], 0.0);
                double BiomassN = MathUtilities.Divide(BiomassC, biom_cn, 0.0);

                // calculate C and N values for active humus
                double HumusC = Soil_OC - BiomassC;
                double HumusN = MathUtilities.Divide(HumusC, hum_cn, 0.0);

                // distribute and calculate the fom N and C
                double fom = MathUtilities.Divide(iniFomWt * fom_FracLayer[layer], fom_FracLayer_tot, 0.0);

                for (int k = 0; k < Patch.Count; k++)
                {
                    Patch[k].inert_c[layer] = InertC;
                    Patch[k].biom_c[layer] = BiomassC;
                    Patch[k].biom_n[layer] = BiomassN;
                    Patch[k].hum_c[layer] = HumusC;
                    Patch[k].hum_n[layer] = HumusN;
                    Patch[k].fom_c_pool1[layer] = fom * fract_carb[0] * c_in_fom;
                    Patch[k].fom_c_pool2[layer] = fom * fract_cell[0] * c_in_fom;
                    Patch[k].fom_c_pool3[layer] = fom * fract_lign[0] * c_in_fom;
                    Patch[k].fom_n_pool1[layer] = MathUtilities.Divide(Patch[k].fom_c_pool1[layer], fomPoolsCNratio[0], 0.0);
                    Patch[k].fom_n_pool2[layer] = MathUtilities.Divide(Patch[k].fom_c_pool2[layer], fomPoolsCNratio[1], 0.0);
                    Patch[k].fom_n_pool3[layer] = MathUtilities.Divide(Patch[k].fom_c_pool3[layer], fomPoolsCNratio[2], 0.0);
                }

                // store today's values
                for (int k = 0; k < Patch.Count; k++)
                    Patch[k].InitCalc();
            }

            // Calculations for NEW sysbal component
            dailyInitialC = SumDoubleArray(TotalC);
            dailyInitialN = SumDoubleArray(TotalN);

            // Initialise the inhibitor factors
            if (InhibitionFactor_Nitrification == null)
                InhibitionFactor_Nitrification = new double[dlayer.Length];

            initDone = true;
        }

        /// <summary>Set the size of all public arrays (with nLayers)</summary>
        /// <param name="nLayers">The n layers.</param>
        private void ResizeLayerArrays(int nLayers)
        {
            // Note: this doesn't clear the existing values

            Array.Resize(ref urea_min, nLayers);
            Array.Resize(ref nh4_min, nLayers);
            Array.Resize(ref no3_min, nLayers);
        }

        /// <summary>Stores the total amounts of C an N</summary>
        private void SaveState()
        {
            // +  Note: needed for both NEW and OLD sysbal component

            dailyInitialN = SumDoubleArray(TotalN);
            dailyInitialC = SumDoubleArray(TotalC);
        }

        /// <summary>Calculates variations in C an N, and publishes MassFlows to APSIM</summary>
        private void DeltaState()
        {

            double dltN = SumDoubleArray(TotalN) - dailyInitialN;
            double dltC = SumDoubleArray(TotalC) - dailyInitialC;

            SendExternalMassFlowN(dltN);
            SendExternalMassFlowC(dltC);
        }

        #endregion

        #region Process events handlers and methods

        #region Daily processes

        /// <summary>
        /// Get the information on potential residue decomposition - perform daily calculations as part of this.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("DoSoilOrganicMatter")]
        private void OnDoSoilOrganicMatter(object sender, EventArgs e)
        {
            // Get potential residue decomposition from surfaceom.
            SurfaceOrganicMatterDecompType SurfaceOrganicMatterDecomp = SurfaceOrganicMatter.PotentialDecomposition();

            foreach (soilCNPatch aPatch in Patch)
                aPatch.OnPotentialResidueDecompositionCalculated(SurfaceOrganicMatterDecomp);

            num_residues = SurfaceOrganicMatterDecomp.Pool.Length;

            sw_dep = Soil.Water;

            // calculate C and N processes
            //    - Assesses potential decomposition of surface residues;
            //		. adjust decomposition if needed;
            //		. accounts for mineralisation/immobilisation of N;
            //	  - Compute the transformations on soil organic matter (including N mineralisation/immobilition);
            //    - Calculates hydrolysis of urea, nitrification, and denitrification;
            for (int k = 0; k < Patch.Count; k++)
                Patch[k].Process();

            // send actual decomposition back to surface OM
            if (!is_pond_active)
                SendActualResidueDecompositionCalculated();
        }

        /// <summary>Performs every-day calculations - before begining of day tasks</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("DoDailyInitialisation")]
        private void OnDoDailyInitialisation(object sender, EventArgs e)
        {
            // + Purpose: reset potential decomposition variables in each patch and get C and N status

            foreach (soilCNPatch aPatch in Patch)
                aPatch.OnTick();

            // Calculations for NEW sysbal component
            SaveState();
        }

        /// <summary>Performs every-day calculations - end of day processes</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("DoUpdate")]
        private void OnDoUpdate(object sender, EventArgs e)
        {
            // + Purpose: Check patch status and clean up, if possible

            if (Patch.Count > 1000) // must set this to one later
            {
                // we have more than one patch, check whether they are similar enough to be merged
                PatchIDs Patches = new PatchIDs();
                Patches = ComparePatches();

                if (Patches.disappearing.Count > 0)
                {  // there are patches that will be merged
                    for (int k = 0; k < Patches.disappearing.Count; k++)
                    {
                        MergePatches(Patches.recipient[k], Patches.disappearing[k]);
                        Summary.WriteMessage(this, "   merging Patch(" + Patches.disappearing[k].ToString() + ") into Patch(" +
                            Patches.recipient[k].ToString() + "). New patch area = " + Patch[Patches.recipient[k]].RelativeArea.ToString("#0.00#"));
                    }
                }
            }
        }

        /// <summary>Send back to SurfaceOM the information about residue decomposition</summary>
        private void SendActualResidueDecompositionCalculated()
        {
            // Note:
            //      Potential decomposition was given to this module by a residue/surfaceOM module. 
            //		Now we explicitly tell the module the actual decomposition
            //      rate for each of its residues.  If there wasn't enough mineral N to decompose, the rate will be reduced from the potential value.

            // will have to pack the SOMdecomp data from each patch and then invoke the event
            //int num_residues = Patch[0].SOMDecomp.Pool.Length;
            int nLayers = dlayer.Length;

            SurfaceOrganicMatterDecompType ActualSOMDecomp = new SurfaceOrganicMatterDecompType();
            Array.Resize(ref ActualSOMDecomp.Pool, num_residues);

            for (int residue = 0; residue < num_residues; residue++)
            {
                double c_summed = 0.0F;
                double n_summed = 0.0F;
                for (int k = 0; k < Patch.Count; k++)
                {
                    c_summed += Patch[k].SOMDecomp.Pool[residue].FOM.C * Patch[k].RelativeArea;
                    n_summed += Patch[k].SOMDecomp.Pool[residue].FOM.N * Patch[k].RelativeArea;
                }

                ActualSOMDecomp.Pool[residue] = new SurfaceOrganicMatterDecompPoolType();
                ActualSOMDecomp.Pool[residue].FOM = new FOMType();
                ActualSOMDecomp.Pool[residue].Name = Patch[0].SOMDecomp.Pool[residue].Name;
                ActualSOMDecomp.Pool[residue].OrganicMatterType = Patch[0].SOMDecomp.Pool[residue].OrganicMatterType;
                ActualSOMDecomp.Pool[residue].FOM.amount = 0.0F;
                ActualSOMDecomp.Pool[residue].FOM.C = c_summed;
                ActualSOMDecomp.Pool[residue].FOM.N = n_summed;
                ActualSOMDecomp.Pool[residue].FOM.P = 0.0F;
                ActualSOMDecomp.Pool[residue].FOM.AshAlk = 0.0F;
            }

            SurfaceOrganicMatter.ActualSOMDecomp = ActualSOMDecomp;
        }

        #endregion

        #region Frequent and sporadic processes

        /// <summary>Partition the given FOM C and N into fractions in each layer (one FOM)</summary>
        /// <param name="inFOMdata">The in fo mdata.</param>
        [EventSubscribe("IncorpFOM")]
        private void OnIncorpFOM(FOMLayerType inFOMdata)
        {
            // Note: In this event all FOM is given as one, so it will be assumed that the CN ratios of all fractions are equal

            foreach (soilCNPatch aPatch in Patch)
                aPatch.OnIncorpFOM(inFOMdata);

            fom_type = Patch[0].fom_type;
        }

        /// <summary>Partition the given FOM C and N into fractions in each layer (FOM pools)</summary>
        /// <param name="inFOMPoolData">The in fom pool data.</param>
        [EventSubscribe("IncorpFOMPool")]
        private void OnIncorpFOMPool(FOMPoolType inFOMPoolData)
        {
            // Note: In this event each of the three pools is given

            foreach (soilCNPatch aPatch in Patch)
                aPatch.OnIncorpFOMPool(inFOMPoolData);
        }

        /// <summary>Get the information about urine being added</summary>
        /// <param name="UrineAdded">Urine deposition data (includes urea N amount, volume, area affected, etc)</param>
        public void AddUrine(AddUrineType UrineAdded)
        {

            // Starting with the minimalist version. To be updated by Val's group to include a urine patch algorithm

            // test for adding urine patches  -RCichota
            // if VolumePerUrination = 0.0 then no patch will be added, otherwise a patch will be added (based on 'base' patch)
            // assuming new PatchArea is passed as a fraction and this will be subtracted from original
            // urea will be added to the top layer for now

            double[] newUrea = new double[dlayer.Length];
            newUrea[0] = UrineAdded.Urea;

            if (UrineAdded.VolumePerUrination > 0.0)
            {
                SplitPatch(0);
                double oldArea = Patch[0].RelativeArea;
                double newArea = oldArea * (1 - UrineAdded.AreaPerUrination);
                Patch[0].RelativeArea = newArea;
                int k = Patch.Count - 1;
                Patch[k].RelativeArea = oldArea * UrineAdded.AreaPerUrination;
                Patch[k].PatchName = "Patch" + k.ToString();
                if (UrineAdded.Urea > EPSILON)
                    Patch[k].dlt_urea = newUrea;
            }
            else
                for (int k = 0; k < Patch.Count; k++)
                    Patch[k].dlt_urea = newUrea;
        }

        /// <summary>Gets and handles the information about new patch and add it to patch list</summary>
        /// <param name="PatchtoAdd">Patch data</param>
        [EventSubscribe("AddSoilCNPatc")]
        private void OnAddSoilCNPatch(AddSoilCNPatchType PatchtoAdd)
        {
            // data passed with this event:
            //.Sender: the name of the module that raised this event
            //.DepositionType: the type of deposition:
            //  - ToAllPaddock: No patch is created, add stuff as given to all patches. It is the default;
            //  - ToSpecificPatch: No patch is created, add stuff to given patches;
            //		(recipient patch is given using its index or name; if not supplied, defaults to homogeneous)
            //  - ToNewPatch: create new patch based on an existing patch, add stuff to created patch;
            //		- recipient or base patch is given using index or name; if not supplied, new patch will be based on the base/Patch[0];
            //      - patches are only created is area is larger than a minimum (minPatchArea);
            //      - new areas are proportional to existing patches;
            //  - NewOverlappingPatches: create new patch(es), these overlap with all existing patches, add stuff to created patches;
            //		(new patches are created only if their area is larger than a minimum (minPatchArea))
            //.AffectedPatches_id (AffectedPatchesByIndex): the index of the existing patches to which urine will be added
            //.AffectedPatches_nm (AffectedPatchesByName): the name of the existing patches to which urine will be added
            //.AreaFraction: the relative area of the patch (0-1)
            //.PatchName: the name(s) of the patch)es) being created
            //.Water: amount of water to add per layer (mm), not handled here
            //.Urea: amount of urea to add per layer (kgN/ha)
            //.Urea: amount of urea to add (per layer) - Do we need other N forms?
            //.NH4: amount of ammonium to add per layer (kgN/ha)
            //.NO3: amount of nitrate to add per layer (kgN/ha)
            //.POX: amount of POx to add per layer (kgP/ha)
            //.SO4: amount of SO4 to add per layer (kgS/ha)
            //.Ashalk: ash amount to add per layer (mol/ha)
            //.FOM_C: amount of carbon in fom (all pools) to add per layer (kgC/ha)  - if present, the entry for pools will be ignored
            //.FOM_C_pool1: amount of carbon in fom_pool1 to add per layer (kgC/ha)
            //.FOM_C_pool2: amount of carbon in fom_pool2 to add per layer (kgC/ha)
            //.FOM_C_pool3: amount of carbon in fom_pool3 to add per layer (kgC/ha)
            //.FOM_N.: amount of nitrogen in fom to add per layer (kgN/ha)

            List<int> PatchesToAddStuff = new List<int>();

            if ((PatchtoAdd.DepositionType.ToLower() == "ToNewPatch".ToLower()) ||
                (PatchtoAdd.DepositionType.ToLower() == "NewOverlappingPatches".ToLower()))
            { // New patch(es) will be added
                AddNewCNPatch(PatchtoAdd);
            }
            else if (PatchtoAdd.DepositionType.ToLower() == "ToSpecificPatch".ToLower())
            {  // add stuff to selected patches, no new patch will be created

                // 1. get the list of patch id's to which stuff will be added
                int[] PatchIDs = CheckPatchIDs(PatchtoAdd.AffectedPatches_id, PatchtoAdd.AffectedPatches_nm);
                // 2. create the list of patches receiving stuff
                for (int i = 0; i < PatchIDs.Length; i++)
                    PatchesToAddStuff.Add(PatchIDs[i]);
                // 3. add the stuff to patches listed
                AddStuffToPatches(PatchesToAddStuff, PatchtoAdd);
            }
            else
            {  // add urine to all existing patches, no new patch will be created

                // 1. create the list of patches receiving stuff (all)
                for (int k = 0; k < Patch.Count; k++)
                    PatchesToAddStuff.Add(k);
                // 2. add the stuff to patches listed
                AddStuffToPatches(PatchesToAddStuff, PatchtoAdd);
            }
        }


        /// <summary>Gets the list of patches that will be merge into one, as defined by user</summary>
        /// <param name="MergeCNPatch">The list of CNPatches to merge</param>
        [EventSubscribe("MergeSoilCNPatc")]
        private void OnMergeSoilCNPatch(MergeSoilCNPatchType MergeCNPatch)
        {
            if ((MergeCNPatch.AffectedPatches_id.Length > 1) | (MergeCNPatch.AffectedPatches_nm.Length > 1))
            {
                // get the list of patch id's to which stuff will be added
                List<int> PatchesToMerge = new List<int>();
                int[] PatchIDs = CheckPatchIDs(MergeCNPatch.AffectedPatches_id, MergeCNPatch.AffectedPatches_nm);
                for (int i = 0; i < PatchIDs.Length; i++)
                    PatchesToMerge.Add(PatchIDs[i]);

                // send the list to merger
                AmalgamatePatches(PatchesToMerge);
            }
        }

        /// <summary>Comunicate other components that N amount in the soil has changed</summary>
        /// <param name="dltN">N changes</param>
        private void SendExternalMassFlowN(double dltN)
        {

            ExternalMassFlowType massBalanceChange = new ExternalMassFlowType();
            if (Math.Abs(dltN) <= EPSILON)
                dltN = 0.0;
            massBalanceChange.FlowType = dltN >= 0 ? "gain" : "loss";
            massBalanceChange.PoolClass = "soil";
            massBalanceChange.N = (float)Math.Abs(dltN);
            if (ExternalMassFlow != null)
                ExternalMassFlow.Invoke(massBalanceChange);
        }

        /// <summary>Comunicate other components that C amount in the soil has changed</summary>
        /// <param name="dltC">C changes</param>
        private void SendExternalMassFlowC(double dltC)
        {
            if (ExternalMassFlow != null)
            {
                ExternalMassFlowType massBalanceChange = new ExternalMassFlowType();
                if (Math.Abs(dltC) <= EPSILON)
                    dltC = 0.0;
                massBalanceChange.FlowType = dltC >= 0 ? "gain" : "loss";
                massBalanceChange.PoolClass = "soil";
                massBalanceChange.N = (float)Math.Abs(dltC);
                ExternalMassFlow.Invoke(massBalanceChange);
            }
        }

        #endregion

        #endregion

        #region Auxiliary functions

        /// <summary>Conversion factor: kg/ha to ppm (mg/kg)</summary>
        /// <param name="Layer">layer to calculate</param>
        /// <returns>conversion factor</returns>
        /// <exception cref="System.Exception"> Error on computing convertion factor, kg/ha to ppm. Value for dlayer or bulk density not valid</exception>
        private double convFactor_kgha2ppm(int Layer)
        {
            if (bd == null || dlayer == null || bd.Length == 0 || dlayer.Length == 0)
            {
                return 0.0;
                throw new Exception(" Error on computing convertion factor, kg/ha to ppm. Value for dlayer or bulk density not valid");
            }
            return MathUtilities.Divide(100.0, bd[Layer] * dlayer[Layer], 0.0);
        }

        /// <summary>Check whether there is any considerable values in the array</summary>
        /// <param name="anArray">The array to analyse</param>
        /// <param name="Lowerue">The minimum considerable value</param>
        /// <returns>True if there is any value greater than the minimum, false otherwise</returns>
        private bool hasValues(double[] anArray, double Lowerue)
        {
            bool result = false;
            if (anArray != null)
            {
                foreach (double Value in anArray)
                {
                    if (Math.Abs(Value) > Lowerue)
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>Calculate the sum of all values of an array of doubles</summary>
        /// <param name="anArray">The array of values</param>
        /// <returns>The sum</returns>
        private double SumDoubleArray(double[] anArray)
        {
            double result = 0.0;
            if (anArray != null)
            {
                foreach (double Value in anArray)
                    result += Value;
            }
            return result;
        }

        /// <summary>Find the index at which the cumulative amount is equal or greater than 'sum'</summary>
        /// <param name="sumTarget">The target value</param>
        /// <param name="anArray">The array to analyse</param>
        /// <returns>The index of the array item at which the sum is equal or greater than the target</returns>
        private int getCumulativeIndex(double sumTarget, double[] anArray)
        {
            double cum = 0.0f;
            for (int i = 0; i < anArray.Length; i++)
            {
                cum += anArray[i];
                if (cum >= sumTarget)
                    return i;
            }
            return anArray.Length - 1;
        }

        #endregion
    }

}