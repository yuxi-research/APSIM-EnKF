using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Core;
using Models.PMF.OldPlant;
using Models.DataAssimilation.DataType;
using System.Threading;
using Models.Soils;
using Models.Soils.SoilWaterBackend;
using System.Data.SQLite;
using Models.PMF;
using APSIM.Shared.Utilities;

namespace Models.DataAssimilation
{
    /// <summary>
    /// A class for state variales.
    /// This class links all the model states and store them in StateTable class.
    /// Get and set states in modules.
    /// </summary>
    [Serializable]
    [ViewName("UserInterface.Views.ProfileView")]
    [PresenterName("UserInterface.Presenters.ProfilePresenter")]
    [ValidParent(ParentType = typeof(Simulation))]
    [ValidParent(ParentType = typeof(IDataAssimilation))]
    [ValidParent(ParentType = typeof(Control))]

    public class StateVariables : Model
    {

        #region ******* Links. *******

        [Link]
        Clock Clock = null;

        [Link]
        Control Control = null;


        //[Link(IsOptional = true)]
        //Plant15 Plant = null;

        [Link(IsOptional = true)]
        Leaf1 Leaf = null;

        [Link(IsOptional = true)]
        Stem1 Stem = null;

        [Link(IsOptional = true)]
        Root1 Root = null;

        [Link(IsOptional = true)]
        Grain Grain = null;

        [Link(IsOptional = true)]
        Pod Pod = null;

        [Link(IsOptional = true)]
        SoilWater SoilWater = null;

        [Link(IsOptional = true)]
        CERESSoilTemperature SoilTemperature = null;

        [Link(IsOptional = true)]
        SoilNitrogen SoilNitrogen = null;

        [Link(IsOptional = true)]
        Soil Soil = null;

        //[Link(IsOptional = true)]
        //Phenology Phenology = null;

        //[Link(IsOptional = true)]
        //SurfaceOrganicMatter SurfaceOrganicMatter = null;



        #endregion

        #region ******* Public field. *******

        /// <summary>State variables involved in the State vector.</summary>
        [Description("State variables involved in the State vectors")]
        public string[] StateNames { get; set; }

        /// <summary>ModelError</summary>
        [Description("Model physics error")]
        public double[] ModelError { get; set; }

        /// <summary>ModelErrorOption</summary>
        [Description("Model physics error option")]
        public int[] ModelErrorOption { get; set; }

        /// <summary>The numbler of state variables involved in the State vector.</summary>
        [XmlIgnore]
        public int Count { get { return StateNames.Count(); } }

        /// <summary>State variable values.</summary>
        [XmlIgnore]
        public double[] StateValues { get; set; }
        #endregion

        #region ******* Private field. *******

        //Store all the states here.
        private static List<StateTable> StatesData = new List<StateTable>();
        private FolderInfo Info = new FolderInfo();

        #endregion

        #region ******* EventHandlers. *******

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [EventSubscribe("PrepareAssimilation")]
        private void OnPrepareAssimilation(object sender, EventArgs e)
        {
            if (Control.DAOption != null && Thread.CurrentThread.Name == "Truth")
                NewStatesData();

            ////Another option is to use a static bool variable: IsInitialising.
            ////if (IsInitialising)
            ////{
            ////    Initialize();
            ////    IsInitialising = false;
            ////}
        }

        [EventSubscribe("NewDay")]
        private void OnNewDay(object sender, EventArgs e)
        {
            if (Control.DAOption != null && Thread.CurrentThread.Name == "Truth")
                foreach (StateTable table in StatesData)
                {
                    table.NewRow(Clock.ID, Clock.Today);
                }
        }

        /// <summary> Write prior results. </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [EventSubscribe("WritePriorResult")]
        private void OnWritePriorResult(object sender, EventArgs e)
        {
            if (Control.DAOption != null)
            {
                Insert();
            }
        }

        /// <summary> Write Posterior results. </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [EventSubscribe("WritePosteriorResult")]
        private void OnWritePosteriorResult(object sender, EventArgs e)
        {
            if (Control.DAOption != null)
            {
                Update();
            }
        }

        /// <summary> Write Posterior results. </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [EventSubscribe("WriteSQLite")]
        private void OnWriteSQLite(object sender, EventArgs e)
        {
            if (Control.DAOption != null)
            {
                WriteSQLite();
            }
        }
        #endregion

        #region ******* Methods. *******

        /// <summary>
        /// Initialize the StateTables.
        /// Call on the first of data assimilation day.
        /// </summary>
        public void NewStatesData()
        {
            foreach (string stateName in StateNames)
            {
                StateTable temp = new StateTable(stateName, Control.EnsembleSize);
                StatesData.Add(temp);
            }
        }

        #region ******** Data exchange between APSIM and StateTable. ********
        /// <summary>
        /// Insert all prior state variables of a single ensemble to StateTable.
        /// </summary>
        public void Insert()
        {
            double value;
            string columnName = Thread.CurrentThread.Name;
            if (columnName.Contains("Ensemble") || columnName == "OpenLoop")
            {
                columnName = "Prior" + columnName;
            }

            foreach (StateTable table in StatesData)
            {
                value = GetFromApsim(table.TableName);
                table.InsertSingle(value, Clock.ID, columnName);
            }
        }

        /// <summary>
        /// Update all posterior state variables of a single ensemble to Apsim.
        /// </summary>
        public void Update()
        {
            double value;
            string columnName = Thread.CurrentThread.Name;
            if (columnName.Contains("Ensemble"))
            {
                columnName = "Posterior" + columnName;

                foreach (StateTable table in StatesData)
                {
                    value = table.GetSingle(Clock.ID, columnName);
                    ReturnToApsim(table.TableName, value);
                }
            }
        }

        /// <summary>
        /// Get the value of state variables based on their names.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public double GetFromApsim(string tableName)
        {
            double value;
            switch (tableName)
            {
                // Plant.
                case "LAI":
                    value = Leaf.LAI;
                    break;
                case "Height":
                    value = Leaf.Height;
                    break;
                case "RootDepth":
                    value = Root.RootDepth;
                    break;
                case "Width":
                    value = Stem.Width;
                    break;

                case "PEP":
                    value = Leaf.PotentialEP;
                    break;

                //Soil.
                case "SW1":
                    value = SoilWater.SW[0];
                    break;
                case "SW2":
                    value = SoilWater.SW[1];
                    break;
                case "SW3":
                    value = SoilWater.SW[2];
                    break;
                case "SW4":
                    value = SoilWater.SW[3];
                    break;
                case "SW5":
                    value = SoilWater.SW[4];
                    break;
                case "SW6":
                    value = SoilWater.SW[5];
                    break;
                case "SW7":
                    value = SoilWater.SW[6];
                    break;

                case "ST1":
                    value = SoilTemperature.Value[0];
                    Console.WriteLine("Warning: ST is not a prognostic state");
                    break;
                case "ST2":
                    value = SoilTemperature.Value[1];
                    Console.WriteLine("Warning: ST is not a prognostic state");
                    break;
                case "ST3":
                    value = SoilTemperature.Value[2];
                    Console.WriteLine("Warning: ST is not a prognostic state");
                    break;
                case "ST4":
                    value = SoilTemperature.Value[3];
                    Console.WriteLine("Warning: ST is not a prognostic state");
                    break;
                case "ST5":
                    value = SoilTemperature.Value[4];
                    Console.WriteLine("Warning: ST is not a prognostic state");
                    break;
                case "ST6":
                    value = SoilTemperature.Value[5];
                    Console.WriteLine("Warning: ST is not a prognostic state");
                    break;
                case "ST7":
                    value = SoilTemperature.Value[6];
                    Console.WriteLine("Warning: ST is not a prognostic state");
                    break;

                case "SoilNO3_1":
                    value = SoilNitrogen.NO3[0];
                    break;
                case "SoilNO3_2":
                    value = SoilNitrogen.NO3[1];
                    break;
                case "SoilNO3_3":
                    value = SoilNitrogen.NO3[2];
                    break;
                case "SoilNO3_4":
                    value = SoilNitrogen.NO3[3];
                    break;
                case "SoilNO3_5":
                    value = SoilNitrogen.NO3[4];
                    break;
                case "SoilNO3_6":
                    value = SoilNitrogen.NO3[5];
                    break;
                case "SoilNO3_7":
                    value = SoilNitrogen.NO3[6];
                    break;

                case "SoilNH4_1":
                    value = SoilNitrogen.NH4[0];
                    break;
                case "SoilNH4_2":
                    value = SoilNitrogen.NH4[1];
                    break;
                case "SoilNH4_3":
                    value = SoilNitrogen.NH4[2];
                    break;
                case "SoilNH4_4":
                    value = SoilNitrogen.NH4[3];
                    break;
                case "SoilNH4_5":
                    value = SoilNitrogen.NH4[4];
                    break;
                case "SoilNH4_6":
                    value = SoilNitrogen.NH4[5];
                    break;
                case "SoilNH4_7":
                    value = SoilNitrogen.NH4[6];
                    break;

                case "LeafN":
                    value = GetBiomass(Leaf.Live, "N");
                    break;
                case "StemN":
                    value = GetBiomass(Stem.Live, "N");
                    break;
                case "GrainN":
                    value = GetBiomass(Grain.Live, "N");
                    break;
                case "PodN":
                    value = GetBiomass(Pod.Live, "N");
                    break;
                case "RootN":
                    value = GetBiomass(Root.Live, "N");
                    break;

                case "LeafWt":
                    value = GetBiomass(Leaf.Live, "Wt");
                    break;
                case "StemWt":
                    value = GetBiomass(Stem.Live, "Wt");
                    break;
                case "GrainWt":
                    value = GetBiomass(Grain.Live, "Wt");
                    break;
                case "PodWt":
                    value = GetBiomass(Pod.Live, "Wt");
                    break;
                case "RootWt":
                    value = GetBiomass(Root.Live, "Wt");
                    break;
                default:
                    {
                        value = -99;
                        break;
                    }
            }
            return value;
        }

        /// <summary>
        /// Get the value of state variables based on their names.
        /// Tips: APSIM states can be updated through Links!
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="value"></param>
        public void ReturnToApsim(string tableName, double value)
        {

            double[] newSW = SoilWater.SW;
            double[] newST = SoilTemperature.Value;
            double[] newSNO3 = SoilNitrogen.NO3;
            double[] newSNH4 = SoilNitrogen.NH4;

            switch (tableName)
            {
                // Plant.
                case "LAI":
                    Leaf.LAI = Constrain(value, 0, value + 1);
                    break;
                case "Height":
                    Stem.Height = Constrain(value, 0, value + 1);
                    break;
                case "RootDepth":
                    double Max = MathUtilities.Sum(Soil.Thickness);
                    Root.RootDepth = Constrain(value, 0, Max); ;
                    break;

                case "Width":
                    Stem.Width = Constrain(value, 0, value + 1);
                    break;

                case "PEP":
                    Leaf.PotentialEP = Constrain(value, 0, value + 1);
                    break;

                // Soil.
                case "SW1":
                    newSW[0] = Constrain(value, 0, 1);
                    break;
                case "SW2":
                    newSW[1] = Constrain(value, 0, 1);
                    break;
                case "SW3":
                    newSW[2] = Constrain(value, 0, 1);
                    break;
                case "SW4":
                    newSW[3] = Constrain(value, 0, 1);
                    break;
                case "SW5":
                    newSW[4] = Constrain(value, 0, 1);
                    break;
                case "SW6":
                    newSW[5] = Constrain(value, 0, 1);
                    break;
                case "SW7":
                    newSW[6] = Constrain(value, 0, 1);
                    break;

                case "ST1":
                    newST[0] = Constrain(value, 0, value + 1);
                    break;
                case "ST2":
                    newST[1] = Constrain(value, 0, value + 1);
                    break;
                case "ST3":
                    newST[2] = Constrain(value, 0, value + 1);
                    break;
                case "ST4":
                    newST[3] = Constrain(value, 0, value + 1);
                    break;
                case "ST5":
                    newST[4] = Constrain(value, 0, value + 1);
                    break;
                case "ST6":
                    newST[5] = Constrain(value, 0, value + 1);
                    break;
                case "ST7":
                    newST[6] = Constrain(value, 0, value + 1);

                    break;
                case "SoilNO3_1":
                    newSNO3[0] = Constrain(value, 0, value + 1);
                    break;
                case "SoilNO3_2":
                    newSNO3[1] = Constrain(value, 0, value + 1);
                    break;
                case "SoilNO3_3":
                    newSNO3[2] = Constrain(value, 0, value + 1);
                    break;
                case "SoilNO3_4":
                    newSNO3[3] = Constrain(value, 0, value + 1);
                    break;
                case "SoilNO3_5":
                    newSNO3[4] = Constrain(value, 0, value + 1);
                    break;
                case "SoilNO3_6":
                    newSNO3[5] = Constrain(value, 0, value + 1);
                    break;
                case "SoilNO3_7":
                    newSNO3[6] = Constrain(value, 0, value + 1);
                    break;

                case "SoilNH4_1":
                    newSNH4[0] = Constrain(value, 0, value + 1);
                    break;
                case "SoilNH4_2":
                    newSNH4[1] = Constrain(value, 0, value + 1);
                    break;
                case "SoilNH4_3":
                    newSNH4[2] = Constrain(value, 0, value + 1);
                    break;
                case "SoilNH4_4":
                    newSNH4[3] = Constrain(value, 0, value + 1);
                    break;
                case "SoilNH4_5":
                    newSNH4[4] = Constrain(value, 0, value + 1);
                    break;
                case "SoilNH4_6":
                    newSNH4[5] = Constrain(value, 0, value + 1);
                    break;
                case "SoilNH4_7":
                    newSNH4[6] = Constrain(value, 0, value + 1);
                    break;

                case "LeafN":
                    SetBiomass(Leaf.Live, "N", value);
                    break;
                case "StemN":
                    SetBiomass(Stem.Live, "N", value);
                    break;
                case "GrainN":
                    SetBiomass(Grain.Live, "N", value);
                    break;
                case "PodN":
                    SetBiomass(Pod.Live, "N", value);
                    break;
                case "RootN":
                     SetBiomass(Root.Live, "N", value);
                    break;

                case "LeafWt":
                    SetBiomass(Leaf.Live, "Wt", value);
                    break;
                case "StemWt":
                    SetBiomass(Stem.Live, "Wt", value);
                    break;
                case "GrainWt":
                     SetBiomass(Grain.Live, "Wt", value);
                    break;
                case "PodWt":
                    SetBiomass(Pod.Live, "Wt", value);
                    break;
                case "RootWt":
                    SetBiomass(Root.Live, "Wt", value);
                    break;

                default:
                    Console.WriteLine("Warning: Wrong state variable name!");
                    break;
            }
            SoilWater.SW = newSW;
            SoilTemperature.Value = newST;
            SoilNitrogen.NO3 = newSNO3;
            SoilNitrogen.NH4 = newSNH4;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="temp"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        private double GetBiomass(Biomass temp, string state)
        {
            double a, b, c;
            if (state == "Wt")
            {
                a = temp.StructuralWt;
                b = temp.NonStructuralWt;
                c = temp.MetabolicWt;
            }
            else if (state == "N")
            {
                a = temp.StructuralN;
                b = temp.NonStructuralN;
                c = temp.MetabolicN;
            }
            else
                throw new Exception("Unknow biomass data type!");
            return a + b + c;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="temp"></param>
        /// <param name="state"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private Biomass SetBiomass(Biomass temp, string state, double value)
        {
            double a = 0, b = 0, c = 0, sum;

            if (state == "Wt")
            {
                a = temp.StructuralWt;
                b = temp.NonStructuralWt;
                c = temp.MetabolicWt;
                sum = a + b + c;
                if (sum != 0)
                {
                    a = a / sum;
                    b = b / sum;
                    c = c / sum;
                }
                else
                {
                    a = 0;
                    b = 0;
                    c = 0;
                }

                if (Math.Abs (value - sum) > 0.0001)
                {
                    temp.StructuralWt = a * value;
                    temp.NonStructuralWt = b * value;
                    temp.MetabolicWt = c * value;
                }
            }
            else if (state == "N")
            {
                a = temp.StructuralN;
                b = temp.NonStructuralN;
                c = temp.MetabolicN;
                sum = a + b + c;
                if (sum != 0)
                {
                    a = a / sum;
                    b = b / sum;
                    c = c / sum;
                }
                else
                {
                    a = 0;
                    b = 0;
                    c = 0;
                }

                if (Math.Abs(value - sum) > 0.0001)
                {
                    temp.StructuralN = a * value;
                    temp.NonStructuralN = b * value;
                    temp.MetabolicN = c * value;
                }
            }
            else
            {
                Console.WriteLine("Warning: Wrong niomass component!");
            }
            return temp;
        }

        /// <summary>
        /// Constrain values in a range.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        /// <returns></returns>
        public double Constrain(double value, double lower, double upper)
        {
            return Math.Min(Math.Max(lower, value), upper);
        }

        #endregion

        #region ******** Data exchange between StateTable and IDataAssiimlation. ********
        //Via StatesOfTheDay.

        /// <summary>
        /// From StateTable to StateOfTheDay.
        /// Called by IDataAssimilaiton At the begining of DoDataAssimilation.
        /// </summary>
        /// <returns></returns>
        public StatesOfTheDay GetPrior()
        {
            StatesOfTheDay States = new StatesOfTheDay();
            double[] ensemble;
            double openLoop;
            foreach (StateTable table in StatesData)
            {
                ensemble = table.ReadPrior(Clock.ID, Control.EnsembleSize);
                States.Prior.Add(ensemble);
                openLoop = table.ReadOpenLoop(Clock.ID);
                States.PriorOL.Add(openLoop);
            }
            return States;
        }

        /// <summary>
        /// Call by StateVariables At the end of DoDataAssimilation.
        /// </summary>
        /// <returns></returns>
        public void ReturnPosterior(StatesOfTheDay states)
        {
            int tableIndex = 0;
            foreach (StateTable table in StatesData)
            {
                table.InsertPosterior(Clock.ID, states, Control.DAOption, tableIndex, Control.EnsembleSize);
                tableIndex++;
            }
        }
        #endregion

        #region ******** Write results to SQLite. ********

        /// <summary>
        /// 
        /// </summary>
        public void WriteSQLite()
        {
            Console.WriteLine("Creating SQLite...");
            SQLiteConnection.CreateFile(Info.SQLite);
            SQLiteConnection sqlCon = new SQLiteConnection("Data Source=" + Info.SQLite);
            sqlCon.Open();
            double span = Clock.Today.Subtract(Clock.StartDate).TotalDays + 1;
            int length = Convert.ToInt16(span);
            int temp = -1;
            foreach (StateTable table in StatesData)
            {
                ++temp;
                if (temp < 100)   //temp == 0: Write to LAI only.
                { }
                else
                {
                    continue;
                }

                Console.Write("\tWriting to Table [{0}]...", table.TableName);
                CreateSQLiteTable(Control.EnsembleSize, table.TableName, sqlCon, Info);
                InsertRows(table, sqlCon, length);

                List<string> ColumnNames = new List<string>();
                ColumnNames.Add("Truth");
                ColumnNames.Add("PriorOpenLoop");
                for (int i = 0; i < Control.EnsembleSize; i++)
                    ColumnNames.Add("PriorEnsemble" + i.ToString());
                ColumnNames.Add("PriorMean");

                ColumnNames.Add("PosteriorOpenLoop");
                for (int i = 0; i < Control.EnsembleSize; i++)
                    ColumnNames.Add("PosteriorEnsemble" + i.ToString());
                ColumnNames.Add("PosteriorMean");

                ColumnNames.Add("Obs");
                for (int i = 0; i < Control.EnsembleSize; i++)
                    ColumnNames.Add("ObsEnsemble" + i.ToString());

                string sqlStr;
                SQLiteTransaction sqlTran = sqlCon.BeginTransaction();
                SQLiteCommand command = sqlCon.CreateCommand();

                command.Transaction = sqlTran;

                foreach (string colName in ColumnNames)
                {
                    for (int i = 0; i < length; i++)
                    {
                        sqlStr = "UPDATE " + table.TableName + " SET ";
                        sqlStr += colName + "='" + table.Table.Rows[i][colName] + "' WHERE ID= '" + i + "'";
                        command.CommandText = sqlStr;
                        command.ExecuteNonQuery();
                    }
                    //Console.WriteLine(table.TableName + " --> " + colName);
                }
                 sqlTran.Commit();
                Console.WriteLine("Done!");
            }
            sqlCon.Close();
            Console.WriteLine("SQLite has been created!");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ensembleSize"></param>
        /// <param name="tableName"></param>
        /// <param name="sqlCon"></param>
        /// <param name="info"></param>
        public static void CreateSQLiteTable(int ensembleSize, string tableName, SQLiteConnection sqlCon, FolderInfo info)
        {
            string sqlStr = "CREATE TABLE " + tableName + " (ID int PRIMARY KEY, Date string, DOY int, Truth double, ";

            sqlStr += "PriorOpenLoop double, ";
            for (int i = 0; i < ensembleSize; i++)
            {
                sqlStr += "PriorEnsemble" + i + " double, ";
            }
            sqlStr += "PriorMean double, ";
            sqlStr += "PosteriorOpenLoop double, ";
            for (int i = 0; i < ensembleSize; i++)
            {
                sqlStr += "PosteriorEnsemble" + i + " double, ";
            }
            sqlStr += "PosteriorMean double, ";

            sqlStr += "Obs double";
            for (int i = 0; i < ensembleSize; i++)
            {
                sqlStr += ", ObsEnsemble" + i + " double";
            }
            sqlStr += ")";

            SQLiteCommand command = new SQLiteCommand(sqlStr, sqlCon);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="sqlCon"></param>
        /// <param name="length"></param>
        public void InsertRows(StateTable table, SQLiteConnection sqlCon, int length)
        {
            string sqlStr;
            SQLiteCommand command;
            for (int j = 0; j < length; j++)
            {
                sqlStr = "INSERT INTO " + table.TableName;
                sqlStr += " (ID,Date, DOY) VALUES (";
                sqlStr += "'" + table.Table.Rows[j]["ID"] + "', ";
                sqlStr += "'" + table.Table.Rows[j]["Date"].ToString() + "', ";
                sqlStr += "'" + table.Table.Rows[j]["DOY"] + "')";

                command = new SQLiteCommand(sqlStr, sqlCon);
                command.ExecuteNonQuery();
            }
        }

        #endregion

        #endregion

    }
}
