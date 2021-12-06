
using System;

namespace APSIM.Shared.Soils
{
    /// <summary>A soil analysis class.</summary>
    [Serializable]
    public class Analysis
    {
        /// <summary>Gets or sets the thickness.</summary>
        public double[] Thickness { get; set; }

        /// <summary>Gets or sets the rocks.</summary>
        [Units("%")]
        public double[] Rocks { get; set; }

        /// <summary>Gets or sets the rocks metadata.</summary>
        public string[] RocksMetadata { get; set; }

        /// <summary>Gets or sets the texture.</summary>
        public string[] Texture { get; set; }

        /// <summary>Gets or sets the texture metadata.</summary>
        public string[] TextureMetadata { get; set; }

        /// <summary>Gets or sets the munsell colour.</summary>
        public string[] MunsellColour { get; set; }

        /// <summary>Gets or sets the munsell metadata.</summary>
        public string[] MunsellMetadata { get; set; }

        /// <summary>Gets or sets the ec.</summary>
        [Units("1:5 dS/m")]
        public double[] EC { get; set; }

        /// <summary>Gets or sets the ec metadata.</summary>
        public string[] ECMetadata { get; set; }

        /// <summary>Gets or sets the ph.</summary>
        public double[] PH { get; set; }

        /// <summary>Gets or sets the ph metadata.</summary>
        public string[] PHMetadata { get; set; }

        /// <summary>Gets or sets the cl.</summary>
        [Units("mg/kg")]
        public double[] CL { get; set; }

        /// <summary>Gets or sets the cl metadata.</summary>
        public string[] CLMetadata { get; set; }

        /// <summary>Gets or sets the boron.</summary>
        [Units("Hot water mg/kg")]
        public double[] Boron { get; set; }

        /// <summary>Gets or sets the boron metadata.</summary>
        public string[] BoronMetadata { get; set; }

        /// <summary>Gets or sets the cec.</summary>
        [Units("cmol+/kg")]
        public double[] CEC { get; set; }

        /// <summary>Gets or sets the cec metadata.</summary>
        public string[] CECMetadata { get; set; }

        /// <summary>Gets or sets the ca.</summary>
        [Units("cmol+/kg")]
        public double[] Ca { get; set; }

        /// <summary>Gets or sets the ca metadata.</summary>
        public string[] CaMetadata { get; set; }

        /// <summary>Gets or sets the mg.</summary>
        [Units("cmol+/kg")]
        public double[] Mg { get; set; }

        /// <summary>Gets or sets the mg metadata.</summary>
        public string[] MgMetadata { get; set; }

        /// <summary>Gets or sets the na.</summary>
        [Units("cmol+/kg")]
        public double[] Na { get; set; }

        /// <summary>Gets or sets the na metadata.</summary>
        public string[] NaMetadata { get; set; }

        /// <summary>Gets or sets the k.</summary>
        [Units("cmol+/kg")]
        public double[] K { get; set; }

        /// <summary>Gets or sets the k metadata.</summary>
        public string[] KMetadata { get; set; }

        /// <summary>Gets or sets the esp.</summary>
        [Units("%")]
        public double[] ESP { get; set; }

        /// <summary>Gets or sets the esp metadata.</summary>
        public string[] ESPMetadata { get; set; }

        /// <summary>Gets or sets the mn.</summary>
        [Units("mg/kg")]
        public double[] Mn { get; set; }

        /// <summary>Gets or sets the mn metadata.</summary>
        public string[] MnMetadata { get; set; }

        /// <summary>Gets or sets the al.</summary>
        [Units("cmol+/kg")]
        public double[] Al { get; set; }

        /// <summary>Gets or sets the al metadata.</summary>
        public string[] AlMetadata { get; set; }

        /// <summary>Gets or sets the particle size sand.</summary>
        [Units("%")]
        public double[] ParticleSizeSand { get; set; }

        /// <summary>Gets or sets the particle size sand metadata.</summary>
        public string[] ParticleSizeSandMetadata { get; set; }

        /// <summary>Gets or sets the particle size silt.</summary>
        [Units("%")]
        public double[] ParticleSizeSilt { get; set; }

        /// <summary>Gets or sets the particle size silt metadata.</summary>
        public string[] ParticleSizeSiltMetadata { get; set; }

        /// <summary>Gets or sets the particle size clay.</summary>
        [Units("%")]
        public double[] ParticleSizeClay { get; set; }

        /// <summary>Gets or sets the particle size clay metadata.</summary>
        public string[] ParticleSizeClayMetadata { get; set; }

        /// <summary>Units for PH.</summary>
        public enum PHUnitsEnum 
        {
            /// <summary>The water units</summary>
            Water,

            /// <summary>The CaCL2</summary>
            CaCl2 
        }

        /// <summary>Gets or sets the ph units.</summary>
        public PHUnitsEnum PHUnits { get; set; }
              
        // Support for Boron units.
        /// <summary>Valid units for Boron</summary>
        public enum BoronUnitsEnum 
        {
            /// <summary>The hot water</summary>
            HotWater,

            /// <summary>The hot ca CL2</summary>
            HotCaCl2 
        }

        /// <summary>Gets or sets the boron units.</summary>
        public BoronUnitsEnum BoronUnits { get; set; }
    }
}
