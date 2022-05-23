using System;
using System.Collections.Generic;
using System.Text;
using Models.Core;
using Models.PMF.Functions;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using Models.PMF.OldPlant;
using APSIM.Shared.Utilities;
using Models.Sensitivity;
using Models.DataAssimilation;

namespace Models.PMF.Phen
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class PhaseChangedType : EventArgs
    {
        /// <summary>The old phase name</summary>
        public String OldPhaseName = "";
        /// <summary>The new phase name</summary>
        public String NewPhaseName = "";
        /// <summary>The stage at phase change</summary>
        public String EventStageName = "";
    }
    /// <summary>
    /// This model simulates the development of the crop through successive developmental <i>phases</i>. 
    /// Each phase is bound by distinct growth <i>stages</i>. 
    /// Phases often require a target to be reached to signal movement to the next phase. 
    /// Differences between cultivars are specified by changing the values of the default parameters shown below.
    /// </summary>
    /// \warning An \ref Models.PMF.Phen.EndPhase "EndPhase" model 
    /// should be included as the last child of 
    /// \ref Models.PMF.Phen.Phenology "Phenology" model
    /// \pre A Models.Clock "Clock" model has to exist.
    /// \retval CurrentPhaseName The current phase name.
    /// \retval CurrentStageName The current stage name. 
    /// Return name of parameter \p Start of 
    /// \ref Models.PMF.Phen.Phase "Phase" model 
    /// on the day of phase starting, or ? in other days.
    /// \retval Stage A one based stage number (start from 1).
    /// \retval DaysAfterSowing The day after sowing. 
    /// \retval FractionInCurrentPhase The fraction in current phase (0-1).
    /// <remarks>
    /// Generally, the plant phenology is divided into \f$N_{p}\f$ 
    /// phases (i.e. \ref Models.PMF.Phen.Phase "Phase" functions) and stages 
    /// (children number of Phenology model, only count 
    /// \ref Models.PMF.Phen.Phase "Phase" models).
    /// 
    /// Any models inherited from \ref Models.PMF.Phen.Phase "Phase" models 
    /// could be used as children of Phenology model, except the last phase,
    /// which should be the \ref Models.PMF.Phen.EndPhase "EndPhase" model.
    /// The stage names are defined by the \p Start and \p End parameters 
    /// in each \ref Models.PMF.Phen.Phase "Phase" model. Consequently,
    /// The parameter value of \p Start should equal to the parameter value
    /// of \p End of previous \ref Models.PMF.Phen.Phase "Phase" model,
    /// except for the first phase. The parameter value of \p End 
    /// should equal to the parameter value of \p Start of next 
    /// \ref Models.PMF.Phen.Phase "Phase" function, except for the last phase. 
    /// The equality of stage and phase number raise from the last phase,
    /// which is \ref Models.PMF.Phen.EndPhase "EndPhase" model and the 
    /// \p End stage is unused. 
    /// 
    /// For example, the wheat phenology model defines 9 stages from sowing to maturity.
    /// See the diagram below for definitions of stages and phases, 
    /// and models used for each phase.
    /// 
    /// \startuml
    /// Sowing -> Germination : Germinating
    /// Germination -> Emergence : Emerging
    /// Emergence ->  TerminalSpikelet : Vegetative
    /// TerminalSpikelet-> FlagLeaf : FloralInitiation\nToFlagLeaf
    /// FlagLeaf -> Flowering : Spike\nDevelopment
    /// Flowering -> StartGrainFill: Grain\nDevelopment
    /// StartGrainFill -> EndGrainFill : GrainFilling
    /// EndGrainFill -> Maturity : Maturing
    /// Maturity -> Unused: ReadyFor\nHarvesting
    /// 
    /// note over Sowing, Germination : GerminatingPhase
    /// note over Germination, Emergence : EmergingPhase
    /// note over Emergence, TerminalSpikelet : GenericPhase
    /// note over TerminalSpikelet, FlagLeaf : LeafAppearancePhase
    /// note over FlagLeaf, Flowering : GenericPhase
    /// note over Flowering, StartGrainFill: GenericPhase
    /// note over StartGrainFill, EndGrainFill : GenericPhase
    /// note over EndGrainFill, Maturity : GenericPhase
    /// note over Maturity, Unused: EndPhase
    /// \enduml
    /// 
    /// On commencing simulation and sowing
    /// ----------------
    /// On commencing simulation and sowing
    /// \ref Models.PMF.Phen.Phenology "Phenology" function and all related 
    /// \ref Models.PMF.Phen.Phase "Phase" functions will be reset.
    /// On harvest
    /// -----------------
    /// On harvest, \ref Models.PMF.Phen.Phenology "Phenology" will 
    /// jump to the last phase function i.e. \ref Models.PMF.Phen.EndPhase 
    /// "EndPhase" function.
    /// On daily
    /// -----------------
    /// Phenology function performs a daily time step function, get the current 
    /// phase to do its development for the day. If thermal time is leftover after 
    /// Phase is progressed, and the time step for the subsequent phase is calculated 
    /// using leftover thermal time.
    /// 
    /// </remarks>
    [Serializable]
    [ValidParent(ParentType = typeof(Plant))]
    [ValidParent(ParentType = typeof(Plant15))]
    public class Phenology : Model
    {
        #region Links
        [Link(IsOptional = true)]
        private Plant Plant = null;

        [Link(IsOptional = true)]
        private Plant15 Plant15 = null;    // This can be deleted once we get rid of plant15.

        /// <summary>The clock</summary>
        [Link]
        private Clock Clock = null;
        /// <summary>The rewind due to biomass removed</summary>
        [Link(IsOptional = true)]
        private IFunction RewindDueToBiomassRemoved = null;
        /// <summary>The above ground period</summary>
        [Link(IsOptional = true)]
        private IFunction AboveGroundPeriod = null;
        /// <summary>The summary</summary>
        [Link]
        ISummary Summary = null;

        [Link(IsOptional = true)]
        Control Control = null;
        #endregion

        #region Events
        /// <summary>Occurs when [phase changed].</summary>
        public event EventHandler<PhaseChangedType> PhaseChanged;

        /// <summary>Occurs when phase is rewound.</summary>
        public event EventHandler PhaseRewind;

        /// <summary>Occurs when [growth stage].</summary>
        public event NullTypeDelegate GrowthStage;

        /// <summary>Occurs when daily phenology timestep completed</summary>
        public event EventHandler PostPhenology;
        #endregion

        #region Parameters
        /// <summary>The thermal time</summary>
        [Link]
        public IFunction ThermalTime = null;

        /// <summary>The phases</summary>
        private List<Phase> Phases;

        #endregion

        #region States
        /// <summary>The current phase index</summary>
        private int CurrentPhaseIndex;
        /// <summary>The Thermal time accumulated tt</summary>
        [XmlIgnore]
        public double AccumulatedTT { get; set; }

        /// <summary>YUXI: The Thermal time in phase from CurrentPhase.</summary>
        [XmlIgnore]
        public double GetTTinPhase { get; set; }

        /// <summary>YUXI: The stressed accumulated thermal time.</summary>
        [XmlIgnore]
        public double GetAccumulatedTT { get; set; }

        /// <summary>The Thermal time accumulated tt following emergence</summary>
        [XmlIgnore]
        public double AccumulatedEmergedTT { get; set; }
        /// <summary>The currently on first day of phase.  This is an array that lists all the stages that are pased on this day</summary>
        private string[] CurrentlyOnFirstDayOfPhase = new string[] { "", "", "", "", "", "" };
        /// <summary>The number of stages that have been passed today</summary>
        private int StagesPassedToday = 0;
        /// <summary>The just initialised</summary>
        private bool JustInitialised = true;
        /// <summary>The fraction biomass removed</summary>
        private double FractionBiomassRemoved = 0;
        /// <summary>The sow date</summary>
        private DateTime SowDate = DateTime.MinValue;
        /// <summary>The emerged</summary>
        [XmlIgnore]
        public bool Emerged = false;
        /// <summary>Germinated test</summary>
        [XmlIgnore]
        public bool Germinated = false;
        /// <summary>
        /// This is the difference in phase index between the current phase and the phas it runs parallel with.
        /// It is 0 for phases that are not run parallel
        /// </summary>
        private int PhaseIndexOffset = 0;

        /// <summary>A one based stage number.</summary>
        [XmlIgnore]
        public double Stage { get; set; }

        /// <summary>Clears this instance.</summary>
        public void Clear()
        {
            DaysAfterSowing = 0;
            Stage = 1;
            AccumulatedTT = 0;
            AccumulatedEmergedTT = 0;
            JustInitialised = true;
            Emerged = false;
            Germinated = false;
            SowDate = Clock.Today;
            CurrentlyOnFirstDayOfPhase = new string[] { "", "", "", "", "", "" };
            CurrentPhaseIndex = 0;
            FractionBiomassRemoved = 0;
            foreach (Phase phase in Phases)
                phase.ResetPhase();
        }

        #endregion

        #region Outputs
        /// <summary>This property is used to retrieve or set the current phase name.</summary>
        /// <value>The name of the current phase.</value>
        /// <exception cref="System.Exception">Cannot jump to phenology phase:  + value + . Phase not found.</exception>

        [XmlIgnore]
        public string CurrentPhaseName
        {
            get
            {
                if (CurrentPhase == null)
                    return "";
                else
                    return CurrentPhase.Name;
            }
            set
            {
                int PhaseIndex = IndexOfPhase(value);
                if (PhaseIndex == -1)
                    throw new Exception("Cannot jump to phenology phase: " + value + ". Phase not found.");
                CurrentPhase = Phases[PhaseIndex];
                Summary.WriteMessage(this, string.Format(this + " has set phase to " + CurrentPhase.Name));
            }
        }

        /// <summary>Return current stage name.</summary>
        /// <value>The name of the current stage.</value>

        public string CurrentStageName
        {
            get
            {
                if (OnDayOf(CurrentPhase.Start))
                    return CurrentPhase.Start;
                else
                    return "?";
            }
        }


        /// <summary>Gets the fraction in current phase.</summary>
        /// <value>The fraction in current phase.</value>
        public double FractionInCurrentPhase
        {
            get
            {
                return Stage - (int)Stage;
            }
        }

        /// <summary>Gets the days after sowing.</summary>
        /// <value>The days after sowing.</value>
        public int DaysAfterSowing { get; set; }

        #endregion

        /// <summary>Constructor</summary>
        public Phenology() { }
        /// <summary>Initialize the phase list of phenology.</summary>
        [EventSubscribe("Loaded")]
        private void OnLoaded()
        {
            Phases = new List<Phase>();
            foreach (Phase phase in Apsim.Children(this, typeof(Phase)))
                Phases.Add(phase);



        }

        /// <summary>Called when [simulation commencing].</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("Commencing")]
        private void OnSimulationCommencing(object sender, EventArgs e)
        {
            Clear();
        }

        /// <summary>Called when crop is ending</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="data">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("PlantSowing")]
        private void OnPlantSowing(object sender, SowPlant2Type data)
        {
            if (data.Plant == Plant)
                Clear();
        }

        /// <summary>Called when crop is being harvested.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("Harvesting")]
        private void OnHarvesting(object sender, EventArgs e)
        {
            if (sender == Plant)
            {
                //Jump phenology to the end
                int EndPhase = Phases.Count;
                CurrentPhaseName = Phases[EndPhase - 1].Name;
            }
        }

        /// <summary>Called when crop is being prunned.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("Pruning")]
        private void OnPruning(object sender, EventArgs e)
        {
            Germinated = false;
            Emerged = false;
        }


        /// <summary>Called when crop is ending</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("PlantEnding")]
        private void OnPlantEnding(object sender, EventArgs e)
        {
            if (sender == Plant)
                Clear();
        }

        /// <summary>Called at the start of each day</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("StartOfDay")]
        private void OnStartOfDay(object sender, EventArgs e)
        {
            //reset all members to the CurrentlyOnFirstDayOfPhase array to nothing so new stages passed today can be inserted
            for (int i = 0; i < CurrentlyOnFirstDayOfPhase.Length; i++)
                CurrentlyOnFirstDayOfPhase[i] = "";
            //reset StagesPassedToday to zero to restart count for the new day
            StagesPassedToday = 0;
            if (PlantIsAlive)
                DaysAfterSowing += 1;
        }


        /// <summary>Look for a particular phase and return it's index or -1 if not found.</summary>
        /// <param name="Name">The name.</param>
        /// <returns></returns>
        public int IndexOfPhase(string Name)
        {
            for (int P = 0; P < Phases.Count; P++)
                if (Phases[P].Name.ToLower() == Name.ToLower())
                    return P;
            return -1;
        }

        /// <summary>
        /// A helper property that checks the parent plant (old or new) to see if it is alive.
        /// </summary>
        /// <value><c>true</c> if plant is alive; otherwise, <c>false</c>.</value>
        private bool PlantIsAlive
        {
            get
            {
                if (Plant != null && Plant.IsAlive)
                    return true;
                if (Plant15 != null && Plant15.IsAlive)
                    return true;
                return false;
            }
        }

        /// <summary>Called by sequencer to perform phenology.</summary>
        /// <remarks>
        /// Perform our daily timestep function. Get the current phase to do its
        /// development for the day. If TT is leftover after Phase is progressed,
        /// and the timestep for the subsequent phase is calculated using leftover TT
        /// </remarks>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.Exception">Cannot transition to the next phase. No more phases exist</exception>
        [EventSubscribe("DoPhenology")]
        private void OnDoPhenology(object sender, EventArgs e)
        {
            if (Control != null && Control.FixPhenology && Control.DoYPheno[CurrentPhaseIndex + 1] > 0)
            {
                DoPhenologyOnFixedDate();
            }
            else
            {
                if (PlantIsAlive)
                {
                    if (ThermalTime.Value() < 0)
                        throw new Exception("Negative Thermal Time, check the set up of the ThermalTime Function in" + this);
                    // If this is the first time through here then setup some variables.
                    if (Phases == null || Phases.Count == 0)
                        OnSimulationCommencing(null, null);

                    if (CurrentlyOnFirstDayOfPhase[0] == "")
                        if (JustInitialised)
                        {
                            CurrentlyOnFirstDayOfPhase[0] = Phases[0].Start;
                            JustInitialised = false;
                        }

                    double FractionOfDayLeftOver = CurrentPhase.DoTimeStep(1.0);

                    if (FractionOfDayLeftOver > 0)
                    {
                        while (FractionOfDayLeftOver > 0)// Transition to the next phase.
                        {
                            if (CurrentPhaseIndex + 1 >= Phases.Count)
                                throw new Exception("Cannot transition to the next phase. No more phases exist");

                            if (Stage >= 1)
                                Germinated = true;

                            if ((CurrentPhase is EmergingPhase) || (CurrentPhase is BuddingPhase))
                            {
                                Plant.SendEmergingEvent();
                                Emerged = true;
                            }

                            CurrentPhase = Phases[CurrentPhaseIndex + 1];
                            if (GrowthStage != null)
                                GrowthStage.Invoke();

                            // run the next phase with the left over time step from the phase we have just completed
                            FractionOfDayLeftOver = CurrentPhase.DoTimeStep(FractionOfDayLeftOver);

                            Stage = (CurrentPhaseIndex + 1) + CurrentPhase.FractionComplete - PhaseIndexOffset;
                        }
                    }
                    else
                    {
                        Stage = (CurrentPhaseIndex + 1) + CurrentPhase.FractionComplete - PhaseIndexOffset;
                    }

                    AccumulatedTT += CurrentPhase.TTForToday;

                    if (Emerged)
                        AccumulatedEmergedTT += CurrentPhase.TTForToday;

                    if (Plant != null)
                        if (Plant.IsAlive && PostPhenology != null)
                            PostPhenology.Invoke(this, new EventArgs());

                    GetTTinPhase = CurrentPhase.TTinPhase;              //Yuxi
                    GetAccumulatedTT += CurrentPhase.TTinPhase;        //Yuxi

                    Util.Debug("Phenology.CurrentPhaseName=%s", CurrentPhase.Name.ToLower());
                    Util.Debug("Phenology.CurrentStage=%f", Stage);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void DoPhenologyOnFixedDate()
        {
            if (PlantIsAlive)
            {
                if (ThermalTime.Value() < 0)
                    throw new Exception("Negative Thermal Time, check the set up of the ThermalTime Function in" + this);
                // If this is the first time through here then setup some variables.
                if (Phases == null || Phases.Count == 0)
                    OnSimulationCommencing(null, null);

                if (CurrentlyOnFirstDayOfPhase[0] == "")
                    if (JustInitialised)
                    {
                        CurrentlyOnFirstDayOfPhase[0] = Phases[0].Start;
                        JustInitialised = false;
                    }

                double FractionOfDayLeftOver = CurrentPhase.DoTimeStep(1.0);

                if (Clock.Today.DayOfYear == Control.DoYPheno[CurrentPhaseIndex + 1])
                {
                    if (CurrentPhaseIndex + 1 >= Phases.Count)
                        throw new Exception("Cannot transition to the next phase. No more phases exist");

                    if (Stage >= 1)
                        Germinated = true;

                    if ((CurrentPhase is EmergingPhase) || (CurrentPhase is BuddingPhase))
                    {
                        Plant.SendEmergingEvent();
                        Emerged = true;
                    }

                    CurrentPhase = Phases[CurrentPhaseIndex + 1];
                    if (GrowthStage != null)
                        GrowthStage.Invoke();

                    // run the next phase with the left over time step from the phase we have just completed
                    FractionOfDayLeftOver = CurrentPhase.DoTimeStep(FractionOfDayLeftOver);

                    Stage = (CurrentPhaseIndex + 1) + CurrentPhase.FractionComplete - PhaseIndexOffset;

                }
                else
                {
                    Stage = (CurrentPhaseIndex + 1) + CurrentPhase.FractionComplete - PhaseIndexOffset;
                }

                AccumulatedTT += CurrentPhase.TTForToday;

                if (Emerged)
                    AccumulatedEmergedTT += CurrentPhase.TTForToday;

                if (Plant != null)
                    if (Plant.IsAlive && PostPhenology != null)
                        PostPhenology.Invoke(this, new EventArgs());

                GetTTinPhase = CurrentPhase.TTinPhase;              //Yuxi
                GetAccumulatedTT += CurrentPhase.TTinPhase;        //Yuxi

                Util.Debug("Phenology.CurrentPhaseName=%s", CurrentPhase.Name.ToLower());
                Util.Debug("Phenology.CurrentStage=%f", Stage);
            }
        }

        /// <summary>A utility property to return the current phase.</summary>
        /// <value>The current phase.</value>
        /// <exception cref="System.Exception">
        /// Cannot jump to phenology phase:  + value + . Phase not found.
        /// or
        /// Cannot goto phase:  + GotoP.PhaseNameToGoto + . Phase not found.
        /// </exception>
        [XmlIgnore]
        public Phase CurrentPhase
        {
            get
            {
                if (CurrentPhaseIndex >= Phases.Count)
                    return null;
                else
                    return Phases[CurrentPhaseIndex];
            }

            private set
            {
                string oldPhaseName = CurrentPhase.Name;
                string stageOnEvent = CurrentPhase.End;
                //double TTRewound;
                double OldPhaseINdex = IndexOfPhase(CurrentPhase.Name);
                CurrentPhaseIndex = IndexOfPhase(value.Name);
                bool HarvestCall = false;
                if (CurrentPhaseIndex == Phases.Count - 1)
                    HarvestCall = true;
                PhaseIndexOffset = 0;
                if (CurrentPhaseIndex == -1)
                    throw new Exception("Cannot jump to phenology phase: " + value + ". Phase not found.");

                CurrentlyOnFirstDayOfPhase[StagesPassedToday] = CurrentPhase.Start;
                StagesPassedToday += 1;

                // If the new phase is a rewind or going ahead more that one phase(comming from a GoToPhase or PhaseSet Function), then reinitialise 
                // all phases that are being wound back over.
                if (((CurrentPhaseIndex <= OldPhaseINdex) && HarvestCall == false) || (CurrentPhaseIndex - OldPhaseINdex > 1) || (Phases[CurrentPhaseIndex] is GotoPhase))
                {
                    foreach (Phase P in Phases)
                    {
                        //Work out how much tt was accumulated at the stage we are resetting to and adjust accumulated TT accordingly
                        if (Phases[CurrentPhaseIndex] is GotoPhase)
                        { //Dont rewind thermal time for Goto phase.  Although it is moving phenology back it is a ongoing progression in phenology of the plant so TT accumulates
                        }
                        else if (IndexOfPhase(P.Name) >= CurrentPhaseIndex)
                        {//for Phase Set function we rewind phenology.  This is called by cut or graze which removes biomass and changes plant phenology so TT rewinds
                            AccumulatedTT -= P.TTinPhase;
                            if (IndexOfPhase(P.Name) >= 2)
                                AccumulatedEmergedTT -= P.TTinPhase;
                        }

                        //Reset phases we are rewinding over.
                        if (IndexOfPhase(P.Name) >= CurrentPhaseIndex)
                            P.ResetPhase();
                    }
                    if (Phases[CurrentPhaseIndex] is GotoPhase)
                    {
                        GotoPhase GotoP = (GotoPhase)Phases[CurrentPhaseIndex];
                        CurrentPhaseIndex = IndexOfPhase(GotoP.PhaseNameToGoto);
                        if (CurrentPhaseIndex == -1)
                            throw new Exception("Cannot goto phase: " + GotoP.PhaseNameToGoto + ". Phase not found.");
                    }
                    if (CurrentPhase.PhaseParallel != null)
                        PhaseIndexOffset = CurrentPhaseIndex - IndexOfPhase(CurrentPhase.PhaseParallel);
                }
                CurrentPhase.ResetPhase();
                // Send a PhaseChanged event.
                if (PhaseChanged != null)
                {
                    //_AccumulatedTT += CurrentPhase.TTinPhase;
                    PhaseChangedType PhaseChangedData = new PhaseChangedType();
                    PhaseChangedData.OldPhaseName = oldPhaseName;
                    PhaseChangedData.NewPhaseName = CurrentPhase.Name;
                    PhaseChangedData.EventStageName = stageOnEvent;
                    PhaseChanged.Invoke(Plant, PhaseChangedData);
                }
            }
        }

        /// <summary>A function that resets phenology to a specified stage</summary>
        /// <value>The current phase.</value>
        /// <exception cref="System.Exception">
        /// Cannot jump to phenology phase:  + value + . Phase not found.
        /// or
        /// Cannot goto phase:  + GotoP.PhaseNameToGoto + . Phase not found.
        /// </exception>
        public void ReSetToStage(double NewStage)
        {
            if (NewStage == 0)
                throw new Exception(this + "Must pass positive stage to set to");
            int SetPhaseIndex = Convert.ToInt32(Math.Floor(NewStage)) - 1;
            CurrentPhase = Phases[SetPhaseIndex];
            Phase Current = Phases[CurrentPhaseIndex];
            double proportionOfPhase = NewStage - CurrentPhaseIndex - 1;
            Current.FractionComplete = proportionOfPhase;
            if (PhaseRewind != null)
                PhaseRewind.Invoke(this, new EventArgs());
        }


        /// <summary>
        /// A utility function to return true if the simulation is on the first day of the
        /// specified stage.
        /// </summary>
        /// <param name="StageName">Name of the stage.</param>
        /// <returns></returns>
        public bool OnDayOf(String StageName)
        {
            bool StageToday = false;
            for (int i = 0; i < CurrentlyOnFirstDayOfPhase.Length; i++)
                if (CurrentlyOnFirstDayOfPhase[i] == StageName)
                    StageToday = true;

            return StageToday;
            //return (StageName.Equals(CurrentlyOnFirstDayOfPhase, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// A utility function to return true if the simulation is currently in the
        /// specified phase.
        /// </summary>
        /// <param name="PhaseName">Name of the phase.</param>
        /// <returns></returns>
        public bool InPhase(String PhaseName)
        {
            return CurrentPhase.Name.ToLower() == PhaseName.ToLower();
        }

        /// <summary>
        /// A utility function to return true if the simulation is currently between
        /// the specified start and end stages.
        /// </summary>
        /// <param name="Start">The start.</param>
        /// <param name="End">The end.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Cannot test between stages  + Start +   + End</exception>
        public bool Between(String Start, String End)
        {
            if (Phases == null)
                return false;

            string StartFractionSt = StringUtilities.SplitOffBracketedValue(ref Start, '(', ')');
            double StartFraction = 0;
            if (StartFractionSt != "")
                StartFraction = Convert.ToDouble(StartFractionSt);

            string EndFractionSt = StringUtilities.SplitOffBracketedValue(ref Start, '(', ')');
            double EndFraction = 0;
            if (EndFractionSt != "")
                EndFraction = Convert.ToDouble(EndFractionSt);

            int StartPhaseIndex = Phases.IndexOf(PhaseStartingWith(Start));
            int EndPhaseIndex = Phases.IndexOf(PhaseEndingWith(End));
            int CurrentPhaseIndex = 0;
            //Set the index of the current phase, if it is a parallel phase set the index to that of its parallel
            if (CurrentPhase.PhaseParallel == null)
                CurrentPhaseIndex = IndexOfPhase(CurrentPhase.Name);
            else CurrentPhaseIndex = IndexOfPhase(CurrentPhase.PhaseParallel);

            if (StartPhaseIndex == -1 || EndPhaseIndex == -1)
                throw new Exception("Cannot test between stages " + Start + " " + End);

            if (CurrentPhaseIndex == StartPhaseIndex)
                return CurrentPhase.FractionComplete >= StartFraction;

            else if (CurrentPhaseIndex == EndPhaseIndex)
                return CurrentPhase.FractionComplete <= EndPhaseIndex;

            else
                return CurrentPhaseIndex >= StartPhaseIndex && CurrentPhaseIndex <= EndPhaseIndex;
        }

        /// <summary>
        /// A utility function to return true if the simulation is at or past
        /// the specified startstage.
        /// </summary>
        /// <param name="Start">The start.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Cannot test between stages  + Start +   + End</exception>
        public bool Beyond(String Start)
        {
            string StartFractionSt = StringUtilities.SplitOffBracketedValue(ref Start, '(', ')');
            double StartFraction = 0;
            if (StartFractionSt != "")
                StartFraction = Convert.ToDouble(StartFractionSt);

            int StartPhaseIndex = Phases.IndexOf(PhaseStartingWith(Start));
            //Set the index of the current phase, if it is a parallel phase set the index to that of its parallel
            if (CurrentPhase.PhaseParallel == null)
                CurrentPhaseIndex = IndexOfPhase(CurrentPhase.Name);
            else CurrentPhaseIndex = IndexOfPhase(CurrentPhase.PhaseParallel);

            if (CurrentPhaseIndex >= StartPhaseIndex)
                return true;
            else
                return false;
        }

        /// <summary>
        /// A utility function to return the phenological phase that starts with
        /// the specified start stage name.
        /// </summary>
        /// <param name="Start">The start.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Unable to find phase starting with  + Start</exception>
        public Phase PhaseStartingWith(String Start)
        {
            foreach (Phase P in Phases)
                if (P.Start == Start)
                    return P;
            throw new Exception("Unable to find phase starting with " + Start);
        }

        /// <summary>
        /// A utility function to return the phenological phase that ends with
        /// the specified start stage name.
        /// </summary>
        /// <param name="End">The end.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Unable to find phase ending with  + End</exception>
        public Phase PhaseEndingWith(String End)
        {
            foreach (Phase P in Phases)
                if (P.End == End)
                    return P;
            throw new Exception("Unable to find phase ending with " + End);
        }

        /// <summary>A utility function to return true if a phenological phase is valid.</summary>
        /// <param name="Start">The start.</param>
        /// <returns></returns>
        public bool IsValidPhase(String Start)
        {
            foreach (Phase P in Phases)
                if (P.Start == Start)
                    return true;
            return false;
        }

        /// <summary>A utility function to return true if a phenological phase is valid.</summary>
        /// <param name="PhaseName">Name of the phase.</param>
        /// <returns></returns>
        public bool IsValidPhase2(String PhaseName)
        {
            foreach (Phase P in Phases)
                if (P.Name == PhaseName)
                    return true;
            return false;
        }

        /// <summary>Write phenology info to summary file.</summary>
        /// <param name="writer">The writer.</param>
        internal void WriteSummary(TextWriter writer)
        {
            writer.WriteLine("   Phases:");
            foreach (Phase P in Phases)
                P.WriteSummary(writer);
        }

        /// <summary>Respond to a remove biomass event.</summary>
        /// <param name="removeBiomPheno">The remove biom pheno.</param>
        internal void OnRemoveBiomass(double removeBiomPheno)
        {
            string existingStage = CurrentStageName;
            if (RewindDueToBiomassRemoved != null)
            {
                FractionBiomassRemoved = removeBiomPheno; // The RewindDueToBiomassRemoved function will use this.

                double ttCritical = TTInAboveGroundPhase;
                double removeFractPheno = RewindDueToBiomassRemoved.Value();
                double removeTTPheno = ttCritical * removeFractPheno;

                string msg;
                msg = "Phenology change:-\r\n";
                msg += "    Fraction DM removed  = " + removeBiomPheno.ToString() + "\r\n";
                msg += "    Fraction TT removed  = " + removeFractPheno.ToString() + "\r\n";
                msg += "    Critical TT          = " + ttCritical.ToString() + "\r\n";
                msg += "    Remove TT            = " + removeTTPheno.ToString() + "\r\n";

                double ttRemaining = removeTTPheno;
                for (int i = Phases.Count - 1; i >= 0; i--)
                {
                    Phase Phase = Phases[i];
                    if (Phase.TTinPhase > 0)
                    {
                        double ttCurrentPhase = Phase.TTinPhase;
                        if (ttRemaining > ttCurrentPhase)
                        {
                            Phase.ResetPhase();
                            ttRemaining -= ttCurrentPhase;
                            CurrentPhaseIndex -= 1;
                            if (CurrentPhaseIndex < 4)  //FIXME - hack to stop onEmergence being fired which initialises biomass parts
                            {
                                CurrentPhaseIndex = 4;
                                break;
                            }
                        }
                        else
                        {
                            Phase.Add(-ttRemaining);
                            // Return fraction of thermal time we are through the current
                            // phenological phase (0-1)
                            //double frac = Phase.FractionComplete;
                            //if (frac > 0.0 && frac < 1.0)  // Don't skip out of this stage - some have very low targets, eg 1.0 in "maturity"
                            //    currentStage = frac + floor(currentStage);

                            break;
                        }
                    }
                    else
                    { // phase is empty - not interested in it
                    }
                }
                Stage = (CurrentPhaseIndex + 1) + CurrentPhase.FractionComplete - PhaseIndexOffset;

                if (existingStage != CurrentStageName)
                {
                    PhaseChangedType PhaseChangedData = new PhaseChangedType();
                    PhaseChangedData.OldPhaseName = existingStage;
                    PhaseChangedData.NewPhaseName = CurrentPhase.Name;
                    PhaseChanged.Invoke(Plant, PhaseChangedData);
                    //Fixme MyPaddock.Publish(CurrentPhase.Start);
                }
            }
        }

        /// <summary>Gets the tt in above ground phase.</summary>
        /// <value>The tt in above ground phase.</value>
        /// <exception cref="System.Exception">Cannot find Phenology.AboveGroundPeriod function in xml file</exception>
        private double TTInAboveGroundPhase
        {
            get
            {
                if (AboveGroundPeriod == null)
                    throw new Exception("Cannot find Phenology.AboveGroundPeriod function in xml file");

                int SavedCurrentPhaseIndex = CurrentPhaseIndex;
                double TTInPhase = 0.0;
                for (CurrentPhaseIndex = 0; CurrentPhaseIndex < Phases.Count; CurrentPhaseIndex++)
                {
                    if (AboveGroundPeriod.Value() == 1)
                        TTInPhase += Phases[CurrentPhaseIndex].TTinPhase;
                }
                CurrentPhaseIndex = SavedCurrentPhaseIndex;
                return TTInPhase;
            }
        }
    }
}
