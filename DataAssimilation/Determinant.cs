using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAssimilation
{
    [Serializable]
    public class Determinant : DataArray
    {
        //Inherit constructors from the base.
        public Determinant(int row) : base(row) { }
        public Determinant(int row, int col) : base(row, col) { }
        public Determinant(double[] matrix, bool isRowVector):base(matrix, isRowVector){ }
        public Determinant(double[,] matrix) : base(matrix) { }
        public Determinant(List<double[]> list) : base(list) { }

        public Determinant Cofactor(int row, int col)
        {
            Determinant detC = new Determinant(Row - 1, Col - 1);

            if (row <= Row && col <= Col && Row > 1 && Col > 1)
            {
                for (int i = 0; i < Row - 1; i++)
                {
                    for (int j = 0; j < Col - 1; j++)
                    {
                        if (i < row - 1 && j < col - 1) { detC.Arr[i, j] = this.Arr[i, j]; }
                        else if (i < row - 1 && j >= col - 1) { detC.Arr[i, j] = this.Arr[i, j + 1]; }
                        else if (i >= row - 1 && j < col - 1) { detC.Arr[i, j] = this.Arr[i + 1, j]; }
                        else { detC.Arr[i, j] = this.Arr[i + 1, j + 1]; }
                    }
                }
                //detC.Display();
                //Console.ReadLine();
                return detC;
            }
            else
            {
                throw new Exception("Dimension mismatch!");
            }
        }
        public double DetValue()
        {
            if (Row == Col && Row > 1)
            {
                double a = 0;
                for (int i = 0; i < Row; i++)
                {
                    //Console.WriteLine("a(i,j) is {0}", Det[i, 0] * factor(i + 2));
                    //Console.ReadLine();
                    a += Arr[i, 0] * Cofactor(i + 1, 1).DetValue() * factor(i + 2);
                    //Console.WriteLine("a is {0}", a);
                }
                return a;
            }

            else if (Row == Col && Row == 1)
            {
                return Arr[0, 0];
            }
            else
                throw new Exception("Dimension mismatch!");
        }

        public int factor(int k)
        {
            if (k % 2 == 0) { return 1; }
            else { return -1; }
        }
    }
}
