using System;
using System.Collections.Generic;
using System.Linq;
using Models.Core;
using Models.PMF.Functions;
using System.Xml.Serialization;
using Models.PMF.Interfaces;
using Models.Interfaces;
using APSIM.Shared.Utilities;
using Models.PMF.Library;

namespace Models.PMF.Organs
{
    /// <summary>
    ///     This provides a phytomer type which predicts the area, biomass and nutrient as the tally of separate cohorts of leaves.  
    ///     A cohort of leaves is represented by a main-stem node position and branch leaves are kept in the same cohort as the mainstem leaf appearing at the same time ([lawless2005wheat]). 
    ///     The Leaf class delegates the status and function of individual cohorts into LeafCohort sub-classes.
    /// </summary>
    [Serializable]
    [Description("Leaf Class")]
    [ViewName("UserInterface.Views.GridView")]
    [PresenterName("UserInterface.Presenters.PropertyPresenter")]
    [ValidParent(ParentType = typeof(Plant))]
    public class Leaf : BaseOrgan, ICanopy, ILeaf, IHasWaterDemand
    {

        /// <summary>The met data</summary>
        [Link]
        public IWeather MetData = null;
        #region Canopy interface

        /// <summary>Gets the canopy. Should return null if no canopy present.</summary>
        public string CanopyType { get { return Plant.CropType; } }

        /// <summary>Albedo.</summary>
        [Description("Canopy Albedo")]
        public double Albedo { get; set; }

        /// <summary>Gets or sets the gsmax.</summary>
        [Description("GSMAX: maximum canopy conductance(m/s)")]
        public double Gsmax { get; set; }

        /// <summary>Gets or sets the R50.</summary>
        [Description("R50: solar radiation at which stomatal conductance decreases to 50% (W/m^2)")]
        public double R50 { get; set; }

        /// <summary>Gets the LAI</summary>
        [Units("m^2/m^2")]
        public double LAI
        {
            get
            {
                int MM2ToM2 = 1000000; // Conversion of mm2 to m2
                foreach (LeafCohort L in Leaves)
                    if (Double.IsNaN(L.LiveArea))
                        throw new Exception("LiveArea of leaf cohort " + L.Name + " is Nan");
                return Leaves.Sum(x => x.LiveArea) / MM2ToM2;
            }
        }

        /// <summary>Gets the LAI live + dead (m^2/m^2)</summary>
        public double LAITotal { get { return LAI + LAIDead; } }

        /// <summary>Gets the cover green.</summary>
        [Units("0-1")]
        public double CoverGreen
        {
            get
            {
                if (Plant != null && Plant.IsAlive)
                    return Math.Min(MaxCover * (1.0 - Math.Exp(-ExtinctionCoeff.Value() * LAI / MaxCover)), 0.999999999);
                return 0;
            }
        }

        /// <summary>Gets the cover total.</summary>
        [Units("0-1")]
        public double CoverTotal 
        { 
            get 
            {
                if (Plant != null && Plant.IsAlive)
                        return 1.0 - (1 - CoverGreen) * (1 - CoverDead);
                return 0;
            } 
        }

        /// <summary>Gets the height.</summary>
        [Units("mm")]
        public double Height { get { return Structure.Height; } }

        /// <summary>Gets the depth.</summary>
        [Units("mm")]
        public double Depth { get { return Structure.Height; } }

        /// <summary>Gets  FRGR.</summary>
        [Units("0-1")]
        public double FRGR { get; set; }
        
        private double _PotentialEP;
        /// <summary>Sets the potential evapotranspiration. Set by MICROCLIMATE.</summary>
        [Units("mm")]
        public double PotentialEP
        {
            get { return _PotentialEP; }
            set
            {
                _PotentialEP = value;
                MicroClimatePresent = true;
            }
        }

        /// <summary>Sets the light profile. Set by MICROCLIMATE.</summary>
        public CanopyEnergyBalanceInterceptionlayerType[] LightProfile { get; set; } 
        #endregion

        #region Has Water Demand Interface
        /// <summary>
        /// Flag to test if Microclimate is present
        /// </summary>
        public bool MicroClimatePresent {get; set;}
        
        /// <summary>Calculates the water demand.</summary>
        public double CalculateWaterDemand()
        {
            return PotentialEP;
        }

        /// <summary>Gets or sets the water allocation.</summary>
        [XmlIgnore]
        public double WaterAllocation { get; set; }

        #endregion

        #region Links
        /// <summary>The structure</summary>
        [Link]
        public Structure Structure = null;
        #endregion

        #region Structures
        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public class LeafCohortParameters : Model
        {
            /// <summary>The maximum area</summary>
            [Link]
            [Units("mm2")]
            public IFunction MaxArea = null;
            /// <summary>The growth duration</summary>
            [Link]
            [Units("deg day")]
            public IFunction GrowthDuration = null;
            /// <summary>The lag duration</summary>
            [Link]
            [Units("deg day")]
            public IFunction LagDuration = null;
            /// <summary>The senescence duration</summary>
            [Link]
            [Units("deg day")]
            public IFunction SenescenceDuration = null;
            /// <summary>The detachment lag duration</summary>
            [Link]
            [Units("deg day")]
            public IFunction DetachmentLagDuration = null;
            /// <summary>The detachment duration</summary>
            [Link]
            [Units("deg day")]
            public IFunction DetachmentDuration = null;
            /// <summary>The specific leaf area maximum</summary>
            [Link]
            public IFunction SpecificLeafAreaMax = null;
            /// <summary>The specific leaf area minimum</summary>
            [Link]
            public IFunction SpecificLeafAreaMin = null;
            /// <summary>The structural fraction</summary>
            [Link]
            public IFunction StructuralFraction = null;
            /// <summary>The maximum n conc</summary>
            [Link]
            public IFunction MaximumNConc = null;
            /// <summary>The minimum n conc</summary>
            [Link]
            public IFunction MinimumNConc = null;
            /// <summary>The initial n conc</summary>
            [Link]
            public IFunction InitialNConc = null;
            /// <summary>The n reallocation factor</summary>
            [Link]
            public IFunction NReallocationFactor = null;
            /// <summary>The dm reallocation factor</summary>
            [Link]
            public IFunction DMReallocationFactor = null;
            /// <summary>The n retranslocation factor</summary>
            [Link]
            public IFunction NRetranslocationFactor = null;
            /// <summary>The expansion stress</summary>
            [Link]
            public IFunction ExpansionStress = null;
            /// <summary>The critical n conc</summary>
            [Link]
            public IFunction CriticalNConc = null;
            /// <summary>The dm retranslocation factor</summary>
            [Link]
            public IFunction DMRetranslocationFactor = null;
            /// <summary>The shade induced senescence rate</summary>
            [Link]
            public IFunction ShadeInducedSenescenceRate = null;
            /// <summary>The drought induced reduction of lag phase through acceleration of tt accumulation by the cohort during this phase</summary>
            [Link]
            public IFunction DroughtInducedLagAcceleration = null;
            /// <summary>The drought induced reduction of senescence phase through acceleration of tt accumulation by the cohort during this phase</summary>
            [Link]
            public IFunction DroughtInducedSenAcceleration = null;
            /// <summary>The non structural fraction</summary>
            [Link]
            public IFunction NonStructuralFraction = null;
            /// <summary>The cell division stress</summary>
            [Link]
            public IFunction CellDivisionStress = null;
            /// <summary>The Shape of the sigmoidal function of leaf area increase</summary>
            [Link]
            public IFunction LeafSizeShapeParameter = null;
            /// <summary>The size of leaves on senessing tillers relative to the dominant tillers in that cohort</summary>
            [Link]
            public IFunction SenessingLeafRelativeSize = null;
            /// <summary>
            /// The proportion of mass that is respired each day
            /// </summary>
            [Link]
            public IFunction MaintenanceRespirationFunction = null;
        }
        #endregion

        #region Parameters

        /// <summary>The initial leaves</summary>
        [DoNotDocument]
        private LeafCohort[] InitialLeaves;
        /// <summary>The leaf cohort parameters</summary>
        [Link] LeafCohortParameters CohortParameters = null;
        /// <summary>The photosynthesis</summary>
        [Link] IFunction Photosynthesis = null;
        /// <summary>The Fractional Growth Rate</summary>
        [Link]
        IFunction FRGRFunction = null;
        /// <summary>The thermal time</summary>
        [Link]
        IFunction ThermalTime = null;
        /// <summary>The extinction coeff</summary>
        [Link]
        IFunction ExtinctionCoeff = null;
        /// <summary>The frost fraction</summary>
        [Link]
        IFunction FrostFraction = null;

        /// <summary>The structural fraction</summary>
        [Link]
        IFunction StructuralFraction = null;
        /// <summary>The dm demand function</summary>
        [Link(IsOptional = true)] //leaving as optional. The code is different for a missing object, not just the value - JF
        IFunction DMDemandFunction = null;

        [Link]
        IFunction DMConversionEfficiencyFunction = null;

        /// <summary>Link to biomass removal model</summary>
        [ChildLink]
        public BiomassRemoval biomassRemovalModel = null;

        /// <summary>Gets or sets the k dead.</summary>
        [Description("Extinction Coefficient (Dead)")]
        public double KDead { get; set; }

        /// <summary>Gets or sets the maximum number of leaves on the main stem</summary>
        [Description("Maximum number of Main-Stem leaves")]
        public int MaximumMainStemLeafNumber { get; set; }


        /// <summary>Total apex number in plant.</summary>
        [Description("Total apex number in plant")]
        public double[] ApexGroupSize { get { return apexGroupSize.ToArray(); } }

        /// <summary>Total apex number in plant.</summary>
        [Description("Total apex number in plant")]
        public double[] ApexGroupAge { get { return apexGroupAge.ToArray(); } }

        /// <summary>The number of apex in each age group.</summary>
        private List<double> apexGroupSize = new List<double>();
        /// <summary>The age of apex in age group.</summary>
        private List<double> apexGroupAge = new List<double>();

        #endregion

        #region States

        /// <summary>The leaves</summary>
        private List<LeafCohort> Leaves = new List<LeafCohort>();

        /// <summary>Initialise all state variables.</summary>
        public double CurrentExpandingLeaf = 0;
        /// <summary>The start fraction expanded</summary>
        public double StartFractionExpanded = 0;
        /// <summary>The fraction nextleaf expanded</summary>
        public double FractionNextleafExpanded = 0;
        /// <summary>The dead nodes yesterday</summary>
        public double DeadNodesYesterday = 0;//Fixme This needs to be set somewhere
        /// <summary>Dry matter conversion efficiency</summary>
        public override double DMConversionEfficiency { get; set; }
        #endregion

        #region Outputs
        //Note on naming convention.  
        //Variables that represent the number of units per meter of area these are called population (Popn suffix) variables 
        //Variables that represent the number of leaf cohorts (integer) in a particular state on an individual main-stem are cohort variables (CohortNo suffix)
        //Variables that represent the number of primordia or nodes (double) in a particular state on an individual mainstem are called number variables (e.g NodeNo or PrimordiaNo suffix)
        //Variables that the number of leaves on a plant or a primary bud have Plant or Primary bud prefixes

        /// <summary>Return the</summary>
        public double CohortCurrentRankCoverAbove
        {
            get
            {
                if (CurrentRank > Leaves.Count)
                    throw new ApsimXException(this, "Curent Rank is greater than the number of leaves appeared when trying to determine CoverAbove this cohort");
                if (CurrentRank <= 0)
                    return 0;
                return Leaves[CurrentRank - 1].CoverAbove;
            }
        }

        /// <summary>
        /// The number of leaves that have visiable tips on the day of emergence
        /// </summary>
        public int TipsAtEmergence {get; set;}
        
        /// <summary>
        /// The number of leaf cohorts to initialised
        /// </summary>
        public int CohortsAtInitialisation { get; set; }
        
        /// <summary>Gets or sets the fraction died.</summary>
        public double FractionDied { get; set; }
        /// <summary>
        /// Gets a value indicating whether [cohorts initialised].
        /// </summary>
        public bool CohortsInitialised { get { return Leaves.Count > 0; } }

        /// <summary>The maximum cover</summary>
        [Description("Max cover")]
        [Units("max units")]
        public double MaxCover;

        /// <summary>Gets the initialised cohort no.</summary>
        [Description("Number of leaf cohort objects that have been initialised")] 
        public double InitialisedCohortNo { get { return CohortCounter("IsInitialised"); } }

        /// <summary>Gets the appeared cohort no.</summary>
        [Description("Number of leaf cohort that have appeared")] 
        public double AppearedCohortNo { get { return CohortCounter("IsAppeared"); } }

        /// <summary>Gets the expanding cohort no.</summary>
        [Description("Number of leaf cohorts that have appeared but not yet fully expanded")]
        public double ExpandingCohortNo { get { return CohortCounter("IsGrowing"); } }

        /// <summary>Gets the expanded cohort no.</summary>
        [Description("Number of leaf cohorts that are fully expanded")]
        public double ExpandedCohortNo { get { return CohortCounter("IsFullyExpanded"); } }

        /// <summary>Gets the green cohort no.</summary>
         [Description("Number of leaf cohorts that are have expanded but not yet fully senesced")]
        public double GreenCohortNo  { get { return CohortCounter("IsGreen"); } }

        /// <summary>Gets the senescing cohort no.</summary>
        [Description("Number of leaf cohorts that are Senescing")]
        public double SenescingCohortNo { get { return CohortCounter("IsSenescing"); } }

        /// <summary>Gets the dead cohort no.</summary>
        [Description("Number of leaf cohorts that have fully Senesced")]
        public double DeadCohortNo { get { return Math.Min(CohortCounter("IsDead"), Structure.MainStemFinalNodeNumber.Value()); } }

        /// <summary>Gets the plant appeared green leaf no.</summary>
        [Units("/plant")]
        [Description("Number of appeared leaves per plant that have appeared but not yet fully senesced on each plant")]
        public double PlantAppearedGreenLeafNo
        {
            get
            {
                return Leaves.Where(l => l.IsAppeared && !l.Finished).Sum(l => l.CohortPopulation) / Plant.Population;
            }
        }

        /// <summary>Gets the plant appeared leaf no.</summary>
        [Units("/plant")]
        [Description("Number of leaves per plant that have appeared")]
        public double PlantAppearedLeafNo
        {
            get { return Leaves.Where(l => l.IsAppeared).Sum(l => l.CohortPopulation); }
        }

        /// <summary>Gets the plant senesced leaf no.</summary>
        [Units("/plant")]
        [Description("Number of leaves per plant that have senesced")]
        public double PlantsenescedLeafNo
        {
            get { return PlantAppearedLeafNo/Plant.Population - PlantAppearedGreenLeafNo; }
        }

        /// <summary>Gets the lai dead.</summary>
        [Units("m^2/m^2")]
        public double LAIDead
        {
            get { return Leaves.Sum(l => l.DeadArea)/1000000; }
        }

        /// <summary>Gets the cohort live.</summary>
        [XmlIgnore]
        [Units("g/m^2")]
        public Biomass CohortLive
        {
            get
            {
                Biomass biomass = new Biomass();
                foreach (LeafCohort L in Leaves)
                    biomass = biomass + L.Live;
                return biomass;
            }
            
        }

        /// <summary>Gets the cohort dead.</summary>
        [XmlIgnore]
        [Units("g/m^2")]
        public Biomass CohortDead
        {
            get
            {
                Biomass biomass = new Biomass();
                foreach (LeafCohort L in Leaves)
                    biomass = biomass + L.Dead;
                return biomass;
            }
        }

        /// <summary>Gets the cover dead.</summary>
        [Units("0-1")]
        public double CoverDead { get { return 1.0 - Math.Exp(-KDead * LAIDead); } }

        /// <summary>Gets the RAD int tot.</summary>
        [Units("MJ/m^2/day")]
        [Description("This is the intercepted radiation value that is passed to the RUE class to calculate DM supply")]
        public double RadIntTot { get { return CoverGreen * MetData.Radn; } }

        /// <summary>Gets the specific area.</summary>
        [Units("mm^2/g")]
        public double SpecificArea { get { return MathUtilities.Divide(LAI * 1000000, Live.Wt , 0); } }

        /// <summary>
        /// Returns the relative expansion of the next leaf to produce its ligule
        /// </summary>
        public double NextExpandingLeafProportion
        {
            get
            {
                if (Plant.IsEmerged)
                    if (ExpandedCohortNo < InitialisedCohortNo)
                        if (Leaves[(int) ExpandedCohortNo].Age > 0)
                            if (AppearedCohortNo < InitialisedCohortNo)
                                return Math.Min(1,
                                    Leaves[(int) ExpandedCohortNo].Age/Leaves[(int) ExpandedCohortNo].GrowthDuration);
                            else
                                return Math.Min(1,
                                    Leaves[(int) ExpandedCohortNo].Age/Leaves[(int) ExpandedCohortNo].GrowthDuration*
                                    Structure.NextLeafProportion);
                        else
                            return 0;
                    else
                        return Structure.NextLeafProportion - 1;
                return 0;
            }
        }

        /// <summary>Gets the size of the cohort.</summary>
        [XmlIgnore]
        [Units("mm3")]
        public double[] CohortSize
        {
            get
            {
                int i = 0;
                double[] values = new double[MaximumMainStemLeafNumber];

                foreach (LeafCohort L in Leaves)
                {
                    if (L.IsAppeared)
                        values[i] = L.LiveArea / L.CohortPopulation;
                    else values[i] = 0;
                    i++;
                }
                return values;
            }
        }

        /// <summary>Gets the cohort area.</summary>
        [Units("mm2")]
        public double[] CohortArea
        {
            get
            {
                int i = 0;
                double[] values = new double[MaximumMainStemLeafNumber];

                foreach (LeafCohort L in Leaves)
                {
                    values[i] = L.LiveArea;
                    i++;
                }
                return values;
            }
        }

        /// <summary>Gets the maximum size of the cohort.</summary>
        [Units("mm2")]
        public double[] CohortMaxSize
        {
            get
            {
                int i = 0;
                double[] values = new double[MaximumMainStemLeafNumber];

                foreach (LeafCohort L in Leaves)
                {
                    values[i] = L.MaxLiveArea / L.MaxCohortPopulation;
                    i++;
                }
                return values;
            }
        }

        /// <summary>Gets the cohort sla.</summary>
        [Units("mm2/g")]
        public double[] CohortSLA
        {
            get
            {
                int i = 0;
                double[] values = new double[MaximumMainStemLeafNumber];

                foreach (LeafCohort L in Leaves)
                {
                    values[i] = L.SpecificArea;
                    i++;
                }
                return values;
            }
        }

        //General Leaf State variables
        /// <summary>Returns the area of the largest leaf.</summary>
        /// <value>The area of the largest leaf</value>
        [Units("mm2")]
        public double AreaLargestLeaf
        {
            get
            {
                double LLA = 0;
                foreach (LeafCohort L in Leaves)
                {
                    LLA = Math.Max(LLA, L.MaxArea);
                }

                return LLA;
            }
        }


        /// <summary>Gets the live n conc.</summary>
        [Units("g/g")]
        public double LiveNConc { get { return Live.NConc; } }

        /// <summary>Gets the potential growth.</summary>
        [Units("g/m^2")]
        public double PotentialGrowth { get { return DMDemand.Structural; } }

        /// <summary>Gets the transpiration.</summary>
        [Units("mm")]
        public double Transpiration { get { return WaterAllocation; } }
        /// <summary>
        /// The amount of mass lost to maintenance respiration
        /// </summary>
        public override double MaintenanceRespiration
        {
            get { return Leaves.Sum(l => l.MaintenanceRespiration); }
        }

        /// <summary>Gets the fw.</summary>
        [Units("0-1")]
        public double Fw { get { return MathUtilities.Divide(WaterAllocation, CalculateWaterDemand(), 1); } }

        /// <summary>Gets the function.</summary>
        [Units("0-1")]
        public double Fn
        {
            get
            {
                if (CohortParameters == null)
                    return 1;

                double f;
                double functionalNConc = (CohortParameters.CriticalNConc.Value() -
                                          CohortParameters.MinimumNConc.Value() * CohortParameters.StructuralFraction.Value()) *
                                         (1/(1 - CohortParameters.StructuralFraction.Value()));
                    if (functionalNConc <= 0)
                        f = 1;
                    else
                    f = Math.Max(0.0, Math.Min(Live.MetabolicNConc / functionalNConc, 1.0));

                return f;
            }
        }

        #endregion

        #region Functions

         /// <summary>1 based rank of the current leaf.</summary>
        private int CurrentRank { get; set; }

        /// <summary>Counts cohorts with a given condition.</summary>
        /// <param name="Condition">The condition.</param>
        /// <returns></returns>
        private int CohortCounter(string Condition)
        {
            int count = 0;
            foreach (LeafCohort L in Leaves)
            {
                object o = ReflectionUtilities.GetValueOfFieldOrProperty(Condition, L);
                if (o == null)
                    throw new ApsimXException(this, "Leaf.CohortCounter returned null for function GetValueOfFieldOrProperty for condition " + Condition);
                if ((bool)o)
                    count++;
            }
            return count;
        }

        private BiomassPoolType GetDMDemand()
        {
            double StructuralDemand = 0.0;
            double NonStructuralDemand = 0.0;
            double MetabolicDemand = 0.0;

            DMConversionEfficiency = DMConversionEfficiencyFunction.Value();
            if (DMDemandFunction != null)
            {
                StructuralDemand = DMDemandFunction.Value() * StructuralFraction.Value();
                NonStructuralDemand = DMDemandFunction.Value() * (1 - StructuralFraction.Value());
            }
            else
            {
                foreach (LeafCohort L in Leaves)
                {
                    StructuralDemand += L.StructuralDMDemand / DMConversionEfficiency;
                    MetabolicDemand += L.MetabolicDMDemand / DMConversionEfficiency;
                    NonStructuralDemand += L.NonStructuralDMDemand / DMConversionEfficiency;
                }
            }
            return new BiomassPoolType { Structural = StructuralDemand, Metabolic = MetabolicDemand, NonStructural = NonStructuralDemand };
        }
        /// <summary>Event from sequencer telling us to do our potential growth.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("DoPotentialPlantGrowth")]
        private void OnDoPotentialPlantGrowth(object sender, EventArgs e)
        {
            if (!Plant.IsEmerged)
                return;

                if (MicroClimatePresent == false)
                    throw new Exception(Name + " is trying to calculate water demand but no MicroClimate module is present.  Include a microclimate node in your zone");

                if (FrostFraction.Value() > 0)
                    foreach (LeafCohort l in Leaves)
                        l.DoFrost(FrostFraction.Value());

                bool nextExpandingLeaf = false;
                foreach (LeafCohort L in Leaves)
                {
                    CurrentRank = L.Rank;
                    L.DoPotentialGrowth(ThermalTime.Value(), CohortParameters);
                    if ((L.IsFullyExpanded == false) && (nextExpandingLeaf == false))
                    {
                        nextExpandingLeaf = true;
                        if (CurrentExpandingLeaf != L.Rank)
                        {
                            CurrentExpandingLeaf = L.Rank;
                            StartFractionExpanded = L.FractionExpanded;
                        }
                        FractionNextleafExpanded = (L.FractionExpanded - StartFractionExpanded) / (1 - StartFractionExpanded);
                    }
                }
                FRGR = FRGRFunction.Value();
        }

        /// <summary>Clears this instance.</summary>
        protected override void Clear()
        {
            Leaves = new List<LeafCohort>();
            WaterAllocation = 0;
            CohortsAtInitialisation = 0;
            TipsAtEmergence = 0;
        }
        /// <summary>Initialises the cohorts.</summary>
        [EventSubscribe("InitialiseLeafCohorts")]
        private void OnInitialiseLeafCohorts(object sender, EventArgs e) //This sets up cohorts (eg at germination)
        {
            Leaves = new List<LeafCohort>();
            foreach (LeafCohort Leaf in InitialLeaves)
            {
                Leaves.Add(Leaf.Clone());
            }
            foreach (LeafCohort Leaf in Leaves)
            {
                CohortsAtInitialisation += 1;
                if (Leaf.Area > 0)
                    TipsAtEmergence += 1;
                Leaf.DoInitialisation();
            }
        }

        /// <summary>Method to initialise new cohorts</summary>
        [EventSubscribe("AddLeafCohort")]
        private void OnAddLeafCohort(object sender, CohortInitParams InitParams)
        {
            if (CohortsInitialised == false)
                throw new Exception("Trying to initialse new cohorts prior to InitialStage.  Check the InitialStage parameter on the leaf object and the parameterisation of NodeInitiationRate.  Your NodeInitiationRate is triggering a new leaf cohort before leaves have been initialised.");

            LeafCohort NewLeaf = InitialLeaves[0].Clone();
            NewLeaf.CohortPopulation = 0;
            NewLeaf.Age = 0;
            NewLeaf.Rank = InitParams.Rank;
            NewLeaf.Area = 0.0;
            NewLeaf.DoInitialisation();
            Leaves.Add(NewLeaf);
            DoApexCalculations();
            
        }

        private void DoApexCalculations()
        {
            for (int i = 0; i < apexGroupAge.Count; i++)
                apexGroupAge[i]++;
            
            while (apexGroupSize.Sum() < Structure.ApexNum)
            {
                if (apexGroupSize.Count == 0)
                    apexGroupSize.Add(Structure.ApexNum);
                else 
                    apexGroupSize.Add(Structure.ApexNum - apexGroupSize[apexGroupSize.Count - 1]);
                apexGroupAge.Add(1);
            }

            while (apexGroupSize.Sum() > Structure.ApexNum)
            {
                double removeApex = apexGroupSize.Sum() - Structure.ApexNum;
                double remainingRemoveApex = removeApex;
                for (int i = apexGroupSize.Count - 1; i > 0; i--)
                {
                    double remove = Math.Min(apexGroupSize[i], remainingRemoveApex);
                    apexGroupSize[i] -= remove;
                    remainingRemoveApex -= remove;
                    if (remainingRemoveApex <= 0)
                        break;
                }
            }
        }

        /// <summary>Method to make leaf cohort appear and start expansion</summary>
        [EventSubscribe("LeafTipAppearance")]
        private void OnLeafTipAppearance(object sender, ApparingLeafParams CohortParams)
        {
            if (CohortsInitialised == false)
                throw new Exception("Trying to initialse new cohorts prior to InitialStage.  Check the InitialStage parameter on the leaf object and the parameterisation of NodeAppearanceRate.  Your NodeAppearanceRate is triggering a new leaf cohort before the initial leaves have been triggered.");
            if (CohortParams.CohortToAppear > InitialisedCohortNo)
                throw new Exception("MainStemNodeNumber exceeds the number of leaf cohorts initialised.  Check primordia parameters to make sure primordia are being initiated fast enough and for long enough");
            int i = CohortParams.CohortToAppear - 1;
            Leaves[i].CohortPopulation = CohortParams.TotalStemPopn;
            Leaves[i].Age = CohortParams.CohortAge;
            Leaves[i].DoAppearance(CohortParams.FinalFraction, CohortParameters);
            if (NewLeaf != null)
                NewLeaf.Invoke();
        }

        /// <summary>Does the nutrient allocations.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("DoActualPlantGrowth")]
        private void OnDoActualPlantGrowth(object sender, EventArgs e)
        {
            if (Plant.IsAlive)
            {
                foreach (LeafCohort L in Leaves)
                    L.DoActualGrowth(ThermalTime.Value(), CohortParameters);

                Structure.UpdateHeight();

                //Work out what proportion of the canopy has died today.  This variable is addressed by other classes that need to perform senescence proces at the same rate as leaf senescnce
                FractionDied = 0;
                if (DeadCohortNo > 0 && GreenCohortNo > 0)
                {
                    double deltaDeadLeaves = DeadCohortNo - DeadNodesYesterday; //Fixme.  DeadNodesYesterday is never given a value as far as I can see.
                    FractionDied = deltaDeadLeaves / GreenCohortNo;
                    DeadNodesYesterday = DeadCohortNo;
                }
            }
        }
        /// <summary>Zeroes the leaves.</summary>
        public virtual void ZeroLeaves()
        {
            Structure.LeafTipsAppeared = 0;
            Structure.Clear();
            Leaves.Clear();
            Summary.WriteMessage(this, "Removing leaves from plant");
        }

        /// <summary>Fractional interception "above" a given node position</summary>
        /// <param name="cohortno">cohort position</param>
        /// <returns>fractional interception (0-1)</returns>
        public double CoverAboveCohort(double cohortno)
        {
            int MM2ToM2 = 1000000; // Conversion of mm2 to m2
            double LAIabove = 0;
            for (int i = Leaves.Count - 1; i > cohortno - 1; i--)
                LAIabove += Leaves[i].LiveArea/MM2ToM2;
            return 1 - Math.Exp(-ExtinctionCoeff.Value() * LAIabove);
        }

        /// <summary>
        /// remove biomass from the leaf.
        /// </summary>
        /// <param name="biomassRemoveType">Name of event that triggered this biomass remove call.</param>
        /// <param name="value">The frations of biomass to remove</param>
        public override void DoRemoveBiomass(string biomassRemoveType, OrganBiomassRemovalType value)
        {
            bool writeToSummary = true;
            foreach (LeafCohort leaf in Leaves)
            {
                if (leaf.IsInitialised)
                {
                    double remainingLiveFraction = biomassRemovalModel.RemoveBiomass(biomassRemoveType, value, leaf.Live, leaf.Dead, leaf.Removed, leaf.Detached, writeToSummary);
                    leaf.LiveArea *= remainingLiveFraction;
                    writeToSummary = false; // only want it written once.
                    Detached.Add(leaf.Detached);
                    Removed.Add(leaf.Removed);
                }
            }
        }

        /// <summary>
        /// remove population elements from the leaf.
        /// </summary>
        /// <param name="ProportionRemoved">The proportion of stems removed by thinning</param>
        public void DoThin(double ProportionRemoved)
        {
            foreach (LeafCohort leaf in Leaves)
                leaf.CohortPopulation *= 1 - ProportionRemoved;
        }
        #endregion

        #region Arbitrator methods

        /// <summary>Gets or sets the dm demand.</summary>
        [Units("g/m^2")]
        public override BiomassPoolType DMDemand
        {
            get { return GetDMDemand(); }
        }

        /// <summary>Daily photosynthetic "net" supply of dry matter for the whole plant (g DM/m2/day)</summary>
        [Units("g/m^2")]
        public override BiomassSupplyType DMSupply
        {
            get { return GetDMSupply(); }
        }

        private BiomassSupplyType GetDMSupply()
        {
            double Retranslocation = 0;
            double Reallocation = 0;

            foreach (LeafCohort L in Leaves)
            {
                Retranslocation += L.LeafStartDMRetranslocationSupply;
                Reallocation += L.LeafStartDMReallocationSupply;
            }

            return new BiomassSupplyType { Fixation = Photosynthesis.Value(), Retranslocation = Retranslocation, Reallocation = Reallocation };
        }

        /// <summary>Sets the dm potential allocation.</summary>
        [Units("g/m^2")]
        public override BiomassPoolType DMPotentialAllocation
        {
            set
            {
                //Allocate Potential Structural DM
                if (DMDemand.Structural == 0 && value.Structural > 0.000000000001)
                        throw new Exception("Invalid allocation of potential DM in" + Name);

                double[] CohortPotentialStructualDMAllocation = new double[Leaves.Count + 2];

                if (value.Structural != 0.0)
                {
                    double DMPotentialsupply = value.Structural * DMConversionEfficiency;
                    double DMPotentialallocated = 0;
                    double TotalPotentialDemand = Leaves.Sum(l => l.StructuralDMDemand);
                    int i = 0;
                    foreach (LeafCohort L in Leaves)
                    {
                        i++;
                        double PotentialAllocation = Math.Min(L.StructuralDMDemand,
                            DMPotentialsupply*(L.StructuralDMDemand/TotalPotentialDemand));
                        CohortPotentialStructualDMAllocation[i] = PotentialAllocation;
                        DMPotentialallocated += PotentialAllocation;
                    }
                    if (DMPotentialallocated - value.Structural > 0.000000001)
                        throw new Exception("the sum of poteitial DM allocation to leaf cohorts is more that that allocated to leaf organ");
                }

                //Allocate Metabolic DM
                if (DMDemand.Metabolic == 0 && value.Metabolic > 0.000000000001)
                        throw new Exception("Invalid allocation of potential DM in" + Name);

                double[] CohortPotentialMetabolicDMAllocation = new double[Leaves.Count + 2];

                if (value.Metabolic != 0.0)
                {
                    double DMPotentialsupply = value.Metabolic * DMConversionEfficiency;
                    double DMPotentialallocated = 0;
                    double TotalPotentialDemand = Leaves.Sum(l => l.MetabolicDMDemand);
                    int i = 0;
                    foreach (LeafCohort L in Leaves)
                    {
                        i++;
                        double PotentialAllocation = Math.Min(L.MetabolicDMDemand,
                            DMPotentialsupply*L.MetabolicDMDemand/TotalPotentialDemand);
                        CohortPotentialMetabolicDMAllocation[i] = PotentialAllocation;
                        DMPotentialallocated += PotentialAllocation;
                    }
                    if (DMPotentialallocated - value.Metabolic > 0.000000001)
                        throw new Exception(
                            "the sum of poteitial DM allocation to leaf cohorts is more that that allocated to leaf organ");
                }

                //Send allocations to cohorts
                int a = 0;
                foreach (LeafCohort L in Leaves)
                {
                    a++;
                    L.DMPotentialAllocation = new BiomassPoolType
                    {
                        Structural = CohortPotentialStructualDMAllocation[a],
                        Metabolic = CohortPotentialMetabolicDMAllocation[a],
                    };
                }
            }
        }
        /// <summary>Sets the dm allocation.</summary>
        [Units("g/m^2")]
        public override BiomassAllocationType DMAllocation
        {
            set
            {
                // get DM lost by respiration (growth respiration)
                GrowthRespiration = 0.0;
                GrowthRespiration += value.Structural * (1.0 - DMConversionEfficiency)
                                  + value.NonStructural * (1.0 - DMConversionEfficiency)
                                  + value.Metabolic * (1.0 - DMConversionEfficiency);

                double[] StructuralDMAllocationCohort = new double[Leaves.Count + 2];
                double StartWt = Live.StructuralWt + Live.MetabolicWt + Live.NonStructuralWt;
                //Structural DM allocation
                if (DMDemand.Structural <= 0 && value.Structural > 0.000000000001)
                    throw new Exception("Invalid allocation of DM in Leaf");
                if (value.Structural > 0.0)
                {
                    double DMsupply = value.Structural * DMConversionEfficiency;
                    double DMallocated = 0;
                    double TotalDemand = Leaves.Sum(l => l.StructuralDMDemand);
                    double DemandFraction = value.Structural * DMConversionEfficiency / TotalDemand;
                    int i = 0;
                    foreach (LeafCohort L in Leaves)
                    {
                        i++;
                        double Allocation = Math.Min(L.StructuralDMDemand * DemandFraction, DMsupply);
                        StructuralDMAllocationCohort[i] = Allocation;
                        DMallocated += Allocation;
                        DMsupply -= Allocation;
                    }
                    if (DMsupply > 0.0000000001)
                        throw new Exception("DM allocated to Leaf left over after allocation to leaf cohorts");
                    if (DMallocated - value.Structural * DMConversionEfficiency > 0.000000001)
                        throw new Exception("the sum of DM allocation to leaf cohorts is more than that available to leaf organ");
                }

                //Metabolic DM allocation
                double[] MetabolicDMAllocationCohort = new double[Leaves.Count + 2];

                if (DMDemand.Metabolic <= 0 && value.Metabolic > 0.000000000001) 
                        throw new Exception("Invalid allocation of DM in Leaf");
                if (value.Metabolic > 0.0)
                {
                    double DMsupply = value.Metabolic * DMConversionEfficiency;
                    double DMallocated = 0;
                    double TotalDemand = Leaves.Sum(l => l.MetabolicDMDemand);
                    double DemandFraction = value.Metabolic * DMConversionEfficiency / TotalDemand;
                    int i = 0;
                    foreach (LeafCohort L in Leaves)
                    {
                        i++;
                        double Allocation = Math.Min(L.MetabolicDMDemand * DemandFraction, DMsupply);
                        MetabolicDMAllocationCohort[i] = Allocation;
                        DMallocated += Allocation;
                        DMsupply -= Allocation;
                    }
                    if (DMsupply > 0.0000000001)
                        throw new Exception("Metabolic DM allocated to Leaf left over after allocation to leaf cohorts");
                    if (DMallocated - value.Metabolic * DMConversionEfficiency > 0.000000001)
                        throw new Exception("the sum of Metabolic DM allocation to leaf cohorts is more that that allocated to leaf organ");
                }

                // excess allocation
                double[] NonStructuralDMAllocationCohort = new double[Leaves.Count + 2];
                double TotalSinkCapacity = 0;
                foreach (LeafCohort L in Leaves)
                    TotalSinkCapacity += L.NonStructuralDMDemand;
                if (value.NonStructural * DMConversionEfficiency > TotalSinkCapacity)
                //Fixme, this exception needs to be turned on again
                { }
                    //throw new Exception("Allocating more excess DM to Leaves then they are capable of storing");
                if (TotalSinkCapacity > 0.0)
                {
                    double SinkFraction = (value.NonStructural * DMConversionEfficiency )/ TotalSinkCapacity;
                    int i = 0;
                    foreach (LeafCohort L in Leaves)
                    {
                        i++;
                        double allocation = Math.Min(L.NonStructuralDMDemand * SinkFraction, value.NonStructural * DMConversionEfficiency);
                        NonStructuralDMAllocationCohort[i] = allocation;
                    }
                }

                // retranslocation
                double[] DMRetranslocationCohort = new double[Leaves.Count + 2];

                if (value.Retranslocation - DMSupply.Retranslocation > 0.0000000001)
                    throw new Exception(Name + " cannot supply that amount for DM retranslocation");
                if (value.Retranslocation > 0)
                {
                    double remainder = value.Retranslocation;
                    int i = 0;
                    foreach (LeafCohort L in Leaves)
                    {
                        i++;
                        double Supply = Math.Min(remainder, L.LeafStartDMRetranslocationSupply);
                        DMRetranslocationCohort[i] = Supply;
                        remainder -= Supply;
                    }
                    if (remainder > 0.0000000001)
                        throw new Exception(Name + " DM Retranslocation demand left over after processing.");
                }

                // Reallocation
                double[] DMReAllocationCohort = new double[Leaves.Count + 2];
                if (value.Reallocation - DMSupply.Reallocation > 0.000000001)
                    throw new Exception(Name + " cannot supply that amount for DM Reallocation");
                if (value.Reallocation < -0.000000001)
                    throw new Exception(Name + " recieved -ve DM reallocation");
                if (value.Reallocation > 0)
                {
                    double remainder = value.Reallocation;
                    int i = 0;
                    foreach (LeafCohort L in Leaves)
                    {
                        i++;
                        double ReAlloc = Math.Min(remainder, L.LeafStartDMReallocationSupply);
                        remainder = Math.Max(0.0, remainder - ReAlloc);
                        DMReAllocationCohort[i] = ReAlloc;
                    }
                    if (!MathUtilities.FloatsAreEqual(remainder, 0.0))
                        throw new Exception(Name + " DM Reallocation demand left over after processing.");
                }

                //Send allocations to cohorts
                int a = 0;
                foreach (LeafCohort L in Leaves)
                {
                    a++;
                    L.DMAllocation = new BiomassAllocationType
                    {
                        Structural = StructuralDMAllocationCohort[a],
                        Metabolic = MetabolicDMAllocationCohort[a],
                        NonStructural = NonStructuralDMAllocationCohort[a],
                        Retranslocation = DMRetranslocationCohort[a],
                        Reallocation = DMReAllocationCohort[a],
                    };
                }

                double EndWt = Live.StructuralWt + Live.MetabolicWt + Live.NonStructuralWt;
                double CheckValue = StartWt + value.Structural*DMConversionEfficiency + value.Metabolic * DMConversionEfficiency + value.NonStructural * DMConversionEfficiency - value.Reallocation - value.Retranslocation - value.Respired;
                double ExtentOfError = Math.Abs(EndWt - CheckValue);
                double FloatingPointError = 0.00000001;
                if (ExtentOfError > FloatingPointError)
                    throw new Exception(Name + "Not all leaf DM allocation was used");
            }
        }
        /// <summary>Gets or sets the n demand.</summary>
        [Units("g/m^2")]
        public override BiomassPoolType NDemand
        {
            get
            {
                double StructuralDemand = 0.0;
                double MetabolicDemand = 0.0;
                double NonStructuralDemand = 0.0;
                foreach (LeafCohort L in Leaves)
                {
                    StructuralDemand += L.StructuralNDemand;
                    MetabolicDemand += L.MetabolicNDemand;
                    NonStructuralDemand += L.NonStructuralNDemand;
                }
                return new BiomassPoolType { Structural = StructuralDemand, Metabolic = MetabolicDemand, NonStructural = NonStructuralDemand };
            }
        }

        /// <summary>Sets the n allocation.</summary>
        [Units("g/m^2")]
        public override BiomassAllocationType NAllocation
        {
            set
            {
                if (NDemand.Structural == 0 && value.Structural > 0) //FIXME this needs to be seperated into compoents
                        throw new Exception("Invalid allocation of N");

                double StartN = Live.StructuralN + Live.MetabolicN + Live.NonStructuralN;

                double[] StructuralNAllocationCohort = new double[Leaves.Count + 2];
                double[] MetabolicNAllocationCohort = new double[Leaves.Count + 2];
                double[] NonStructuralNAllocationCohort = new double[Leaves.Count + 2];
                double[] NReallocationCohort = new double[Leaves.Count + 2];
                double[] NRetranslocationCohort = new double[Leaves.Count + 2];

                if (value.Structural + value.Metabolic + value.NonStructural > 0.0)
                {
                    //setup allocation variables
                    double[] CohortNAllocation = new double[Leaves.Count + 2];
                    double[] StructuralNDemand = new double[Leaves.Count + 2];
                    double[] MetabolicNDemand = new double[Leaves.Count + 2];
                    double[] NonStructuralNDemand = new double[Leaves.Count + 2];
                    double TotalStructuralNDemand = 0;
                    double TotalMetabolicNDemand = 0;
                    double TotalNonStructuralNDemand = 0;

                    int i = 0;
                    foreach (LeafCohort L in Leaves)
                    {
                            i++;
                            CohortNAllocation[i] = 0;
                            StructuralNDemand[i] = L.StructuralNDemand;
                            TotalStructuralNDemand += L.StructuralNDemand;
                            MetabolicNDemand[i] = L.MetabolicNDemand;
                            TotalMetabolicNDemand += L.MetabolicNDemand;
                            NonStructuralNDemand[i] = L.NonStructuralNDemand;
                            TotalNonStructuralNDemand += L.NonStructuralNDemand;
                        }
                    double NSupplyValue = value.Structural;
                    
                    // first make sure each cohort gets the structural N requirement for growth (includes MinNconc for structural growth and MinNconc for nonstructural growth)
                    if (NSupplyValue > 0 && TotalStructuralNDemand > 0)
                    {
                        i = 0;
                        foreach (LeafCohort L in Leaves)
                        {
                            i++;
                            StructuralNAllocationCohort[i] = Math.Min(StructuralNDemand[i], NSupplyValue * (StructuralNDemand[i] / TotalStructuralNDemand));
                        }

                    }
                    // then allocate additional N relative to leaves metabolic demands
                    NSupplyValue = value.Metabolic;
                    if (NSupplyValue > 0 && TotalMetabolicNDemand > 0)
                    {
                        i = 0;
                        foreach (LeafCohort L in Leaves)
                        {
                            i++;
                            MetabolicNAllocationCohort[i] = Math.Min(MetabolicNDemand[i],
                                NSupplyValue*(MetabolicNDemand[i]/TotalMetabolicNDemand));
                        }
                    }
                    // then allocate excess N relative to leaves N sink capacity
                    NSupplyValue = value.NonStructural;
                    if (NSupplyValue > 0 && TotalNonStructuralNDemand > 0)
                    {
                        i = 0;
                        foreach (LeafCohort L in Leaves)
                        {
                            i++;
                            NonStructuralNAllocationCohort[i] += Math.Min(NonStructuralNDemand[i], NSupplyValue * (NonStructuralNDemand[i] / TotalNonStructuralNDemand));
                        }
                    }
                }

                // Retranslocation
                if (value.Retranslocation - NSupply.Retranslocation > 0.000000001)
                    throw new Exception(Name + " cannot supply that amount for N retranslocation");
                if (value.Retranslocation < -0.000000001)
                    throw new Exception(Name + " recieved -ve N retranslocation");
                if (value.Retranslocation > 0)
                {
                    int i = 0;
                    double remainder = value.Retranslocation;
                    foreach (LeafCohort L in Leaves)
                    {
                        i++;
                        double Retrans = Math.Min(remainder, L.LeafStartNRetranslocationSupply);
                        NRetranslocationCohort[i] = Retrans;
                        remainder = Math.Max(0.0, remainder - Retrans);
                    }
                    if (!MathUtilities.FloatsAreEqual(remainder, 0.0))
                        throw new Exception(Name + " N Retranslocation demand left over after processing.");
                }

                // Reallocation
                if (value.Reallocation - NSupply.Reallocation > 0.000000001)
                    throw new Exception(Name + " cannot supply that amount for N Reallocation");
                if (value.Reallocation < -0.000000001)
                    throw new Exception(Name + " recieved -ve N reallocation");
                if (value.Reallocation > 0)
                {
                    int i = 0;
                    double remainder = value.Reallocation;
                    foreach (LeafCohort L in Leaves)
                    {
                        i++;
                        double ReAlloc = Math.Min(remainder, L.LeafStartNReallocationSupply);
                        NReallocationCohort[i] = ReAlloc;
                        remainder = Math.Max(0.0, remainder - ReAlloc);
                    }
                    if (!MathUtilities.FloatsAreEqual(remainder, 0.0))
                        throw new Exception(Name + " N Reallocation demand left over after processing.");
                }

                //Send allocations to cohorts
                int a = 0;
                foreach (LeafCohort L in Leaves)
                {
                    a++;
                    L.NAllocation = new BiomassAllocationType
                    {
                        Structural = StructuralNAllocationCohort[a],
                        Metabolic = MetabolicNAllocationCohort[a],
                        NonStructural = NonStructuralNAllocationCohort[a],
                        Retranslocation = NRetranslocationCohort[a],
                        Reallocation = NReallocationCohort[a],
                    };
                }

                double endN = Live.StructuralN + Live.MetabolicN + Live.NonStructuralN;
                double checkValue = StartN + value.Structural + value.Metabolic + value.NonStructural -
                                    value.Reallocation - value.Retranslocation - value.Respired;
                double extentOfError = Math.Abs(endN - checkValue);
                if (extentOfError > 0.00000001)
                    throw new Exception(Name + "Some Leaf N was not allocated.");
            }
        }

        /// <summary>Gets or sets the n supply.</summary>
        [Units("g/m^2")]
        public override BiomassSupplyType NSupply
        {
            get
            {
                double RetransSupply = 0;
                double ReallocationSupply = 0;
                foreach (LeafCohort L in Leaves)
                {
                    RetransSupply += Math.Max(0, L.LeafStartNRetranslocationSupply);
                    ReallocationSupply += L.LeafStartNReallocationSupply;
                }
                return new BiomassSupplyType { Retranslocation = RetransSupply, Reallocation = ReallocationSupply };
            }
        }

        /// <summary>Gets or sets the minimum nconc.</summary>
        public override double MinNconc
        {
            get
            {
                return CohortParameters.CriticalNConc.Value();
            }
        }
        #endregion

        #region Event handlers and publishers

        /// <summary>Occurs when [new leaf].</summary>
        public event NullTypeDelegate NewLeaf;

        /// <summary>Called when [remove lowest leaf].</summary>
        [EventSubscribe("RemoveLowestLeaf")]
        private void OnRemoveLowestLeaf()
        {
            Summary.WriteMessage(this, "Removing lowest Leaf");
            Leaves.RemoveAt(0);
        }
        
        /// <summary>Called when crop is ending</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="data">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("PlantSowing")]
        private void OnPlantSowing(object sender, SowPlant2Type data)
        {
            if (data.Plant == Plant)
            {
                MicroClimatePresent = false;
                Clear();
                if (data.MaxCover <= 0.0)
                    throw new Exception("MaxCover must exceed zero in a Sow event.");
                MaxCover = data.MaxCover;
            }
        }

        /// <summary>Called when [kill leaf].</summary>
        /// <param name="KillLeaf">The kill leaf.</param>
        [EventSubscribe("KillLeaf")]
        private void OnKillLeaf(KillLeafType KillLeaf)
        {
            Summary.WriteMessage(this, "Killing " + KillLeaf.KillFraction + " of leaves on plant");
            foreach (LeafCohort L in Leaves)
                L.DoKill(KillLeaf.KillFraction);
        }

        /// <summary>Called when crop is being cut.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("Cutting")]
        private void OnCutting(object sender, EventArgs e)
        {
            if (sender == Plant)
            {
                Summary.WriteMessage(this, "Cutting " + Name + " from " + Plant.Name);

                if (Wt > 0)
                    SurfaceOrganicMatter.Add(Wt * 10, N * 10, 0, Plant.CropType, Name);
                Live.Clear();
                Dead.Clear();
                Leaves.Clear();
                CohortsAtInitialisation = 0;
            }
        }

        /// <summary>Called when crop is being prunned.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("Pruning")]
        private void OnPruning(object sender, EventArgs e)
        {
            Structure.PotLeafTipsAppeared = 0;
            Structure.CohortToInitialise = 0;
            Structure.TipToAppear = 0;
            Structure.Emerged = false;
            Structure.Clear();
            Structure.ResetStemPopn();
            Structure.LeafTipsAppeared = 0;
            Structure.NextLeafProportion = 1.0;

            Leaves.Clear();
            CohortsAtInitialisation = 0;
            TipsAtEmergence = 0;
            Structure.Germinated = false;

        }

        /// <summary>Called when [simulation commencing].</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("Commencing")]
        private new void OnSimulationCommencing(object sender, EventArgs e)
        {
            base.OnSimulationCommencing(sender, e);
            List<LeafCohort> initialLeaves = new List<LeafCohort>();
            foreach (LeafCohort initialLeaf in Apsim.Children(this, typeof(LeafCohort)))
                initialLeaves.Add(initialLeaf);
            InitialLeaves = initialLeaves.ToArray();
        }

        /// <summary>Called when crop is ending</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("PlantEnding")]
        private void OnPlantEnding(object sender, EventArgs e)
        {
            if (Wt > 0.0)
            {
                Detached.Add(Live);
                Detached.Add(Dead);
                SurfaceOrganicMatter.Add(Wt * 10, N * 10, 0, Plant.CropType, Name);
            }

            Clear();
            CohortsAtInitialisation = 0;
        }

        /// <summary>Called when crop is being cut.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("Harvesting")]
        private void OnHarvesting(object sender, EventArgs e)
        {
            CohortsAtInitialisation = 0;
        }
        #endregion
    }
}
