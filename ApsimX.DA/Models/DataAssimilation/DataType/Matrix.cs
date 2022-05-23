using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DataAssimilation.DataType
{
    /// <summary> The Matrix. </summary>
    [Serializable]
    public class Matrix : DataArray
    {
        #region Inherit constructors from base.

        /// <summary> Constructor. </summary>
        public Matrix() : base() { }

        /// <summary> Constructor. </summary>
        /// <param name="row"></param>
        public Matrix(int row) : base(row) { }

        /// <summary> Constructor. </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        public Matrix(int row, int col) : base(row, col) { }

        /// <summary> Constructor. </summary>
        /// <param name="matrix"></param>
        /// <param name="isRowVector"></param>
        public Matrix(double[] matrix, bool isRowVector) : base(matrix, isRowVector) { }

        /// <summary> Constructor. </summary>
        /// <param name="matrix"></param>
        public Matrix(double[,] matrix) : base(matrix) { }

        /// <summary> Constructor. </summary>
        /// <param name="list"></param>
        public Matrix(List<double[]> list) : base(list) { }

        /// <summary> Constructor. </summary>
        /// <param name="list"></param>
        public Matrix(List<double> list) : base(list) { }

        /// <summary> Constructor. </summary>
        /// <param name="list"></param>
        public Matrix(List<List<double>> list) : base(list) { }

        /// <summary> Constructor. </summary>
        /// <param name="list"></param>
        public Matrix(List<List<int>> list) : base(list){ }
        #endregion

        /// <summary> Return the determinant of  matrix as an array. </summary>
        /// <returns></returns>
        public Determinant ToDet()
        {
            Determinant detA = new Determinant(Arr);
            return detA;
        }

        /// <summary> Addition between two matrices. </summary>
        /// <param name="matrixA"></param>
        /// <param name="matrixB"></param>
        /// <returns></returns>
        public static Matrix operator +(Matrix matrixA, Matrix matrixB)
        {
            Matrix matrixC = new Matrix(matrixA.Row, matrixA.Col);
            if (matrixA.Row == matrixB.Row && matrixA.Col == matrixB.Col)
            {
                for (int i = 0; i < matrixC.Row; i++)
                {
                    for (int j = 0; j < matrixC.Col; j++)
                    {
                        matrixC.Arr[i, j] = matrixA.Arr[i, j] + matrixB.Arr[i, j];
                    }
                }
                return matrixC;
            }
            else if (matrixA.Row == matrixB.Row && matrixB.Col == 1)
            {
                for (int i = 0; i < matrixC.Row; i++)
                {
                    for (int j = 0; j < matrixC.Col; j++)
                    {
                        matrixC.Arr[i, j] = matrixA.Arr[i, j] + matrixB.Arr[i, 0];
                    }
                }
                return matrixC;
            }
            else if (matrixA.Col == matrixB.Col && matrixB.Row == 1)
            {
                for (int i = 0; i < matrixC.Row; i++)
                {
                    for (int j = 0; j < matrixC.Col; j++)
                    {
                        matrixC.Arr[i, j] = matrixA.Arr[i, j] + matrixB.Arr[0, j];
                    }
                }
                return matrixC;
            }
            else
            {
                throw new Exception("Dimension mismatch!");
            }
        }

        /// <summary> Subtraction between two matrices. </summary>
        /// <param name="matrixA"></param>
        /// <param name="matrixB"></param>
        /// <returns></returns>
        public static Matrix operator -(Matrix matrixA, Matrix matrixB)
        {
            Matrix matrixC = new Matrix(matrixA.Row, matrixA.Col);

            if (matrixA.Row == matrixB.Row && matrixA.Col == matrixB.Col)
            {
                for (int i = 0; i < matrixC.Row; i++)
                {
                    for (int j = 0; j < matrixC.Col; j++)
                    {
                        matrixC.Arr[i, j] = matrixA.Arr[i, j] - matrixB.Arr[i, j];
                    }
                }
                return matrixC;
            }
            else if (matrixA.Row == matrixB.Row && matrixB.Col == 1)
            {
                for (int i = 0; i < matrixC.Row; i++)
                {
                    for (int j = 0; j < matrixC.Col; j++)
                    {
                        matrixC.Arr[i, j] = matrixA.Arr[i, j] - matrixB.Arr[i, 0];
                    }
                }
                return matrixC;
            }
            else if (matrixA.Col == matrixB.Col && matrixB.Row == 1)
            {
                for (int i = 0; i < matrixC.Row; i++)
                {
                    for (int j = 0; j < matrixC.Col; j++)
                    {
                        matrixC.Arr[i, j] = matrixA.Arr[i, j] - matrixB.Arr[0, j];
                    }
                }
                return matrixC;
            }
            else
            {
                throw new Exception("Dimension mismatch!");
            }
        }

        /// <summary> Multiplication between two matrices. </summary>
        /// <param name="matrixA"></param>
        /// <param name="matrixB"></param>
        /// <returns></returns>
        public static Matrix operator *(Matrix matrixA, Matrix matrixB)
        {
            if (matrixA.Col == matrixB.Row)
            {
                Matrix matrixC = new Matrix(matrixA.Row, matrixB.Col);
                for (int i = 0; i < matrixA.Row; i++)
                {
                    for (int j = 0; j < matrixB.Col; j++)
                    {
                        matrixC.Arr[i, j] = 0;
                        for (int k = 0; k < matrixA.Col; k++)
                        {
                            matrixC.Arr[i, j] += matrixA.Arr[i, k] * matrixB.Arr[k, j];
                        }
                    }
                }
                return matrixC;
            }
            else
            {
                throw new Exception("Dimension mismatch!");
            }
        }

        /// <summary> Return the transpose matrix. </summary>
        /// <returns></returns>
        public Matrix Transpose()
        {
            Matrix matrixA = new Matrix(Col, Row);
            for (int i = 0; i < Col; i++)
            {
                for (int j = 0; j < Row; j++)
                {
                    matrixA.Arr[i, j] = Arr[j, i];
                }
            }
            return matrixA;
        }

        /// <summary> Multiplication between a value and a matrix. </summary>
        /// <param name="a"></param>
        /// <param name="matrixA"></param>
        /// <returns></returns>
        public static Matrix operator *(double a, Matrix matrixA)
        {
            Matrix matrixC = new Matrix(matrixA.Row, matrixA.Col);
            for (int i = 0; i < matrixA.Row; i++)
            {
                for (int j = 0; j < matrixA.Col; j++)
                {
                    matrixC.Arr[i, j] = a * matrixA.Arr[i, j];
                }
            }
            return matrixC;
        }

        /// <summary> Multiplication between a value and a matrix. </summary>
        /// <param name="matrixA"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Matrix operator *(Matrix matrixA, double a)
        {
            return a * matrixA;
        }

        /// <summary>
        /// Return the inverse matrix of a square matrix.
        /// </summary>
        /// <returns></returns>
        public Matrix Inverse()
        {
            if (Row == 1 && Col == 1)
            {
                Matrix matrixC = new Matrix(1, 1);
                matrixC.Arr[0, 0] = 1 / this.Arr[0, 0];
                return matrixC;
            }
            else
            {
                double detValue = ToDet().DetValue();
                if (detValue != 0)
                {
                    detValue = 1 / detValue;
                    Matrix matrixC = new Matrix(Row, Col);
                    matrixC = detValue * AdjointMatrix();
                    return matrixC;
                }
                else
                {
                    throw new DivideByZeroException();
                }
            }
        }

        /// <summary>
        /// Add value in matrixB only when the value of matrixA is more than zero.
        /// </summary>
        /// <param name="matrixB"></param>
        /// <returns></returns>
        public Matrix NonZeroAdd(Matrix matrixB)
        {
            Matrix matrixA = new Matrix(Row, Col);
            if (Row == matrixB.Row && Col == matrixB.Col)
            {

                for (int i = 0; i < Row; i++)
                {
                    for (int j = 0; j < Col; j++)
                    {
                        if (Arr[i, j] > 0)
                        {
                            matrixA.Arr[i, j] = Arr[i, j] + matrixB.Arr[i, j];
                        }
                        else
                        {
                            matrixA.Arr[i, j] = Arr[i, j];
                        }
                    }
                }
            }
            else
            {
                throw new Exception("Dimension mismatch!");
            }
            return matrixA;

        }
        /// <summary>
        /// Return the adjoint matrix of a squre matrix.
        /// </summary>
        /// <returns></returns>
        public Matrix AdjointMatrix()
        {
            Matrix matrixA = new Matrix(Row, Col);

            for (int i = 0; i < Row; i++)
            {
                for (int j = 0; j < Col; j++)
                {
                    matrixA.Arr[i, j] = ToDet().Cofactor(i + 1, j + 1).DetValue() * factor(i + j);

                }
            }
            //            Console.WriteLine("Cofactor Matrix is");
            //            matrixA.Display();
            return matrixA.Transpose();
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

