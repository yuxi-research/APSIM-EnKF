using System;
using System.Collections.Generic;
using System.Text;
using Models.Core;
using Models.PMF.Organs;
using System.Xml.Serialization;

namespace Models.PMF.Phen
{
    /// <summary>
    /// Leaf death phenological phase
    /// </summary>
    [Serializable]
    [ViewName("UserInterface.Views.GridView")]
    [PresenterName("UserInterface.Presenters.PropertyPresenter")]
    public class LeafDeathPhase : Phase
    {
        /// <summary>The leaf</summary>
        [Link]
        private Leaf Leaf = null;

        /// <summary>The structure</summary>
        [Link]
        Structure Structure = null;

        /// <summary>The dead node no at start</summary>
        private double DeadNodeNoAtStart = 0;
        /// <summary>The first</summary>
        private bool First = true;

        /// <summary>Resets the phase.</summary>
        public override void ResetPhase()
        {
            base.ResetPhase();
            DeadNodeNoAtStart = 0;
            First = true;
        }
        /// <summary>Do our timestep development</summary>
        /// <param name="PropOfDayToUse">The property of day to use.</param>
        /// <returns></returns>
        public override double DoTimeStep(double PropOfDayToUse)
        {
            base.DoTimeStep(PropOfDayToUse);

            if (First)
            {
                DeadNodeNoAtStart = Leaf.DeadCohortNo;
                First = false;
            }

            if ((Leaf.DeadCohortNo >= Structure.MainStemFinalNodeNumber.Value()) || (Leaf.CohortsInitialised == false))
                return 0.00001;
            else
                return 0;
        }

        /// <summary>Return a fraction of phase complete.</summary>
        /// <value>The fraction complete.</value>
        [XmlIgnore]
        public override double FractionComplete
        {
            get
            {
                double F = (Leaf.DeadCohortNo - DeadNodeNoAtStart) / (Structure.MainStemFinalNodeNumber.Value() - DeadNodeNoAtStart);
                if (F < 0) F = 0;
                if (F > 1) F = 1;
                return F;
            }
            set
            {
                throw new Exception("Not possible to set phenology into "+ this + " phase (at least not at the moment because there is no code to do it");
            }
        }

    }
}

      
      
