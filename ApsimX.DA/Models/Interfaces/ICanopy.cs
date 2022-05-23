﻿// -----------------------------------------------------------------------
// <copyright file="ICanopy.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
//-----------------------------------------------------------------------
namespace Models.Interfaces
{
    using System;

    /// <summary>This interface describes MicroClimate / canopy comms.</summary>
    public interface ICanopy
    {
        /// <summary>Albedo.</summary>
        double Albedo { get; }

        /// <summary>Gets or sets the gsmax.</summary>
        double Gsmax { get; }

        /// <summary>Gets or sets the R50.</summary>
        double R50 { get; }

        /// <summary>Gets the LAI (m^2/m^2)</summary>
        double LAI { get; }

        /// <summary>Gets the maximum LAI (m^2/m^2)</summary>
        double LAITotal { get; }

        /// <summary>Gets the cover green (0-1)</summary>
        double CoverGreen { get; }

        /// <summary>Gets the cover total (0-1)</summary>
        double CoverTotal { get; }

        /// <summary>Gets the canopy height (mm)</summary>
        double Height { get; }

        /// <summary>Gets the canopy depth (mm)</summary>
        double Depth { get; }

        /// <summary>Gets  FRGR.</summary>
        double FRGR { get; }

        /// <summary>Sets the potential evapotranspiration.</summary>
        double PotentialEP { get;  set; }

        /// <summary>Sets the light profile.</summary>
        CanopyEnergyBalanceInterceptionlayerType[] LightProfile { set; } 
    }

    /// <summary>
    /// A canopy energy balance type
    /// </summary>
    [Serializable]
    public class CanopyEnergyBalanceInterceptionlayerType
    {
        /// <summary>The thickness</summary>
        public double thickness;
        /// <summary>The amount</summary>
        public double amount;
    }
}
