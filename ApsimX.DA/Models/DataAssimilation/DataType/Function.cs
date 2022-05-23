using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Models.DataAssimilation.DataType
{
    /// <summary>
    /// Function.
    /// </summary>
    [Serializable]
    public class Function
    {
        /// <summary>
        /// Draw a random value from Normal distribution N~(0,1).
        /// Do not use static methods for the consideration of thread safety.
        /// </summary>
        /// <returns></returns>
        public double NormalRand()
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double Constrain(double value,double lower, double upper)
        {
            return Math.Min(Math.Max(lower, value), upper);
        }
    }
}
