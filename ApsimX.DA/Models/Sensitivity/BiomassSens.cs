using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Core;
using System.Xml.Serialization;
using Models.PMF;
using Models.PMF.OldPlant;

namespace Models.Sensitivity
{
    /// <summary>
    /// Sensitivity analysis for Biomass.Live.
    /// </summary>
    [Serializable]
    [ViewName("UserInterface.Views.GridView")]
    [PresenterName("UserInterface.Presenters.PropertyPresenter")]
    [ValidParent(ParentType = typeof(Simulations))]
    [ValidParent(ParentType = typeof(Simulation))]
    [ValidParent(ParentType = typeof(ISensitivity))]
    public class BiomassSens : Model, ISensitivity
    {

        #region ******* Links. *******

        [Link(IsOptional = true)]
        Leaf1 Leaf = null;

        [Link(IsOptional = true)]
        Stem1 Stem = null;

        [Link(IsOptional = true)]
        Grain Grain = null;

        [Link(IsOptional = true)]
        Root1 Root = null;

        [Link(IsOptional = true)]
        Pod Pod = null;

        #endregion

        #region ******* Public field. *******

        /// <summary></summary>
        [Description("Sensitivity analysis of Biomass organ names")]
        public string[] Organs { get; set; }

        /// <summary></summary>
        [Description("State: Wt or N?")]
        public string[] States { get; set; }

        /// <summary>cultivar offset</summary>
        [Description("Biomass offset")]
        public double[] Offset { get; set; }

        /// <summary>InputOffsetOption</summary>
        [Description("Input offset option")]
        public int[] OffsetOption { get; set; }

        /// <summary>Biomass allocation option</summary>
        [Description("Biomass allocation option (0=proportional)")]
        public int AllocationRule { get; set; }

        /// <summary></summary>
        [Description("Change biomass on date")]
        public DateTime Date { get; set; }

        #endregion

        #region ******* Private field. *******



        #endregion


        #region ******* EventHandlers. *******

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [EventSubscribe("DoBiomassSens")]
        private void DoBiomassSens(object sender, EventArgs e)
        {
            for (int i = 0; i < Organs.Count(); i++)
            {
                DoSensitivity(Organs[i], States[i], Offset[i], OffsetOption[i]);
            }
        }

        #endregion

        #region ******* Methods. *******

        /// <summary>
        /// 
        /// </summary>
        public void DoSensitivity(string organName, string state, double offset, int option)
        {
            if (organName == "Leaf")
            {
                ReAllocate(Leaf.Live, state, offset, option);
            }
            else if (organName == "Grain")
            {
                ReAllocate(Grain.Live, state, offset, option);
            }
            else if (organName == "Root")
            {
                ReAllocate(Root.Live, state, offset, option);
            }
            else if (organName == "Pod")
            {
                ReAllocate(Pod.Live, state, offset, option);
            }
            else if (organName == "Stem")
            {
                ReAllocate(Stem.Live, state, offset, option);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="temp"></param>
        /// <param name="state"></param>
        /// <param name="offset"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        private Biomass ReAllocate(Biomass temp, string state, double offset, int option)
        {
            double a, b, c, newValue;
            if (state == "Wt")
            {
                a = temp.StructuralWt;
                b = temp.NonStructuralWt;
                c = temp.MetabolicWt;
            }
            else if (state == "N")
            {
                a = temp.StructuralN;
                b = temp.NonStructuralN;
                c = temp.MetabolicN;
            }
            else
                throw new Exception("Unknow biomass data type!");

            newValue = a + b + c;

            if (newValue == 0)
            {
                return temp;
            }
            else
            {
                newValue = newValue * (1 + option * offset) + offset * (1 - option);
                newValue = ConstrainToBound(newValue, 0, newValue + 1);
                if (AllocationRule == 0)
                {
                    double sum = a + b + c;
                    if (state == "Wt")
                    {
                        if (sum != 0)
                        {
                            temp.StructuralWt = newValue * a / sum;
                            temp.NonStructuralWt = newValue * b / sum;
                            temp.MetabolicWt = newValue * c / sum;
                        }
                        else
                        {
                            temp.StructuralWt = 0;
                            temp.NonStructuralWt = 0;
                            temp.MetabolicWt = 0;
                        }
                    }
                    else if (state == "N")
                    {
                        if (sum != 0)
                        {
                            temp.StructuralN = newValue * a / sum;
                            temp.NonStructuralN = newValue * b / sum;
                            temp.MetabolicN = newValue * c / sum;
                        }
                        else
                        {
                            temp.StructuralN = 0;
                            temp.NonStructuralN = 0;
                            temp.MetabolicN = 0;
                        }
                    }
                }
                else
                    throw new Exception("Wrong AllocationRule type!");

                return temp;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        private double ConstrainToBound(double value, double lower, double upper)
        {
            if (value < lower)
                value = lower;
            else if (value > upper)
                value = upper;
            return value;
        }
        #endregion

    }
}