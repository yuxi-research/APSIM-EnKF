# -*- coding: utf-8 -*-
"""
Created on Wed May 17 22:20:56 2018

@author: Yuxi Zhang

This script plot results of EnKF data assimilation using data from SQLite database.
Output including.

The curve "openloop" is replaced by the ensemble mean of a perturbed open-loop run.

"""


def plot(enkf, ol, fig_settings, phenology=None, is_simple=True, is_ol=False):
    import settings.apsim_dict as dict
    import matplotlib.pyplot as plt
    import numpy as np
    plt.rcParams['pdf.fonttype'] = 42

    set_type, set_xtick, set_ytick, set_ylabel, set_legend, exp_eps = fig_settings
    if set_type == 'default':
        figtype = dict.figtype[enkf.table_name]
    else:
        figtype = set_type

    yscale = dict.yscale[enkf.table_name]
    yunit = dict.unit_AEE[enkf.table_name]
    ylabel = dict.ylabel[enkf.table_name]

    if set_xtick:
        figsize = dict.size_label[figtype]
        l, r, w, b, t, h = dict.adjust_label[figtype]
    else:
        figsize = dict.size_no_label[figtype]
        l, r, w, b, t, h = dict.adjust_no_label[figtype]

    index = [0]
    for i in range(0, len(enkf.obs)):
        if enkf.prior_mean[i] != enkf.posterior_mean[i]:
            index.append(i)
    index.append(len(enkf.obs) + 1)
    obs_number = len(index) - 2

    # Plot zone
    plt.figure(figsize=figsize)
    plt.subplots_adjust(left=l, right=r, wspace=w, bottom=b, top=t, hspace=h)

    ax = plt.gca()
    ax.minorticks_on()
    ax.tick_params(which='both', direction='in')

    if set_xtick:
        ax.set_xlabel("Day of year")
        #ax.set_xlabel("")
    else:
        ax.set_xticklabels([""])

    if set_ylabel:
        ax.set_ylabel(ylabel + ' (' + yunit + ')')
        #ax.set_ylabel(enkf.table_name)

    if not set_ytick:
        ax.set_yticklabels([""])

    if figtype == "grain":
        ax.set_xlim([230, 300])
    elif figtype == "wheat":
        ax.set_xlim([122, 300])
    else:
        ax.set_xlim([0, 300])

    ax.set_ylim(yscale)

    # plot phenology
    if phenology:
        truth_phenology = phenology[0]
        ax2 = ax.twiny()
        # Note: set xticks before xlim.
        ax2.set_xticks(truth_phenology)
        ax2.set_xlim(ax.get_xlim())
        ax2.set_ylim(ax.get_ylim())
        ax2.set_xticklabels([])
        ax2.tick_params(which='major', direction='in')
        ax2.grid(which='major', axis='x', color='grey', linewidth=0.2, linestyle=':')
        if set_xtick:
            ax2.set_xlabel("Phenological stage")
            #ax2.set_xlabel("")
            ax2.set_xticklabels(["", "", "", "4", "5", "6", "7", "", "9", ""])

        # ["", "", "", "EndOfJuvenile", "FloralInitiation", "Flowering", "StartGrainFill", "", "Maturity", ""]

    if not is_simple:
        # Plot Posterior Ensembles
        temp_matrix = enkf.prior.copy()
        for i in range(0, len(index) - 1):
            for j in range(0, len(temp_matrix)):
                temp_matrix[j, index[i]:index[i] + 1] = enkf.posterior[j, index[i]:index[i] + 1]
                plt.plot(np.array(enkf.doy[index[i]:index[i + 1] + 1]), temp_matrix[j, index[i]:index[i + 1] + 1].A1,
                         color='silver', ls='-', linewidth=0.3)
        plt.plot([0, 1], [-99, -99], color='silver', ls='-', linewidth=0.3, label="State ensembles")  # Legend

        if not is_ol:
            # Plot Obs Ensembles
            for i in range(0, enkf.ensemble_size):
                plt.plot(enkf.doy, enkf.perturbed_obs[i, :].A1, color='gray', marker='D', ls='', markersize=0.3)

            plt.plot([0, 1], [-99, -99], color='gray', marker='D', ls='', markersize=0.3, label="Obs ensembles")  # Legend

    # Plot Obs, Open-Loop, Truth and Ensemble Mean

    plt.plot(enkf.doy, enkf.truth, 'k-', linewidth=1, label="Truth")  # line width was 0.5 for report
    plt.plot(enkf.doy, ol.posterior_mean, 'g-', linewidth=1, label="Open-loop")
    plt.plot(enkf.doy, enkf.obs, color='black', marker='D', ls='', markersize=1.5, label="Obs")

    if not is_ol:
        temp_mean = enkf.prior_mean.copy()
        for i in range(0, len(index) - 1):
            temp_mean[index[i]:index[i] + 1] = enkf.posterior_mean[index[i]:index[i] + 1]
            plt.plot(np.asarray(enkf.doy[index[i]:index[i + 1] + 1]), temp_mean[index[i]:index[i + 1] + 1], color='b',
                     ls='-', linewidth=1)
        plt.plot([0, 1], [-99, -99], color='b', ls='-', linewidth=1, label="Data assimilation")  # Legend

    if set_legend:
        plt.legend(loc='upper left', fontsize='x-small')

    fig_ext=''
    if not set_xtick:
        fig_ext = '_no_xlabel'
    elif not set_ytick:
        fig_ext = '_no_ylabel'

    plt.savefig(enkf.table_name + fig_ext+ ".png", dpi=dict.dpi)

    if exp_eps:
        plt.savefig(enkf.table_name + fig_ext + ".pdf")

    plt.close()
