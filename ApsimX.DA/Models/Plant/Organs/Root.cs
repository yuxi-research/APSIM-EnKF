namespace Models.PMF.Organs
{
    using APSIM.Shared.Utilities;
    using Library;
    using Models.Core;
    using Models.PMF.Functions;
    using Models.PMF.Interfaces;
    using Models.Soils;
    using Models.Soils.Arbitrator;
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    ///<summary>
    /// The generic root model calculates root growth in terms of rooting depth, biomass accumulation and subsequent 
    /// root length density.
    /// 
    /// **Root Growth**
    /// 
    /// Roots grow downward through the soil profile and rate is determined by RootFrontVelocity. The RootFrontVelocity 
    /// is also influenced by the extension resistance posed by the soil, paramterised using the soil XF value.
    /// 
    /// **Dry Matter Demands**
    /// 
    /// 100% of the dry matter (DM) demanded from the root is structural. The daily DM demand from root is calculated as a 
    /// proportion of total DM supply using a PartitionFraction function. The daily loss of roots is calculated using
    /// a SenescenceRate function.
    /// 
    /// **Nitrogen Demands**
    /// 
    /// The daily structural N demand from root is the product of total DM demand and a nitrogen concentration of MinimumNConc%.
    /// 
    /// **Nitrogen Uptake**
    /// 
    /// Potential N uptake by the root system is calculated for each soil layer that the roots have extended into.
    /// In each layer potential uptake is calculated as the product of the mineral nitrogen in the layer, a factor 
    /// controlling the rate of extraction (kNO<sub>3</sub> and kNH<sub>4</sub>), the concentration of of N (ppm) 
    /// and a soil moisture factor which decreases as the soil dries. Nitrogen uptake demand is limited to the maximum 
    /// of potential uptake and the plants N demand.  Uptake N demand is then passed to the soil arbitrator which 
    /// determines how much of their Nitrogen uptake demand each plant instance will be allowed to take up.
    /// 
    /// **Water Uptake**
    /// 
    /// Potential water uptake by the root system is calculated for each soil layer that the roots have extended into.
    /// In each layer potential uptake is calculated as the product of the available Water in the layer, and a factor 
    /// controlling the rate of extraction (KL). The KL values are set in the soil and may be further modified by the crop
    /// via KLModifier, KNO3 and KN4.
    ///</summary>
    [Serializable]
    [Description("Root Class")]
    [ViewName("UserInterface.Views.GridView")]
    [PresenterName("UserInterface.Presenters.PropertyPresenter")]
    public class Root : BaseOrgan, IWaterNitrogenUptake
    {
        #region Links
        /// <summary>The arbitrator</summary>
        [Link]
        OrganArbitrator Arbitrator = null;

        #endregion

        #region Parameters

        /// <summary>Link to the KNO3 link</summary>
        [Link]
        IFunction KNO3 = null;

        /// <summary>Link to the KNH4 link</summary>
        [Link]
        IFunction KNH4 = null;

        /// <summary>Soil water factor for N Uptake</summary>
        [Link]
        IFunction NUptakeSWFactor = null;

        /// <summary>Gets or sets the initial biomass dry matter weight</summary>
        [Link]
        [Units("g/plant")]
        IFunction InitialDM = null;

        /// <summary>Gets or sets the specific root length</summary>
        [Link]
        [Units("m/g")]
        IFunction SpecificRootLength = null;

        /// <summary>The nitrogen demand switch</summary>
        [Link]
        IFunction NitrogenDemandSwitch = null;

        /// <summary>The N retranslocation factor</summary>
        [Link(IsOptional = true)]
        [Units("/d")]
        IFunction NRetranslocationFactor = null;

        /// <summary>The N reallocation factor</summary>
        [Link(IsOptional = true)]
        [Units("/d")]
        IFunction NReallocationFactor = null;

        /// <summary>The DM retranslocation factor</summary>
        [Link(IsOptional = true)]
        [Units("/d")]
        IFunction DMRetranslocationFactor = null;

        /// <summary>The DM reallocation factor</summary>
        [Link(IsOptional = true)]
        [Units("/d")]
        IFunction DMReallocationFactor = null;

        /// <summary>The biomass senescence rate</summary>
        [Link]
        [Units("/d")]
        IFunction SenescenceRate = null;
        
        /// <summary>The root front velocity</summary>
        [Link]
        [Units("mm/d")]
        public IFunction RootFrontVelocity = null;

        /// <summary>The DM structural fraction</summary>
        [Link(IsOptional = true)]
        [Units("g/g")]
        IFunction StructuralFraction = null;

        /// <summary>The biomass partition fraction</summary>
        [Link]
        [Units("0-1")]
        IFunction PartitionFraction = null;
        
        /// <summary>The maximum N concentration</summary>
        [Link]
        [Units("g/g")]
        IFunction MaximumNConc = null;

        /// <summary>The minimum N concentration</summary>
        [Link]
        [Units("g/g")]
        IFunction MinimumNConc = null;

        /// <summary>The critical N concentration</summary>
        [Link(IsOptional = true)]
        [Units("g/g")]
        IFunction CriticalNConc = null;

        /// <summary>The maximum daily N uptake</summary>
        [Link]
        [Units("kg N/ha")]
        IFunction MaxDailyNUptake = null;
        
        /// <summary>The kl modifier</summary>
        [Link]
        [Units("0-1")]
        IFunction KLModifier = null;
        
        /// <summary>The Maximum Root Depth</summary>
        [Link]
        [Units("0-1")]
        public IFunction MaximumRootDepth = null;

        /// <summary>Link to biomass removal model</summary>
        [ChildLink]
        public BiomassRemoval biomassRemovalModel = null;

        /// <summary>A list of other zone names to grow roots in</summary>
        public List<string> ZoneNamesToGrowRootsIn { get; set; }

        /// <summary>The root depths for each addition zone.</summary>
        public List<double> ZoneRootDepths { get; set; }

        /// <summary>The live weights for each addition zone.</summary>
        public List<double> ZoneInitialDM { get; set; }

        private double BiomassToleranceValue = 0.0000000001;   // 10E-10

        #endregion

        #region States

        /// <summary>A list of all zones to grow roots in</summary>
        [XmlIgnore]
        public List<ZoneState> Zones { get; set; }

        /// <summary>The zone where the plant is growing</summary>
        [XmlIgnore]
        public ZoneState PlantZone { get; set; }

        /// <summary>The DM supply for retranslocation</summary>
        private double DMRetranslocationSupply = 0.0;

        /// <summary>The DM supply for reallocation</summary>
        private double DMReallocationSupply = 0.0;

        /// <summary>The N supply for retranslocation</summary>
        private double NRetranslocationSupply = 0.0;

        /// <summary>The N supply for reallocation</summary>
        private double NReallocationSupply = 0.0;

        /// <summary>The structural DM demand</summary>
        private double StructuralDMDemand = 0.0;

        /// <summary>The non structural DM demand</summary>
        private double NonStructuralDMDemand = 0.0;

        /// <summary>The metabolic DM demand</summary>
        private double MetabolicDMDemand = 0.0;

        /// <summary>The structural N demand</summary>
        private double StructuralNDemand = 0.0;

        /// <summary>The non structural N demand</summary>
        private double NonStructuralNDemand = 0.0;

        /// <summary>The metabolic N demand</summary>
        private double MetabolicNDemand = 0.0;

        #endregion

        #region Outputs

        /// <summary>Gets the root length density.</summary>
        [Units("mm/mm3")]
        public double[] LengthDensity
        {
            get
            {
                double[] value;
                if (PlantZone == null)
                    value = new double[0];
                else
                {
                    value = new double[PlantZone.soil.Thickness.Length];
                    for (int i = 0; i < PlantZone.soil.Thickness.Length; i++)
                        value[i] = PlantZone.LayerLive[i].Wt * SpecificRootLength.Value() * 1000 / 1000000 / PlantZone.soil.Thickness[i];
                }
                return value;
            }
        }

        ///<Summary>Total DM demanded by roots</Summary>
        [Units("g/m2")]
        [XmlIgnore]
        public double TotalDMDemand { get; set; }

        ///<Summary>The amount of N taken up after arbitration</Summary>
        [Units("g/m2")]
        [XmlIgnore]
        public double NTakenUp { get; set; }

        /// <summary>Root depth.</summary>
        [XmlIgnore]
        public double Depth { get { return PlantZone.Depth; } }

        /// <summary>Layer live</summary>
        [XmlIgnore]
        public Biomass[] LayerLive { get { if (PlantZone != null) return PlantZone.LayerLive; else return new Biomass[0]; } }

        /// <summary>Layer dead.</summary>
        [XmlIgnore]
        public Biomass[] LayerDead { get { return PlantZone.LayerDead; } }

        /// <summary>Gets or sets the length.</summary>
        [XmlIgnore]
        public double Length { get { return PlantZone.Length; } }

        /// <summary>Gets or sets the water uptake.</summary>
        [Units("mm")]
        public double WaterUptake
        {
            get
            {
                double uptake = 0;
                foreach (ZoneState zone in Zones)
                    uptake = uptake + MathUtilities.Sum(zone.Uptake);
                return -uptake;
            }
        }

        /// <summary>Gets or sets the water uptake.</summary>
        [Units("kg/ha")]
        public double NUptake
        {
            get
            {
                double uptake = 0;
                foreach (ZoneState zone in Zones)
                    uptake = MathUtilities.Sum(zone.NitUptake);
                return uptake;
            }
        }

        /// <summary>Gets or sets the mid points of each layer</summary>
        [XmlIgnore]
        public double[] LayerMidPointDepth { get; private set; }

        /// <summary>Gets or sets root water content</summary>
        [XmlIgnore]
        public double[] RWC { get; private set; }

        #endregion

        #region Functions

        /// <summary>Constructor</summary>
        public Root()
        {
            Zones = new List<ZoneState>();
            ZoneNamesToGrowRootsIn = new List<string>();
            ZoneRootDepths = new List<double>();
            ZoneInitialDM = new List<double>();
        }

        /// <summary>Initialise all zones.</summary>
        private void InitialiseZones()
        {
            Zones.Clear();
            Zones.Add(PlantZone);
            if (ZoneRootDepths.Count != ZoneNamesToGrowRootsIn.Count ||
                ZoneRootDepths.Count != ZoneInitialDM.Count)
                throw new Exception("The root zone variables (ZoneRootDepths, ZoneNamesToGrowRootsIn, ZoneInitialDM) need to have the same number of values");

            for (int i = 0; i < ZoneNamesToGrowRootsIn.Count; i++)
            {
                Zone zone = Apsim.Find(this, ZoneNamesToGrowRootsIn[i]) as Zone;
                if (zone != null)
                {
                    Soil soil = Apsim.Find(zone, typeof(Soil)) as Soil;
                    if (soil == null)
                        throw new Exception("Cannot find soil in zone: " + zone.Name);
                    if (soil.Crop(Plant.Name) == null)
                        throw new Exception("Cannot find a soil crop parameterisation for " + Plant.Name);
                    ZoneState newZone = new ZoneState(Plant, this, soil, ZoneRootDepths[i], ZoneInitialDM[i], Plant.Population, MaximumNConc.Value());
                    Zones.Add(newZone);
                }
            }
        }

        /// <summary>Does the water uptake.</summary>
        /// <param name="Amount">The amount.</param>
        /// <param name="zoneName">Zone name to do water uptake in</param>
        public void DoWaterUptake(double[] Amount, string zoneName)
        {
            ZoneState zone = Zones.Find(z => z.Name == zoneName);
            if (zone == null)
                throw new Exception("Cannot find a zone called " + zoneName);
				
			zone.Uptake = MathUtilities.Multiply_Value(Amount, -1.0);
            zone.soil.SoilWater.dlt_sw_dep = zone.Uptake;
        }

        /// <summary>Does the Nitrogen uptake.</summary>
        /// <param name="zonesFromSoilArbitrator">List of zones from soil arbitrator</param>
        public void DoNitrogenUptake(List<ZoneWaterAndN> zonesFromSoilArbitrator)
        {
            foreach (ZoneWaterAndN thisZone in zonesFromSoilArbitrator)
            {
                ZoneState zone = Zones.Find(z => z.Name == thisZone.Zone.Name);
                if (zone != null)
                {
                    zone.solutes.Subtract("NO3", thisZone.NO3N);
                    zone.solutes.Subtract("NH4", thisZone.NH4N);

                    zone.NitUptake = MathUtilities.Multiply_Value(MathUtilities.Add(thisZone.NO3N, thisZone.NH4N), -1);
                }
            }
        }

        /// <summary>Called when crop is ending</summary>
        [EventSubscribe("PlantEnding")]
        protected void DoPlantEnding(object sender, EventArgs e)
        {
            //Send all root biomass to soil FOM
            DoRemoveBiomass(null, new OrganBiomassRemovalType() { FractionLiveToResidue = 1.0 });
            Clear();
        }

        /// <summary>Clears this instance.</summary>
        protected override void Clear()
        {
            base.Clear();
            PlantZone.Clear();
            Zones.Clear();
        }

        /// <summary>Gets a factor to account for root zone Water tension weighted for root mass.</summary>
        [Units("0-1")]
        public double WaterTensionFactor
        {
            get
            {
                double MeanWTF = 0;
                
                if (Live.Wt > 0)
                   foreach (ZoneState Z in Zones)
                       for (int i = 0; i < Z.LayerLive.Length; i++)
                           MeanWTF += Z.LayerLive[i].Wt/ Live.Wt * MathUtilities.Bound(2 * Z.soil.PAW[i] / Z.soil.PAWC[i], 0, 1);

                return MeanWTF;
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>Called when [simulation commencing].</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="ApsimXException">Cannot find a soil crop parameterisation for  + Name</exception>
        [EventSubscribe("Commencing")]
        private new void OnSimulationCommencing(object sender, EventArgs e)
        {
            Soil soil = Apsim.Find(this, typeof(Soil)) as Soil;
            if (soil == null)
                throw new Exception("Cannot find soil");
            if (soil.Crop(Plant.Name) == null && soil.Weirdo == null)
                throw new Exception("Cannot find a soil crop parameterisation for " + Plant.Name);

            PlantZone = new ZoneState(Plant, this, soil, 0, InitialDM.Value(), Plant.Population, MaximumNConc.Value());
            Zones = new List<ZoneState>();
            base.OnSimulationCommencing(sender, e);
        }

        /// <summary>Called when crop is sown</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="data">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("PlantSowing")]
        private void OnPlantSowing(object sender, SowPlant2Type data)
        {
            if (data.Plant == Plant)
            {
                PlantZone.Initialise(Plant.SowingData.Depth, InitialDM.Value(), Plant.Population, MaximumNConc.Value());
                InitialiseZones();
            }
        }

        /// <summary>Event from sequencer telling us to do our potential growth.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("DoPotentialPlantGrowth")]
        private void OnDoPotentialPlantGrowth(object sender, EventArgs e)
        {
            if (Plant.IsEmerged)
            {
                PlantZone.Length = MathUtilities.Sum(LengthDensity);
                DoSupplyCalculations(); //TODO: This should be called from the Arbitrator, OnDoPotentialPlantPartioning
            }
        }

        /// <summary>Computes the DM and N amounts that are made available for new growth</summary>
        public void DoSupplyCalculations()
        {
            DMRetranslocationSupply = AvailableDMRetranslocation();
            DMReallocationSupply = AvailableDMReallocation();
            NRetranslocationSupply = AvailableNRetranslocation();
            NReallocationSupply = AvailableNReallocation();
        }

        /// <summary>Computes the DM and N amounts demanded this computation round</summary>
        public void DoDMDemandCalculations()
        {
            if (Plant.SowingData.Depth < PlantZone.Depth)
            {
                StructuralDMDemand = DemandedDMStructural();
                NonStructuralDMDemand = DemandedDMNonStructural();
                TotalDMDemand = StructuralDMDemand + NonStructuralDMDemand + MetabolicDMDemand;
                ////This sum is currently not necessary as demand is not calculated on a layer basis.
                //// However it might be some day... and can consider non structural too
            }
        }

        /// <summary>Does the nutrient allocations.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("DoActualPlantGrowth")]
        private void OnDoActualPlantGrowth(object sender, EventArgs e)
        {
            if (Plant.IsAlive)
            {
                foreach(ZoneState Z in Zones)
                    Z.GrowRootDepth();
                // Do Root Senescence
                DoRemoveBiomass(null, new OrganBiomassRemovalType() { FractionLiveToResidue = SenescenceRate.Value() });
            }
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
        }

        #endregion

        #region IArbitrator interface

        /// <summary>Gets the DM demand for this computation round.</summary>
        public override BiomassPoolType DMDemand
        {
            get
            {
                if (Plant.IsEmerged)
                    DoDMDemandCalculations(); //TODO: This should be called from the Arbitrator, OnDoPotentialPlantPartioning
                return new BiomassPoolType
                {
                    Structural = StructuralDMDemand,
                    NonStructural = NonStructuralDMDemand,
                    Metabolic = 0.0
                };
            }
        }

        /// <summary>Computes the amount of structural DM demanded.</summary>
        public double DemandedDMStructural()
        {
            if (DMConversionEfficiency > 0.0)
            {
                double demandedDM = Arbitrator.DM.TotalFixationSupply * PartitionFraction.Value();
                if (StructuralFraction != null)
                    demandedDM *= StructuralFraction.Value() / DMConversionEfficiency;
                else
                    demandedDM /= DMConversionEfficiency;

                return demandedDM;
            }
             // Conversion efficiency is zero!!!!
                return 0.0;
        }

        /// <summary>Computes the amount of non structural DM demanded.</summary>
        public double DemandedDMNonStructural()
        {
            if ((DMConversionEfficiency > 0.0) && (StructuralFraction != null))
            {
                double rootLiveStructuralWt = 0.0;
                double rootLiveNonStructuralWt = 0.0;
                foreach (ZoneState Z in Zones)
                    for (int i = 0; i < Z.LayerLive.Length; i++)
                    {
                        rootLiveStructuralWt += Z.LayerLive[i].StructuralWt;
                        rootLiveNonStructuralWt += Z.LayerLive[i].NonStructuralWt;
                    }

                double theoreticalMaximumDM = (rootLiveStructuralWt + StructuralDMDemand) / StructuralFraction.Value();
                double baseAllocated = rootLiveStructuralWt + rootLiveNonStructuralWt + StructuralDMDemand;
                double demandedDM = Math.Max(0.0, theoreticalMaximumDM - baseAllocated) / DMConversionEfficiency;
                return demandedDM;
            }
             // Either there is no NonStructural fraction or conversion efficiency is zero!!!!
                return 0.0;
        }

        /// <summary>Gets the DM supply for this computation round.</summary>
        public override BiomassSupplyType DMSupply
        {
            get
            {
                return new BiomassSupplyType
                {
                    Fixation = 0.0,
                    Retranslocation = DMRetranslocationSupply,
                    Reallocation = DMReallocationSupply
                };
            }
        }

        /// <summary>Computes the amount of DM available for retranslocation.</summary>
        public double AvailableDMRetranslocation()
        {
            if (DMRetranslocationFactor != null)
            {
                double rootLiveNonStructuralWt = 0.0;
                foreach (ZoneState Z in Zones)
                    for (int i = 0; i < Z.LayerLive.Length; i++)
                        rootLiveNonStructuralWt += Z.LayerLive[i].NonStructuralWt;

                double availableDM = Math.Max(0.0, rootLiveNonStructuralWt - DMReallocationSupply) * DMRetranslocationFactor.Value();
                if (availableDM < -BiomassToleranceValue)
                    throw new Exception("Negative DM retranslocation value computed for " + Name);

                return availableDM;
            }
            else
            { // By default retranslocation is turned off!!!!
                return 0.0;
            }
        }

        /// <summary>Computes the amount of DM available for reallocation.</summary>
        public double AvailableDMReallocation()
        {
            if (DMReallocationFactor != null)
            {
                double rootLiveNonStructuralWt = 0.0;
                foreach (ZoneState Z in Zones)
                    for (int i = 0; i < Z.LayerLive.Length; i++)
                        rootLiveNonStructuralWt += Z.LayerLive[i].NonStructuralWt;

                double availableDM = rootLiveNonStructuralWt*SenescenceRate.Value() * DMReallocationFactor.Value();
                if (availableDM < -BiomassToleranceValue)
                    throw new Exception("Negative DM reallocation value computed for " + Name);
                return availableDM;
            }
            // By default reallocation is turned off!!!!
            return 0.0;
        }

        /// <summary>Sets the dm potential allocation.</summary>
        public override BiomassPoolType DMPotentialAllocation
        {
            set
            {
                if (PlantZone.Uptake == null)
                    throw new Exception("No water and N uptakes supplied to root. Is Soil Arbitrator included in the simulation?");
           
                if (PlantZone.Depth <= 0)
                    return; //cannot allocate growth where no length

                if (DMDemand.Structural == 0 && value.Structural > 0.000000000001)
                    throw new Exception("Invalid allocation of potential DM in" + Name);
                

                double TotalRAw = 0;
                foreach (ZoneState Z in Zones)
                    TotalRAw += MathUtilities.Sum(Z.CalculateRootActivityValues());

                if (TotalRAw==0 && value.Structural>0)
                    throw new Exception("Error trying to partition potential root biomass");

                if (TotalRAw > 0)
                {
                    foreach (ZoneState Z in Zones)
                    {
                        double[] RAw = Z.CalculateRootActivityValues();
                        for (int layer = 0; layer < Z.soil.Thickness.Length; layer++)
                            Z.LayerLive[layer].PotentialDMAllocation = value.Structural * RAw[layer] / TotalRAw;
                    }
                }
            }
        }

        /// <summary>Sets the dm allocation.</summary>
        public override BiomassAllocationType DMAllocation
        {
            set
            {
                double TotalRAw = 0;
                foreach (ZoneState Z in Zones)
                    TotalRAw += MathUtilities.Sum(Z.CalculateRootActivityValues());

                Allocated.StructuralWt = value.Structural;
                Allocated.NonStructuralWt = value.NonStructural;
                Allocated.MetabolicWt = value.Metabolic;

                if (TotalRAw==0 && Allocated.Wt>0)
                    throw new Exception("Error trying to partition root biomass");

                foreach (ZoneState Z in Zones)
                    Z.PartitionRootMass(TotalRAw, Allocated.Wt);

            }
        }

        /// <summary>Gets the N demand for this computation round.</summary>
        [Units("g/m2")]
        public override BiomassPoolType NDemand
        {
            get
            {
                DoNDemandCalculations();
                return new BiomassPoolType
                {
                    Structural = StructuralNDemand,
                    Metabolic = MetabolicNDemand,
                    NonStructural = NonStructuralNDemand
                };
            }
        }

        /// <summary>Computes the N demanded for this organ.</summary>
        /// <remarks>
        /// This is basic the old/original function. with added metabolicN
        /// Calculate N demand based on amount of N needed to bring root N content in each layer up to maximum
        /// </remarks>
        private void DoNDemandCalculations()
        {
            double NitrogenSwitch = (NitrogenDemandSwitch == null) ? 1.0 : NitrogenDemandSwitch.Value();
            double criticalN = (CriticalNConc == null) ? MinimumNConc.Value() : CriticalNConc.Value();

            StructuralNDemand = 0.0;
            MetabolicNDemand = 0.0;
            NonStructuralNDemand = 0.0;
            foreach (ZoneState Z in Zones)
            {
                Z.StructuralNDemand = new double[Z.soil.Thickness.Length];
                Z.NonStructuralNDemand = new double[Z.soil.Thickness.Length];
                //Note: MetabolicN is assumed to be zero

                double NDeficit = 0.0;
                for (int i = 0; i < Z.LayerLive.Length; i++)
                {
                    Z.StructuralNDemand[i] = Z.LayerLive[i].PotentialDMAllocation * MinimumNConc.Value() * NitrogenSwitch;
                    NDeficit = Math.Max(0.0, MaximumNConc.Value() * (Z.LayerLive[i].Wt + Z.LayerLive[i].PotentialDMAllocation) - (Z.LayerLive[i].N + Z.StructuralNDemand[i]));
                    Z.NonStructuralNDemand[i] = Math.Max(0, NDeficit - Z.StructuralNDemand[i]) * NitrogenSwitch;

                    StructuralNDemand += Z.StructuralNDemand[i];
                    NonStructuralNDemand += Z.NonStructuralNDemand[i];
                }
            }
        }


        /// <summary>Gets the N supply for this computation round.</summary>
        [XmlIgnore]
        public override BiomassSupplyType NSupply
        {
            get
            {
                return new BiomassSupplyType()
                {
                    Fixation = 0.0,
                    Uptake = 0.0, // computed via arbitrator
                    Retranslocation = NRetranslocationSupply,
                    Reallocation = NReallocationSupply
                };
            }
        }

        /// <summary>Computes the N amount available for retranslocation.</summary>
        /// <remarks>This is limited to ensure Nconc does not go below MinimumNConc</remarks>
        public double AvailableNRetranslocation()
        {
            if (NRetranslocationFactor != null)
            {
                double labileN = 0.0;
                foreach (ZoneState Z in Zones)
                    for (int i = 0; i < Z.LayerLive.Length; i++)
                        labileN += Math.Max(0.0, Z.LayerLive[i].NonStructuralN - Z.LayerLive[i].NonStructuralWt * MinimumNConc.Value());

                double availableN = Math.Max(0.0, labileN - NReallocationSupply) * NRetranslocationFactor.Value();
                if (availableN < -BiomassToleranceValue)
                    throw new Exception("Negative N retranslocation value computed for " + Name);

                return availableN;
            }
            else
            {  // By default retranslocation is turned off!!!!
                return 0.0;
            }
        }

        /// <summary>Computes the N amount available for reallocation.</summary>
        public double AvailableNReallocation()
        {
            if (NReallocationFactor != null)
            {
                double rootLiveNonStructuralN = 0.0;
                foreach (ZoneState Z in Zones)
                    for (int i = 0; i < Z.LayerLive.Length; i++)
                        rootLiveNonStructuralN += Z.LayerLive[i].NonStructuralN;

                double availableN = rootLiveNonStructuralN * SenescenceRate.Value() * NReallocationFactor.Value();
                if (availableN < -BiomassToleranceValue)
                    throw new Exception("Negative N reallocation value computed for " + Name);

                return availableN;
            }
            else
            {  // By default reallocation is turned off!!!!
                return 0.0;
            }
        }

        /// <summary>Gets the nitrogen supply from the specified zone.</summary>
        /// <param name="zone">The zone.</param>
        /// <param name="NO3Supply">The returned NO3 supply</param>
        /// <param name="NH4Supply">The returned NH4 supply</param>
        public void CalculateNitrogenSupply(ZoneWaterAndN zone, ref double[] NO3Supply, ref double[] NH4Supply)
        {

            ZoneState myZone = Zones.Find(z => z.Name == zone.Zone.Name);
            if (myZone != null)
            {
                if (RWC == null || RWC.Length != myZone.soil.Thickness.Length)
                    RWC = new double[myZone.soil.Thickness.Length];
                double NO3Uptake = 0;
                double NH4Uptake = 0;
                for (int layer = 0; layer < myZone.soil.Thickness.Length; layer++)
                {
                    if (myZone.LayerLive[layer].Wt > 0)
                    {
                        RWC[layer] = (myZone.soil.Water[layer] - myZone.soil.SoilWater.LL15mm[layer]) / (myZone.soil.SoilWater.DULmm[layer] - myZone.soil.SoilWater.LL15mm[layer]);
                        RWC[layer] = Math.Max(0.0, Math.Min(RWC[layer], 1.0));
                        double SWAF = NUptakeSWFactor.Value(layer);

                        double kno3 = KNO3.Value(layer);
                        double NO3ppm = zone.NO3N[layer] * (100.0 / (myZone.soil.BD[layer] * myZone.soil.Thickness[layer]));
                        NO3Supply[layer] = Math.Min(zone.NO3N[layer] * kno3 * NO3ppm * SWAF, (MaxDailyNUptake.Value() - NO3Uptake));
                        NO3Uptake += NO3Supply[layer];

                        double knh4 = KNH4.Value(layer);
                        double NH4ppm = zone.NH4N[layer] * (100.0 / (myZone.soil.BD[layer] * myZone.soil.Thickness[layer]));
                        NH4Supply[layer] = Math.Min(zone.NH4N[layer] * knh4 * NH4ppm * SWAF, (MaxDailyNUptake.Value() - NH4Uptake));
                        NH4Uptake += NH4Supply[layer];
                    }
                }
            }
        }

        /// <summary>Sets the n allocation.</summary>
        public override BiomassAllocationType NAllocation
        {
            set
            {
                double totalStructuralNDemand = 0;
                double totalNDemand = 0;

                foreach (ZoneState Z in Zones)
                {
                    totalStructuralNDemand += MathUtilities.Sum(Z.StructuralNDemand);
                    totalNDemand += MathUtilities.Sum(Z.StructuralNDemand) + MathUtilities.Sum(Z.NonStructuralNDemand);
                }
                NTakenUp = value.Uptake;
                Allocated.StructuralN = value.Structural;
                Allocated.NonStructuralN = value.NonStructural;
                Allocated.MetabolicN = value.Metabolic;

                double surplus = Allocated.N - totalNDemand;
                if (surplus > 0.000000001)
                    throw new Exception("N Allocation to roots exceeds Demand");
                double NAllocated = 0;

                foreach (ZoneState Z in Zones)
                {
                    for (int i = 0; i < Z.LayerLive.Length; i++)
                    {
                        if (totalStructuralNDemand > 0)
                        {
                            double StructFrac = Z.StructuralNDemand[i] / totalStructuralNDemand;
                            Z.LayerLive[i].StructuralN += value.Structural * StructFrac;
                            NAllocated += value.Structural * StructFrac;
                        }
                        double totalNonStructuralNDemand = MathUtilities.Sum(Z.NonStructuralNDemand);
                        if (totalNonStructuralNDemand > 0)
                        {
                            double NonStructFrac = Z.NonStructuralNDemand[i] / totalNonStructuralNDemand;
                            Z.LayerLive[i].NonStructuralN += value.NonStructural * NonStructFrac;
                            NAllocated += value.NonStructural * NonStructFrac;
                        }
                    }
                }

                if (!MathUtilities.FloatsAreEqual(NAllocated - Allocated.N, 0.0))
                    throw new Exception("Error in N Allocation: " + Name);

            }
        }

        /// <summary>Gets or sets the minimum nconc.</summary>
        public override double MinNconc { get { return MinimumNConc.Value(); } }

        /// <summary>Gets or sets the water supply.</summary>
        /// <param name="zone">The zone.</param>
        public double[] CalculateWaterSupply(ZoneWaterAndN zone)
        {
            ZoneState myZone = Zones.Find(z => z.Name == zone.Zone.Name);
            if (myZone == null)
                return null;

            double[] supply = new double[myZone.soil.Thickness.Length];
            LayerMidPointDepth = Soil.ToMidPoints(myZone.soil.Thickness);
            for (int layer = 0; layer < myZone.soil.Thickness.Length; layer++)
            {
                if (layer <= Soil.LayerIndexOfDepth(myZone.Depth, myZone.soil.Thickness))
                {
                    if (myZone.soil.Weirdo == null)
                    {
                        supply[layer] = Math.Max(0.0, myZone.soil.KL(Plant.Name)[layer] * KLModifier.Value(layer) *
                            (zone.Water[layer] - myZone.soil.LL(Plant.Name)[layer] * myZone.soil.Thickness[layer]) * Soil.ProportionThroughLayer(layer, myZone.Depth, myZone.soil.Thickness));
                    }
                    else supply[layer] = 0; //With Weirdo, water extraction is not done through the arbitrator because the time step is different.
                }
            }

            return supply;
        }

        #endregion

        #region Biomass Removal
        /// <summary>Removes biomass from root layers when harvest, graze or cut events are called.</summary>
        /// <param name="biomassRemoveType">Name of event that triggered this biomass remove call.</param>
        /// <param name="removal">The fractions of biomass to remove</param>
        public override void DoRemoveBiomass(string biomassRemoveType, OrganBiomassRemovalType removal)
        {
            biomassRemovalModel.RemoveBiomassToSoil(biomassRemoveType, removal, PlantZone.LayerLive, PlantZone.LayerDead, Removed, Detached);
        }
        #endregion
    }
}