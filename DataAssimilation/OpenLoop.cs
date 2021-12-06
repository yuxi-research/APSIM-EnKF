using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Data.SQLite;
using System.IO;

namespace DataAssimilation
{
    [Serializable]
    public class OpenLoop
    {

        private FolderStructure folder = new FolderStructure(0);   //  YUXI: remove a "../" when starting from APSIM.
        private DAControl control = new DAControl(0);
        private Distribution dis = new Distribution();
        public int Day;

        public double[] SinglePriorState;
        public List<double[]> AllPriorStates;

        public Matrix PriorStates { get; set; }
        public Matrix PriorMean { get; set; }

        public Matrix PosteriorStates { get; set; }
        public Matrix PosteriorMean { get; set; }

        /// <summary>Constructor.</summary>
        public OpenLoop()
        { }

        public void DoOpenLoop(int day, Matrix Obs, Matrix ObsIndices)
        {
            Day = day;
            ReadAllStates(control.StateNames);

            this.PriorMean = new DataAssimilation.Matrix(PriorStates.Row, 1);
            Calc_PriorMean();

            this.PosteriorStates = new DataAssimilation.Matrix(PriorStates.Row, PriorStates.Col);
            Add_ModelError();
            this.PosteriorMean = new DataAssimilation.Matrix(PosteriorStates.Row, 1);
            Calc_PosteriorMean();
            UpdateAllStates();
        }

        public void Add_ModelError()
        {
            for (int i = 0; i < PriorStates.Row; i++)
            {
                double[] normRand = new double[PriorStates.Col];
                normRand[PriorStates.Col - 1] = dis.NormalRand();
                for (int j = 0; j < PriorStates.Col - 1; j++)
                {
                    normRand[j] = dis.NormalRand() + normRand[PriorStates.Col - 1];
                }

                if (control.ModelErrorOption[i] == 1)
                {
                    for (int j = 0; j < PriorStates.Col; j++)
                    {
                        PosteriorStates.Arr[i, j] = PriorStates.Arr[i, j] + PriorStates.Arr[i, j] * control.ModelError[i] * normRand[j];
                    }
                }
                else
                {
                    for (int j = 0; j < PriorStates.Col; j++)
                    {
                        PosteriorStates.Arr[i, j] = PriorStates.Arr[i, j] + control.ModelError[i] * normRand[j];
                    }
                }
            }
        }

        public void Calc_PriorMean()
        {
            for (int i = 0; i < PriorStates.Row; i++)
            {
                double sum = 0;
                for (int j = 0; j < PriorStates.Col; j++)
                {
                    sum += PriorStates.Arr[i, j];
                }
                PriorMean.Arr[i, 0] = sum / PriorStates.Col;
            }
            //Console.WriteLine("PriorMean = ");
            //PriorMean.Display();
        }

        public void Calc_PosteriorMean()
        {
            for (int i = 0; i < PosteriorStates.Row; i++)
            {
                double sum = 0;
                for (int j = 0; j < PosteriorStates.Col; j++)
                {
                    sum += PosteriorStates.Arr[i, j];
                }
                PosteriorMean.Arr[i, 0] = sum / PosteriorStates.Col;
            }
            //Console.WriteLine("PosteriorMean = ");
            //PosteriorMean.Display();
        }

        public void ReadAllStates(string[] tableNames)
        {
            AllPriorStates = new List<double[]>();
            SQLiteConnection sqlCon = new SQLiteConnection("Data Source=" + folder.SQLite);
            sqlCon.Open();
            for (int i = 0; i < tableNames.Count(); i++)
            {
                ReadSingleState(tableNames[i], sqlCon);
            }
            sqlCon.Close();
            PriorStates = new Matrix(AllPriorStates);
        }
        public void ReadSingleState(string tableName, SQLiteConnection sqlCon)
        {
            string sqlStr = "Select * FROM " + tableName + " WHERE ID =" + Day.ToString();
            SQLiteCommand command = new SQLiteCommand(sqlStr, sqlCon);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                string data;
                SinglePriorState = new double[control.EnsembleSize];
                for (int i = control.StartIndex; i < control.EndIndex; i++)
                {
                    if ((data = reader.GetValue(i).ToString()) != "")
                    {
                        SinglePriorState[i - control.StartIndex] = Convert.ToDouble(data);
                    }
                }
                AllPriorStates.Add(SinglePriorState);
            }
        }

        //YUXI: Not finished.
        public void UpdateAllStates()
        {
            Console.WriteLine("Day = " + Day);
            SQLiteConnection sqlCon = new SQLiteConnection("Data Source=" + folder.SQLite);
            sqlCon.Open();
            for (int i = 0; i < control.StateNames.Count(); i++)
            {
                UpdateSingleState(control.StateNames[i], i, sqlCon);
            }
            sqlCon.Close();
        }
        public void UpdateSingleState(string tableName, int stateIndex, SQLiteConnection sqlCon)
        {
            string sqlStr = "UPDATE " + tableName + " SET ";


            for (int i = 0; i < PriorStates.Col; i++)
            {
                sqlStr += "Ensemble" + i.ToString() + "_Posterior = '" + PosteriorStates.Arr[stateIndex, i] + "', ";
            }

            sqlStr += "PriorMean = '" + PriorMean.Arr[stateIndex, 0] + "',";
            sqlStr += "PosteriorMean = '" + PosteriorMean.Arr[stateIndex, 0] + "'";
            sqlStr += " WHERE ID='" + Day.ToString() + "'";
            SQLiteCommand command;
            command = new SQLiteCommand(sqlStr, sqlCon);
            command.ExecuteNonQuery();
        }

        public void UpdateSQLite(string sqlStr)
        {
            SQLiteConnection sqlCon = new SQLiteConnection("Data Source=" + folder.SQLite);
            sqlCon.Open();
            SQLiteCommand command;
            command = new SQLiteCommand(sqlStr, sqlCon);
            command.ExecuteNonQuery();
            sqlCon.Close();
        }
    }
}
