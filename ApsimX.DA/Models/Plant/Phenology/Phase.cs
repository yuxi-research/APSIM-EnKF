using System;
using System.Collections.Generic;
using System.Text;
using Models.Core;
using System.ComponentModel;
using Models.PMF.Functions;
using System.IO;
using System.Xml.Serialization;
using Models.Sensitivity;

namespace Models.PMF.Phen
{
    /// <summary>
    /// A base model representing a phenological phase。
    /// </summary>
    /// \warning Do not use this model in \ref Models.PMF.Phen.Phenology "Phenology" model.
    /// \pre All Phase models have to be the children of 
    ///     \ref Models.PMF.Phen.Phenology "Phenology" model.
    /// \param End The stage name of phase ending, 
    ///     which should be the same as the Start name 
    ///     in previous phase except the first phase.
    /// \param Start The stage name of phase starting, 
    ///     which should be the same as the End name.
    ///     in next phase except the last phase.
    /// \param ThermalTime Optional. The daily thermal time.
    /// \param Stress Optional. The environmental stress factors.
    /// \retval TTinPhase The cumulated thermal time in current phase (<sup>o</sup>Cd).
    /// \retval TTForToday The thermal time for today in current phase (<sup>o</sup>Cd).
    /// \retval FractionComplete The complete fraction in current phase (from 0 to 1).
    /// <remarks>
    /// This is a base model in phenology. \ref Models.PMF.Phen.Phenology "Phenology" model
    /// will call \a DoTimeStep to calculate phenology development.
    /// 
    /// The actual daily thermal time (\f$\Delta TT_{pheno}\f$) is calculated 
    /// the daily thermal time (\f$\Delta TT\f$) multiply by 
    /// generic and/or environmental stresses (\f$f_{s,\, pheno}\f$) if child \a Stress exists. 
    /// \f[
    ///     \Delta TT_{pheno}=\Delta TT\times f_{s,\, pheno}
    /// \f]
    /// if \a Stress no exist.
    /// \f[
    ///     \Delta TT_{pheno}=\Delta TT
    /// \f] 
    /// The daily thermal time (\f$\Delta TT\f$) gets from the value of \a ThermalTime. 
    /// The generic and/or environmental stresses (\f$f_{s,\, pheno}\f$) gets from the value of \a Stress. 
    /// </remarks>
    [Serializable]
    [ValidParent(ParentType = typeof(Phenology))]
    abstract public class Phase : Model
    {
        /// <summary>The start</summary>
        [Models.Core.Description("Start")]
        public string Start { get; set; }

        /// <summary>The end</summary>
        [Models.Core.Description("End")]
        public string End { get; set; }
        /// <summary>The phase that this one is equivelent to</summary>
        [Models.Core.Description("Phase that this is equivelent to in phenology order")]
        public string PhaseParallel { get; set; }

        /// <summary>The phenology</summary>
        [Link]
        protected Phenology Phenology = null;

        // ThermalTime is optional because GerminatingPhase doesn't require it.
        /// <summary>The thermal time</summary>
        [Link(IsOptional = true)]
        public IFunction ThermalTime = null;  //FIXME this should be called something to represent rate of progress as it is sometimes used to represent other things that are not thermal time.

        /// <summary>The stress</summary>
        [Link(IsOptional = true)]
        public IFunction Stress = null;

        [Link(IsOptional = true)]
        Clock Clock = null;

        [Link(IsOptional = true)]
        ThermalTimeSens TTSens = null;

        private double TTDeficit;

        /// <summary>The property of day unused</summary>
        protected double PropOfDayUnused = 0;
        /// <summary>The _ tt for today</summary>
        protected double _TTForToday = 0;

        /// <summary>Gets the tt for today.</summary>
        /// <value>The tt for today.</value>
        public double TTForToday
        {
            get
            {
                if (ThermalTime == null)
                    return 0;
                return ThermalTime.Value();
            }
        }

        /// <summary>Gets the t tin phase.</summary>
        /// <value>The t tin phase.</value>
        [XmlIgnore]
        public double TTinPhase { get; set; }

        /// <summary>
        /// This function increments thermal time accumulated in each phase
        /// and returns a non-zero value if the phase target is met today so
        /// the phenology class knows to progress to the next phase and how
        /// much tt to pass it on the first day.
        /// </summary>
        /// <param name="PropOfDayToUse">The property of day to use.</param>
        /// <returns></returns>
        virtual public double DoTimeStep(double PropOfDayToUse)
        {
            // Calculate the TT for today and Accumulate.
            _TTForToday = ThermalTime.Value() * PropOfDayToUse;

            if (TTSens != null && TTSens.DoTTSens)
            {
                if (TTSens.Date == Clock.Today)
                {
                    TTDeficit += TTSens.TTOffset;
                }

                if (TTDeficit != 0)
                {
                    if (_TTForToday + TTDeficit < 0)
                    {
                        TTDeficit = _TTForToday + TTDeficit;
                        _TTForToday = 0;
                    }
                    else
                    {
                        _TTForToday = _TTForToday + TTDeficit;
                        TTDeficit = 0;
                    }
                }
            }

            if (Stress != null)
            {
                _TTForToday *= Stress.Value();
            }
            TTinPhase += _TTForToday;

            return PropOfDayUnused;
        }

        // Return proportion of TT unused
        /// <summary>Adds the tt.</summary>
        /// <param name="PropOfDayToUse">The property of day to use.</param>
        /// <returns></returns>
        virtual public double AddTT(double PropOfDayToUse)
        {
            TTinPhase += ThermalTime.Value() * PropOfDayToUse;
            return 0;
        }
        /// <summary>Adds the specified DLT_TT.</summary>
        /// <param name="dlt_tt">The DLT_TT.</param>
        virtual public void Add(double dlt_tt) { TTinPhase += dlt_tt; }
        /// <summary>Gets the fraction complete.</summary>
        /// <value>The fraction complete.</value>
        [XmlIgnore]
        abstract public double FractionComplete { get; set; }

        /// <summary>Called when [simulation commencing].</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("Commencing")]
        private void OnSimulationCommencing(object sender, EventArgs e)
        { ResetPhase(); }
        /// <summary>Resets the phase.</summary>
        public virtual void ResetPhase()
        {
            _TTForToday = 0;
            TTinPhase = 0;
            PropOfDayUnused = 0;
        }


        /// <summary>Writes the summary.</summary>
        /// <param name="writer">The writer.</param>
        internal virtual void WriteSummary(TextWriter writer)
        {
            writer.WriteLine("      " + Name);
        }

        /// <summary>Writes documentation for this function by adding to the list of documentation tags.</summary>
        /// <param name="tags">The list of tags to add to.</param>
        /// <param name="headingLevel">The level (e.g. H2) of the headings.</param>
        /// <param name="indent">The level of indentation 1, 2, 3 etc.</param>
        public override void Document(List<AutoDocumentation.ITag> tags, int headingLevel, int indent)
        {
            // add a heading.
            tags.Add(new AutoDocumentation.Heading(Name + " Phase", headingLevel));

            // Describe the start and end stages
            tags.Add(new AutoDocumentation.Paragraph("This phase goes from " + Start + " to " + End + ".  ", indent));

            // write memos.
            foreach (IModel memo in Apsim.Children(this, typeof(Memo)))
                memo.Document(tags, -1, indent);

            // get description of this class.
            AutoDocumentation.GetClassDescription(this, tags, indent);

            // write children.
            foreach (IModel child in Apsim.Children(this, typeof(IFunction)))
                child.Document(tags, -1, indent);
        }
    }
}
