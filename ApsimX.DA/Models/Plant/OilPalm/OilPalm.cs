﻿using System;
using System.Collections.Generic;
using System.Text;
using Models.Core;

using System.Reflection;
using System.Collections;
using Models.PMF.Functions;
using Models.Soils;
using System.Xml.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Models.Soils.Arbitrator;
using Models.Interfaces;
using APSIM.Shared.Utilities;


namespace Models.PMF.OilPalm
{
    /// <summary>
    /// An oil palm model
    /// </summary>
    [Serializable]
    [ViewName("UserInterface.Views.GridView")]
    [PresenterName("UserInterface.Presenters.PropertyPresenter")]
    [ValidParent(ParentType = typeof(Zone))]
    public class OilPalm : ModelCollectionFromResource, ICrop, ICanopy, IUptake
    {
        #region Canopy interface
        /// <summary>Canopy type</summary>
        public string CanopyType { get { return "OilPalm"; } }

        /// <summary>Albedo.</summary>
        public double Albedo { get { return 0.15; } }

        /// <summary>Gets or sets the gsmax.</summary>
        public double Gsmax { get { return 0.01; } }

        /// <summary>Gets or sets the R50.</summary>
        public double R50 { get { return 200; } }

        /// <summary>Gets the lai.</summary>
        /// <value>The lai.</value>
        [Description("Leaf Area Index")]
        [Units("m^2/m^2")]
        public double LAI
        {
            get
            {
                if (CropInGround)
                {
                    double FrondArea = 0.0;
                    foreach (FrondType F in Fronds)
                        FrondArea += F.Area;
                    return FrondArea * SowingData.Population;
                }
                else
                    return 0;
            }

        }
        
        /// <summary>Gets the maximum LAI (m^2/m^2)</summary>
        public double LAITotal { get { return LAI; } }

        /// <summary>Gets the cover green (0-1)</summary>
        public double CoverGreen { get { return cover_green; } }

        /// <summary>Gets the cover total (0-1)</summary>
        public double CoverTotal { get { return cover_tot; } }

        /// <summary>Gets the canopy height (mm)</summary>
        public double Height { get { return 10000; } }

        /// <summary>Gets the canopy depth (mm)</summary>
        public double Depth { get { return 10000; } }

        /// <summary>Gets the LAI (m^2/m^2)</summary>
        [Units("0-1")]
        public double FRGR { get { return 1; } }

        /// <summary>Potential evapotranspiration</summary>
        [XmlIgnore]
        [Units("mm")]
        public double PotentialEP { get; set; }

        /// <summary>MicroClimate supplies LightProfile</summary>
        [XmlIgnore]
        public CanopyEnergyBalanceInterceptionlayerType[] LightProfile { get; set; }

        #endregion

        /// <summary>
        /// Is the plant alive?
        /// </summary>
        public bool IsAlive
        {
            get { return plant_status == "alive"; }
        }

        /// <summary>Returns true if the crop is ready for harvesting</summary>
        public bool IsReadyForHarvesting { get { return false; } }

        /// <summary>End the crop</summary>
        public void EndCrop() { }

        /// <summary>The plant_status</summary>
        [XmlIgnore]
        public string plant_status = "out";
        /// <summary>The clock</summary>
        [Link]
        Clock Clock = null;
        /// <summary>The met data</summary>
        [Link]
        IWeather MetData = null;
        /// <summary>The soil</summary>
        [Link]
        Soils.Soil Soil = null;
        /// <summary>The summary</summary>
        [Link]
        ISummary Summary = null;

        /// <summary>Link to Apsim's solute manager module.</summary>
        [Link]
        private SoluteManager solutes = null;


        /// <summary>The soil crop</summary>
        private SoilCropOilPalm soilCrop;
        /// <summary>The cultivar definition</summary>
        private Cultivar cultivarDefinition;

        /// <summary>Gets a list of cultivar names</summary>
        public string[] CultivarNames
        {
            get
            {
                SortedSet<string> cultivarNames = new SortedSet<string>();
                foreach (Cultivar cultivar in this.Cultivars)
                {
                    cultivarNames.Add(cultivar.Name);
                    if (cultivar.Aliases != null)
                    {
                        foreach (string alias in cultivar.Aliases)
                            cultivarNames.Add(alias);
                    }
                }

                return new List<string>(cultivarNames).ToArray();
            }
        }

        /// <summary>Gets a list of all cultivar definitions.</summary>
        /// <value>The cultivars.</value>
        private List<Cultivar> Cultivars
        {
            get
            {
                List<Cultivar> cultivars = new List<Cultivar>();
                foreach (Model model in Apsim.Children(this, typeof(Cultivar)))
                {
                    cultivars.Add(model as Cultivar);
                }

                return cultivars;
            }
        }

        /// <summary>Height to top of plant canopy</summary>
        [XmlIgnore]
        [Units("mm")]
        public double height = 10000.0;

        /// <summary>Total cover provided by plant canopies</summary>
        /// <value>The cover_tot.</value>
        [Units("0-1")]
        public double cover_tot {
            get { return cover_green + (1 - cover_green) * UnderstoryCoverGreen; }
                }

        /// <summary>Amount of rainfall intercepted by the plant canopy</summary>
        [Units("mm")]
        public double interception = 0.0;


        /// <summary>Gets or sets the understory cover maximum.</summary>
        /// <value>The understory cover maximum.</value>
        [Description("Maximum understory cover (0-1)")]
        [Units("0-1")]
        public double UnderstoryCoverMax { get; set; }
        /// <summary>Gets or sets the understory legume fraction.</summary>
        /// <value>The understory legume fraction.</value>
        [Description("Fraction of understory that is legume (0-1)")]
        [Units("0-1")]
        public double UnderstoryLegumeFraction { get; set; }
        /// <summary>Gets or sets the interception fraction.</summary>
        /// <value>The interception fraction.</value>
        [Description("Fraction of rainfall intercepted by canopy (0-1)")]
        [Units("0-1")]
        public double InterceptionFraction { get; set; }
        /// <summary>Gets or sets the maximum root depth.</summary>
        /// <value>The maximum root depth.</value>
        [Description("Maximum palm root depth (mm)")]
        [Units("mm")]
        public double MaximumRootDepth { get; set; }

        /// <summary>The ndemand</summary>
        double Ndemand = 0.0;

        /// <summary>Palm Rooting Depth</summary>
        /// <value>The root depth.</value>
        [Units("mm")]
        public double RootDepth {get; set;}

        /// <summary>The pot sw uptake</summary>
        double[] PotSWUptake;

        /// <summary>The sw uptake</summary>
        double[] SWUptake;

        /// <summary>Potential daily evapotranspiration for the palm canopy</summary>
        /// <value>The pep.</value>
        [XmlIgnore]
        [Units("mm")]
        public double PEP { get; set; }

        /// <summary>Daily evapotranspiration from the palm canopy</summary>
        /// <value>The ep.</value>
        [XmlIgnore]
        [Units("mm")]
        public double EP { get; set; }

        /// <summary>Daily total plant dry matter growth</summary>
        /// <value>The DLT dm.</value>
        [Units("g/m2")]
        public double DltDM { get; set; }
        /// <summary>The excess</summary>
        double Excess = 0.0;

        /// <summary>Factor for daily water stress effect on photosynthesis</summary>
        /// <value>The fw.</value>
        [XmlIgnore]
        [Units("0-1")]
        public double FW { get; set; }

        /// <summary>Factor for daily water stress effect on canopy expansion</summary>
        [Units("0-1")]
        double FWexpan = 0.0;

        /// <summary>Factor for daily VPD effect on photosynthesis</summary>
        [XmlIgnore]
        [Units("0-1")]
        public double Fvpd { get; set; }


        /// <summary>Factor for daily nitrogen stress effect on photosynthesis</summary>
        /// <value>The function.</value>
        [XmlIgnore]
        [Units("0-1")]
        public double Fn { get; set; }

        /// <summary>Cumulative frond production since planting</summary>
        /// <value>The cumulative frond number.</value>
        [XmlIgnore]
        [Units("/palm")]
        public double CumulativeFrondNumber { get; set; }

        /// <summary>Cumulative bunch production since planting</summary>
        /// <value>The cumulative bunch number.</value>
        [XmlIgnore]
        [Units("/palm")]
        public double CumulativeBunchNumber { get; set; }

        /// <summary>Proportion of daily growth partitioned into reproductive parts</summary>
        /// <value>The reproductive growth fraction.</value>
        [Units("0-1")]
        public double ReproductiveGrowthFraction {get; set;}

        /// <summary>Amount of carbon limitation for todays potential growth (ie supply/demand)</summary>
        /// <value>The carbon stress.</value>
        [XmlIgnore]
        [Units("0-1")]
        public double CarbonStress { get; set; }

        /// <summary>Number of bunches harvested on a harvesting event</summary>
        /// <value>The harvest bunches.</value>
        [XmlIgnore]
        [Units("/palm")]
        public double HarvestBunches { get; set; }

        /// <summary>Mass of harvested FFB on a harvesting event</summary>
        /// <value>The harvest FFB.</value>
        [XmlIgnore]
        [Units("t/ha")]
        public double HarvestFFB { get; set; }

        /// <summary>Nitrogen removed at a harvesting event</summary>
        /// <value>The harvest n removed.</value>
        [XmlIgnore]
        [Units("kg/ha")]
        public double HarvestNRemoved { get; set; }

        /// <summary>Mean size of bunches at a harvesting event</summary>
        /// <value>The size of the harvest bunch.</value>
        [XmlIgnore]
        [Units("kg")]
        public double HarvestBunchSize { get; set; }

        /// <summary>Time since planting</summary>
        /// <value>The age.</value>
        [XmlIgnore]
        [Units("y")]
        public double Age { get; set; }

        /// <summary>Gets or sets the population.</summary>
        /// <value>The population.</value>
        [XmlIgnore]
        [Units("/m^2")]
        public double Population { get; set; }

        /// <summary>The sowing data</summary>
        [XmlIgnore]
        public SowPlant2Type SowingData = new SowPlant2Type();

        /// <summary>Potential daily nitrogen uptake from each soil layer by palms</summary>
        [Units("kg/ha")]
        double[] PotNUptake;

        /// <summary>Daily nitrogen uptake from each soil layer by palms</summary>
        /// <value>The n uptake.</value>
        [XmlIgnore]
        [Units("kg/ha")]
        public double[] NUptake { get; set; }

        /// <summary>Daily stem dry matter growth</summary>
        /// <value>The stem growth.</value>
        [XmlIgnore]
        [Units("g/m^2")]
        public double StemGrowth { get; set; }
        /// <summary>Daily frond dry matter growth</summary>
        /// <value>The frond growth.</value>
        [XmlIgnore]
        [Units("g/m^2")]
        public double FrondGrowth { get; set; }
        /// <summary>Daily root dry matter growth</summary>
        /// <value>The root growth.</value>
        [XmlIgnore]
        [Units("g/m^2")]
        public double RootGrowth { get; set; }
        /// <summary>Daily bunch dry matter growth</summary>
        /// <value>The bunch growth.</value>
        [XmlIgnore]
        [Units("g/m^2")]
        public double BunchGrowth { get; set; }

        /// <summary>The fronds</summary>
        [XmlIgnore]
        private List<FrondType> Fronds = new List<FrondType>();
        /// <summary>The bunches</summary>
        [XmlIgnore]
        private List<BunchType> Bunches = new List<BunchType>();
        /// <summary>The roots</summary>
        [XmlIgnore]
        private List<RootType> Roots = new List<RootType>();

        /// <summary>The frond appearance rate</summary>
        [Link]
        [Description("This function returns the frond appearance rate under optimal temperature conditions.")]
        [Units("d")]
        IFunction FrondAppearanceRate = null;
        /// <summary>The relative developmental rate</summary>
        [Link]
        [Description("This function returns the relative rate of plant development (e.g. frond appearance) as affected by air temperature.")]
        [Units("0-1")]
        IFunction RelativeDevelopmentalRate = null;
        /// <summary>The frond maximum area</summary>
        [Link]
        [Description("This function returns the maximum area of an individual frond.")]
        [Units("m^2")]
        IFunction FrondMaxArea = null;
        /// <summary>The direct extinction coeff</summary>
        [Link]
        [Description("This function returns the Beer-Lambert law extinction coefficient for direct beam radiation.")]
        [Units("unitless")]
        IFunction DirectExtinctionCoeff = null;
        /// <summary>The diffuse extinction coeff</summary>
        [Link]
        [Description("This function returns the Beer-Lambert law extinction coefficient for diffuse beam radiation.")]
        [Units("unitless")]
        IFunction DiffuseExtinctionCoeff = null;
        /// <summary>The expanding fronds</summary>
        [Link]
        [Description("This function returns the number of expanding fronds at a given point in time.")]
        [Units("/palm")]
        IFunction ExpandingFronds = null;
        /// <summary>The initial frond number</summary>
        [Link]
        [Description("This function returns the number of fronds on the palm at planting.")]
        [Units("/palm")]
        IFunction InitialFrondNumber = null;
        /// <summary>The rue</summary>
        [Link]
        [Description("This function returns the radiation use efficiency for total short wave radiation.")]
        [Units("g/m^2")]
        IFunction RUE = null;
        /// <summary>The root front velocity</summary>
        [Link]
        [Description("This function returns the root front velocity, that is the vertical rate of root front advance.")]
        [Units("mm/d")]
        IFunction RootFrontVelocity = null;
        /// <summary>The root senescence rate</summary>
        [Link]
        [Description("This function returns the fraction of the live root system that senesces per day (ie first order decay coefficient).")]
        [Units("/d")]
        IFunction RootSenescenceRate = null;
        /// <summary>The specific leaf area</summary>
        [Link]
        [Description("This function returns the amount of frond area per unit frond mass. This is used to calculate frond dry matter demand.")]
        [Units("m^2/g")]
        IFunction SpecificLeafArea = null;
        /// <summary>The specific leaf area maximum</summary>
        [Link]
        [Description("This function returns the maximum amount of frond area per unit frond mass. Used to limit area growth when dry matter is limiting.")]
        [Units("m^2/g")]
        IFunction SpecificLeafAreaMax = null;
        /// <summary>The root fraction</summary>
        [Link]
        [Description("This function returns the fraction of daily growth partitioned into the root system.")]
        [Units("0-1")]
        IFunction RootFraction = null;
        /// <summary>The bunch size maximum</summary>
        [Link]
        [Description("This function returns the maximum bunch size on a dry mass basis.")]
        [Units("g")]
        IFunction BunchSizeMax = null;
        /// <summary>The female flower fraction</summary>
        [Link]
        [Description("This function returns the female fraction of a cohort's population of inflorescences as affected by age.")]
        [Units("0-1")]
        IFunction FemaleFlowerFraction = null;
        /// <summary>The FFF stress impact</summary>
        [Link]
        [Description("This function returns the fraction of inflorescences that become female each day during the gender determination phase.")]
        [Units("0-1")]
        IFunction FFFStressImpact = null;
        /// <summary>The stem to frond fraction</summary>
        [Link]
        [Description("This function returns the ratio of stem to frond growth as affected by plant age.")]
        [Units("g/g")]
        IFunction StemToFrondFraction = null;
        /// <summary>The flower abortion fraction</summary>
        [Link]
        [Description("This function returns the fraction of inflorescences that become aborted each day during the flower abortion phase.")]
        [Units("0-1")]
        IFunction FlowerAbortionFraction = null;
        /// <summary>The bunch failure fraction</summary>
        [Link]
        [Description("This function returns the fraction of bunches that fail each day during the bunch failure phase.")]
        [Units("0-1")]
        IFunction BunchFailureFraction = null;

        /// <summary>The initial root depth</summary>
        private double InitialRootDepth = 300;

        /// <summary>The kn o3</summary>
        [Link]
        [Description("This function describes the NO3 uptake coefficient for a simple second-order decay.  Its value represents the fraction of NO3 available at a soil concentration of 1ppm. ")]
        [Units("/ppm")]
        IFunction KNO3 = null;

        /// <summary>The stem n concentration</summary>
        [Link]
        [Description("This function returns the stem nitrogen concentration on dry mass basis.")]
        [Units("%")]
        IFunction StemNConcentration = null;
        /// <summary>The bunch n concentration</summary>
        [Link]
        [Description("This function returns the bunch nitrogen concentration on dry mass basis.")]
        [Units("%")]
        IFunction BunchNConcentration = null;
        /// <summary>The root n concentration</summary>
        [Link]
        [Description("This function returns the root nitrogen concentration on dry mass basis.")]
        [Units("%")]
        IFunction RootNConcentration = null;
        /// <summary>The bunch oil conversion factor</summary>
        [Link]
        [Description("This function returns the conversion factor to convert carbohydrate to bunch dry mass to account for oil content.")]
        [Units("g/g")]
        IFunction BunchOilConversionFactor = null;
        /// <summary>The ripe bunch water content</summary>
        [Link] 
        [Description("This function returns the fractional contribution of water to fresh bunch mass.")]
        [Units("g/g")]
        IFunction RipeBunchWaterContent = null;
        /// <summary>The harvest frond number</summary>
        [Link]
        [Description("This function returns the frond number removed when bunches are ready for harvest.  This is used to determine harvest time.")]
        [Units("/palm")]
        IFunction HarvestFrondNumber = null;
        /// <summary>The frond maximum n concentration</summary>
        [Link]
        [Description("This function returns the maximum frond nitrogen concentration on dry mass basis.")]
        [Units("%")]
        IFunction FrondMaximumNConcentration = null;
        /// <summary>The frond critical n concentration</summary>
        [Link]
        [Description("This function returns the critical frond nitrogen concentration on dry mass basis.")]
        [Units("%")]
        IFunction FrondCriticalNConcentration = null;
        /// <summary>The frond minimum n concentration</summary>
        [Link]
        [Description("This function returns the minimum frond nitrogen concentration on dry mass basis.")]
        [Units("%")]
        IFunction FrondMinimumNConcentration = null;

        /// <summary>Proportion of green cover provided by the understory canopy</summary>
        /// <value>The understory cover green.</value>
        [Units("0-1")]
        public double UnderstoryCoverGreen { get; set; }
        /// <summary>The understory k lmax</summary>
        private double UnderstoryKLmax = 0.12;

        /// <summary>Potential soil water uptake from each soil layer by understory</summary>
        double[] UnderstoryPotSWUptake;

        /// <summary>Actual Soil water uptake from each soil layer by understory</summary>
        double[] UnderstorySWUptake;
        /// <summary>Potential nitrogen water uptake from each soil layer by understory</summary>
        /// <value>The understory pot n uptake.</value>
        public double[] UnderstoryPotNUptake { get; set; }

        /// <summary>Actual soil nitrogen uptake from each soil layer by understory</summary>
        /// <value>The understory n uptake.</value>
        [XmlIgnore]
        public double[] UnderstoryNUptake { get; set; }

        /// <summary>Understory rooting depth</summary>
        [XmlIgnore]
        [Units("mm")]
        public double UnderstoryRootDepth = 500;

        /// <summary>Potential daily evapotranspiration for the understory</summary>
        [XmlIgnore]
        [Units("mm")]
        public double UnderstoryPEP { get; set; }

        /// <summary>Daily evapotranspiration for the understory</summary>
        [XmlIgnore]
        [Units("mm")]
        public double UnderstoryEP {get;set;}

        /// <summary>Understory plant water stress factor</summary>
        [XmlIgnore]
        [Units("0-1")]
        public double UnderstoryFW {get;set;}

        /// <summary>Daily understory dry matter growth</summary>
        [XmlIgnore]
        [Units("g/m^2")]
        public double UnderstoryDltDM{get;set;}

        /// <summary>Daily understory nitrogen fixation</summary>
        /// <value>The understory n fixation.</value>
        [XmlIgnore]
        [Units("kg/ha")]
        public double UnderstoryNFixation { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public class RootType
        {
            /// <summary>The mass</summary>
            public double Mass = 0;
            /// <summary>The n</summary>
            public double N = 0;
            /// <summary>The length</summary>
            public double Length = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public class FrondType
        {
            /// <summary>The mass</summary>
            public double Mass; // g/frond
            /// <summary>The n</summary>
            public double N;    // g/frond
            /// <summary>The area</summary>
            public double Area; // m2/frond
            /// <summary>The age</summary>
            public double Age;  //days
        }

        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public class BunchType
        {
            /// <summary>The mass</summary>
            public double Mass = 0;
            /// <summary>The n</summary>
            public double N = 0;
            /// <summary>The age</summary>
            public double Age = 0;
            /// <summary>The female fraction</summary>
            public double FemaleFraction = 1;
            /// <summary>Duration of Bunch Filling</summary>
            public double FillDuration = 0;
        }


        /// <summary>Gets or sets the stem mass.</summary>
        /// <value>The stem mass.</value>
        [XmlIgnore]
        //[Description("Stem mass on a dry matter basis")]
        [Units("g/m^2")]
        public double StemMass { get; set; }

        /// <summary>Gets or sets the stem n.</summary>
        /// <value>The stem n.</value>
        [XmlIgnore]
        //[Description("Stem nitrogen")]
        [Units("g/m^2")]
        public double StemN { get; set; }

        /// <summary>Gets the stem n conc.</summary>
        /// <value>The stem n conc.</value>
        [XmlIgnore]
        [Description("Stem nitrogen concention on a dry mass basis")]
        [Units("%")]
        double StemNConc
        {
            get
            {
                if (StemMass > 0)
                    return StemN / StemMass * 100;
                else
                    return 0.0;
            }
        }

        /// <summary>The crop in ground</summary>
        private bool CropInGround = false;

        //[Description("Flag to indicate whether oil palm has been planted")]
        /// <summary>Gets or sets a value indicating whether this instance is crop in ground.</summary>
        /// <value>
        /// <c>true</c> if this instance is crop in ground; otherwise, <c>false</c>.
        /// </value>
        [Units("True/False")]
        [XmlIgnore]
        public bool IsCropInGround
        {
            get { return CropInGround; }
            set { CropInGround = value; }
        }

        // The following event handler will be called once at the beginning of the simulation
        /// <summary>Called when [simulation commencing].</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("Commencing")]
        private void OnSimulationCommencing(object sender, EventArgs e)
        {
            //zero public properties
            CumulativeFrondNumber = 0;
            CumulativeBunchNumber = 0;
            CarbonStress = 0;
            HarvestBunches = 0;
            HarvestFFB = 0;
            HarvestNRemoved = 0;
            HarvestBunchSize = 0;
            Age = 0;
            Population = 0;
            UnderstoryNFixation = 0;
            UnderstoryDltDM = 0;
            UnderstoryEP = 0;
            UnderstoryPEP = 0;
            UnderstoryFW = 0;
            StemMass = 0;
            StemN = 0;
            CropInGround = false;
            NUptake = new double[] { 0 };
            UnderstoryNUptake = new double[] { 0 };
            UnderstoryCoverGreen = 0;
            StemGrowth = 0;
            RootGrowth = 0;
            FrondGrowth = 0;
            BunchGrowth = 0;
            Fn = 1;
            FW = 1;
            Fvpd = 1;
            PEP = 0;
            EP = 0;
            RootDepth = 0;
            DltDM = 0;
            ReproductiveGrowthFraction = 0;

            Fronds = new List<FrondType>();
            Bunches = new List<BunchType>();
            Roots = new List<RootType>();

            soilCrop = Soil.Crop(Name) as SoilCropOilPalm; 
            
            //MyPaddock.Parent.ChildPaddocks
            PotSWUptake = new double[Soil.Thickness.Length];
            SWUptake = new double[Soil.Thickness.Length];
            PotNUptake = new double[Soil.Thickness.Length];
            NUptake = new double[Soil.Thickness.Length];

            UnderstoryPotSWUptake = new double[Soil.Thickness.Length];
            UnderstorySWUptake = new double[Soil.Thickness.Length];
            UnderstoryPotNUptake = new double[Soil.Thickness.Length];
            UnderstoryNUptake = new double[Soil.Thickness.Length];

            for (int i = 0; i < Soil.Thickness.Length; i++)
            {
                RootType R = new RootType();
                Roots.Add(R);
                Roots[i].Mass = 0.1;
                Roots[i].N = Roots[i].Mass * RootNConcentration.Value() / 100;
            }

            double FMA = FrondMaxArea.Value();
            double GrowthDuration = ExpandingFronds.Value() * FrondAppearanceRate.Value();

            for (int i = 0; i < (int)InitialFrondNumber.Value(); i++)
            {
                FrondType F = new FrondType();
                F.Age = ((int)InitialFrondNumber.Value() - i) * FrondAppearanceRate.Value();
                F.Area = SizeFunction(F.Age, FMA, GrowthDuration);
                F.Mass = F.Area / SpecificLeafArea.Value();
                F.N = F.Mass * FrondCriticalNConcentration.Value() / 100.0;
                Fronds.Add(F);
                CumulativeFrondNumber += 1;
            }
            for (int i = 0; i < (int)InitialFrondNumber.Value() + 60; i++)
            {
                BunchType B = new BunchType();
                if (i>40) 
                   B.FemaleFraction =  FemaleFlowerFraction.Value();
                else
                    B.FemaleFraction = 0;

                Bunches.Add(B);
            }
            RootDepth = InitialRootDepth;
        }

        /// <summary>Sows the specified cultivar.</summary>
        /// <param name="cultivar">The cultivar.</param>
        /// <param name="population">The population.</param>
        /// <param name="depth">The depth.</param>
        /// <param name="rowSpacing">The row spacing.</param>
        /// <param name="maxCover">The maximum cover.</param>
        /// <param name="budNumber">The bud number.</param>
        /// <exception cref="System.Exception">Cultivar not specified on sow line.</exception>
        public void Sow(string cultivar, double population, double depth, double rowSpacing, double maxCover = 1, double budNumber = 1)
        {
            SowingData = new SowPlant2Type();
            SowingData.Population = population;
            this.Population = population;
            SowingData.Depth = depth;
            SowingData.Cultivar = cultivar;
            SowingData.MaxCover = maxCover;
            SowingData.BudNumber = budNumber;
            SowingData.RowSpacing = rowSpacing;
            CropInGround = true;

            if (SowingData.Cultivar == "")
                throw new Exception("Cultivar not specified on sow line.");

            // Find cultivar and apply cultivar overrides.
            cultivarDefinition = Cultivar.Find(Cultivars, SowingData.Cultivar);
            cultivarDefinition.Apply(this);

            // Invoke a sowing event.
            if (Sowing != null)
                Sowing.Invoke(this, new EventArgs());

            Summary.WriteMessage(this, string.Format("A crop of "+SowingData.Cultivar+" OilPalm was sown today at a population of " + population + " plants/m2 with " + budNumber + " buds per plant at a row spacing of " + rowSpacing + " and a depth of " + depth + " mm"));
        }

        /// <summary>Harvest the crop.</summary>
        public void Harvest()
        {
            // Invoke a harvesting event.
            if (Harvesting != null)
                Harvesting.Invoke(this, new EventArgs());
        }

        /// <summary>Occurs when [sowing].</summary>
        public event EventHandler Sowing;

        /// <summary>Occurs when [harvesting].</summary>
        public event EventHandler Harvesting;

        /// <summary>Occurs when [incorp fom].</summary>
        public event FOMLayerDelegate IncorpFOM;

        /// <summary>Occurs when [biomass removed].</summary>
        public event BiomassRemovedDelegate BiomassRemoved;

        /// <summary>Called when [sow].</summary>
        /// <param name="Sow">The sow.</param>
        [EventSubscribe("Sow")]
        private void OnSow(SowPlant2Type Sow)
        {
            SowingData = Sow;
            plant_status = "alive";
            Population = SowingData.Population;

            if (Sowing != null)
                Sowing.Invoke(this, new EventArgs());
        }

        // The following event handler will be called each day at the beginning of the day
        /// <summary>Called when [do daily initialisation].</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("DoDailyInitialisation")]
        private void OnDoDailyInitialisation(object sender, EventArgs e)
        {
            interception = MetData.Rain * InterceptionFraction;
        }

        /// <summary>Called when [do plant growth].</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("DoPlantGrowth")]
        private void OnDoPlantGrowth(object sender, EventArgs e)
        {
            if (!CropInGround)
                return;

            DoWaterBalance();
            DoGrowth();
            DoNBalance();
            DoDevelopment();
            DoFlowerAbortion();
            DoGenderDetermination();
            DoUnderstory();
        }

        /// <summary>Placeholder for SoilArbitrator</summary>
        /// <param name="soilstate">soil state</param>
        /// <returns></returns>
        public List<ZoneWaterAndN> GetSWUptakes(SoilState soilstate)
        {
            throw new NotImplementedException();
        }

        /// <summary>Placeholder for SoilArbitrator</summary>
        /// <param name="soilstate">soil state</param>
        /// <returns></returns>
        public List<ZoneWaterAndN> GetNUptakes(SoilState soilstate)
        {
            throw new NotImplementedException();

        }

        /// <summary>
        /// Set the sw uptake for today
        /// </summary>
        public void SetSWUptake(List<ZoneWaterAndN> info)
        { }
        /// <summary>
        /// Set the n uptake for today
        /// </summary>
        public void SetNUptake(List<ZoneWaterAndN> info)
        { }

        /// <summary>Does the flower abortion.</summary>
        private void DoFlowerAbortion()
        {
            // Main abortion stage occurs around frond 11 over 3 plastochrons

            int B = Fronds.Count - 11;
            if (B > 0)
            {
                double AF = (1 - FlowerAbortionFraction.Value());
                Bunches[B - 1].FemaleFraction *= AF;
                Bunches[B].FemaleFraction *= AF;
                Bunches[B + 1].FemaleFraction *= AF;
            }

            // Bunch failure stage occurs around frond 21 over 1 plastochron
            B = Fronds.Count - 21;
            if (B > 0)
            {
                double BFF = (1 - BunchFailureFraction.Value());
                Bunches[B].FemaleFraction *= BFF;
            }

        }

        /// <summary>Does the gender determination.</summary>
        private void DoGenderDetermination()
        {
            // Main abortion stage occurs 25 plastochroons before spear leaf over 9 plastochrons
            // NH Try 20 as this allows for 26 per year and harvest at 32 - ie 26*2 - 32
            int B = 53; //Fronds.Count + 20;
            Bunches[B - 4].FemaleFraction *= (1.0 - FFFStressImpact.Value());
            Bunches[B - 3].FemaleFraction *= (1.0 - FFFStressImpact.Value());
            Bunches[B - 2].FemaleFraction *= (1.0 - FFFStressImpact.Value());
            Bunches[B - 1].FemaleFraction *= (1.0 - FFFStressImpact.Value());
            Bunches[B + 0].FemaleFraction *= (1.0 - FFFStressImpact.Value());
            Bunches[B + 1].FemaleFraction *= (1.0 - FFFStressImpact.Value());
            Bunches[B + 2].FemaleFraction *= (1.0 - FFFStressImpact.Value());
            Bunches[B + 3].FemaleFraction *= (1.0 - FFFStressImpact.Value());
            Bunches[B + 4].FemaleFraction *= (1.0 - FFFStressImpact.Value());


        }
        /// <summary>Does the root growth.</summary>
        /// <param name="Allocation">The allocation.</param>
        /// <exception cref="System.Exception">Error trying to partition root biomass</exception>
        private void DoRootGrowth(double Allocation)
        {
            int RootLayer = LayerIndex(RootDepth);
            RootDepth = RootDepth + RootFrontVelocity.Value() * soilCrop.XF[RootLayer];
            RootDepth = Math.Min(MaximumRootDepth, RootDepth);
            RootDepth = Math.Min(MathUtilities.Sum(Soil.Thickness), RootDepth);

            // Calculate Root Activity Values for water and nitrogen
            double[] RAw = new double[Soil.Thickness.Length];
            double[] RAn = new double[Soil.Thickness.Length];
            double TotalRAw = 0;
            double TotalRAn = 0;

            for (int layer = 0; layer < Soil.Thickness.Length; layer++)
            {
                if (layer <= LayerIndex(RootDepth))
                    if (Roots[layer].Mass > 0)
                    {
                        RAw[layer] = SWUptake[layer] / Roots[layer].Mass
                                   * Soil.Thickness[layer]
                                   * RootProportion(layer, RootDepth);
                        RAw[layer] = Math.Max(RAw[layer], 1e-20);  // Make sure small numbers to avoid lack of info for partitioning

                        RAn[layer] = NUptake[layer] / Roots[layer].Mass
                                   * Soil.Thickness[layer]
                                   * RootProportion(layer, RootDepth);
                        RAn[layer] = Math.Max(RAw[layer], 1e-10);  // Make sure small numbers to avoid lack of info for partitioning

                    }
                    else if (layer > 0)
                    {
                        RAw[layer] = RAw[layer - 1];
                        RAn[layer] = RAn[layer - 1];
                    }
                    else
                    {
                        RAw[layer] = 0;
                        RAn[layer] = 0;
                    }
                TotalRAw += RAw[layer];
                TotalRAn += RAn[layer];
            }
            double allocated = 0;
            for (int layer = 0; layer < Soil.Thickness.Length; layer++)
            {
                if (TotalRAw > 0)

                    Roots[layer].Mass += Allocation * RAw[layer] / TotalRAw;
                else if (Allocation > 0)
                    throw new Exception("Error trying to partition root biomass");
                allocated += Allocation * RAw[layer] / TotalRAw;
            }



            // Do Root Senescence
            FOMLayerLayerType[] FOMLayers = new FOMLayerLayerType[Soil.Thickness.Length];

            for (int layer = 0; layer < Soil.Thickness.Length; layer++)
            {
                double Fr = RootSenescenceRate.Value();
                double DM = Roots[layer].Mass * Fr * 10.0;
                double N = Roots[layer].N * Fr * 10.0;
                Roots[layer].Mass *= (1.0 - Fr);
                Roots[layer].N *= (1.0 - Fr);
                Roots[layer].Length *= (1.0 - Fr);


                FOMType fom = new FOMType();
                fom.amount = (float)DM;
                fom.N = (float)N;
                fom.C = (float)(0.44 * DM);
                fom.P = 0;
                fom.AshAlk = 0;

                FOMLayerLayerType Layer = new FOMLayerLayerType();
                Layer.FOM = fom;
                Layer.CNR = 0;
                Layer.LabileP = 0;

                FOMLayers[layer] = Layer;
            }
            FOMLayerType FomLayer = new FOMLayerType();
            FomLayer.Type = CanopyType;
            FomLayer.Layer = FOMLayers;
            IncorpFOM.Invoke(FomLayer);


        }
        /// <summary>Does the growth.</summary>
        private void DoGrowth()
        {
            double RUEclear = RUE.Value();
            double RUEcloud = RUE.Value() * (1 + 0.33 * cover_green);
            double WF = DiffuseLightFraction;
            double RUEadj = WF * WF * RUEcloud + (1 - WF * WF) * RUEclear;
            DltDM = RUEadj * Math.Min(Fn,Fvpd) * MetData.Radn * cover_green * FW;

            double DMAvailable = DltDM;
            double[] FrondsAge = new double[Fronds.Count];
            double[] FrondsAgeDelta = new double[Fronds.Count];

            //precalculate  above two arrays
            double FMA = FrondMaxArea.Value();
            double frondAppearanceRate = FrondAppearanceRate.Value();
            double GrowthDuration = ExpandingFronds.Value() * frondAppearanceRate;

            for (int i = 0; i < Fronds.Count; i++)
                {
                    FrondsAge[i] = SizeFunction(Fronds[i].Age, FMA, GrowthDuration);
                    FrondsAgeDelta[i] = SizeFunction(Fronds[i].Age + DeltaT, FMA, GrowthDuration);
                }

            RootGrowth = (DltDM * RootFraction.Value());
            DMAvailable -= RootGrowth;
            DoRootGrowth(RootGrowth);

            double[] BunchDMD = new double[Bunches.Count];
            for (int i = 0; i < 6; i++)
            {
                Bunches[i].FillDuration += DeltaT/frondAppearanceRate;
                BunchDMD[i] = BunchSizeMax.Value() / (6 * frondAppearanceRate / DeltaT) * Fn * Population * Bunches[i].FemaleFraction * BunchOilConversionFactor.Value();
            }
            if (FrondNumber > HarvestFrondNumber.Value())  // start growing the 7th as well so that it can be ready to harvest on time
            {
                Bunches[6].FillDuration += DeltaT / frondAppearanceRate;
                BunchDMD[6] = BunchSizeMax.Value() / (6 * frondAppearanceRate / DeltaT) * Fn * Population * Bunches[7].FemaleFraction * BunchOilConversionFactor.Value();
            }
            double TotBunchDMD = MathUtilities.Sum(BunchDMD);

            double[] FrondDMD = new double[Fronds.Count];
            double specificLeafArea = SpecificLeafArea.Value();
            for (int i = 0; i < Fronds.Count; i++)
                FrondDMD[i] = (FrondsAgeDelta[i] - FrondsAge[i]) / specificLeafArea * Population * Fn;
            double TotFrondDMD = MathUtilities.Sum(FrondDMD);

            double StemDMD = TotFrondDMD * StemToFrondFraction.Value();

            double Fr = Math.Min(DMAvailable / (TotBunchDMD + TotFrondDMD + StemDMD), 1.0);
            Excess = 0.0;
            if (Fr > 1.0)
                Excess = DMAvailable - (TotBunchDMD + TotFrondDMD + StemDMD);

            //why is this here? -JF 
            if (Age > 10 && Fr < 1) 
            { }

            BunchGrowth = 0; // zero the daily value before incrementally building it up again with today's growth of individual bunches

            for (int i = 0; i < 7; i++)
            {
                double IndividualBunchGrowth = BunchDMD[i] * Fr / Population / BunchOilConversionFactor.Value();
                Bunches[i].Mass += IndividualBunchGrowth;
                BunchGrowth += IndividualBunchGrowth * Population;
            }
            if (DltDM > 0)
                ReproductiveGrowthFraction = TotBunchDMD * Fr / DltDM;
            else
                ReproductiveGrowthFraction = 0;

            FrondGrowth = 0; // zero the daily value before incrementally building it up again with today's growth of individual fronds

            double specificLeafAreaMax = SpecificLeafAreaMax.Value();

            for (int i = 0; i < Fronds.Count; i++)
            {
                double IndividualFrondGrowth = FrondDMD[i] * Fr / Population;
                Fronds[i].Mass += IndividualFrondGrowth;
                FrondGrowth += IndividualFrondGrowth * Population;
                if (Fr >= specificLeafArea / specificLeafAreaMax)
                    Fronds[i].Area += (FrondsAgeDelta[i] - FrondsAge[i]) * Fn;
                else
                    Fronds[i].Area += IndividualFrondGrowth * specificLeafAreaMax;

            };

            StemGrowth = StemDMD * Fr;// +Excess; 
            StemMass += StemGrowth;

            CarbonStress = Fr;

        }
        /// <summary>Does the development.</summary>
        private void DoDevelopment()
        {
            Age = Age + 1.0 / 365.0;
            //for (int i = 0; i < Frond.Length; i++)
            //    Frond[i].Age += 1;
            foreach (FrondType F in Fronds)
            {
                F.Age += DeltaT;
                //F.Area = SizeFunction(F.Age);
            }
            if (Fronds[Fronds.Count - 1].Age >= FrondAppearanceRate.Value())
            {
                FrondType F = new FrondType();
                Fronds.Add(F);
                CumulativeFrondNumber += 1;

                BunchType B = new BunchType();
                B.FemaleFraction = FemaleFlowerFraction.Value();
                Bunches.Add(B);
            }

            //if (Fronds[0].Age >= (40 * FrondAppRate.Value))
            //if (FrondNumber > Math.Round(HarvestFrondNumber.Value)&&Bunches[0].FillDuration>6)
            //if (FrondNumber > Math.Round(HarvestFrondNumber.Value))
            if (FrondNumber > HarvestFrondNumber.Value() && Bunches[0].FillDuration > 6)
                {
                HarvestBunches = Bunches[0].FemaleFraction;
                double HarvestYield = Bunches[0].Mass * Population / (1.0 - RipeBunchWaterContent.Value());
                HarvestFFB = HarvestYield / 100;
                HarvestNRemoved = Bunches[0].N * Population * 10;
                HarvestBunchSize = Bunches[0].Mass / (1.0 - RipeBunchWaterContent.Value()) / Bunches[0].FemaleFraction;
                if (Harvesting != null)
                    Harvesting.Invoke(this, new EventArgs());
                // Now rezero these outputs - they can only be output non-zero on harvesting event.
                HarvestBunches = 0.0;
                HarvestYield = 0.0;
                HarvestFFB = 0.0;
                HarvestBunchSize = 0.0;
                HarvestNRemoved = 0.0;

                CumulativeBunchNumber += Bunches[0].FemaleFraction;
                Bunches.RemoveAt(0);

                BiomassRemovedType BiomassRemovedData = new BiomassRemovedType();
                BiomassRemovedData.crop_type = CanopyType;
                BiomassRemovedData.dm_type = new string[1] { "frond" };
                BiomassRemovedData.dlt_crop_dm = new float[1] { (float)(Fronds[0].Mass * Population * 10) };
                BiomassRemovedData.dlt_dm_n = new float[1] { (float)(Fronds[0].N * Population * 10) };
                BiomassRemovedData.dlt_dm_p = new float[1] { 0 };
                BiomassRemovedData.fraction_to_residue = new float[1] { 1 };
                Fronds.RemoveAt(0);
                BiomassRemoved.Invoke(BiomassRemovedData);
            }
        }
        /// <summary>Saturated Vapour Pressuer</summary>
        /// <param name="temp">The temperature.</param>
        /// <returns></returns>
        private double svp(double temp)  
        {
            return 6.1078 * Math.Exp(17.269 * temp / (237.3 + temp));
        }

        /// <summary>VPDs this instance.</summary>
        /// <returns></returns>
        /// The following helper functions [VDP and svp] are for calculating Fvdp
        ///         /// <summary>Gets the lai.</summary>
        /// <value>The lai.</value>
        [Description("Vapour Pressure Deficit")]
        [Units("kPa")]
        public double VPD
        {
            get
            {
                double VPDmint = svp(MetData.MinT) - MetData.VP;
                VPDmint = Math.Max(VPDmint, 0.0);

                double VPDmaxt = svp(MetData.MaxT) - MetData.VP;
                VPDmaxt = Math.Max(VPDmaxt, 0.0);

                double vdp = 0.75 * VPDmaxt + 0.25 * VPDmint;
                return vdp;
            }
        }
        /// <summary>Does the water balance.</summary>
        private void DoWaterBalance()
        {
            if (VPD <= 18.0)
                Fvpd = 1;
            else
                Fvpd = Math.Max(0.0, 1 - (VPD - 18) / (50 - 18));


            PEP = Soil.SoilWater.Eo * cover_green*Math.Min(Fn, Fvpd);


            for (int j = 0; j < Soil.SoilWater.LL15mm.Length; j++)
                PotSWUptake[j] = Math.Max(0.0, RootProportion(j, RootDepth) * soilCrop.KL[j] * Math.Max(cover_green / 0.9, 0.01) * (Soil.Water[j] - Soil.SoilWater.LL15mm[j]));

            double TotPotSWUptake = MathUtilities.Sum(PotSWUptake);
            if (TotPotSWUptake == 0)
                throw new Exception("Total potential soil water uptake is zero");

            EP = 0.0;
            for (int j = 0; j < Soil.SoilWater.LL15mm.Length; j++)
            {
                SWUptake[j] = PotSWUptake[j] * Math.Min(1.0, PEP / TotPotSWUptake);
                EP += SWUptake[j];
                Soil.SoilWater.SetSWmm(j, Soil.SoilWater.SWmm[j] - SWUptake[j]);

            }

            if (PEP > 0.0)
            {
                FW = EP / PEP;
                //FWexpan = Math.Max(0.0, Math.Min(1.0, (TotPotSWUptake / PEP - 0.5) / 0.6));
                FWexpan = Math.Max(0.0, Math.Min(1.0, (TotPotSWUptake / PEP - 0.5) / 1.0));

            }
            else
            {
                FW = 1.0;
                FWexpan = 1.0;
            }
        }

        /// <summary>Does the n balance.</summary>
        /// <exception cref="System.Exception">Error in N Allocation</exception>
        private void DoNBalance()
        {
            double StartN = PlantN;

            double StemNDemand = StemGrowth * StemNConcentration.Value() / 100.0 * 10.0;  // factor of 10 to convert g/m2 to kg/ha
            double RootNDemand = Math.Max(0.0, (RootMass * RootNConcentration.Value() / 100.0 - RootN)) * 10.0;  // kg/ha
            double FrondNDemand = Math.Max(0.0, (FrondMass * FrondMaximumNConcentration.Value() / 100.0 - FrondN)) * 10.0;  // kg/ha 
            double BunchNDemand = Math.Max(0.0, (BunchMass * BunchNConcentration.Value() / 100.0 - BunchN)) * 10.0;  // kg/ha 

            Ndemand = StemNDemand + FrondNDemand + RootNDemand + BunchNDemand;  //kg/ha


            for (int j = 0; j < Soil.SoilWater.LL15mm.Length; j++)
            {
                double swaf = 0;
                swaf = (Soil.Water[j] - Soil.SoilWater.LL15mm[j]) / (Soil.SoilWater.DULmm[j] - Soil.SoilWater.LL15mm[j]);
                swaf = Math.Max(0.0, Math.Min(swaf, 1.0));
                double no3ppm = Soil.NO3N[j] * (100.0 / (Soil.BD[j] * Soil.Thickness[j]));
                PotNUptake[j] = Math.Max(0.0, RootProportion(j, RootDepth) * KNO3.Value() * Soil.NO3N[j] * swaf);
            }

            double TotPotNUptake = MathUtilities.Sum(PotNUptake);
            double Fr = Math.Min(1.0, Ndemand / TotPotNUptake);

            for (int j = 0; j < Soil.SoilWater.LL15mm.Length; j++)
                NUptake[j] = PotNUptake[j] * Fr;
            solutes.Subtract("NO3", NUptake);

            Fr = Math.Min(1.0, Math.Max(0, MathUtilities.Sum(NUptake) / BunchNDemand));
            double DeltaBunchN = BunchNDemand * Fr;

            double Tot = 0;
            foreach (BunchType B in Bunches)
            {
                Tot += Math.Max(0.0, B.Mass * BunchNConcentration.Value() / 100.0 - B.N) * Fr / SowingData.Population;
                B.N += Math.Max(0.0, B.Mass * BunchNConcentration.Value() / 100.0 - B.N) * Fr;
            }

            // Calculate fraction of N demand for Vegetative Parts
            if ((Ndemand - DeltaBunchN) > 0)
                Fr = Math.Max(0.0, ((MathUtilities.Sum(NUptake) - DeltaBunchN) / (Ndemand - DeltaBunchN)));
            else
                Fr = 0.0;

            StemN += StemNDemand / 10 * Fr;

            double[] RootNDef = new double[Soil.SoilWater.LL15mm.Length];
            double TotNDef = 1e-20;
            for (int j = 0; j < Soil.SoilWater.LL15mm.Length; j++)
            {
                RootNDef[j] = Math.Max(0.0, Roots[j].Mass * RootNConcentration.Value() / 100.0 - Roots[j].N);
                TotNDef += RootNDef[j];
            }
            for (int j = 0; j < Soil.SoilWater.LL15mm.Length; j++)
                Roots[j].N += RootNDemand / 10 * Fr * RootNDef[j] / TotNDef;

            foreach (FrondType F in Fronds)
                F.N += Math.Max(0.0, F.Mass * FrondMaximumNConcentration.Value() / 100.0 - F.N) * Fr;

            double EndN = PlantN;
            double Change = EndN - StartN;
            double Uptake = MathUtilities.Sum(NUptake) / 10.0;
            if (Math.Abs(Change - Uptake) > 0.001)
                throw new Exception("Error in N Allocation");

            double Nact = FrondNConc;
            double Ncrit = FrondCriticalNConcentration.Value();
            double Nmin = FrondMinimumNConcentration.Value();
            Fn = Math.Min(Math.Max(0.0, (Nact - Nmin) / (Ncrit - Nmin)), 1.0);

        }



        /// <summary>Gets the frond area.</summary>
        /// <value>The frond area.</value>
        [Description("Area of an average frond")]
        [Units("m^2")]
        public double FrondArea
        {
            get
            {
                double A = 0.0;

                foreach (FrondType F in Fronds)
                    A += F.Area;
                return A / Fronds.Count;
            }

        }

        /// <summary>Gets the frond17 area.</summary>
        /// <value>The frond17 area.</value>
        [Units("m^2")]
        [Description("Area of the 17th frond")]
        public double Frond17Area
        {
            get
            {
                //note frond 17 is 18th frond because they ignore the spear leaf
                if (Fronds.Count > 18)
                    return Fronds[Fronds.Count - 18].Area;
                else
                    return 0;

            }

        }

        /// <summary>Gets the frond mass.</summary>
        /// <value>The frond mass.</value>
        [Description("Frond mass on a dry mass basis")]
        [Units("g/m^2")]
        public double FrondMass
        {
            get
            {
                double FrondMass = 0.0;

                //for (int i = 0; i < Frond.Length; i++)
                //   FrondArea = FrondArea + Frond[i].Area;
                foreach (FrondType F in Fronds)
                    FrondMass += F.Mass;
                return FrondMass * Population;
            }

        }

        /// <summary>Gets the frond n.</summary>
        /// <value>The frond n.</value>
        [Description("Frond nitrogen content")]
        [Units("g/m^2")]
        public double FrondN
        {
            get
            {
                double FrondN = 0.0;

                //for (int i = 0; i < Frond.Length; i++)
                //   FrondArea = FrondArea + Frond[i].Area;
                foreach (FrondType F in Fronds)
                    FrondN += F.N;
                return FrondN * SowingData.Population;
            }

        }

        /// <summary>Gets the frond n conc.</summary>
        /// <value>The frond n conc.</value>
        [Description("Frond nitrogen concentration on a dry mass basis")]
        [Units("%")]
        public double FrondNConc
        {
            get
            {
                return FrondN / FrondMass * 100.0;
            }

        }

        /// <summary>Gets the bunch mass.</summary>
        /// <value>The bunch mass.</value>
        [Description("Bunch mass on a dry mass basis")]
        [Units("g/m^2")]
        public double BunchMass
        {
            get
            {
                double BunchMass = 0.0;

                foreach (BunchType B in Bunches)
                    BunchMass += B.Mass;
                return BunchMass * SowingData.Population;
            }

        }

        /// <summary>Gets the bunch n.</summary>
        /// <value>The bunch n.</value>
        [Description("Bunch nitrogen content")]
        [Units("g/m^2")]
        public double BunchN
        {
            get
            {
                double BunchN = 0.0;

                foreach (BunchType B in Bunches)
                    BunchN += B.N * SowingData.Population;
                return BunchN;
            }

        }

        /// <summary>Gets the bunch n conc.</summary>
        /// <value>The bunch n conc.</value>
        [Description("Bunch nitrogen concentration on a dry mass basis")]
        [Units("%")]
        public double BunchNConc
        {
            get
            {
                if (BunchMass > 0)
                    return BunchN / BunchMass * 100.0;
                else
                    return 0;
            }

        }

        /// <summary>Gets the root mass.</summary>
        /// <value>The root mass.</value>
        [Description("Root mass on a dry mass basis")]
        [Units("g/m^2")]
        public double RootMass
        {
            get
            {
                double RootMass = 0.0;

                foreach (RootType R in Roots)
                    RootMass += R.Mass;
                return RootMass;
            }

        }

        /// <summary>Gets the root n.</summary>
        /// <value>The root n.</value>
        [Description("Root nitrogen content")]
        [Units("g/m^2")]
        public double RootN
        {
            get
            {
                double RootN = 0.0;

                foreach (RootType R in Roots)
                    RootN += R.N;
                return RootN;
            }

        }

        /// <summary>Gets the root n conc.</summary>
        /// <value>The root n conc.</value>
        [Description("Root nitrogen concentration on a dry mass basis")]
        [Units("%")]
        public double RootNConc
        {
            get
            {
                return RootN / RootMass * 100.0;
            }

        }

        /// <summary>Gets the plant n.</summary>
        /// <value>The plant n.</value>
        [Description("Total palm nitrogen content")]
        [Units("g/m^2")]
        public double PlantN
        {
            get
            {
                return FrondN + RootN + StemN + BunchN;
            }
        }

        /// <summary>Gets the total frond number.</summary>
        /// <value>The total frond number.</value>
        [Description("Total number of fronds on a palm")]
        [Units("/palm")]
        public double TotalFrondNumber
        {
            get
            {
                return Fronds.Count;
            }
        }

        /// <summary>Gets the frond number.</summary>
        /// <value>The frond number.</value>
        [Description("Number of expanded fronds on a palm")]
        [Units("/palm")]
        public double FrondNumber
        {
            get
            {
                return Math.Max(Fronds.Count - ExpandingFronds.Value(), 0.0);
            }
        }

        /// <summary>Gets the cover_green.</summary>
        /// <value>The cover_green.</value>
        [Description("Green canopy cover provided by the palms")]
        [Units("0-1")]
        public double cover_green
        {
            get
            {
                double DF = DiffuseLightFraction;
                double DirectCover = 1.0 - Math.Exp(-DirectExtinctionCoeff.Value() * LAI);
                double DiffuseCover = 1.0 - Math.Exp(-DiffuseExtinctionCoeff.Value() * LAI);
                return DF * DiffuseCover + (1 - DF) * DirectCover;
            }
        }

        /// <summary>Gets the sla.</summary>
        /// <value>The sla.</value>
        [Description("Frond specific leaf area")]
        [Units("cm^2/g")]
        public double SLA
        {
            get { return LAI * 10000.0 / FrondMass; }
        }

        /// <summary>Gets the FFF.</summary>
        /// <value>The FFF.</value>
        [Description("Female flower fraction of the oldest cohort of bunches")]
        [Units("0-1")]
        public double FFF
        {
            get { return Bunches[0].FemaleFraction; }
        }

        /// <summary>Sizes the function.</summary>
        /// <param name="Age">The age.</param>
        /// <param name="FMA">FMA</param>
        /// <param name="GrowthDuration">Groth duration</param>
        /// <returns></returns>
        protected double SizeFunction(double Age, double FMA, double GrowthDuration)
        {
            double alpha = -Math.Log((1 / 0.99 - 1) / (FMA / (FMA * 0.01) - 1)) / GrowthDuration;
            double leafsize = FMA / (1 + (FMA / (FMA * 0.01) - 1) * Math.Exp(-alpha * Age));
            return leafsize;

        }
        /// <summary>Roots the proportion.</summary>
        /// <param name="layer">The layer.</param>
        /// <param name="root_depth">The root_depth.</param>
        /// <returns></returns>
        private double RootProportion(int layer, double root_depth)
        {
            double depth_to_layer_bottom = 0;   // depth to bottom of layer (mm)
            double depth_to_layer_top = 0;      // depth to top of layer (mm)
            double depth_to_root = 0;           // depth to root in layer (mm)
            double depth_of_root_in_layer = 0;  // depth of root within layer (mm)
            // Implementation Section ----------------------------------
            for (int i = 0; i <= layer; i++)
                depth_to_layer_bottom += Soil.Thickness[i];
            depth_to_layer_top = depth_to_layer_bottom - Soil.Thickness[layer];
            depth_to_root = Math.Min(depth_to_layer_bottom, root_depth);
            depth_of_root_in_layer = Math.Max(0.0, depth_to_root - depth_to_layer_top);

            return depth_of_root_in_layer / Soil.Thickness[layer];
        }
        /// <summary>Layers the index.</summary>
        /// <param name="depth">The depth.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Depth deeper than bottom of soil profile</exception>
        private int LayerIndex(double depth)
        {
            double CumDepth = 0;
            for (int i = 0; i < Soil.Thickness.Length; i++)
            {
                CumDepth = CumDepth + Soil.Thickness[i];
                if (CumDepth >= depth) { return i; }
            }
            throw new Exception("Depth deeper than bottom of soil profile");
        }
        /// <summary>Gets the delta t.</summary>
        /// <value>The delta t.</value>
        private double DeltaT
        {
            get
            {
                //return Math.Min(Math.Pow(Fn,0.5),1.0);
                //return Math.Min(1.4 * Fn, RelativeDevelopmentalRate.Value);
                //return Math.Min(1.0 * Fn, RelativeDevelopmentalRate.Value);
                return Math.Min(1.25 * Fn, 1.0) * RelativeDevelopmentalRate.Value();
            }
        }


        /// <summary>Does the understory.</summary>
        private void DoUnderstory()
        {
            DoUnderstoryWaterBalance();
            DoUnderstoryGrowth();
            DoUnderstoryNBalance();

            // Now add today's growth to the soil - ie assume plants are in steady state.
            BiomassRemovedType BiomassRemovedData = new BiomassRemovedType();
            BiomassRemovedData.crop_type = "OilPalmUnderstory";
            BiomassRemovedData.dm_type = new string[1] { "litter" };
            BiomassRemovedData.dlt_crop_dm = new float[1] { (float)(UnderstoryDltDM * 10) };
            BiomassRemovedData.dlt_dm_n = new float[1] { (float)(UnderstoryNFixation + MathUtilities.Sum(UnderstoryNUptake)) };
            BiomassRemovedData.dlt_dm_p = new float[1] { 0 };
            BiomassRemovedData.fraction_to_residue = new float[1] { 1 };
            BiomassRemoved.Invoke(BiomassRemovedData);

        }
        /// <summary>Does the understory growth.</summary>
        private void DoUnderstoryGrowth()
        {
            double RUE = 1.3;
            UnderstoryDltDM = RUE * MetData.Radn * UnderstoryCoverGreen * (1 - cover_green) * UnderstoryFW;
        }

        /// <summary>Does the understory water balance.</summary>
        private void DoUnderstoryWaterBalance()
        {

            UnderstoryCoverGreen = UnderstoryCoverMax * (1 - cover_green);
            UnderstoryPEP = Soil.SoilWater.Eo * UnderstoryCoverGreen * (1 - cover_green);

            for (int j = 0; j < Soil.Thickness.Length; j++)
                UnderstoryPotSWUptake[j] = Math.Max(0.0, RootProportion(j, UnderstoryRootDepth) * UnderstoryKLmax * UnderstoryCoverGreen * (Soil.Water[j] - Soil.SoilWater.LL15mm[j]));

            double TotUnderstoryPotSWUptake = MathUtilities.Sum(UnderstoryPotSWUptake);

            UnderstoryEP = 0.0;
            double[] sw_dep = Soil.Water;
            for (int j = 0; j < Soil.Thickness.Length; j++)
            {
                UnderstorySWUptake[j] = UnderstoryPotSWUptake[j] * Math.Min(1.0, UnderstoryPEP / TotUnderstoryPotSWUptake);
                UnderstoryEP += UnderstorySWUptake[j];
                sw_dep[j] = sw_dep[j] - UnderstorySWUptake[j];

            }
            Soil.Water = sw_dep;

            if (UnderstoryPEP > 0.0)
                UnderstoryFW = UnderstoryEP / UnderstoryPEP;
            else
                UnderstoryFW = 1.0;

        }
        /// <summary>Does the understory n balance.</summary>
        private void DoUnderstoryNBalance()
        {
            double LegumeNdemand = UnderstoryDltDM * UnderstoryLegumeFraction * 10 * 0.021;
            double NonLegumeNdemand = UnderstoryDltDM * (1 - UnderstoryLegumeFraction) * 10 * 0.005;
            double UnderstoryNdemand = LegumeNdemand + NonLegumeNdemand;
            UnderstoryNFixation = Math.Max(0.0, LegumeNdemand * .44);

            for (int j = 0; j < Soil.Thickness.Length; j++)
            {
                UnderstoryPotNUptake[j] = Math.Max(0.0, RootProportion(j, UnderstoryRootDepth) * Soil.NO3N[j]);
            }

            double TotUnderstoryPotNUptake = MathUtilities.Sum(UnderstoryPotNUptake);
            double Fr = Math.Min(1.0, (UnderstoryNdemand - UnderstoryNFixation) / TotUnderstoryPotNUptake);

            double[] no3 = Soil.NO3N;
            for (int j = 0; j < Soil.Thickness.Length; j++)
            {
                UnderstoryNUptake[j] = UnderstoryPotNUptake[j] * Fr;
                no3[j] = no3[j] - UnderstoryNUptake[j];
            }
            Soil.NO3N = no3;

            //UnderstoryNFixation += UnderstoryNdemand - MathUtilities.Sum(UnderstoryNUptake);

            //NFixation = Math.Max(0.0, Ndemand - MathUtilities.Sum(NUptake));

        }

        /// <summary>Gets or sets the defoliation fraction.</summary>
        /// <value>The defoliation fraction.</value>
        [XmlIgnore]
        public double DefoliationFraction
        {
            get
            {
                return 0;
            }
            set
            {
                FrondType Loss = new FrondType();
                foreach (FrondType F in Fronds)
                {
                    Loss.Mass += F.Mass * value;
                    Loss.N += F.N * value;

                    F.Mass = F.Mass * (1.0 - value);
                    F.N = F.N * (1.0 - value);
                    F.Area = F.Area * (1.0 - value);
                }


                // Now publish today's losses
                BiomassRemovedType BiomassRemovedData = new BiomassRemovedType();
                BiomassRemovedData.crop_type = "OilPalm";
                BiomassRemovedData.dm_type = new string[1] { "fronds" };
                BiomassRemovedData.dlt_crop_dm = new float[1] { (float)(Loss.Mass * SowingData.Population * 10.0) };
                BiomassRemovedData.dlt_dm_n = new float[1] { (float)(Loss.N * SowingData.Population * 10.0) };
                BiomassRemovedData.dlt_dm_p = new float[1] { 0 };
                BiomassRemovedData.fraction_to_residue = new float[1] { 0 };
                if (BiomassRemoved != null)
                    BiomassRemoved.Invoke(BiomassRemovedData);

            }
        }


        /// <summary>Gets the diffuse light fraction.</summary>
        /// <value>The diffuse light fraction.</value>
        public double DiffuseLightFraction       // This was originally in the RUEModel class inside "Potential" function (PFR)
        {
            get
            {

                double Q = Q0(MetData.Latitude, Clock.Today.DayOfYear);
                double T = MetData.Radn / Q;
                double X1 = (0.80 - 0.0017 * MetData.Latitude + 0.000044 * MetData.Latitude * MetData.Latitude);
                double A1 = ((0.05 - 0.96) / (X1 - 0.26));
                double A0 = (0.05 - A1 * X1);

                return Math.Min(Math.Max(0.0, A0 + A1 * T), 1.0);  //Taken from Roderick paper Ag For Met(?)

            }
        }

        /// <summary>Q0s the specified lat.</summary>
        /// <param name="lat">The lat.</param>
        /// <param name="day">The day.</param>
        /// <returns></returns>
        private double Q0(double lat, int day)                         // (PFR)
        {
            double DEC = (23.45 * Math.Sin(2.0 * 3.14159265 / 365.25 * (day - 79.25)));
            double DECr = (DEC * 2.0 * 3.14159265 / 360.0);
            double LATr = (lat * 2.0 * 3.14159265 / 360.0);
            double HS = Math.Acos(-Math.Tan(LATr) * Math.Tan(DECr));

            return 86400.0 * 1360.0 * (HS * Math.Sin(LATr) * Math.Sin(DECr) + Math.Cos(LATr) * Math.Cos(DECr) * Math.Sin(HS)) / 3.14159265 / 1000000.0;
        }
    }
}