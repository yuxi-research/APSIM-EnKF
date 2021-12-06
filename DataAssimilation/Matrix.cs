using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAssimilation
{
    [Serializable]
    public class Matrix : DataArray
    {
        //Inherit constructors from the base.
        public Matrix(int row) : base(row) { }
        public Matrix(int row, int col) : base(row, col) { }
        public Matrix(double[] matrix, bool isRowVector) : base(matrix, isRowVector) { }
        public Matrix(double[,] matrix) : base(matrix) { }
        public Matrix(List<double[]> list) : base(list) { }
        public Matrix(List<List<double>> list) : base(list) { }
        public Matrix(List<List<int>> list) : base(list){ }

        public Determinant ToDet()
        {
            Determinant detA = new Determinant(Arr);
            return detA;
        }

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
        public static Matrix operator *(Matrix matrixA, double a)
        {
            return a * matrixA;
        }

        /// <summary>
        /// Return the inverse matrix of a squre matrix.
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
        public int factor(int k)
        {
            if (k % 2 == 0) { return 1; }
            else { return -1; }
        }


    }
}

