using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DataAssimilation
{
    [Serializable]
    public class Distribution
    {
        public double NormalRand()
        {
            Random random = new Random();
            double u1, u2, normalRandom;
            u1 = random.NextDouble();
            u2 = random.NextDouble();
            //Normal distribution. Generated with Box-Muller transform.
            normalRandom = Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);
            Thread.Sleep(10);
            return normalRandom;
        }
    }
}
