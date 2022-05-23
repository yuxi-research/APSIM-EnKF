﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Models.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Models.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;Plant&gt;
        ///  &lt;Name&gt;Maize&lt;/Name&gt;
        ///  &lt;Memo&gt;
        ///    &lt;Name&gt;TitlePage&lt;/Name&gt;
        ///    &lt;MemoText&gt;
        ///      &lt;![CDATA[
        ///# The APSIM Maize Model
        ///
        ///_Brown, H.E., Teixeira, E.I., Huth, N.I. and Holzworth, D.P._
        ///
        ///The APSIM maize model has been developed using the Plant Modelling Framework (PMF) of [brown_plant_2014]. This new framework provides a library of plant organ and process submodels that can be coupled, at runtime, to construct a model in much the same way that models can be coupled to construct a simulation. This mea [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Maize {
            get {
                return ResourceManager.GetString("Maize", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;OilPalm&gt;
        ///  &lt;Name&gt;OilPalm&lt;/Name&gt;
        ///  &lt;Memo&gt;
        ///    &lt;Name&gt;Memo&lt;/Name&gt;
        ///    &lt;MemoText&gt;&lt;![CDATA[The base configuration of the oil palm model has been configured to match commercial dura x pisifera palms developed in Dami, West New Britain in Papua New Guinea.  Other varieties are specified in terms of how they differ from this base variety.]]&gt;&lt;/MemoText&gt;
        ///  &lt;/Memo&gt;
        ///  &lt;UnderstoryCoverMax&gt;0.4&lt;/UnderstoryCoverMax&gt;
        ///  &lt;UnderstoryLegumeFraction&gt;1&lt;/UnderstoryLegumeFraction&gt;
        ///  &lt;InterceptionFraction&gt;0&lt;/InterceptionFra [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string OilPalm {
            get {
                return ResourceManager.GetString("OilPalm", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
        ///&lt;ResidueTypesList&gt;
        ///  &lt;Name&gt;ResidueTypes&lt;/Name&gt;
        ///  &lt;ResidueType&gt;
        ///    &lt;fom_type&gt;base_type&lt;/fom_type&gt;
        ///    &lt;fraction_C description=&quot;fraction of Carbon in FOM (0-1)&quot;&gt;0.4&lt;/fraction_C&gt;
        ///    &lt;po4ppm description=&quot;labile P concentration(ppm)&quot;&gt;0.0&lt;/po4ppm&gt;
        ///    &lt;nh4ppm description=&quot;ammonium N concentration (ppm)&quot;&gt;0.0&lt;/nh4ppm&gt;
        ///    &lt;no3ppm description=&quot;nitrate N concentration (ppm)&quot;&gt;0.0&lt;/no3ppm&gt;
        ///    &lt;specific_area description=&quot;specific area of residue (ha/kg&quot;&gt;0.0005&lt;/specifi [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string ResidueTypes {
            get {
                return ResourceManager.GetString("ResidueTypes", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;parameters name=&quot;standard&quot; version=&quot;2.0&quot;&gt;
        ///  &lt;par name=&quot;editor&quot;&gt;Andrew Moore&lt;/par&gt;
        ///  &lt;par name=&quot;edited&quot;&gt;30 Jan 2013&lt;/par&gt;
        ///  &lt;par name=&quot;dairy&quot;&gt;false&lt;/par&gt;
        ///  &lt;par name=&quot;c-srs-&quot;&gt;1.2,1.4&lt;/par&gt;
        ///  &lt;par name=&quot;c-i-&quot;&gt;,1.7,,,,25.0,22.0,,,,,0.15,,0.002,0.5,1.0,0.01,20.0,3.0,1.5&lt;/par&gt;
        ///  &lt;par name=&quot;c-r-&quot;&gt;0.8,0.17,1.7,,0.6,,,,0.14,0.28,10.5,0.8,0.35,1.0,0.0,0.0,0.012,1.0,1.0,11.5&lt;/par&gt;
        ///  &lt;par name=&quot;c-k-&quot;&gt;0.5,0.02,0.85,0.7,0.4,0.02,0.6,0.133,0.95,0.84,0.8,0.7,0.035,0.33,0.12,0.043&lt;/par&gt;
        ///  &lt;par name=&quot;c-m-&quot;&gt;0.09,,0. [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string RUMINANT_PARAM_GLB {
            get {
                return ResourceManager.GetString("RUMINANT_PARAM_GLB", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;Plant&gt;
        ///  &lt;Name&gt;SCRUM&lt;/Name&gt;
        ///  &lt;Memo&gt;
        ///    &lt;Name&gt;TitlePage&lt;/Name&gt;
        ///    &lt;MemoText&gt;
        ///      &lt;![CDATA[
        ///# SCRUM: the Simple Crop Resource Uptake Model
        ///
        ///_Hamish Brown and Rob Zyskowski, Plant and Food Research, New Zealand_
        ///
        ///This model has been built using the Plant Modelling Framework (PMF) of [brown_plant_2014] to simulate a range of different crops in simulations where water and nitrogen balance are of interest but a fully mechanistic plant model is not needed or is not available. It is a daily time step implement [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string SCRUM {
            get {
                return ResourceManager.GetString("SCRUM", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to     &lt;Plant&gt;
        ///      &lt;Name&gt;Slurp&lt;/Name&gt;
        ///      &lt;Memo&gt;
        ///        &lt;Name&gt;TitlePage&lt;/Name&gt;
        ///        &lt;MemoText&gt;&lt;![CDATA[
        ///# SLURP: the Sound of a crop using water
        ///
        ///This model has been built using the Plant Modelling Framework (PMF) of [brown_plant_2014] to provide a simple representation of crops.  It is usefull for water and nitrogen balance studies where the focus is on soil processes and a very simple crop is adequate.  The model does not predict crop growth, development or yields.  It simply takes up water an [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Slurp {
            get {
                return ResourceManager.GetString("Slurp", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Name                              |R    DM    DMD    M/D     EE     CP     dg    ADIP     P        S       AA    MaxP Locales
        ///Alfalfa Hay Early-bloom           |Y  0.900  0.640   9.50  0.030  0.200  0.650  0.110  0.00250  0.00300  1.200  0.000 ca;us         es:&quot;Alfalfa Heno florac temprana&quot;
        ///Alfalfa Hay Full-bloom            |Y  0.900  0.610   9.00  0.030  0.170  0.650  0.160  0.00240  0.00300  1.200  0.000 ca;us         es:&quot;Alfalfa Heno plena floración&quot;
        ///Alfalfa Hay Mature                |Y  0.900  0.540  [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Supplement {
            get {
                return ResourceManager.GetString("Supplement", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to   &lt;Plant15&gt;
        ///      &lt;Name&gt;Wheat&lt;/Name&gt;
        ///  &lt;AutoHarvest&gt;false&lt;/AutoHarvest&gt;
        ///  &lt;CropType&gt;Wheat&lt;/CropType&gt;
        ///  &lt;EOCropFactor&gt;1.5&lt;/EOCropFactor&gt;
        ///  &lt;NSupplyPreference&gt;active&lt;/NSupplyPreference&gt;
        ///  &lt;DoRetranslocationBeforeNDemand&gt;false&lt;/DoRetranslocationBeforeNDemand&gt;
        ///  &lt;RemovedByAnimal /&gt;
        ///      &lt;Population1&gt;
        ///        &lt;Name&gt;Population&lt;/Name&gt;
        ///        &lt;PhaseBasedSwitch&gt;
        ///          &lt;Name&gt;CropFailureStressPeriod&lt;/Name&gt;
        ///          &lt;Start&gt;Emergence&lt;/Start&gt;
        ///          &lt;End&gt;Flowering&lt;/End&gt;
        ///        &lt;/PhaseBasedSwitch&gt; [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Wheat {
            get {
                return ResourceManager.GetString("Wheat", resourceCulture);
            }
        }
    }
}
