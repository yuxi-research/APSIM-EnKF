using System;
using System.Xml.Serialization;
using Models.Core;
using System.Threading;
using Models.DataAssimilation;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using Models.Sensitivity;

namespace Models
{
    /// <summary>
    /// The clock model
    /// </summary>
    [Serializable]
    [ViewName("UserInterface.Views.GridView")]
    [PresenterName("UserInterface.Presenters.PropertyPresenter")]
    [ValidParent(ParentType = typeof(Simulation))]
    public class Clock : Model, IClock
    {
        /// <summary>The arguments</summary>
        private EventArgs args = new EventArgs();

        // Links
        /// <summary>The summary</summary>
        [Link]
        private ISummary Summary = null;

        /// <summary>Gets or sets the start date.</summary>
        /// <value>The start date.</value>
        [Summary]
        [Description("The start date of the simulation")]
        public DateTime StartDate { get; set; }

        /// <summary>Gets or sets the end date.</summary>
        /// <value>The end date.</value>
        [Summary]
        [Description("The end date of the simulation")]
        public DateTime EndDate { get; set; }

        // Public events that we're going to publish.
        /// <summary>Occurs when [start of simulation].</summary>
        public event EventHandler StartOfSimulation;
        /// <summary>Occurs when [start of day].</summary>
        public event EventHandler StartOfDay;
        /// <summary>Occurs when [start of month].</summary>
        public event EventHandler StartOfMonth;
        /// <summary>Occurs when [start of year].</summary>
        public event EventHandler StartOfYear;
        /// <summary>Occurs when [start of week].</summary>
        public event EventHandler StartOfWeek;
        /// <summary>Occurs when [end of day].</summary>
        public event EventHandler EndOfDay;
        /// <summary>Occurs when [end of month].</summary>
        public event EventHandler EndOfMonth;
        /// <summary>Occurs when [end of year].</summary>
        public event EventHandler EndOfYear;
        /// <summary>Occurs when [end of week].</summary>
        public event EventHandler EndOfWeek;
        /// <summary>Occurs when [end of simulation].</summary>
        public event EventHandler EndOfSimulation;

        /// <summary>Occurs when [do weather].</summary>
        public event EventHandler DoWeather;
        /// <summary>Occurs when [do daily initialisation].</summary>
        public event EventHandler DoDailyInitialisation;
        /// <summary>Occurs when [do initial summary].</summary>
        public event EventHandler DoInitialSummary;
        /// <summary>Occurs when [do management].</summary>
        public event EventHandler DoManagement;
        /// <summary>Occurs when [do energy arbitration].</summary>
        public event EventHandler DoEnergyArbitration;                                //MicroClimate
        /// <summary>Occurs when [do soil water movement].</summary>
        public event EventHandler DoSoilWaterMovement;                                //Soil module
        /// <summary>Occurs when [do soil temperature].</summary>
        public event EventHandler DoSoilTemperature;
        //DoSoilNutrientDynamics will be here
        /// <summary>Occurs when [do soil organic matter].</summary>
        public event EventHandler DoSoilOrganicMatter;                                 //SurfaceOM
        /// <summary>Occurs when [do surface organic matter decomposition].</summary>
        public event EventHandler DoSurfaceOrganicMatterDecomposition;                 //SurfaceOM
        /// <summary>Occurs when [do water arbitration].</summary>
        public event EventHandler DoWaterArbitration;                                  //Arbitrator
        /// <summary>Occurs when [do phenology].</summary>                             
        public event EventHandler DoPhenology;                                         // Plant 
        /// <summary>Occurs when [do potential plant growth].</summary>
        public event EventHandler DoPotentialPlantGrowth;                              //Refactor to DoWaterLimitedGrowth  Plant        
        /// <summary>Occurs when [do potential plant partioning].</summary>
        public event EventHandler DoPotentialPlantPartioning;                          // PMF OrganArbitrator.
        /// <summary>Occurs when [do nutrient arbitration].</summary>
        public event EventHandler DoNutrientArbitration;                               //Arbitrator
        /// <summary>Occurs when [do potential plant partioning].</summary>
        public event EventHandler DoActualPlantPartioning;                             // PMF OrganArbitrator.
        /// <summary>Occurs when [do actual plant growth].</summary>
        public event EventHandler DoActualPlantGrowth;                                 //Refactor to DoNutirentLimitedGrowth Plant
        /// <summary>Occurs when [do plant growth].</summary>
        public event EventHandler DoPlantGrowth;                       //This will be removed when comms are better sorted  do not use  MicroClimate only
        /// <summary>Occurs when [do update].</summary>
        public event EventHandler DoUpdate;
        /// <summary>Occurs when [do management calculations].</summary>
        public event EventHandler DoManagementCalculations;
        /// <summary>Occurs when [do report calculations].</summary>
        public event EventHandler DoReportCalculations;
        /// <summary>Occurs when [do report].</summary>
        public event EventHandler DoReport;
        /// <summary> Process stock methods in GrazPlan Stock </summary>
        public event EventHandler DoStock;
        /// <summary> Process a Pest and Disease lifecycle object </summary>
        public event EventHandler DoLifecycle;

        /// <summary>WholeFarm update pasture</summary>
        public event EventHandler WFUpdatePasture;
        ///// <summary>WholeFarm update resources other than pasture</summary>
        //public event EventHandler WFGetResourcesRequired;
        /// <summary>WholeFarm cut and carry</summary>
        public event EventHandler WFDoCutAndCarry;
        /// <summary>WholeFarm Do Animal (Ruminant and Other) Breeding and milk calculations</summary>
        public event EventHandler WFAnimalBreeding;
        /// <summary>Get potential intake. This includes suckling milk consumption</summary>
        public event EventHandler WFPotentialIntake;
        /// <summary>Request and allocate resources to all Activities based on UI Tree order of priority. Some activities will obtain resources here and perform actions later</summary>
        public event EventHandler WFGetResourcesRequired;
        /// <summary>WholeFarm Calculate Animals (Ruminant and Other) milk production</summary>
        public event EventHandler WFAnimalMilkProduction;
        /// <summary>WholeFarm Calculate Animals(Ruminant and Other) weight gain</summary>
        public event EventHandler WFAnimalWeightGain;
        /// <summary>WholeFarm Do Animal (Ruminant and Other) death</summary>
        public event EventHandler WFAnimalDeath;
        /// <summary>WholeFarm Do Animal (Ruminant and Other) milking</summary>
        public event EventHandler WFAnimalMilking;
        /// <summary>WholeFarm Do Animal (Ruminant and Other) Herd Management (Kulling, Castrating, Weaning, etc.)</summary>
        public event EventHandler WFAnimalManage;
        /// <summary>WholeFarm stock animals to pasture availability or other metrics</summary>
        public event EventHandler WFAnimalStock;
        /// <summary>WholeFarm sell animals to market including transporting and labour</summary>
        public event EventHandler WFAnimalSell;
        /// <summary>WholeFarm Age your resources (eg. Decomose Fodder, Age your labour, Age your Animals)</summary>
        public event EventHandler WFAgeResources;
        // WholeFarm versions of the following events to ensure APSIM tasks perfomed before WF not yet implemented
        ///// <summary>WholeFarm start of simulation performed after APSIM StartOfSimulation</summary>
        //public event EventHandler WFStartOfSimulation;
        ///// <summary>WholeFarm start of month performed after APSIM StartOfMonth</summary>
        //public event EventHandler WFStartOfMonth;
        ///// <summary>WholeFarm end of month performed after APSIM EndOfMonth</summary>
        //public event EventHandler WFEndOfMonth;

        // Public properties available to other models.
        /// <summary>Gets the today.</summary>
        /// <value>The today.</value>
        [XmlIgnore]
        public DateTime Today { get; private set; }

        #region Data assimilation

        [Link(IsOptional = true)]
        IDataAssimilation DA = null;

        [Link(IsOptional = true)]
        Control Control = null;

        [Link(IsOptional = true)]
        InitialConditions Initial = null;

        [Link(IsOptional = true)]
        WeatherSens WeatherSens = null;
        [Link(IsOptional = true)]
        StateSens StateSens = null;

        [Link(IsOptional = true)]
        SoilTemperatureSens SoilTempSens = null;

        [Link(IsOptional = true)]
        BiomassSens BiomassSens = null;

        [Link(IsOptional = true)]
        LeafNodeSens LeafNodeSens = null;

        /// <summary>The Table ID. </summary>
        [XmlIgnore]
        public int ID
        {
            get
            {
                double Span = Today.Subtract(StartDate).Days;
                return Convert.ToInt16(Span);
            }
        }

        private int name;
        private static int threadNumber = 0;
        private static List<AutoResetEvent> resetEvents1 = new List<AutoResetEvent>();
        private static List<AutoResetEvent> resetEvents2 = new List<AutoResetEvent>();
        private static ManualResetEvent resetEvent3 = new ManualResetEvent(false);
        private static ManualResetEvent resetEvent4 = new ManualResetEvent(false);

        /// <summary>Occur when [PrepareAssimilation].</summary>
        public event EventHandler PrepareAssimilation;
        /// <summary>Occur when [NewDay].</summary>
        public event EventHandler NewDay;
        /// <summary>Occur when [Set Initial Condition].</summary>
        public event EventHandler SetInitialCondition;
        /// <summary>Occur when [Perturb Initial Condition].</summary>
        public event EventHandler PerturbInitialCondition;
        /// <summary>Occur when [Write prior result].</summary>
        public event EventHandler WritePriorResult;
        /// <summary>Occur when [Do Data Assimilation].</summary>
        public event EventHandler DoDataAssimilation;
        /// <summary>Occur when [Write prior result].</summary>
        public event EventHandler WritePosteriorResult;
        /// <summary>Occur when [WriteSQLite].</summary>
        public event EventHandler WriteSQLite;

        /// <summary>Occur when [WeatherSensitivity].</summary>
        public event EventHandler DoWeatherSens;
        /// <summary>Occur when [StateSensitivity].</summary>
        public event EventHandler DoStateSens;
        /// <summary>Occur when [SoilTemperatureSenstivitiy].</summary>
        public event EventHandler DoSoilTempSens;
        /// <summary>Occur when [BiomassSenstivitiy].</summary>
        public event EventHandler DoBiomassSens;
        /// <summary>Occur when [BiomassSenstivitiy].</summary>
        public event EventHandler DoLeafNodeSens;

        #endregion

        /// <summary>An event handler to allow us to initialise ourselves.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("Commencing")]
        private void OnSimulationCommencing(object sender, EventArgs e)
        {
            Today = StartDate;
        }

        /// <summary>An event handler to signal start of a simulation.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("DoCommence")]
        private void OnDoCommence(object sender, Simulation.CommenceArgs e)
        {
            if (DoInitialSummary != null)
                DoInitialSummary.Invoke(this, args);

            if (StartOfSimulation != null)
                StartOfSimulation.Invoke(this, args);


            #region Thread control 1.

            if (DA != null)
            {

                if (Thread.CurrentThread.Name == "Truth")
                {
                    name = Control.EnsembleSize;
                }
                else if (Thread.CurrentThread.Name == "OpenLoop")
                {
                    name = Control.EnsembleSize + 1;
                }
                else
                {
                    name = Convert.ToInt16(Thread.CurrentThread.Name.Substring(8));
                }
                if (name == Control.EnsembleSize)
                {
                    for (int i = 0; i < Control.EnsembleSize + 2; i++)
                    {
                        AutoResetEvent resetEvent = new AutoResetEvent(false);
                        resetEvents1.Add(resetEvent);
                    }

                    for (int i = 0; i < Control.EnsembleSize + 2; i++)
                    {
                        AutoResetEvent resetEvent = new AutoResetEvent(false);
                        resetEvents2.Add(resetEvent);
                    }
                }

                threadNumber++;

                Thread.Sleep(5000);

                //Note: Let the Clock module decide with thread do data assimilation.
                //Initialize only once.
                if (PrepareAssimilation != null && name == Control.EnsembleSize)    //Truth
                {
                    PrepareAssimilation.Invoke(this, args);
                }

                if (name == Control.EnsembleSize)   //Truth
                {
                    Console.WriteLine("Simulation started");
                }
            }
            #endregion

            while (Today <= EndDate)
            {

                // If this is being run on a background worker thread then check for cancellation
                if (e != null && e.workerThread != null && e.workerThread.CancellationPending)
                {
                    Summary.WriteMessage(this, "Simulation cancelled");
                    return;
                }

                #region Thread control 2.

                if (DA != null)
                {
                    if (name != Control.EnsembleSize)       //Not The Truth
                        resetEvents1[name].WaitOne();

                    if (NewDay != null && name == Control.EnsembleSize)     //Truth
                    {
                        NewDay.Invoke(this, args);
                        Console.WriteLine(Today.Date);
                    }

                    if (Today == Initial.InitialDate && SetInitialCondition != null)
                    {
                        SetInitialCondition.Invoke(this, args);
                    }

                    if (Today == Initial.InitialDate && name != Control.EnsembleSize && PerturbInitialCondition != null)
                    {
                        PerturbInitialCondition.Invoke(this, args);
                    }

                }

                #endregion

                if (StateSens != null && DoStateSens != null)
                    if (StateSens.Date == Today)
                        DoStateSens.Invoke(this, args);

                if (BiomassSens != null && DoBiomassSens != null)
                    if (BiomassSens.Date == Today)
                        DoBiomassSens.Invoke(this, args);

                if (LeafNodeSens != null && DoLeafNodeSens != null)
                    if (LeafNodeSens.Date == Today)
                        DoLeafNodeSens.Invoke(this, args);

                if (DoWeather != null)
                    DoWeather.Invoke(this, args);

                if (WeatherSens != null && DoWeatherSens != null)
                    DoWeatherSens.Invoke(this, args);

                if (DoDailyInitialisation != null)
                    DoDailyInitialisation.Invoke(this, args);


                if (StartOfDay != null)
                    StartOfDay.Invoke(this, args);

                if (Today.Day == 1 && StartOfMonth != null)
                    StartOfMonth.Invoke(this, args);

                if (Today.DayOfYear == 1 && StartOfYear != null)
                    StartOfYear.Invoke(this, args);

                if (Today.DayOfWeek == DayOfWeek.Sunday && StartOfWeek != null)
                    StartOfWeek.Invoke(this, args);

                if (Today.DayOfWeek == DayOfWeek.Saturday && EndOfWeek != null)
                    EndOfWeek.Invoke(this, args);

                if (DoManagement != null)
                    DoManagement.Invoke(this, args);

                if (DoEnergyArbitration != null)
                    DoEnergyArbitration.Invoke(this, args);

                if (DoSoilWaterMovement != null)
                    DoSoilWaterMovement.Invoke(this, args);

                if (DoSoilTemperature != null)
                    DoSoilTemperature.Invoke(this, args);

                if (SoilTempSens != null && DoSoilTempSens != null)
                    if (SoilTempSens.Date == Today)
                        DoSoilTempSens.Invoke(this, args);

                if (DoSoilOrganicMatter != null)
                    DoSoilOrganicMatter.Invoke(this, args);

                if (DoSurfaceOrganicMatterDecomposition != null)
                    DoSurfaceOrganicMatterDecomposition.Invoke(this, args);

                if (DoWaterArbitration != null)
                    DoWaterArbitration.Invoke(this, args);

                if (DoPhenology != null)
                    DoPhenology.Invoke(this, args);

                if (DoPotentialPlantGrowth != null)
                    DoPotentialPlantGrowth.Invoke(this, args);

                if (DoPotentialPlantPartioning != null)
                    DoPotentialPlantPartioning.Invoke(this, args);

                if (DoNutrientArbitration != null)
                    DoNutrientArbitration.Invoke(this, args);

                if (DoActualPlantPartioning != null)
                    DoActualPlantPartioning.Invoke(this, args);

                if (DoActualPlantGrowth != null)
                    DoActualPlantGrowth.Invoke(this, args);

                if (DoPlantGrowth != null)
                    DoPlantGrowth.Invoke(this, args);

                if (DoUpdate != null)
                    DoUpdate.Invoke(this, args);

                if (DoManagementCalculations != null)
                    DoManagementCalculations.Invoke(this, args);

                if (DoStock != null)
                    DoStock.Invoke(this, args);

                if (DoLifecycle != null)
                    DoLifecycle.Invoke(this, args);

                if (DoReportCalculations != null)
                    DoReportCalculations.Invoke(this, args);

                #region Thread Control 3 & Data Assimilation.

                if (DA != null)
                {

                    //Write all results.
                    if (WritePriorResult != null)
                        WritePriorResult.Invoke(this, args);

                    if (name != Control.EnsembleSize - 1)     //Not The last ensemble
                    {
                        //If not the last thread, let the next thread run a new day.
                        resetEvents1[GetNext(name, Control.EnsembleSize + 2)].Set();
                        resetEvent3.WaitOne();
                    }
                    else
                    {   //The last ensemble
                        //If the last thread, let all threads continue.
                        //Hold all thread until the last thread finish.

                        if (Control.DAOption != "" && DoDataAssimilation != null) //Control.DAOption != "" &&
                            DoDataAssimilation.Invoke(this, args);

                        Thread.Sleep(10);
                        resetEvent3.Set();
                        Thread.Sleep(10);
                        resetEvent3.Reset();
                    }

                    if (name != Control.EnsembleSize)   //Truth
                        resetEvents2[name].WaitOne();

                    //Calculate error covariances.
                    if (WritePosteriorResult != null)
                        WritePosteriorResult.Invoke(this, args);

                    //Release all threads for the next day.
                }


                #endregion

                if (Today == EndDate && EndOfSimulation != null)
                    EndOfSimulation.Invoke(this, args);

                if (Today.Day == 31 && Today.Month == 12 && EndOfYear != null)
                    EndOfYear.Invoke(this, args);

                if (Today.AddDays(1).Day == 1 && EndOfMonth != null) // is tomorrow the start of a new month?
                {
                    // WholeFarm events performed before APSIM EndOfMonth
                    if (WFUpdatePasture != null)
                        WFUpdatePasture.Invoke(this, args);
                    if (WFDoCutAndCarry != null)
                        WFDoCutAndCarry.Invoke(this, args);
                    if (WFAnimalBreeding != null)
                        WFAnimalBreeding.Invoke(this, args);
                    if (WFPotentialIntake != null)
                        WFPotentialIntake.Invoke(this, args);
                    if (WFGetResourcesRequired != null)
                        WFGetResourcesRequired.Invoke(this, args);
                    if (WFAnimalMilkProduction != null)
                        WFAnimalMilkProduction.Invoke(this, args);
                    if (WFAnimalWeightGain != null)
                        WFAnimalWeightGain.Invoke(this, args);
                    if (WFAnimalDeath != null)
                        WFAnimalDeath.Invoke(this, args);
                    if (WFAnimalMilking != null)
                        WFAnimalMilking.Invoke(this, args);
                    if (WFAnimalManage != null)
                        WFAnimalManage.Invoke(this, args);
                    if (WFAnimalStock != null)
                        WFAnimalStock.Invoke(this, args);
                    if (WFAnimalSell != null)
                        WFAnimalSell.Invoke(this, args);
                    if (WFAgeResources != null)
                        WFAgeResources.Invoke(this, args);

                    EndOfMonth.Invoke(this, args);
                }

                if (EndOfDay != null)
                    EndOfDay.Invoke(this, args);

                if (DoReport != null)
                    DoReport.Invoke(this, args);

                // Write all results to DAControl.
                if (Today == EndDate && Control != null && name == Control.EnsembleSize - 1)   //The last ensemble
                    if (DA != null && Control.WriteSQL == true && WriteSQLite != null)
                    {
                        WriteSQLite.Invoke(this, args);
                    }

                Today = Today.AddDays(1);

                #region Thread Control 4.
                if (DA != null)
                {
                    if (name != Control.EnsembleSize - 1)   //Not The last ensemble
                    {
                        resetEvents2[GetNext(name, Control.EnsembleSize + 2)].Set();
                        resetEvent4.WaitOne();
                    }
                    else
                    {           //The last ensemble
                        Thread.Sleep(10);
                        resetEvent4.Set();
                        Thread.Sleep(10);
                        resetEvent4.Reset();
                    }
                }
                #endregion


            }
            Summary.WriteMessage(this, "Simulation terminated normally");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <param name="length"></param>
        public int GetNext(int n, int length)
        {
            return (n + 1) % length;
        }
    }
}