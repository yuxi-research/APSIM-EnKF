using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAssimilation
{
    [Serializable]
    public class DataArray
    {
        private int row;
        private int col;
        private double[,] arr;
        public int Row
        {
            get { return row; }
            set { row = value; }
        }
        public int Col
        {
            get { return col; }
            set { col = value; }
        }
        public double[,] Arr
        {
            get { return arr; }
            set { arr = value; }
        }
        //Constructors.
        public DataArray()
        {
        }
        public DataArray(int row)
        {
            Row = row;
            Col = row;
            Arr = new double[Row, Row];
        }
        public DataArray(int row, int col)
        {
            Row = row;
            Col = col;
            Arr = new double[Row, Col];
        }
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
        public DataArray(double[,] matrix)
        {
            Row = matrix.GetLength(0);
            Col = matrix.GetLength(1);
            Arr = matrix;
        }
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
        public bool IsZero()
        {
            bool isZero = true;
            for(int i = 0; i < Row; i++)
            {
                for (int j = 0; j < Col; j++)
                {
                    if (Arr[i, j] != 0)
                    {
                        isZero = false;
                        break;
                    }
                }
            }
            return isZero;
        }
        public void Assign(double value)
        {
            for (int i = 0; i < Row; i++)
            {
                for (int j = 0; j < Col; j++)
                {
                    Arr[i, j] = value;
                }
            }
        }
    }
}

