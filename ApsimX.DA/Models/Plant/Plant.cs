// -----------------------------------------------------------------------
// <copyright file="Plant.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
//-----------------------------------------------------------------------
namespace Models.PMF
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;
    using Models.Core;
    using Models.Interfaces;
    using Models.PMF.Functions;
    using Models.PMF.Interfaces;
    using Models.PMF.Organs;
    using Models.PMF.Phen;
    using Models.Soils.Arbitrator;
    using APSIM.Shared.Utilities;

    ///<summary>
    /// The generic plant model
    /// </summary>
    /// \pre Summary A Summary model has to exist to write summary message.
    /// \pre Phenology A \ref Models.PMF.Phen.Phenology Phenology model is 
    /// optional to check whether plant has emerged.
    /// \pre OrganArbitrator A OrganArbitrator model is optional (not used currently).
    /// \pre Structure A Structure model is optional (not used currently).
    /// \pre Leaf A \ref Models.PMF.Organs.Leaf Leaf model is optional 
    /// to calculate water supply and demand ratio.
    /// \pre Root A Models.PMF.Organs.Root Root model is optional 
    /// to calculate water supply and demand ratio.
    /// \param CropType Used by several organs to determine the type of crop.
    /// \retval Population Number of plants per square meter. 
    /// \retval IsAlive Return true if plant is alive and in the ground.
    /// \retval IsEmerged Return true if plant has emerged.
    /// 
    /// On commencing simulation
    /// ------------------------
    /// OnSimulationCommencing is called on commencing simulation. Organs contain 
    /// all children which derive from model IOrgan. The model variables 
    /// are reset.
    /// 
    /// On sowing 
    /// -------------------------
    /// Plant is sown by a manager script in a APSIM model. For example,    
    /// \code
    /// 2012-10-23 [Maize].Sow(population:11, cultivar:"Pioneer_3153", depth:50, rowSpacing:710);
    /// \endcode
    /// 
    /// Sowing parameters should be specified, i.e. cultivar, population, depth, rowSpacing,
    /// maxCover (optional), and budNumber (optional).
    /// 
    /// Two events "Sowing" and "PlantSowing" are invoked to notify other models 
    /// to execute sowing events.
    /// <remarks>
    /// </remarks>
    [ValidParent(ParentType = typeof(Zone))]
    [Serializable]
    [ScopedModel]
    public class Plant : ModelCollectionFromResource, ICrop
    {
        #region Class links
        /// <summary>The summary</summary>
        [Link]
        ISummary Summary = null;

        /// <summary> The plant's zone</summary>
        [Link]
        public Zone Zone = null;

        /// <summary>The phenology</summary>
        [Link(IsOptional = true)]
        public Phenology Phenology = null;
        /// <summary>The arbitrator</summary>
        [Link(IsOptional = true)]
        public OrganArbitrator Arbitrator = null;
        /// <summary>The structure</summary>
        [Link(IsOptional = true)]
        public Structure Structure = null;
        /// <summary>The Canopy</summary>
        [Link(IsOptional = true)]
        public ICanopy Canopy = null;
        /// <summary>The leaf</summary>
        [Link(IsOptional = true)]
        public Leaf Leaf = null;
        /// <summary>The root</summary>
        [Link(IsOptional = true)]
        public Root Root = null;
        [Link(IsOptional = true)]
        Biomass AboveGround = null;
        /// <summary>
        /// Clock
        /// </summary>
        [Link]
        public Clock Clock = null;

        #endregion

        #region Class properties and fields
        /// <summary>Used by several organs to determine the type of crop.</summary>
        public string CropType { get; set; }

        /// <summary>The sowing data</summary>
        [XmlIgnore]
        public SowPlant2Type SowingData { get; set; }

        /// <summary>Gets the organs.</summary>
        [XmlIgnore]
        public IOrgan[] Organs { get; private set; }
 
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
        
        /// <summary>A property to return all cultivar definitions.</summary>
        private List<Cultivar> Cultivars
        {
            get
            {
                List<Cultivar> cultivars = new List<Cultivar>();
                foreach (Model model in Apsim.ChildrenRecursively(this, typeof(Cultivar)))
                {
                    cultivars.Add(model as Cultivar);
                }

                return cultivars;
            }
        }

        /// <summary>The current cultivar definition.</summary>
        private Cultivar cultivarDefinition;

        /// <summary>Gets the water supply demand ratio.</summary>
        [Units("0-1")]
        public double WaterSupplyDemandRatio
        {
            get
            {
                double F;

                if (Canopy != null && Canopy.PotentialEP > 0)
                    F = Root.WaterUptake / Canopy.PotentialEP;
                else
                    F = 1;
                return F;
            }
        }


        /// <summary>Holds the number of plants.</summary>
        private double plantPopulation = 0.0;
        /// <summary>
        /// Holds the date of sowing
        /// </summary>
        public DateTime SowingDate { get; set; }

        /// <summary>Gets or sets the plant population.</summary>
        [XmlIgnore]
        [Description("Number of plants per meter2")]
        [Units("/m2")]
        public double Population
        {
            get { return plantPopulation; }
            set
            {
                double InitialPopn = plantPopulation;
                if (IsAlive && value <= 0.01)                    
                    EndCrop();  // the plant is dying due to population decline
                else
                {
                    plantPopulation = value;
                    if (Structure != null)
                    {
                        Structure.DeltaPlantPopulation = InitialPopn - value;
                        Structure.ProportionPlantMortality = 1 - (value / InitialPopn);
                    }
                }
            }
        }

        /// <summary>Return true if plant is alive and in the ground.</summary>
        public bool IsAlive { get { return SowingData != null; } }

        /// <summary>Return true if plant has emerged</summary>
        public bool IsEmerged
        {
            get
            {
                if (Phenology != null)
                    return Phenology.Emerged;
                    //If the crop model has phenology and the crop is emerged return true
                else
                    return IsAlive;
                    //Else if the crop is in the grown returen true
            }
        }

        /// <summary>Return true if plant has germinated</summary>
        public bool IsGerminated
        {
            get
            {
                if (Phenology != null)
                    return Phenology.Germinated;
                //If the crop model has phenology and the crop is emerged return true
                else
                    return IsAlive;
                //Else if the crop is in the grown returen true
            }
        }
        /// <summary>Returns true if the crop is ready for harvesting</summary>
        public bool IsReadyForHarvesting { get { return Phenology.CurrentPhaseName == "ReadyForHarvesting"; } }

        /// <summary>Harvest the crop</summary>
        public void Harvest() { Harvest(null); }

        /// <summary>The plant mortality rate</summary>
        [Link(IsOptional = true)]
        [Units("")]
        IFunction MortalityRate = null;
        #endregion

        #region Class Events
        /// <summary>Occurs when a plant is about to be sown.</summary>
        public event EventHandler Sowing;
        /// <summary>Occurs when a plant is sown.</summary>
        public event EventHandler<SowPlant2Type> PlantSowing;
        /// <summary>Occurs when a plant is about to be sown.</summary>
        public event EventHandler PlantEmerging;
        /// <summary>Occurs when a plant is about to be harvested.</summary>
        public event EventHandler Harvesting;
        /// <summary>Occurs when a plant is ended via EndCrop.</summary>
        public event EventHandler PlantEnding;
        /// <summary>Occurs when a plant is about to be pruned.</summary>
        public event EventHandler Pruning;
        #endregion

        #region External Communications.  Method calls and EventHandlers
        /// <summary>Things the plant model does when the simulation starts</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("Commencing")]
        private void OnSimulationCommencing(object sender, EventArgs e)
        {
            List<IOrgan> organs = new List<IOrgan>();
            
            foreach (IOrgan organ in Apsim.Children(this, typeof(IOrgan)))
            {
                organs.Add(organ);
            }

            Organs = organs.ToArray();

            Clear();

        }

        /// <summary>Called when [phase changed].</summary>
        /// <param name="phaseChange">The phase change.</param>
        /// <param name="sender">Sender plant.</param>
        [EventSubscribe("PhaseChanged")]
        private void OnPhaseChanged(object sender, PhaseChangedType phaseChange)
        {
            if (sender == this && Phenology != null && Canopy != null && AboveGround != null)
            {
                string message = Phenology.CurrentPhase.Start + "\r\n";
                if (Canopy != null)
                {
                    message += "  LAI = " + Canopy.LAI.ToString("f2") + " (m^2/m^2)" + "\r\n";
                    message += "  Above Ground Biomass = " + AboveGround.Wt.ToString("f2") + " (g/m^2)" + "\r\n";
                }
                Summary.WriteMessage(this, message);
            }
        }

        /// <summary>Event from sequencer telling us to do our potential growth.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("DoPotentialPlantGrowth")]
        private void OnDoPotentialPlantGrowth(object sender, EventArgs e)
        {
            //Reduce plant population in case of mortality
            if (Population > 0.0 && MortalityRate != null)
            {
                double DeltaPopulation = Population * MortalityRate.Value();
                Population -= DeltaPopulation;
                if (Structure != null)
                {
                    Structure.DeltaPlantPopulation = DeltaPopulation;
                    Structure.ProportionPlantMortality = MortalityRate.Value();
                }
            }
        }

        /// <summary>Sow the crop with the specified parameters.</summary>
        /// <param name="cultivar">The cultivar.</param>
        /// <param name="population">The population.</param>
        /// <param name="depth">The depth.</param>
        /// <param name="rowSpacing">The row spacing.</param>
        /// <param name="maxCover">The maximum cover.</param>
        /// <param name="budNumber">The bud number.</param>
        public void Sow(string cultivar, double population, double depth, double rowSpacing, double maxCover = 1, double budNumber = 1)
        {
            SowingDate = Clock.Today;

            SowingData = new SowPlant2Type();
            SowingData.Plant = this;
            SowingData.Population = population;
            SowingData.Depth = depth;
            SowingData.Cultivar = cultivar;
            SowingData.MaxCover = maxCover;
            SowingData.BudNumber = budNumber;
            SowingData.RowSpacing = rowSpacing;
            this.Population = population;

            // Find cultivar and apply cultivar overrides.
            cultivarDefinition = PMF.Cultivar.Find(Cultivars, SowingData.Cultivar);
            cultivarDefinition.Apply(this);


            // Invoke an AboutToSow event.
            if (Sowing != null)
                Sowing.Invoke(this, new EventArgs());

            // Invoke a sowing event.
            if (PlantSowing != null)
                PlantSowing.Invoke(this, SowingData);

            if (Phenology == null)
                SendEmergingEvent();

            Summary.WriteMessage(this, string.Format("A crop of " + CropType + " (cultivar = " + cultivar + ") was sown today at a population of " + Population + " plants/m2 with " + budNumber + " buds per plant at a row spacing of " + rowSpacing + " and a depth of " + depth + " mm"));
        }

        /// <summary>
        /// Send out an emerging event
        /// </summary>
        public void SendEmergingEvent()
        {
            if (PlantEmerging != null)
                PlantEmerging.Invoke(this, null);
        }

        /// <summary>Harvest the crop.</summary>
        public void Harvest(RemovalFractions removalData)
        {
            RemoveBiomass("Harvest", removalData);
        }

        /// <summary>Harvest the crop.</summary>
        public void RemoveBiomass(string biomassRemoveType, RemovalFractions removalData = null)
        {
            // Invoke an event.
            if (biomassRemoveType == "Harvest" && Harvesting != null)
                Harvesting.Invoke(this, new EventArgs());
            Summary.WriteMessage(this, string.Format("Biomass removed from crop " + Name + " by " + biomassRemoveType.TrimEnd('e') + "ing"));

            // Set up the default BiomassRemovalData values
            foreach (IOrgan organ in Organs)
            {
                // Get the default removal fractions
                OrganBiomassRemovalType biomassRemoval = null;
                if (removalData != null)
                    biomassRemoval = removalData.GetFractionsForOrgan(organ.Name);
                organ.DoRemoveBiomass(biomassRemoveType, biomassRemoval);
            }

            // Reset the phenology if SetPhenologyStage specified.
            if (removalData != null && removalData.SetPhenologyStage != 0)
                Phenology.ReSetToStage(removalData.SetPhenologyStage);

            // Reduce plant and stem population if thinning proportion specified
            if (removalData != null && removalData.SetThinningProportion != 0)
                Structure.doThin(removalData.SetThinningProportion);

            // Pruning event (winter pruning, summer pruning is called as cut) reset the phenology if SetPhenologyStage specified.
            if (biomassRemoveType == "Prune" && Pruning != null)
                Pruning.Invoke(this, new EventArgs());
                
        }

        /// <summary>End the crop.</summary>
        public void EndCrop()
        {
            if (IsAlive == false)
                throw new Exception("EndCrop method called when no crop is planted.  Either your planting rule is not working or your end crop is happening at the wrong time");
            Summary.WriteMessage(this, "Crop ending");

            // Invoke a plant ending event.
            if (PlantEnding != null)
                PlantEnding.Invoke(this, new EventArgs());

            Clear();
            if (cultivarDefinition != null)
                cultivarDefinition.Unapply();
        }
        #endregion

        #region Private methods
        /// <summary>Clears this instance.</summary>
        private void Clear()
        {
            SowingData = null;
            plantPopulation = 0.0;
        }
        #endregion
        
        /// <summary>Writes documentation for this function by adding to the list of documentation tags.</summary>
        /// <param name="tags">The list of tags to add to.</param>
        /// <param name="headingLevel">The level (e.g. H2) of the headings.</param>
        /// <param name="indent">The level of indentation 1, 2, 3 etc.</param>
        public override void Document(List<AutoDocumentation.ITag> tags, int headingLevel, int indent)
        {
            foreach (IModel child in Apsim.Children(this, typeof(IModel)))
                child.Document(tags, headingLevel + 1, indent);
        }
    }
}
