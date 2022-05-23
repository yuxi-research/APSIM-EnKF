using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DataAssimilation.DataType
{
    /// <summary> The Determinant. </summary>
    [Serializable]
    public class Determinant : DataArray
    {
        #region Inherit constructors from base.

        /// <summary> Constructor. </summary>
        public Determinant() : base() { }

        /// <summary> Constructor. </summary>
        /// <param name="row"></param>
        public Determinant(int row) : base(row) { }

        /// <summary> Constructor. </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        public Determinant(int row, int col) : base(row, col) { }

        /// <summary> Constructor. </summary>
        /// <param name="matrix"></param>
        /// <param name="isRowVector"></param>
        public Determinant(double[] matrix, bool isRowVector):base(matrix, isRowVector){ }

        /// <summary> Constructor. </summary>
        /// <param name="matrix"></param>
        public Determinant(double[,] matrix) : base(matrix) { }

        /// <summary> Constructor. </summary>
        /// <param name="list"></param>
        public Determinant(List<double[]> list) : base(list) { }
        #endregion

        /// <summary> Calculate Cofactor. </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
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
                return detC;
            }
            else
            {
                throw new Exception("Dimension mismatch!");
            }
        }

        /// <summary> Calculate determinant value. </summary>
        /// <returns></returns>
        public double DetValue()
        {
            if (Row == Col && Row > 1)
            {
                double temp = 0;
                for (int i = 0; i < Row; i++)
                {
                    temp += Arr[i, 0] * Cofactor(i + 1, 1).DetValue() * factor(i + 2);
                }
                return temp;
            }

            else if (Row == Col && Row == 1)
            {
                return Arr[0, 0];
            }
            else
                throw new Exception("Dimension mismatch!");
        }

        /// <summary> A factor to determine "+" or "-". </summary>
        /// <param name="k"></param>
        /// <returns></returns>
        public int factor(int k)
        {
            if (k % 2 == 0) { return 1; }
            else { return -1; }
        }
    }
}
