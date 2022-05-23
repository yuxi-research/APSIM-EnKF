using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Core;
using System.Xml.Serialization;
using Models;
using Models.Soils;
using Models.PMF.OldPlant;

namespace Models.Sensitivity
{
    /// <summary>
    /// Sensitivity analysis for input data.
    /// </summary>
    [Serializable]
    [ViewName("UserInterface.Views.GridView")]
    [PresenterName("UserInterface.Presenters.PropertyPresenter")]
    [ValidParent(ParentType = typeof(Simulations))]
    [ValidParent(ParentType = typeof(Simulation))]
    [ValidParent(ParentType = typeof(ISensitivity))]
    public class LeafNodeSens : Model, ISensitivity
    {

        #region ******* Links. *******

        [Link(IsOptional = true)]
        Leaf1 Leaf = null;

        #endregion

        #region ******* Public field. *******

        /// <summary>Node offset</summary>
        [Description("Node offset")]
        public double NodeOffset { get; set; }

        /// <summary>Leaves per node offset</summary>
        [Description("Leaves per Node offset")]
        public int[] LeavePerNodeOffset { get; set; }

        /// <summary></summary>
        [Description("Change leaf and node on date")]
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
        [EventSubscribe("DoLeafNodeSens")]
        private void OnDoLeafNodeSens(object sender, EventArgs e)
        {
            DoSensitivity();
        }

        #endregion

        #region ******* Methods. *******

        /// <summary>
        /// 
        /// </summary>
        private void DoSensitivity()
        {
            double NodeNo;
            NodeNo = Leaf.NodeNo;
            NodeNo = NodeNo + NodeOffset;
            Leaf.NodeNo = ConstrainToBound(NodeNo, Leaf.InitialLeafNumber, NodeNo + 1);
            double[] LeavesPerNode = new double[Leaf.LeafNo.Count()];
            for(int i=0;i< (int)Leaf.NodeNo; i++)
            {
                LeavesPerNode[i] = Leaf.LeafNo[i] + LeavePerNodeOffset[i];
                LeavesPerNode[i] = ConstrainToBound(LeavesPerNode[i], 1, 25);
            }
            Leaf.LeafNo = LeavesPerNode;
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
