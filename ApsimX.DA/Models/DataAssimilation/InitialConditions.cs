﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Core;
using Models.Soils;
using System.Threading;

namespace Models.DataAssimilation
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [ViewName("UserInterface.Views.ProfileView")]
    [PresenterName("UserInterface.Presenters.ProfilePresenter")]
    [ValidParent(ParentType = typeof(Simulation))]
    [ValidParent(ParentType = typeof(IDataAssimilation))]
    [ValidParent(ParentType = typeof(Control))]

    public class InitialConditions:Model
    {
        #region ******* Links. *******

        [Link(IsOptional = true)]
        SoilWater SoilWater = null;
        #endregion

        #region ******* Public field. *******

        /// <summary>Initial date.</summary>
        [Description("Set initial conditions days after the start day")]
        public DateTime InitialDate { get; set; }

        /// <summary>Initial SW.</summary>
        [Description("Initial soil water content (mm/mm)")]
        public double[] InitialSW { get; set; }

        /// <summary>InitialSWError</summary>
        [Description("Initial soil water error")]
        public double[] InitialSWError { get; set; }

        #endregion

        #region ******* Private field. *******
        private DataType.Function Fun = new DataType.Function();

        #endregion


        #region ******* EventHandlers. *******


        /// <summary> Set initial conditions. </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [EventSubscribe("SetInitialCondition")]
        private void OnSetInitialCondition(object sender, EventArgs e)
        {
            Console.WriteLine("Initial condition set [{0}]", Thread.CurrentThread.Name);
            double[] temp = SoilWater.SW;
            for (int i = 0; i < SoilWater.SW.Count(); i++)
            {
                temp[i] = InitialSW[i];
            }
            SoilWater.SW = temp;
        }

        /// <summary> Perturb initial conditions. </summary>
        /// <summary> No necessary when InitialSW was perturbed on input file. </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [EventSubscribe("PerturbInitialCondition")]
        private void OnPerturbInitialCondition(object sender, EventArgs e)
        {
            //Console.WriteLine("Initial condition perturbed [{0}]", Thread.CurrentThread.Name);
            //double[] temp = SoilWater.SW;
            //for (int i = 0; i < SoilWater.SW.Count(); i++)
            //{
            //    temp[i] = SoilWater.SW[i] + InitialSWError[i] * Fun.NormalRand();
            //    temp[i] = Fun.Constrain(temp[i], 0, 1);
            //}
            //SoilWater.SW = temp;
        }

        #endregion

        #region ******* Methods. *******



        #endregion



    }
}
