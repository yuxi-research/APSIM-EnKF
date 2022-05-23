﻿// -----------------------------------------------------------------------
// <copyright file="Sample.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
// -----------------------------------------------------------------------
namespace Models.Soils
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml.Serialization;
    using Models.Core;
    using APSIM.Shared.Utilities;

    /// <summary>
    /// The class represents a soil sample.
    /// </summary>
    [Serializable]
    [ViewName("UserInterface.Views.ProfileView")]
    [PresenterName("UserInterface.Presenters.ProfilePresenter")]
    [ValidParent(ParentType=typeof(Soil))]
    public class Sample : Model
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sample" /> class.
        /// </summary>
        public Sample() 
        { 
            this.Name = "Sample"; 
        }

        #region Enumerations
        /// <summary>
        /// An enumeration for specifying nitrogen units.
        /// </summary>
        public enum NUnitsEnum
        {
            /// <summary>
            /// parts per million
            /// </summary>
            ppm,

            /// <summary>
            /// kilograms per hectare
            /// </summary>
            kgha
        }

        /// <summary>
        /// An enumeration for specifying soil water units
        /// </summary>
        public enum SWUnitsEnum
        {
            /// <summary>
            /// Volumetric mm/mm
            /// </summary>
            Volumetric,

            /// <summary>
            /// Gravimetric soil water
            /// </summary>
            Gravimetric,

            /// <summary>
            /// mm of water
            /// </summary>
            mm
        }

        /// <summary>
        /// An enumeration for specifying organic carbon units
        /// </summary>
        public enum OCSampleUnitsEnum
        {
            /// <summary>
            /// Organic carbon as total percent
            /// </summary>
            Total,

            /// <summary>
            /// Organic carbon as walkley black percent
            /// </summary>
            [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed.")]
            WalkleyBlack
        }

        /// <summary>
        /// An enumeration for specifying PH units
        /// </summary>
        public enum PHSampleUnitsEnum
        {
            /// <summary>
            /// PH as water method
            /// </summary>
            Water,

            /// <summary>
            /// PH as Calcium chloride method
            /// </summary>
            CaCl2
        }
        #endregion

        /// <summary>
        /// Gets or sets the sample thickness (mm)
        /// </summary>
        public double[] Thickness { get; set; }

        /// <summary>
        /// Gets or sets the depth strings e.g. 0-10
        /// </summary>
        [Summary]
        [Description("Depth")]
        [XmlIgnore]
        [Units("cm")]
        public string[] Depth
        {
            get
            {
                return Soil.ToDepthStrings(this.Thickness);
            }

            set
            {
                this.Thickness = Soil.ToThickness(value);
            }
        }

        #region Raw variables serialised and edited in GUI

        /// <summary>
        /// Gets or sets the nitrate NO3. Units will be as specified by NO3Units
        /// </summary>
        [Description("NO3")]
        [Display(Format = "N1", ShowTotal = true)]
        public double[] NO3 { get; set; }

        /// <summary>
        /// Gets or sets ammonia NH4. Units will be as specified by NH4Units
        /// </summary>
        [Description("NH4")]
        [Display(Format = "N1", ShowTotal = true)]
        public double[] NH4 { get; set; }

        /// <summary>
        /// Gets or sets soil water. Units will be as specified by SWUnits
        /// </summary>
        [Description("SW")]
        [Display(Format = "N3", ShowTotal = true)]
        public double[] SW { get; set; }

        /// <summary>
        /// Gets or sets organic carbon. Units will be as specified by OCUnits
        /// </summary>
        [Description("OC")]
        public double[] OC { get; set; }

        /// <summary>
        /// Gets or sets electrical conductivity (1:5 dS/m)
        /// </summary>
        [Description("EC")]
        [Units("1:5 dS/m")]
        public double[] EC { get; set; }

        /// <summary>
        /// Gets or sets chloride (mg/kg)
        /// </summary>
        [Description("CL")]
        [Units("mg/kg")]
        public double[] CL { get; set; }

        /// <summary>
        /// Gets or sets ESP (%)
        /// </summary>
        [Description("ESP")]
        [Units("%")]
        public double[] ESP { get; set; }

        /// <summary>
        /// Gets or sets PH. Units will be as specified by PHUnits
        /// </summary>
        [Description("PH")]
        public double[] PH { get; set; }

        #endregion

        #region Units
        /// <summary>
        /// Gets or sets the units of NO3
        /// </summary>
        public NUnitsEnum NO3Units { get; set; }

        /// <summary>Change the units used for NO3.</summary>
        /// <param name="ToUnits">To units.</param>
        public void NO3UnitsSet(NUnitsEnum ToUnits)
        {
            if (ToUnits != NO3Units)
            {
                // convert the numbers
                if (ToUnits == NUnitsEnum.ppm)
                    NO3 = NO3ppm;
                else
                    NO3 = NO3kgha;
                NO3Units = ToUnits;
            }
        }

        /// <summary>
        /// Gets or sets the units of NH4
        /// </summary>
        public NUnitsEnum NH4Units { get; set; }

        /// <summary>Change the units used for NH4.</summary>
        /// <param name="ToUnits">To units.</param>
        public void NH4UnitsSet(NUnitsEnum ToUnits)
        {
            if (ToUnits != NH4Units)
            {
                // convert the numbers
                if (ToUnits == NUnitsEnum.ppm)
                    NH4 = NH4ppm;
                else
                    NH4 = NH4kgha;
                NH4Units = ToUnits;
            }
        }

        /// <summary>
        /// Gets or sets the units of SW
        /// </summary>
        public SWUnitsEnum SWUnits { get; set; }

        /// <summary>Change the units used for SW.</summary>
        /// <param name="ToUnits">To units.</param>
        public void SWUnitsSet(SWUnitsEnum ToUnits)
        {
            if (ToUnits != SWUnits)
            {
                // convert the numbers
                if (ToUnits == SWUnitsEnum.Gravimetric)
                    SW = SWGravimetric;
                else if (ToUnits == SWUnitsEnum.mm)
                    SW = SWmm;
                else if (ToUnits == SWUnitsEnum.Volumetric)
                    SW = SWVolumetric;
                SWUnits = ToUnits;
            }
        }

        /// <summary>
        /// Gets or sets the units of organic carbon
        /// </summary>
        public OCSampleUnitsEnum OCUnits { get; set; }

        /// <summary>Ocs the units set.</summary>
        /// <param name="ToUnits">To units.</param>
        public void OCUnitsSet(OCSampleUnitsEnum ToUnits)
        {
            if (ToUnits != OCUnits)
            {
                // convert the numbers
                if (ToUnits == OCSampleUnitsEnum.WalkleyBlack)
                    OC = OCWalkleyBlack;
                else
                    OC = OCTotal;
                OCUnits = ToUnits;
            }
        }


        /// <summary>
        /// Gets or sets the units of P
        /// </summary>
        public PHSampleUnitsEnum PHUnits { get; set; }

        /// <summary>Phes the units set.</summary>
        /// <param name="ToUnits">To units.</param>
        public void PHUnitsSet(PHSampleUnitsEnum ToUnits)
        {
            if (ToUnits != PHUnits)
            {
                // convert the numbers
                if (ToUnits == PHSampleUnitsEnum.Water)
                {
                    PH = PHWater;
                }
                else
                {
                    PH = PHCaCl2;
                }
                PHUnits = ToUnits;
            }
        }


        #endregion

        #region Properties for returning variables with particular units

        /// <summary>
        /// Gets NO3. Units: ppm.
        /// </summary>
        public double[] NO3ppm
        {
            get
            {
                if (this.NO3 != null && this.Soil != null)
                {
                    double[] values = (double[])this.NO3.Clone();
                    if (this.NO3Units != NUnitsEnum.ppm)
                    {
                        double[] bd = Soil.BDMapped(this.Thickness);
                        for (int i = 0; i < values.Length; i++)
                        {
                            if (values[i] != double.NaN)
                            {
                                values[i] = values[i] * 100 / (bd[i] * this.Thickness[i]);
                            }
                        }
                    }

                    return values;
                }

                return null;
            }
        }
        
        /// <summary>
        /// Gets NO3. Units: kg/ha.
        /// </summary>
        [Summary]
        [Display(Format = "N1", ShowTotal = true)]
        public double[] NO3kgha
        {
            get
            {
                if (this.NO3 != null && this.Soil != null)
                {
                    double[] values = (double[])this.NO3.Clone();
                    if (this.NO3Units != NUnitsEnum.kgha)
                    {
                        double[] bd = Soil.BDMapped(this.Thickness);
                        for (int i = 0; i < values.Length; i++)
                        {
                            if (values[i] != double.NaN)
                            {
                                values[i] = values[i] / 100 * (bd[i] * this.Thickness[i]);
                            }
                        }
                    }

                    return values;
                }

                return null;
            }
        }
        
        /// <summary>
        /// Gets NH4. Units: ppm.
        /// </summary>
        public double[] NH4ppm
        {
            get
            {
                if (this.NH4 != null && this.Soil != null)
                {
                    double[] values = (double[])this.NH4.Clone();
                    if (this.NH4Units != NUnitsEnum.ppm)
                    {
                        double[] bd = Soil.BDMapped(this.Thickness);
                        for (int i = 0; i < values.Length; i++)
                        {
                            if (values[i] != double.NaN)
                            {
                                values[i] = values[i] * 100 / (bd[i] * this.Thickness[i]);
                            }
                        }
                    }

                    return values;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets NH4. Units: kg/ha.
        /// </summary>
        [Summary]
        [Display(Format = "N1", ShowTotal = true)]
        public double[] NH4kgha
        {
            get
            {
                if (this.NH4 != null && this.Soil != null)
                {
                    double[] values = (double[])this.NH4.Clone();
                    if (this.NH4Units != NUnitsEnum.kgha)
                    {
                        double[] bd = Soil.BDMapped(this.Thickness);
                        for (int i = 0; i < values.Length; i++)
                        {
                            if (values[i] != double.NaN)
                            {
                                values[i] = values[i] / 100 * (bd[i] * this.Thickness[i]);
                            }
                        }
                    }

                    return values;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets SW. Units: mm/mm.
        /// </summary>
        [Summary]
        [Display(Format = "N1", ShowTotal = true)]
        public double[] SWmm
        {
            get
            {
                if (this.Soil != null && this.SW != null)
                {
                    if (this.SWUnits == SWUnitsEnum.Volumetric)
                    {
                        return MathUtilities.Multiply(this.SW, this.Thickness);
                    }
                    else if (this.SWUnits == SWUnitsEnum.Gravimetric)
                    {
                        return MathUtilities.Multiply(MathUtilities.Multiply(this.SW, this.Soil.BDMapped(this.Thickness)), this.Thickness);
                    }
                    else
                    {
                        return this.SW;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Gets SW. Units: kg/kg.
        /// </summary>
        public double[] SWGravimetric
        {
            get
            {
                if (this.Soil != null && this.SW != null)
                {
                    if (this.SWUnits == SWUnitsEnum.Volumetric)
                    {
                        return MathUtilities.Divide(this.SW, this.Soil.BDMapped(this.Thickness));
                    }
                    else if (this.SWUnits == SWUnitsEnum.Gravimetric)
                    {
                        return this.SW;
                    }
                    else
                    {
                        return MathUtilities.Divide(MathUtilities.Divide(this.SW, this.Soil.BDMapped(this.Thickness)), this.Thickness);
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Gets SW. Units: mm/mm.
        /// </summary>
        public double[] SWVolumetric
        {
            get
            {
                if (this.Soil != null && this.SW != null)
                {
                    if (this.SWUnits == SWUnitsEnum.Volumetric)
                    {
                        return this.SW;
                    }
                    else if (this.SWUnits == SWUnitsEnum.Gravimetric)
                    {
                        return MathUtilities.Multiply(this.SW, this.Soil.BDMapped(this.Thickness));
                    }
                    else
                    {
                        return MathUtilities.Divide(this.SW, this.Thickness);
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Gets organic carbon. Units: Total %
        /// </summary>
        public double[] OCTotal
        {
            get
            {
                if (this.OCUnits == OCSampleUnitsEnum.WalkleyBlack)
                {
                    return MathUtilities.Multiply_Value(this.OC, 1.3);
                }
                else
                {
                    return this.OC;
                }
            }
        }

        /// <summary>
        /// Gets organic carbon. Units: WalkleyBlack %
        /// </summary>
        public double[] OCWalkleyBlack
        {
            get
            {
                if (this.OCUnits == OCSampleUnitsEnum.Total)
                {
                    return MathUtilities.Divide_Value(this.OC, 1.3);
                }
                else
                {
                    return this.OC;
                }
            }
        }

        /// <summary>
        /// Gets PH. Units: (1:5 water)
        /// </summary>
        public double[] PHWater
        {
            get
            {
                if (this.PHUnits == PHSampleUnitsEnum.CaCl2)
                {
                    // pH in water = (pH in CaCl X 1.1045) - 0.1375
                    return MathUtilities.Subtract_Value(MathUtilities.Multiply_Value(this.PH, 1.1045), 0.1375);
                }
                else
                {
                    return this.PH;
                }
            }
        }

        /// <summary>
        /// Gets PH. Units: (1:5 water)
        /// </summary>
        public double[] PHCaCl2
        {
            get
            {
                if (this.PHUnits == PHSampleUnitsEnum.Water)
                {
                    // pH in CaCl = (pH in water + 0.1375) / 1.1045
                    return MathUtilities.Divide_Value(MathUtilities.AddValue(PH, 0.1375), 1.1045);
                }
                else
                {
                    return this.PH;
                }
            }
        }
        #endregion

        /// <summary>
        /// Gets the soil associated with this sample
        /// </summary>
        private Soil Soil
        {
            get
            {
                return Apsim.Parent(this, typeof(Soil)) as Soil;
            }
        }
    }
}
