using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace Shared
{
    public class Distribution
    {
        public static double NormalRand()
        {
            Random random = new Random();
            double u1, u2, normalRandom;
            u1 = 1 - random.NextDouble();
            u2 = 1 - random.NextDouble();
            //Normal distribution. Generated with Box-Muller transform.
            normalRandom = Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);
            Thread.Sleep(20);
            return normalRandom;
        }

        public static double Constrain(double value, double lower, double upper)
        {
            return Math.Min(Math.Max(value, lower), upper);
        }

        public static DataType.Matrix MultiNormalRand(double[] std, List<double[]> corr, int ensembleSize)
        {
            DataType.Matrix Std = new DataType.Matrix(std, true);
            if (Std.isAll(0))
            {
                Console.WriteLine("Warning: STD is all zeros!");
                DataType.Matrix Zeros = new DataType.Matrix(std.Count(), ensembleSize);
                return Zeros;
            }
            DataType.Matrix Cov = Std.Transpose() * Std;
            for (int i = 0; i < Cov.Row; i++)
            {
                for (int j = 0; j < Cov.Col; j++)
                {
                    Cov.Arr[i, j] *= corr[i][j];
                }
            }

            DataType.Matrix B, Z;
            B = Cov.Decompose();
            Z = new DataType.Matrix(Cov.Row, ensembleSize);
            for (int i = 0; i < Cov.Row; i++)
            {
                for (int j = 0; j < ensembleSize; j++)
                {
                    Z.Arr[i, j] = NormalRand();
                }
            }
            return B * Z;
        }
    }
}

