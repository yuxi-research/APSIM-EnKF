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
    public class EnKF
    {

        private FolderStructure folder = new FolderStructure(0);
        private DAControl control = new DAControl(0);
        private Distribution dis = new Distribution();
        public int Day;
        public Matrix Obs;

        public int ObsNumber;
        public static DateTime Today { get; set; }

        public double[] SinglePriorState;
        public List<double[]> AllPriorStates;
        public static List<List<double>> AllObs = new List<List<double>>();
        public static List<List<int>> AllObsIndices = new List<List<int>>();

        public Matrix PriorStates { get; set; }
        public Matrix PriorMean { get; set; }
        public Matrix D { get; set; }
        public Matrix H { get; set; }
        public Matrix R { get; set; }
        public Matrix P { get; set; }
        public Matrix K { get; set; }
        public Matrix y { get; set; }
        public Matrix hx { get; set; }
        public Matrix v { get; set; }
        public Matrix dlt_x { get; set; }
        public Matrix PosteriorStates { get; set; }
        public Matrix PosteriorMean { get; set; }

        /// <summary>Constructor.</summary>
        public EnKF()
        { }

        public void OnDoAssimilation(int day, Matrix Obs, Matrix ObsIndices)
        {
            Day = day;
            this.Obs = Obs;
            ReadAllStates(control.StateNames);

            H = ObsIndices;     //  r=observation; c=states
            ObsNumber = H.Row;

            Add_ModelError();
            this.PriorMean = new DataAssimilation.Matrix(PriorStates.Row, 1);
            Calc_PriorMean();
            D = new DataAssimilation.Matrix(PriorStates.Row, PriorStates.Row);
            Calc_D();
            Calc_P();
            //this.H = new DataAssimilation.Matrix(ObsNumber, PriorStates.Row);     //  r=observation; c=states
            this.R = new DataAssimilation.Matrix(ObsNumber, ObsNumber);
            this.v = new DataAssimilation.Matrix(this.ObsNumber, PriorStates.Col);
            Calc_K();
            Calc_hx();
            Calc_v();
            this.y = new DataAssimilation.Matrix(ObsNumber, PriorStates.Col);
            PerturbObs();
            Calc_PosteriorStates();
            this.PosteriorMean = new DataAssimilation.Matrix(PosteriorStates.Row, 1);
            Calc_PosteriorMean();
            UpdateAllStates();
        }
        public void Add_ModelError()
        {
            for (int i = 0; i < PriorStates.Row; i++)
            {
                double[] normRand = new double[PriorStates.Col + 1];
                normRand[PriorStates.Col] = dis.NormalRand();
                for (int j = 0; j < PriorStates.Col; j++)
                {
                    normRand[j] = dis.NormalRand() + normRand[PriorStates.Col];
                }

                if (control.ModelErrorOption[i] == 1)
                {
                    for (int j = 0; j < PriorStates.Col; j++)
                    {
                        PriorStates.Arr[i, j] = PriorStates.Arr[i, j] + PriorStates.Arr[i, j] * control.ModelError[i] * normRand[j];
                        PriorStates.Arr[i, j] = Math.Max(0, PriorStates.Arr[i, j]);
                    }
                }
                else
                {
                    for (int j = 0; j < PriorStates.Col; j++)
                    {
                        PriorStates.Arr[i, j] = PriorStates.Arr[i, j] + control.ModelError[i] * normRand[j];
                        PriorStates.Arr[i, j] = Math.Max(0, PriorStates.Arr[i, j]);
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
        public void Calc_D()
        {
            D = PriorStates - PriorMean;
            //Console.WriteLine("D = ");
            //D.Display();
        }
        public void Calc_P()
        {
            P = (1.0 / (PriorStates.Col - 1)) * (D * D.Transpose());

            ////Assume all states are unrelated.    //No need for this.
            //for (int i = 0; i < P.Row; i++)
            //{
            //    if (i != 0)
            //    {
            //        P.Arr[i, 0] = 0;
            //    }
            //}
            //for (int i = 0; i < P.Col; i++)
            //{
            //    if (i != 0)
            //    {
            //        P.Arr[0, i] = 0;
            //    }
            //}
        }
        public void Calc_K()
        {
            for (int i = 0; i < R.Col; i++)
            {
                for (int j = 0; j < R.Col; j++)
                {
                    if (i == j)
                        if (control.ObsErrorOption[i] == 1 && Obs.Arr[i, 0] != 0)
                        {
                            R.Arr[i, j] = control.ObsError[i] * control.ObsError[i] * Obs.Arr[i, 0] * Obs.Arr[i, 0];        //Default. Read R matrix directly will be better.
                        }
                        else if (control.ObsErrorOption[i] == 1 && Obs.Arr[i, 0] == 0)
                        {
                            R.Arr[i, j] = control.ObsError[i] * control.ObsError[i] * PriorMean.Arr[i, 0] * PriorMean.Arr[i, 0];
                        }
                        else
                        {
                            R.Arr[i, j] = control.ObsError[i] * control.ObsError[i];        //Default. Read R matrix directly will be better.
                        }
                    else
                        R.Arr[i, j] = 0;    //Assume independent.
                }
            }
            //Console.WriteLine("H = ");
            //H.Display();
            //Console.WriteLine("K = ");
            //K = H * P * H.Transpose() + R;
            //K.Display();
            //K = K.Inverse();
            //K.Display();
            //K = P* H * K;
            //K.Display();
            Matrix temp = H * P * H.Transpose() + R;
            if (temp.IsZero() || H.IsZero())
            {
                K = new Matrix(PriorStates.Row, ObsNumber);
                K.Assign(0);
            }
            else
            {
                K = P * H.Transpose() * temp.Inverse();
            }
            //K.Display();
        }
        public void Calc_hx()
        {
            hx = H * PriorStates;
        }
        public void Calc_v()
        {
            //Observation error.
            for (int i = 0; i < v.Arr.GetLength(0); i++)
            {
                if (control.ObsErrorOption[i] == 1)
                {
                    for (int j = 0; j < v.Arr.GetLength(1); j++)
                    {
                        if (hx.Arr[i, j] != 0)
                        {

                            v.Arr[i, j] = 0 + control.ObsError[i] * Obs.Arr[i, 0] * dis.NormalRand();
                        }
                        else
                        {
                            v.Arr[i, j] = 0;
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < v.Arr.GetLength(1); j++)
                    {
                        if (hx.Arr[i, j] != 0)
                        {

                            v.Arr[i, j] = 0 + control.ObsError[i] * dis.NormalRand();
                        }
                        else
                        {
                            v.Arr[i, j] = 0;
                        }
                    }
                }
            }
            //Console.WriteLine("v = ");
            //v.Display();
        }
        public void PerturbObs()
        {
            for (int i = 0; i < y.Row; i++)
            {
                if (control.ObsErrorOption[i] == 1)
                {
                    for (int j = 0; j < y.Col; j++)
                    {
                        if (Obs.Arr[i, 0] != 0)
                        {
                            y.Arr[i, j] = Math.Max(0, Obs.Arr[i, 0] + control.ObsError[i]* Obs.Arr[i, 0] * dis.NormalRand());
                        }
                        else
                        {
                            y.Arr[i, j] = Obs.Arr[i, 0];
                        }
                    }
                }
                else if(control.ObsErrorOption[i]==0)
                {
                    for (int j = 0; j < y.Col; j++)
                    {
                        if (Obs.Arr[i, 0] != 0)
                        {
                            y.Arr[i, j] = Math.Max(0, Obs.Arr[i, 0] + control.ObsError[i] * dis.NormalRand());
                        }
                        else
                        {
                            y.Arr[i, j] = Obs.Arr[i, 0];
                        }
                    }
                }

            }
        }
        public void Calc_PosteriorStates()
        {
            PosteriorStates = PriorStates + K * (y - hx + v);
            for (int i = 0; i < PosteriorStates.Row; i++)
            {
                for (int j = 0; j < PosteriorStates.Col; j++)
                {
                    PosteriorStates.Arr[i, j] = Math.Max(PosteriorStates.Arr[i, j], 0);
                }
            }
            //Console.WriteLine("PosteriorStates = ");
            //PosteriorStates.Display();
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
                sqlStr += "Ensemble" + i.ToString() + "_Prior = '" + PriorStates.Arr[stateIndex, i] + "',";
            }

            for (int i = 0; i < PriorStates.Col; i++)
            {
                sqlStr += "Ensemble" + i.ToString() + "_Posterior = '" + PosteriorStates.Arr[stateIndex, i] + "',";
            }

            for (int i = 0; i < control.StateNamesObs.Count(); i++)
            {
                if (tableName == control.StateNamesObs[i])
                {
                    sqlStr += "Obs = '" + this.Obs.Arr[i, 0] + "',";
                    sqlStr += "K = '" + K.Arr[stateIndex, i] + "',";
                    for (int j = 0; j < PriorStates.Col; j++)
                    {
                        sqlStr += "Ensemble" + j.ToString() + "_Obs = '" + y.Arr[i, j] + "',";
                    }
                }
            }

            sqlStr += "PriorMean = '" + PriorMean.Arr[stateIndex, 0] + "',";
            sqlStr += "P = '" + P.Arr[stateIndex, stateIndex] + "',";
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
