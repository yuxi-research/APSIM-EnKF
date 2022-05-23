﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Models.Core;

namespace Models.WholeFarm.Resources
{
    /// <summary>
    /// This stores the parameters for a ruminant Type
    /// </summary>
    [Serializable]
    [ViewName("UserInterface.Views.GridView")]
    [PresenterName("UserInterface.Presenters.PropertyPresenter")]
    [ValidParent(ParentType = typeof(RuminantHerd))]
    public class RuminantType : WFModel
    {
        /// <summary>
        /// Breed
        /// </summary>
        [Description("Breed")]
        public string Breed { get; set; }

		#region Grow Activity

		/// <summary>
		/// Energy maintenance efficiency coefficient
		/// </summary>
		[Description("Energy maintenance efficiency coefficient")]
		public double EMaintCoefficient { get; set; }
		/// <summary>
		/// Energy maintenance efficiency intercept
		/// </summary>
		[Description("Energy maintenance efficiency intercept")]
		public double EMaintIntercept { get; set; }
		/// <summary>
		/// Energy maintenance exponent
		/// </summary>
		[Description("Energy maintenance exponent")]
		public double EMaintExponent { get; set; }
		/// <summary>
		/// Energy growth efficiency coefficient
		/// </summary>
		[Description("Energy growth efficiency coefficient")]
		public double EGrowthCoefficient { get; set; }
		/// <summary>
		/// Energy growth efficiency intercept
		/// </summary>
		[Description("Energy growth efficiency intercept")]
		public double EGrowthIntercept { get; set; }
		/// <summary>
		/// Energy lactation efficiency coefficient
		/// </summary>
		[Description("Energy lactation efficiency coefficient")]
		public double ELactationCoefficient { get; set; }
		/// <summary>
		/// Energy lactation efficiency intercept
		/// </summary>
		[Description("Energy lactation efficiency intercept")]
		public double ELactationIntercept { get; set; }
		/// <summary>
		/// Breed factor for maintenence energy
		/// </summary>
		[Description("Breed factor for maintenence energy")]
		public double Kme { get; set; }
		/// <summary>
		/// Parameter for energy for growth #1
		/// </summary>
		[Description("Parameter for energy for growth #1")]
		public double GrowthEnergyIntercept1 { get; set; }
		/// <summary>
		/// Parameter for energy for growth #2
		/// </summary>
		[Description("Parameter for energy for growth #2")]
		public double GrowthEnergyIntercept2 { get; set; }
		/// <summary>
		/// Growth efficiency
		/// </summary>
		[Description("Growth efficiency")]
		public double GrowthEfficiency { get; set; }

		/// <summary>
		/// Standard Reference Weight of female
		/// </summary>
		[Description("Standard Ref. Weight (kg) for a female")]
		public double SRWFemale { get; set; }
		/// <summary>
		/// Standard Reference Weight for male from female multiplier
		/// </summary>
		[Description("Male Standard Ref. Weight multiplier from female")]
		public double SRWMaleMultiplier { get; set; }
		/// <summary>
		/// Standard Reference Weight at birth
		/// </summary>
		[Description("Birth mass (proportion of female SRW)")]
		public double SRWBirth { get; set; }
		/// <summary>
		/// Age growth rate coefficient
		/// </summary>
		[Description("Age growth rate coefficient")]
		public double AgeGrowthRateCoefficient { get; set; }
		/// <summary>
		/// SWR growth scalar
		/// </summary>
		[Description("SWR growth scalar")]
		public double SRWGrowthScalar { get; set; }
		/// <summary>
		/// Intake coefficient in relation to Live Weight
		/// </summary>
		[Description("Intake coefficient in relation to Live Weight")]
		public double IntakeCoefficient { get; set; }
		/// <summary>
		/// Intake intercept In relation to SRW
		/// </summary>
		[Description("Intake intercept In relation to SRW")]
		public double IntakeIntercept { get; set; }
		/// <summary>
		/// Protein requirement coeff (g/kg feed)
		/// </summary>
		[Description("Protein requirement coeff (g/kg feed)")]
		public double ProteinCoefficient { get; set; }
		/// <summary>
		/// Weight(kg) of 1 animal equivalent(steer)
		/// </summary>
		[Description("Weight(kg) of 1 animal equivalent(steer)")]
		public double BaseAnimalEquivalent { get; set; }
		/// <summary>
		/// Maximum green in diet
		/// </summary>
		[Description("Maximum green in diet")]
		public double GreenDietMax { get; set; }
		/// <summary>
		/// Shape of curve for diet vs pasture
		/// </summary>
		[Description("Shape of curve for diet vs pasture")]
		public double GreenDietCoefficient { get; set; }
		/// <summary>
		/// Proportion green in pasture at zero in diet
		/// was %
		/// </summary>
		[Description("Proportion green in pasture at zero in diet")]
		public double GreenDietZero { get; set; }
		/// <summary>
		/// Coefficient to adjust intake for herbage quality
		/// </summary>
		[Description("Coefficient to adjust intake for herbage quality")]
		public double IntakeTropicalQuality { get; set; }
		/// <summary>
		/// Coefficient to adjust intake for tropical herbage quality
		/// </summary>
		[Description("Coefficient to adjust intake for tropical herbage quality")]
		public double IntakeCoefficientQuality { get; set; }
		/// <summary>
		/// Coefficient to adjust intake for herbage biomass
		/// </summary>
		[Description("Coefficient to adjust intake for herbage biomass")]
		public double IntakeCoefficientBiomass { get; set; }
		/// <summary>
		/// Enforce strict feeding limits
		/// </summary>
		[Description("Enforce strict feeding limits")]
		public bool StrictFeedingLimits { get; set; }
		/// <summary>
		/// Coefficient of juvenile milk intake
		/// </summary>
		[Description("Coefficient of juvenile milk intake")]
		public double MilkIntakeCoefficient { get; set; }
		/// <summary>
		/// Intercept of juvenile milk intake
		/// </summary>
		[Description("Intercept of juvenile milk intake")]
		public double MilkIntakeIntercept { get; set; }
		/// <summary>
		/// Maximum juvenile milk intake
		/// </summary>
		[Description("Maximum juvenile milk intake")]
		public double MilkIntakeMaximum { get; set; }
		/// <summary>
		/// Milk as proportion of LWT for fodder substitution
		/// </summary>
		[Description("Milk as proportion of LWT for fodder substitution")]
		public double MilkLWTFodderSubstitutionProportion { get; set; }
		/// <summary>
		/// Max juvenile (suckling) intake as proportion of LWT
		/// </summary>
		[Description("Max juvenile (suckling) intake as proportion of LWT")]
		public double MaxJuvenileIntake { get; set; }
		/// <summary>
		/// Proportional discount to intake due to milk intake
		/// </summary>
		[Description("Proportional discount to intake due to milk intake")]
		public double ProportionalDiscountDueToMilk { get; set; }
		/// <summary>
		/// Proportion of max body weight needed for survival
		/// </summary>
		[Description("Proportion of max body weight needed for survival")]
		public double ProportionOfMaxWeightToSurvive { get; set; }
		/// <summary>
		/// Lactating Potential intake modifier Coefficient A
		/// </summary>
		[Description("Lactating Potential intake modifier Coefficient A")]
		public double LactatingPotentialModifierConstantA { get; set; }
		/// <summary>
		/// Lactating Potential intake modifier Coefficient B
		/// </summary>
		[Description("Lactating Potential intake modifier Coefficient B")]
		public double LactatingPotentialModifierConstantB { get; set; }
		/// <summary>
		/// Lactating Potential intake modifier Coefficient C
		/// </summary>
		[Description("Lactating Potential intake modifier Coefficient C")]
		public double LactatingPotentialModifierConstantC { get; set; }
		/// <summary>
		/// Maximum size of individual relative to SRW
		/// </summary>
		[Description("Maximum size of individual relative to SRW")]
		public double MaximumSizeOfIndividual { get; set; }

		/// <summary>
		/// Mortality rate base
		/// </summary>
		[Description("Mortality rate base")]
		public double MortalityBase { get; set; }
		/// <summary>
		/// Mortality rate coefficient
		/// </summary>
		[Description("Mortality rate coefficient")]
		public double MortalityCoefficient { get; set; }
		/// <summary>
		/// Mortality rate intercept
		/// </summary>
		[Description("Mortality rate intercept")]
		public double MortalityIntercept { get; set; }
		/// <summary>
		/// Mortality rate exponent
		/// </summary>
		[Description("Mortality rate exponent")]
		public double MortalityExponent { get; set; }

		/// <summary>
		/// Juvenile mortality rate coefficient
		/// </summary>
		[Description("Juvenile mortality rate coefficient")]
		public double JuvenileMortalityCoefficient { get; set; }
		/// <summary>
		/// Juvenile mortality rate maximum
		/// </summary>
		[Description("Juvenile mortality rate maximum")]
		public double JuvenileMortalityMaximum { get; set; }
		/// <summary>
		/// Juvenile mortality rate exponent
		/// </summary>
		[Description("Juvenile mortality rate exponent")]
		public double JuvenileMortalityExponent { get; set; }

		/// <summary>
		/// Wool coefficient
		/// </summary>
		[Description("Wool coefficient")]
		public double WoolCoefficient { get; set; }

		/// <summary>
		/// Cashmere coefficient
		/// </summary>
		[Description("Cashmere coefficient")]
		public double CashmereCoefficient { get; set; }

		#endregion

		#region Breed activity

		/// <summary>
		/// Milk curve shape suckling
		/// </summary>
		[Description("Milk curve shape suckling")]
		public double MilkCurveSuckling { get; set; }
		/// <summary>
		/// Milk curve shape non suckling
		/// </summary>
		[Description("Milk curve shape non suckling")]
		public double MilkCurveNonSuckling { get; set; }
		/// <summary>
		/// Number of days for milking
		/// </summary>
		[Description("Number of days for milking")]
		public double MilkingDays { get; set; }
		/// <summary>
		/// Peak milk yield(kg/day)
		/// </summary>
		[Description("Peak milk yield (kg/day)")]
		public double MilkPeakYield { get; set; }
		/// <summary>
		/// Milk offset day
		/// </summary>
		[Description("Milk offset day")]
		public double MilkOffsetDay { get; set; }
		/// <summary>
		/// Milk peak day
		/// </summary>
		[Description("Milk peak day")]
		public double MilkPeakDay { get; set; }
		/// <summary>
		/// Inter-parturition interval intercept of PW (months)
		/// </summary>
		[Description("Inter-parturition interval intercept of PW (months)")]
		public double InterParturitionIntervalIntercept { get; set; }
		/// <summary>
		/// Inter-parturition interval coefficient of PW (months)
		/// </summary>
		[Description("Inter-parturition interval coefficient of PW (months)")]
		public double InterParturitionIntervalCoefficient { get; set; }
		/// <summary>
		/// Months between conception and parturition
		/// </summary>
		[Description("Months between conception and parturition")]
		public double GestationLength { get; set; }
		/// <summary>
		/// Minimum age for 1st mating (months)
		/// </summary>
		[Description("Minimum age for 1st mating (months)")]
		public double MinimumAge1stMating { get; set; }
		/// <summary>
		/// Minimum size for 1st mating, proportion of SRW
		/// </summary>
		[Description("Minimum size for 1st mating, proportion of SRW")]
		public double MinimumSize1stMating { get; set; }
		/// <summary>
		/// Rate at which twins are concieved
		/// </summary>
		[Description("Rate at which twins are concieved")]
		public double TwinRate { get; set; }
		/// <summary>
		/// Proportion of SRW for zero calving/lambing rate
		/// </summary>
		[Description("Proportion of SRW for zero Calving/lambing rate")]
		public double CriticalCowWeight { get; set; }
		/// <summary>
		/// Conception rate coefficient of breeder PW
		/// </summary>
		[Description("Conception rate coefficient of breeder PW (12 mnth, 24 mth, 2nd calf, 3rd+ calf)")]
		public double[] ConceptionRateCoefficent { get; set; }
		/// <summary>
		/// Conception rate intercept of breeder PW
		/// </summary>
		[Description("Conception rate intercept of breeder PW (12 mnth, 24 mth, 2nd calf, 3rd+ calf)")]
		public double[] ConceptionRateIntercept { get; set; }
		/// <summary>
		/// Conception rate assymtote
		/// </summary>
		[Description("Conception rate assymtote (12 mnth, 24 mth, 2nd calf, 3rd+ calf)")]
		public double[] ConceptionRateAsymptote { get; set; }
		/// <summary>
		/// Maximum number of matings per male per day
		/// </summary>
		[Description("Maximum number of matings per male per day")]
		public double MaximumMaleMatingsPerDay { get; set; }
		/// <summary>
		/// Prenatal mortality rate
		/// </summary>
		[Description("Mortality rate from conception to birth (proportion)")]
		public double PrenatalMortality { get; set; }
		/// <summary>
		/// Maximum conception rate from uncontrolled breeding 
		/// </summary>
		[Description("Maximum conception rate from uncontrolled breeding")]
		public double MaximumConceptionUncontrolledBreeding { get; set; }

		#endregion

		////Ruminant Coefficients
		////---------------------

		//      /// <summary>
		//      /// Proportion of SRW for zero Calving/lambing rate
		//      /// </summary>
		//      [Description("Proportion of SRW for zero Calving/lambing rate")]
		//      public double Critical_cow_wt { get; set; }
		//      /// <summary>
		//      /// Age growth rate coefficient
		//      /// </summary>
		//      [Description("Age growth rate coefficient")]
		//      public double grwth_coeff1 { get; set; }
		//      /// <summary>
		//      /// SRW growth scalar
		//      /// </summary>
		//      [Description("SRW growth scalar")]
		//      public double grwth_coeff2 { get; set; }
		//      /// <summary>
		//      /// Energy Lactation efficiency coeff.
		//      /// </summary>
		//      [Description("Energy Lactation efficiency coeff.")]
		//      public double kl_coeff { get; set; }
		//      /// <summary>
		//      /// Energy Lactation efficiency intercept
		//      /// </summary>
		//      [Description("Energy Lactation efficiency intercept")]
		//      public double kl_incpt { get; set; }
		//      /// <summary>
		//      /// Breed factor for maintenence energy
		//      /// </summary>
		//      [Description("Breed factor for maintenence energy")]
		//      public double kme { get; set; }
		//      /// <summary>
		//      /// Intake coefficient In relation to LW
		//      /// </summary>
		//      [Description("Intake coefficient In relation to LW")]
		//      public double intake_coeff { get; set; }
		//      /// <summary>
		//      /// Intake intercept In relation to SRW
		//      /// </summary>
		//      [Description("Intake intercept In relation to SRW")]
		//      public double intake_incpt { get; set; }
		//      /// <summary>
		//      /// Age at 1st parturition coefficient of PW (months)
		//      /// </summary>
		//      [Description("Age at 1st parturition coefficient of PW (months)")]
		//      public double AFP_coeff { get; set; }
		//      /// <summary>
		//      /// Age at 1st parturition intercept of PW (months)
		//      /// </summary>        
		//      [Description("Age at 1st parturition intercept of PW (months)")]
		//      public double AFP_incpt { get; set; }
		//      /// <summary>
		//      /// Inter-parturition interval coefficient of PW (months)
		//      /// </summary>
		//      [Description("Inter-parturition interval coefficient of PW (months)")]
		//      public double IPI_coeff { get; set; }
		//      /// <summary>
		//      /// Inter-parturition interval intercept of PW (months)
		//      /// </summary>
		//      [Description("Inter-parturition interval intercept of PW (months)")]
		//      public double IPI_incpt { get; set; }
		//      /// <summary>
		//      /// Birth rate, coefficient of breeder PW
		//      /// </summary>
		//      [Description("Birth rate, coefficient of breeder PW")]
		//      public double concep_rate_coeff { get; set; }
		//      /// <summary>
		//      /// Birth rate, intercept of breeder PW
		//      /// </summary>
		//      [Description("Birth rate, intercept of breeder PW")]
		//      public double concep_rate_incpt { get; set; }
		//      /// <summary>
		//      /// Birth rate assymtote
		//      /// </summary>
		//      [Description("Birth rate assymtote")]
		//      public double concep_rate_assym { get; set; }
		//      /// <summary>
		//      /// Maximum birth rate
		//      /// </summary>
		//      [Description("Maximum birth rate")]
		//      public double concep_rate_max { get; set; }
		//      /// <summary>
		//      /// Juvenile mortality rate, coefficient of breeder LWt/NormWt
		//      /// </summary>
		//      [Description("Juvenile mortality rate, coefficient of breeder LWt/NormWt")]
		//      public double juvenile_mort_coeff { get; set; }
		//      /// <summary>
		//      /// Juvenile mortality rate, exponent of breeder  LWt/NormWt
		//      /// </summary>
		//      [Description("Juvenile mortality rate, exponent of breeder  LWt/NormWt")]
		//      public double juvenile_mort_exp { get; set; }
		//      /// <summary>
		//      /// Juvenile mortality maximum rate
		//      /// </summary>
		//      [Description("Juvenile mortality maximum rate")]
		//      public double juvenile_mort_max { get; set; }
		//      /// <summary>
		//      /// Wool growth vs DM intake
		//      /// </summary>
		//      [Description("Wool growth vs DM intake")]
		//      public double wool_coeff { get; set; }
		//      /// <summary>
		//      /// Cashmere growth vs DM intake
		//      /// </summary>
		//      [Description("Cashmere growth vs DM intake")]
		//      public double cashmere_coeff { get; set; }
		//      /// <summary>
		//      /// Months between conception and parturition
		//      /// </summary>
		//      [Description("Months between conception and parturition")]
		//      public double Rum_gest_int { get; set; }
		//      /// <summary>
		//      /// Milk_offset_day
		//      /// </summary>
		//      [Description("Offset")]
		//      public double Milk_offset_day { get; set; }
		//      /// <summary>
		//      /// Day of peak yield
		//      /// </summary>
		//      [Description("Day of peak yield")]
		//      public double Milk_Peak_day { get; set; }
		//      /// <summary>
		//      /// Curve shape (suckling)
		//      /// </summary>
		//      [Description("Curve shape (suckling)")]
		//      public double Milk_Curve_suck { get; set; }
		//      /// <summary>
		//      /// Curve shape (not suckling)
		//      /// </summary>
		//      [Description("Curve shape (not suckling)")]
		//      public double Milk_Curve_nonsuck { get; set; }
		//      /// <summary>
		//      /// Protein requirement coeff (g/kg feed)
		//      /// </summary>
		//      [Description("Protein requirement coeff (g/kg feed)")]
		//      public double protein_coeff { get; set; }
		//      /// <summary>
		//      /// Conception rate, coefficient of 12m heifer
		//      /// </summary>
		//      [Description("Conception rate, coefficient of 12m heifer")]
		//      public double concep_rate_coeff12 { get; set; }
		//      /// <summary>
		//      /// Conception rate, intercept of 12m heifer
		//      /// </summary>
		//      [Description("Conception rate, intercept of 12m heifer")]
		//      public double concep_rate_incpt12 { get; set; }
		//      /// <summary>
		//      /// Conception rate assymtote 12m heifer
		//      /// </summary>
		//      [Description("Conception rate assymtote 12m heifer")]
		//      public double concep_rate_assym12 { get; set; }
		//      /// <summary>
		//      /// Conception rate, coefficient of 24m heifer
		//      /// </summary>
		//      [Description("Conception rate, coefficient of 24m heifer")]
		//      public double concep_rate_coeff24 { get; set; }
		//      /// <summary>
		//      /// Conception rate, intercept of 24m heifer
		//      /// </summary>
		//      [Description("Conception rate, intercept of 24m heifer")]
		//      public double concep_rate_incpt24 { get; set; }
		//      /// <summary>
		//      /// Conception rate assymtote 24m heifer
		//      /// </summary>
		//      [Description("Conception rate assymtote 24m heifer")]
		//      public double concep_rate_assym24 { get; set; }
		//      /// <summary>
		//      /// Conception rate, coefficient of 2nd calf heifer
		//      /// </summary>
		//      [Description("Conception rate, coefficient of 2nd calf heifer")]
		//      public double concep_rate_coeff2nd { get; set; }
		//      /// <summary>
		//      /// Conception rate, intercept of 2nd calf heifer
		//      /// </summary>
		//      [Description("Conception rate, intercept of 2nd calf heifer")]
		//      public double concep_rate_incpt2nd { get; set; }
		//      /// <summary>
		//      /// Conception rate assymtote 2nd calf heifer
		//      /// </summary>
		//      [Description("Conception rate assymtote 2nd calf heifer")]
		//      public double concep_rate_assym2nd { get; set; }
		//      /// <summary>
		//      /// % Mortality rate from conception to post birth
		//      /// </summary>
		//      [Description("% Mortality rate from conception to post birth")]
		//      public double prenatal_mort { get; set; }
		//      /// <summary>
		//      /// Maximum size of of animal relative to its SRW
		//      /// </summary>
		//      [Description("Maximum size of of animal relative to its SRW")]            //Not on any input form
		//      public double Anim_max { get; set; }
		//      /// <summary>
		//      /// Male SRW relative to female SRW
		//      /// </summary>
		//      [Description("Male SRW relative to female SRW")]                          //Not on any input form
		//      public double Male_SRW { get; set; }
		//      /// <summary>
		//      /// Minimum days from birth to re-conception
		//      /// </summary>
		//      [Description("Minimum days from birth to re-conception")]                 //Not on any input form
		//      public double Min_birth2concep { get; set; }
		//      /// <summary>
		//      /// Coeffiicent to adjust intake for lactation
		//      /// </summary>
		//      [Description("Coeffiicent to adjust intake for lactation")]               //Not on any input form
		//      public double Intake_coeff_lact { get; set; }
		//      /// <summary>
		//      /// Day of peak intake due to lactation
		//      /// </summary>
		//      [Description("Day of peak intake due to lactation")]                      //Not on any input form
		//      public double Intake_peak_lact { get; set; }
		//      /// <summary>
		//      /// Exponent to adjust intake for lactation
		//      /// </summary>
		//      [Description("Exponent to adjust intake for lactation")]                  //Not on any input form
		//      public double Intake_exp_lact { get; set; }
		//      /// <summary>
		//      /// Exponent to adjust intake for herbage biomass
		//      /// </summary>
		//      [Description("Exponent to adjust intake for herbage biomass")]            //Not on any input form
		//      public double Intake_coeff_biomass { get; set; }
		//      /// <summary>
		//      /// Minimum restriction on intake due to protein deficit
		//      /// </summary>
		//      [Description("Minimum restriction on intake due to protein deficit")]     //Not on any input form
		//      public double protein_intake_min { get; set; }
		//      /// <summary>
		//      /// Maximum age for energy maintenance calculation (yrs)
		//      /// </summary>
		//      [Description("Maximum age for energy maintenance calculation (yrs)")]     //Not on any input form
		//      public double Maint_max_age { get; set; }
		//      /// <summary>
		//      /// Coefficent for maintenance energy on age
		//      /// </summary>
		//      [Description("Coefficent for maintenance energy on age")]                 //Not on any input form
		//      public double Maint_energy_coeff { get; set; }
		//      /// <summary>
		//      /// Exponent for maintenance energy on age
		//      /// </summary>
		//      [Description("Exponent for maintenance energy on age")]                   //Not on any input form
		//      public double Maint_energy_exp { get; set; }
		//      /// <summary>
		//      /// Coefficent for energy for intake
		//      /// </summary>
		//      [Description("Coefficent for energy for intake")]                         //Not on any input form
		//      public double Maint_energy_int { get; set; }
		//      /// <summary>
		//      /// Parameter for energy for growth
		//      /// </summary>
		//      [Description("Parameter for energy for growth")]                          //Not on any input form
		//      public double Growth_energy_incpt1 { get; set; }
		//      /// <summary>
		//      /// Parameter for energy for growth 2
		//      /// </summary>
		//      [Description("Parameter for energy for growth 2")]                        //Not on any input form
		//      public double Growth_energy_incpt2 { get; set; }
		//      /// <summary>
		//      /// Breed adjust for growth efficiency
		//      /// </summary>
		//      [Description("Breed adjust for growth efficiency")]                       //Not on any input form
		//      public double Growth_effic { get; set; }
		//      /// <summary>
		//      /// Milk as proportion of LWT for fodder substitution
		//      /// </summary>
		//      [Description("Milk as proportion of LWT for fodder substitution")]        //Not on any input form
		//      public double Calf_sub_prop { get; set; }
		//      /// <summary>
		//      /// Maximum calf intake as proportion of LWT
		//      /// </summary>
		//      [Description("Maximum calf intake as proportion of LWT")]                 //Not on any input form
		//      public double Calf_intake_prop { get; set; }
		//      /// <summary>
		//      /// Proportional discount to intake due to milk intake
		//      /// </summary>
		//      [Description("Proportional discount to intake due to milk intake")]       //Not on any input form
		//      public double Calf_milk_sub { get; set; }
		//      /// <summary>
		//      /// Coefficent of calf milk intake (/kg lwt)
		//      /// </summary>
		//      [Description("Coefficent of calf milk intake (/kg lwt)")]                 //Not on any input form
		//      public double Milk_intake_coeff { get; set; }
		//      /// <summary>
		//      /// Intercept of calf milk intake
		//      /// </summary>
		//      [Description("Intercept of calf milk intake")]                            //Not on any input form
		//      public double Milk_intake_incpt { get; set; }
		//      /// <summary>
		//      /// Maximum milk intake (litres)
		//      /// </summary>
		//      [Description("Maximum milk intake (litres)")]                             //Not on any input form
		//      public double Milk_intake_max { get; set; }
		//      /// <summary>
		//      /// Animal mortality rate, coefficient of LWt/NormWt
		//      /// </summary>
		//      [Description("Animal mortality rate, coefficient of LWt/NormWt")]         //Not on any input form
		//      public double Anim_mort_coeff { get; set; }
		//      /// <summary>
		//      /// Animal mortality rate, intpercept of LWt/NormWt
		//      /// </summary>
		//      [Description("Animal mortality rate, intpercept of LWt/NormWt")]          //Not on any input form
		//      public double Anim_mort_incpt { get; set; }
		//      /// <summary>
		//      /// Animal mortality rate, exponent of LWt/NormWt
		//      /// </summary>
		//      [Description("Animal mortality rate, exponent of LWt/NormWt")]            //Not on any input form
		//      public double Anim_mort_exp { get; set; }





		//      //Ruminant Specifications
		//      //-----------------------

		//      /// <summary>
		//      /// Wool price/kg
		//      /// </summary>
		//      [Description("Wool price/kg")]
		//      public double wool_price { get; set; }
		//      /// <summary>
		//      /// Cashmere price/kg
		//      /// </summary>
		//      [Description("Cashmere price/kg")]
		//      public double cashmere_price { get; set; }
		//      /// <summary>
		//      /// Maximum no. breeders can keep
		//      /// </summary>
		//      [Description("Maximum no. breeders can keep")]
		//      public double Max_breeders { get; set; }
		//      /// <summary>
		//      /// Maximum breeder age (months) for culling
		//      /// </summary>
		//      [Description("Maximum breeder age (months) for culling")]
		//      public double Max_breeder_age { get; set; }
		//      /// <summary>
		//      /// Culling rate for dry breeders (%)
		//      /// </summary>
		//      [Description("Culling rate for dry breeders (%)")]
		//      public double Dry_breeder_cull_rate { get; set; }
		//      /// <summary>
		//      /// Selling age (months)
		//      /// </summary>
		//      [Description("Selling age (months)")]
		//      public double Anim_sell_age { get; set; }
		//      /// <summary>
		//      /// Selling weight (kg)
		//      /// </summary>
		//      [Description("Selling weight (kg)")]
		//      public double Anim_sell_wt { get; set; }
		//      /// <summary>
		//      /// Weaning age (months)
		//      /// </summary>
		//      [Description("Weaning age (months)")]
		//      public double Weaning_age { get; set; }
		//      /// <summary>
		//      /// Weaning weight (kg)
		//      /// </summary>
		//      [Description("Weaning weight (kg)")]
		//      public double Weaning_weight { get; set; }
		//      /// <summary>
		//      /// Labour-days/animal/month for feeding (excl. cut&amp;carry)
		//      /// </summary>
		//      [Description("Labour-days/animal/month for feeding (excl. cut&carry)")]
		//      public double Feeding { get; set; }
		//      /// <summary>
		//      /// Labour-hours/breeder/day for milking
		//      /// </summary>
		//      [Description("Labour-hours/breeder/day for milking")]
		//      public double Milking { get; set; }
		//      /// <summary>
		//      /// Labour-days /herd/month for herding
		//      /// </summary>
		//      [Description("Labour-days /herd/month for herding")]
		//      public double Mustering { get; set; }
		//      /// <summary>
		//      /// Labour-days/herd/month for transporting
		//      /// </summary>
		//      [Description("Labour-days/herd/month for transporting")]
		//      public double Transporting { get; set; }
		//      /// <summary>
		//      /// Other Labour-days/herd/month (e.g. shearing)
		//      /// </summary>
		//      [Description("Other Labour-days/herd/month (e.g. shearing)")]
		//      public double Other_Labour { get; set; }
		//      /// <summary>
		//      /// Costs for vets/head/year
		//      /// </summary>
		//      [Description("Costs for vets/head/year")]
		//      public double Vet_costs { get; set; }
		//      /// <summary>
		//      /// Cost for vaccines &amp; drenches per animal/year
		//      /// </summary>
		//      [Description("Cost for vaccines & drenches per animal/year")]
		//      public double Vaccines { get; set; }
		//      /// <summary>
		//      /// Cost for dips &amp; sprays per animal/year
		//      /// </summary>
		//      [Description("Cost for dips &sprays per animal/year")]
		//      public double Dips { get; set; }
		//      /// <summary>
		//      /// Feed trough available? (Y/N)
		//      /// </summary>
		//      [Description("Feed trough available? (Y/N)")]
		//      public bool FeedTrough { get; set; }
		//      /// <summary>
		//      /// Milk kept for home consumption (L/day)
		//      /// </summary>
		//      [Description("Milk kept for home consumption (L/day)")]
		//      public double Home_milk { get; set; }
		//      /// <summary>
		//      /// Value of milk per litre
		//      /// </summary>
		//      [Description("Value of milk per litre")]
		//      public double Milk_value { get; set; }
		//      /// <summary>
		//      /// Milking animal (Y/N)
		//      /// </summary>
		//      [Description("Milking animal (Y/N)")]
		//      public bool MilkYN { get; set; }
		//      /// <summary>
		//      /// Costs (per cow per month)
		//      /// </summary>
		//      [Description("Costs (per cow per month)")]
		//      public double Milk_Costs { get; set; }
		//      /// <summary>
		//      /// Basic mortality rate (%)
		//      /// </summary>
		//      [Description("Basic mortality rate (%)")]
		//      public double Mortality_base { get; set; }
		//      /// <summary>
		//      /// Annual home consumption
		//      /// </summary>
		//      [Description("Annual home consumption")]
		//      public double Rum_Hcon_No { get; set; }
		//      /// <summary>
		//      /// Animal category used for home consumption
		//      /// </summary>
		//      [Description("Animal category used for home consumption")]
		//      public double Rum_Hcon_Cat { get; set; }
		//      /// <summary>
		//      /// Maximum age for breeding sire (months) for culling
		//      /// </summary>
		//      [Description("Maximum age for breeding sire (months) for culling")]
		//      public double Max_Bull_age { get; set; }
		//      /// <summary>
		//      /// Cost of replacing breeding sire
		//      /// </summary>
		//      [Description("Cost of replacing breeding sire")]
		//      public double Bull_replace_cost { get; set; }
		//      /// <summary>
		//      /// Sets inter birth interval to 12 months
		//      /// </summary>
		//      [Description("Sets inter birth interval to 12 months")]
		//      public bool Seasonal_mating { get; set; }
		//      /// <summary>
		//      /// Cut&amp;carry, Grazing or both
		//      /// </summary>
		//      [Description("Cut&carry, Grazing or both")]
		//      public double Feeding_system { get; set; }
		//      /// <summary>
		//      /// Percentage of breeders that have twins
		//      /// </summary>
		//      [Description("Percentage of breeders that have twins")]
		//      public double Twin_rate { get; set; }
		//      /// <summary>
		//      /// Months between mating when seasonal mating
		//      /// </summary>
		//      [Description("Months between mating when seasonal mating")]
		//      public double Seas_mating_int { get; set; }
		//      /// <summary>
		//      /// % decrease if not seasonal mating
		//      /// </summary>
		//      [Description("% decrease if not seasonal mating")]
		//      public double Seas_mat_ben { get; set; }
		//      /// <summary>
		//      /// Minimum age for 1st mating (months)
		//      /// </summary>
		//      [Description("Minimum age for 1st mating (months)")]
		//      public double Joining_age { get; set; }
		//      /// <summary>
		//      /// Minimum size for 1st mating (% of mature wt, SRW)
		//      /// </summary>
		//      [Description("Minimum size for 1st mating (% of mature wt, SRW)")]
		//      public double Joining_size { get; set; }
		//      /// <summary>
		//      /// Peak milk yield (kg/day)
		//      /// </summary>
		//      [Description("Peak milk yield (kg/day)")]
		//      public double Milk_max { get; set; }
		//      /// <summary>
		//      /// Number of months for milking
		//      /// </summary>
		//      [Description("Number of months for milking")]
		//      public double Milk_end { get; set; }
		//      /// <summary>
		//      /// Hours grazing per day if mixed grazing and Cut&amp;carry
		//      /// </summary>
		//      [Description("Hours grazing per day if mixed grazing and Cut&carry")]
		//      public double Graze_hrs { get; set; }
		//      /// <summary>
		//      /// Distance to market (kl)
		//      /// </summary>
		//      [Description("Distance to market (kl)")]
		//      public double market_dist { get; set; }
		//      /// <summary>
		//      /// Cost per kl/truck
		//      /// </summary>
		//      [Description("Cost per kl/truck")]
		//      public double kl_cost { get; set; }
		//      /// <summary>
		//      /// Number of 450kg animals per truck load
		//      /// </summary>
		//      [Description("Number of 450kg animals per truck load")]
		//      public double No_load { get; set; }
		//      /// <summary>
		//      /// Yard fees (/hd) when sold
		//      /// </summary>
		//      [Description("Yard fees (/hd) when sold")]
		//      public double Yard_fees { get; set; }
		//      /// <summary>
		//      /// MLA R&amp;D fees (/hd) when sold
		//      /// </summary>
		//      [Description("MLA R&D fees (/hd) when sold")]
		//      public double MLA_fees { get; set; }
		//      /// <summary>
		//      /// Sales commission to agent (%)
		//      /// </summary>
		//      [Description("Sales commission to agent (%)")]
		//      public double Commission { get; set; }
		//      /// <summary>
		//      /// Selling of young females same as males
		//      /// </summary>
		//      [Description("Selling of young females same as males")]
		//      public bool Female_selling { get; set; }
		//      /// <summary>
		//      /// Adjust breeder intake for day of lactation
		//      /// </summary>
		//      [Description("Adjust breeder intake for day of lactation")]
		//      public bool Intake_lactating { get; set; }
		//      /// <summary>
		//      /// Minimum conception rate before doing any culling
		//      /// </summary>
		//      [Description("Minimum conception rate before doing any culling")]
		//      public double Min_concep_cull { get; set; }
		//      /// <summary>
		//      /// Maximum green in diet
		//      /// </summary>
		//      [Description("Maximum green in diet")]
		//      public double Green_diet_max { get; set; }
		//      /// <summary>
		//      /// Shape of curve for diet vs pasture
		//      /// </summary>
		//      [Description("Shape of curve for diet vs pasture")]
		//      public double Green_diet_coeff { get; set; }
		//      /// <summary>
		//      /// % green in pasture at zero in diet
		//      /// </summary>
		//      [Description("% green in pasture at zero in diet")]
		//      public double Green_diet_zero { get; set; }
		//      /// <summary>
		//      /// Maximum conception rate for uncontrolled mating
		//      /// </summary>
		//      [Description("Maximum conception rate for uncontrolled mating")]
		//      public double Max_concep_uncontrolled { get; set; }





		/// <summary>
		/// Create the individual ruminant animals for this Ruminant Type (Breed)
		/// </summary>
		/// <returns></returns>
		public List<Ruminant> CreateIndividuals()
        {
            List<Ruminant> Individuals = new List<Ruminant>();

            List<IModel> childNodes = Apsim.Children(this, typeof(IModel));

            foreach (IModel childModel in childNodes)
            {
                //cast the generic IModel to a specfic model.
                RuminantTypeCohort cohort = childModel as RuminantTypeCohort;
                Individuals.AddRange(cohort.CreateIndividuals());
            }

            return Individuals;
        }



    }




}