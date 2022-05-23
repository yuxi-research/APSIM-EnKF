﻿using System;
using System.Collections.Generic;
using APSIM.Shared.Utilities;
using MathNet.Numerics.LinearAlgebra;
namespace SWIMFrame
{
    /// <summary>
    /// This class will form the main input interface for APSIM.
    /// </summary>
    static class Input
    {
        //From example4.f95
        /* This program solves for water flow in a duplex soil.Solve is called daily
        ! for 100 days with precipitation of 1 cm/h on day 1. The potential evaporation
        ! rate is 0.05 cm/h throughout.There is vapour flow, but no sink term. It
        ! differs from example 2 by including two solutes. Solute 1 is not adsorbed
        ! while solute 2 follows a Freundlich adsorption isotherm in soil type 1, and
        ! a Langmuir in soil type 2.
        */
        static int n = 10;
        static int nt = 2;
        static int ns = 2; // 10 layers, 2 soil types, 2 solutes
        static int j;
        static int[] jt, sidx;//n
        static int nsteps;
        static int[] nssteps; //ns
        static double drn, evap, h0, h1, h2, infil, qevap, qprec, runoff;
        static double S1, S2, ti, tf, ts, win, wp, wpi;
        static double[] bd, dis; //nt
        static double[] c0, cin, sdrn, sinfil, soff, spi;//ns
        static double[] h, S, x;//n
        static double[,] sm;//(n, ns)
        //define isotype and isopar for solute 2
        static string[] isotype;//(nt)
        static double[,] isopar; //2 params (nt,2)

        //public static void Setup()
        public static void Main(string[] args)
        {
            Soil.SoilProperties = new Dictionary<string, SoilProps>();
            Fluxes.FluxTables = new Dictionary<string, FluxTable>();
            Program.GenerateFlux();

            c0 = new double[ns + 1];
            cin = new double[ns + 1];
            h = new double[n + 1];
            S = new double[n + 1];
            isotype = new string[nt + 1];
            isopar = new double[nt + 1, 2 + 1];
            soff = new double[ns + 1];
            sdrn = new double[ns + 1];
            sinfil = new double[ns + 1];
            SoilData sd = new SoilData();
            Flow.sink = new SinkDripperDrain(); //set the type of sink to use
            jt = new int[n+1];
            nssteps = new int[ns + 1];
            sidx = new int[n+1];
            sm = new double[ns + 1, n + 1];
            //set a list of soil layers(cm).
            x = new double[] { 0, 10.0, 20.0, 30.0, 40.0, 60.0, 80.0, 100.0, 120.0, 160.0, 200.0 };
            for (int c = 1; c <= n; c++)
                sidx[c] = c < 5 ? 103 : 109; //soil ident of layers
                                             //set required soil hydraulic params
            sd.GetTables(n, sidx, x);
            SolProps solProps = new SolProps(nt, ns);
            bd = new double[] {0, 1.3, 1.3 };
            dis = new double[] {0, 20.0, 20.0 };
            //set isotherm type and params for solute 2 here
            isotype[1] = "Fr";
            isotype[2] = "La";
            isopar[1, 1] = 1.0;
            isopar[1, 2] = 1.0;
            isopar[2, 1] = 0.5;
            isopar[2, 2] = 0.01;
            Matrix<double> isoparM = Matrix<double>.Build.DenseOfArray(isopar);
            for (j = 1; j <= nt; j++) //set params
            {
                solProps.Solpar(j, bd[j], dis[j]);
                //set isotherm type and params
                solProps.Setiso(j, 2, isotype[j], isoparM.Column(j).ToArray());
            }
            //initialise for run
            ts = 0.0; //start time
                      //dSmax controls time step.Use 0.05 for a fast but fairly accurate solution.
                      //Use 0.001 to get many steps to test execution time per step.
            Flow.dSmax = 0.01; //0.01 ensures very good accuracy
            for (int c = 1; c <= n; c++)
                jt[c] = c < 5 ? 1 : 2; //!4 layers of type 1, rest of type2
            h0 = 0.0; //pond depth initially zero
            h1 = -1000.0;
            h2 = -400.0; //initial matric heads
            double Sh = 0; //not used for this call but required as C# does not have 'present' operator
            sd.Sofh(h1, 1, out S1, out Sh); //solve uses degree of satn
            sd.Sofh(h2, 5, out S2, out Sh);
            for (int c = 1; c <= n; c++)
                S[c] = c < 5 ? S1 : S2;
            wpi = MathUtilities.Sum(MathUtilities.Multiply(MathUtilities.Multiply(sd.ths, S), sd.dx)); //water in profile initially
            nsteps = 0; //no.of time steps for water soln(cumulative)
            win = 0.0; //water input(total precip)
            evap = 0.0;
            runoff = 0.0;
            infil = 0.0;
            drn = 0.0;
            for (int col = 1; col < sm.GetLength(0); col++)
                sm[col, 1] = 1000.0 / sd.dx[1];
            //initial solute concn(mass units per cc of soil)
            //solute in profile initially
            spi = new []{1000.0, 1000.0};
            Flow.dsmmax = 0.1 * sm[1, 1]; //solute stepsize control param
            Flow.nwsteps = 10;
            MathUtilities.Zero(c0);
            MathUtilities.Zero(cin); //no solute input
            nssteps.Populate(0); //no.of time steps for solute soln(cumulative)
            MathUtilities.Zero(soff);
            MathUtilities.Zero(sinfil);
            MathUtilities.Zero(sdrn);
            qprec = 1.0; //precip at 1 cm / h for first 24 h
            ti = ts;
            qevap = 0.05;// potential evap rate from soil surface
            double[,] wex = new double[1,1]; //unused option params in FORTRAN... must be a better way of doing this
            double[,,] sex=new double[1,1,1];

            //timer here in FORTRAN, this basically runs the solution for 100 days
            for (j = 1; j <= 100; j++)
            {
                tf = ti + 24.0;
                Flow.Solve(solProps, sd, ti, tf, qprec, qevap, ns, Flow.sink.nex, ref h0, ref S, ref evap, ref runoff, ref infil, ref drn, ref nsteps, jt, cin, ref c0, ref sm, ref soff, ref sinfil, ref sdrn, ref nssteps, ref wex,ref sex);
                win = win + qprec * (tf - ti);
                if (j == 1)
                ti = tf;
                qprec = 0.0;
            }
            win = win + qprec * (tf - ti);
            wp = MathUtilities.Sum(MathUtilities.Multiply(MathUtilities.Multiply(sd.ths,S), sd.dx)); //!water in profile
            double hS = 0; //hS is not used used here, but is a required parameter
            for (j = 1; j <= n; j++)
                sd.hofS(S[j], j, out h[j], out hS);
        }

        /// <summary>
        /// Implements a cut down version of FORTRAN spread.
        /// The source array is copied n times by appending into a new array.
        /// </summary>
        /// <param name="source">The array to copy.</param>
        /// <param name="n">The number of times to copy the array.</param>
        /// <returns>A 1-indexed array of length (source.length * n + 1)</returns>
        private static double[] Spread(double[] source, int n)
        {
            double[] res = new double[(source.Length - 1) * n];
            for(int i=0;i< n;i++)
                Array.Copy(source, 1, res, n * source.Length + 1, source.Length - 1);
            return res;
        }
    }
}
