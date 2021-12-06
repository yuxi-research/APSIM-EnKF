using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DataAssimilation.DataType
{
    /// <summary> The Data Array. </summary>
    [Serializable]
    public class DataArray
    {
        /// <summary> The row. </summary>
        public int Row { get; set; }

        /// <summary> The column. </summary>
        public int Col { get; set; }

        /// <summary> The Arr. </summary>
        [XmlIgnore]
        public double[,] Arr { get; set; }

        #region Constructors.
        /// <summary> Constructor. </summary>
        public DataArray() { }

        /// <summary> Constructor. </summary>
        /// <param name="row"></param>
        public DataArray(int row)
        {
            Row = row;
            Col = row;
            Arr = new double[Row, Row];
        }

        /// <summary> Constructors. </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        public DataArray(int row, int col)
        {
            Row = row;
            Col = col;
            Arr = new double[Row, Col];
        }

        /// <summary> Constructor. </summary>
        /// <param name="array"></param>
        /// <param name="isRowVector"></param>
        public DataArray(double[] array, bool isRowVector)
        {
            if (isRowVector)
            {
                Row = 1;
                Col = array.GetLength(1);
                Arr = new double[Row, Col];
                for (int i = 0; i < Col; i++)
                {
                    Arr[0, i] = array[i];
                }
            }
            else
            {
                Row = array.GetLength(0);
                Col = 1;
                Arr = new double[Row, Col];
                for (int i = 0; i < Row; i++)
                {
                    Arr[i, 0] = array[i];
                }
            }
        }

        /// <summary> Constructor. </summary>
        /// <param name="matrix"></param>
        public DataArray(double[,] matrix)
        {
            Row = matrix.GetLength(0);
            Col = matrix.GetLength(1);
            Arr = matrix;
        }

        /// <summary> Constructor. </summary>
        /// <param name="list"></param>
        public DataArray(List<double[]> list)
        {
            Row = list.Count();
            Col = list[0].Count();
            Arr = new double[Row, Col];
            for (int i = 0; i < Row; i++)
            {
                for (int j = 0; j < Col; j++)
                {
                    Arr[i, j] = list[i][j];
                }
            }
        }

        /// <summary> Constructor. </summary>
        /// <param name="list"></param>
        public DataArray(List<double> list)
        {
            Row = list.Count();
            Col = 1;
            Arr = new double[Row, Col];
            for (int i = 0; i < Row; i++)
            {
                Arr[i, 0] = list[i];
            }
        }
        /// <summary> Constructor. </summary>
        /// <param name="list"></param>
        public DataArray(List<List<double>> list)
        {
            Row = list.Count();
            Col = list[0].Count();
            Arr = new double[Row, Col];
            for (int i = 0; i < Row; i++)
            {
                for (int j = 0; j < Col; j++)
                {
                    Arr[i, j] = list[i][j];
                }
            }
        }

        /// <summary> Constructor. </summary>
        /// <param name="list"></param>
        public DataArray(List<List<int>> list)
        {
            Row = list.Count();
            Col = list[0].Count();
            Arr = new double[Row, Col];
            for (int i = 0; i < Row; i++)
            {
                for (int j = 0; j < Col; j++)
                {
                    Arr[i, j] = list[i][j];
                }
            }
        }
        #endregion

        /// <summary> Addition. </summary>
        /// <param name="arrayA"></param>
        /// <param name="arrayB"></param>
        /// <returns></returns>
        public static DataArray operator +(DataArray arrayA, DataArray arrayB)
        {
            if (arrayA.Row == arrayB.Row && arrayB.Col == arrayB.Col)
            {
                DataArray arrayC = new DataArray(arrayA.Row, arrayA.Col);
                for (int i = 0; i < arrayC.Row; i++)
                {
                    for (int j = 0; j < arrayC.Col; j++)
                    {
                        arrayC.Arr[i, j] = arrayA.Arr[i, j] + arrayB.Arr[i, j];
                    }
                }
                return arrayC;
            }
            else
            {
                throw new Exception("Dimension mismatch!");
            }
        }

        /// <summary> Subtraction. </summary>
        /// <param name="arrayA"></param>
        /// <param name="arrayB"></param>
        /// <returns></returns>
        public static DataArray operator -(DataArray arrayA, DataArray arrayB)
        {
            if (arrayA.Row == arrayB.Row && arrayB.Col == arrayB.Col)
            {
                DataArray arrayC = new DataArray(arrayA.Row, arrayA.Col);
                for (int i = 0; i < arrayC.Row; i++)
                {
                    for (int j = 0; j < arrayC.Col; j++)
                    {
                        arrayC.Arr[i, j] = arrayA.Arr[i, j] - arrayB.Arr[i, j];
                    }
                }
                return arrayC;
            }
            else
            {
                throw new Exception("Dimension mismatch!");
            }
        }

        /// <summary> Multiplication. </summary>
        /// <param name="arrayA"></param>
        /// <param name="arrayB"></param>
        /// <returns></returns>
        public static DataArray operator *(DataArray arrayA, DataArray arrayB)
        {
            if (arrayA.Row == arrayB.Row && arrayB.Col == arrayB.Col)
            {
                DataArray arrayC = new DataArray(arrayA.Row, arrayA.Col);
                for (int i = 0; i < arrayC.Row; i++)
                {
                    for (int j = 0; j < arrayC.Col; j++)
                    {
                        arrayC.Arr[i, j] = arrayA.Arr[i, j] * arrayB.Arr[i, j];
                    }
                }
                return arrayC;
            }
            else
            {
                throw new Exception("Dimension mismatch!");
            }
        }

        /// <summary> Division. </summary>
        /// <param name="arrayA"></param>
        /// <param name="arrayB"></param>
        /// <returns></returns>
        public static DataArray operator /(DataArray arrayA, DataArray arrayB)
        {
            if (arrayA.Row == arrayB.Row && arrayB.Col == arrayB.Col)
            {
                DataArray arrayC = new DataArray(arrayA.Row, arrayA.Col);
                for (int i = 0; i < arrayC.Row; i++)
                {
                    for (int j = 0; j < arrayC.Col; j++)
                    {
                        arrayC.Arr[i, j] = arrayA.Arr[i, j] / arrayB.Arr[i, j];
                    }
                }
                return arrayC;
            }
            else
            {
                throw new Exception("Dimension mismatch!");
            }
        }

        /// <summary> Check if all the data is zero. </summary>
        /// <returns></returns>
        public bool EqualTo(double value)
        {
            bool equalTo = true;
            for (int i = 0; i < Row; i++)
            {
                for (int j = 0; j < Col; j++)
                {
                    if (Arr[i, j] != value)
                    {
                        equalTo = false;
                        break;
                    }
                }
            }
            return equalTo;
        }

        #region Display and check
        /// <summary> Write array to console. </summary>
        public void Display()
        {
            for (int i = 0; i < Row; i++)
            {
                for (int j = 0; j < Col; j++)
                {
                    Console.Write(Arr[i, j] + "\t");
                }
                Console.Write("\n");
            }
        }

        /// <summary>
        /// Set all NaNs to a value (0 by default). 
        /// </summary>
        /// <param name="value"></param>
        public Matrix CheckNaN(double value = 0)
        {
            Matrix Map = new Matrix(Row, Col);
            for (int i = 0; i < Row; i++)
            {
                for (int j = 0; j < Col; j++)
                {
                    if (Double.IsNaN(Arr[i, j]))
                    {
                        Arr[i, j] = value;
                        Map.Arr[i, j] = 1;
                    }
                    else
                    {
                        Map.Arr[i, j] = 0;
                    }
                }
            }
            return Map;
        }

        /// <summary>
        /// Set all negative to a value (0 by default). 
        /// </summary>
        /// <param name="value"></param>
        public void CheckNegative(double value = 0)
        {
            for (int i = 0; i < Row; i++)
            {
                for (int j = 0; j < Col; j++)
                {
                    if (Arr[i, j] < 0)
                    {
                        Arr[i, j] = value;
                    }
                }
            }
        }
        /// <summary>
        /// Set dataarray to value (0 by default).
        /// </summary>
        /// <param name="value"></param>
        public void Set(double value = 0)
        {
            for (int i = 0; i < Row; i++)
            {
                for (int j = 0; j < Col; j++)
                {
                    Arr[i, j] = value;
                }
            }
        }
        #endregion
    }
}

