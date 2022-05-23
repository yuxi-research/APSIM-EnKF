# -*- coding: utf-8 -*-
"""
Created on Wed May 17 22:20:56 2018

@author: Yuxi Zhang

This script plot results of EnKF data assimilation using data from SQLite database.
Output including.

The curve "openloop" is replaced by the ensemble mean of a perturbed open-loop run.

"""


def plot_hist(db, debug=False):
    # This histogram calculation is wrong.
    import matplotlib.pyplot as plt
    import settings.apsim_dict as dict
    import numpy as np
    # Rank ensemble values.
    row, col = np.shape(db.prior)
    for c in [251]:
        bin_max = np.max(db.prior[:, c])
        bin_min = np.min(db.prior[:, c])
        bins = np.linspace(bin_min, bin_max, db.ensemble_size+1)
        plt.figure(figsize=(3, 3))
        plt.subplots_adjust(left=0.2, bottom=0.2, right=0.9, top=0.9, wspace=0.2, hspace=0.5)
        plt.xlim(bin_min, bin_max)
        # plt.ylim(0, 1)
        plt.hist(db.prior[:, c], bins=db.ensemble_size, normed=1)
        plt.savefig(db.table_name + "_Hist_Day_" + str(c-1) + ".png", dpi=dict.dpi)
        plt.savefig(db.table_name + "_Hist_Day_" + str(c-1) + ".eps")
        plt.close()

def calc_stat(db,  phenology=None, do_plot=True, units=""):
    import function.basic as basic
    import numpy as np
    start = 122
    end = 300
    del_index = np.nonzero(np.array(db.obs) < 0)
    del_index = del_index[0]
    obs_index = np.nonzero(np.array(db.obs) >= 0)
    obs_index = obs_index[0]

    msd = basic.msd(db.posterior, db.obs)
    ensp = basic.ensp(db.posterior)
    ensk = basic.ensk(db.posterior, db.obs)
    rmsd = np.power(msd, 0.5)
    rensp = np.power(ensp, 0.5)
    rensk = np.power(ensk, 0.5)
    skew = basic.skew(db.posterior)
    kurt = basic.kurt(db.posterior)

    ratio = rmsd.copy()
    for i in range(0, len(del_index)):
        rmsd[0, del_index[i]] = None
        rensk[0, del_index[i]] = None
        ratio[0, del_index[i]] = 0
    sum_msd = 0
    sum_ensk = 0
    sum_ensp = 0
    sum_rmsd = 0
    sum_rensk = 0
    sum_rensp = 0
    mean_msd = 0
    mean_ensk = 0
    mean_ensp = 0
    mean_rmsd = 0
    mean_rensk = 0
    mean_rensp = 0
    for i in range(0, len(obs_index)):
        if rmsd[0, obs_index[i]] == 0:
            ratio[0, obs_index[i]] = 0
        else:
            ratio[0, obs_index[i]] = rensp[0, obs_index[i]] / rmsd[0, obs_index[i]]

        sum_msd = sum_msd + msd[0, obs_index[i]]
        sum_ensk = sum_ensk + ensk[0, obs_index[i]]
        sum_ensp = sum_ensp + ensp[0, obs_index[i]]
        sum_rmsd = sum_rmsd + rmsd[0, obs_index[i]]
        sum_rensk = sum_rensk + rensk[0, obs_index[i]]
        sum_rensp = sum_rensp + rensp[0, obs_index[i]]
    if len(obs_index > 0):
        mean_msd = sum_msd / len(obs_index)
        mean_ensk = sum_ensk / len(obs_index)
        mean_ensp = sum_ensp / len(obs_index)
        mean_rmsd = sum_rmsd / len(obs_index)
        mean_rensk = sum_rensk / len(obs_index)
        mean_rensp = sum_rensp / len(obs_index)
    out_ratio1 = None
    if sum_ensp != 0:
        out_ratio1 = mean_ensk / mean_ensp
    out_ratio2 = None
    if sum_rmsd != 0:
        out_ratio2 = sum_rensk / sum_rmsd

    posterior_mean=np.matrix(db.posterior_mean)
    truth = np.matrix(db.truth)
    truth_max = np.max(truth[:, start:end])
    out_max = np.max(posterior_mean[:, start:end])
    out_max_diff = out_max - np.max(truth[:, start:end])
    out_ns = basic.ns(posterior_mean[:, start:end], truth[:, start:end])
    out_rmsd = basic.rmsd(posterior_mean[:, start:end], truth[:, start:end])
    out_mrmsd = basic.mrmsd(db.posterior[:, start:end], truth[:, start:end])
    out_bias = basic.bias(db.posterior[:, start:end], truth[:, start:end])

    my_file = open("Statistics.csv", 'a')  # a= append, w=write, r=read
    sep = ","  # Separator
    my_file.write(db.table_name + sep +

                  str(mean_msd) + sep +
                  str(mean_ensk) + sep +
                  str(mean_ensp) + sep +
                  str(mean_rmsd) + sep +
                  str(mean_rensk) + sep +
                  str(mean_rensp) + sep +

                  str(truth_max) + sep +
                  str(out_max) + sep +
                  str(out_max_diff) + sep +
                  str(out_ns) + sep +
                  str(out_rmsd) + sep +
                  str(out_mrmsd) + sep +
                  str(out_bias) + sep +
                  str(out_ratio1) + sep +
                  str(out_ratio2) + "\n")
    my_file.close()

    if do_plot:
        import matplotlib.pyplot as plt
        import settings.apsim_dict as dict
        import matplotlib.axes as ax
        #plt.rc('font', )
        #plt.rc('font', size=16)
        #plt.rc('ytick', labelsize=14)
        #plt.rc('xtick', labelsize=14)

        fig = plt.figure(figsize=(7, 2))
        plt.subplots_adjust(left=0.1, bottom=0.22, right=0.9, top=0.8, wspace=0.2, hspace=0.5)

        ax1 = fig.add_subplot(111)
        ax1.minorticks_on()

        if (db.table_name.find("SW") == -1) & (db.table_name.find("Soil") == -1):
            ax1.set_xlim([122, 300])
        else:
            ax1.set_xlim([0, 300])

        ax1.set_xlabel('Day of Year')

        ax1.tick_params(which='both', axis='both', direction='in')
        ax1.set_ylim([-1, 1])
        ax1.set_yticks([0, 0.5, 1])
        ax1.set_ylabel("rensp:rmse")
        ax1.grid(which='major', axis='y', color='grey', linewidth=0.2, linestyle=':')

        ax2 = ax1.twinx()  # this is the important function
        ax2.set_xlim(ax1.get_xlim())

        if db.table_name == "LAI":
            ax2.set_ylim([0, 2.5])
        elif db.table_name == "SW1":
            ax2.set_ylim([0, 0.2])
        ax2.set_ylabel('rmse or rensp')

        ax1.plot(db.doy, ratio.A1, color='grey', linestyle='--', linewidth=1, label="rensp:rmse")
        ax2.plot(db.doy, rensp.A1, "k-", linewidth=1.5, label="rensp")
        ax2.plot(db.doy, rmsd.A1, "k.", marker='D', markersize=3, label="rmse")

        ax1.legend(loc='upper left')
        ax2.legend(loc='lower left')

        ax2.tick_params(which='both', axis='both', direction='in')

        if phenology:
            truth_phenology = phenology[0]
            ax3 = ax1.twiny()
            ax3.set_xlim(ax1.get_xlim())
            ax3.set_ylim(ax1.get_ylim())
            ax3.set_xticks(truth_phenology)
            ax3.set_xticklabels(["", "", "", "4", "5", "6", "7", "", "9", ""])
            ax3.set_xlabel("Phenology stage")
            ax3.tick_params(which='major', direction='in')
            ax3.grid(which='major', axis='x', color='grey', linewidth=0.2, linestyle=':')

        plt.savefig(db.table_name + "_ensp.png", dpi=dict.dpi)
        plt.savefig(db.table_name + "_ensp.eps")

        if (db.table_name == "GrainWt") or (db.table_name == "GrainN"):
            plt.figure(figsize=(3.5, 2))
            plt.subplots_adjust(left=0.2, bottom=0.22, right=0.9, top=0.8, wspace=0.2, hspace=0.5)
            plt.plot(db.doy, skew.A1, "k-", label="skew")
            plt.plot(db.doy, kurt.A1, "r--", label="kurt")
            ax1 = plt.gca()
            ax1.set_ylim(-5, 10)
            ax1.set_xlim(240, 300)
            plt.legend(loc="upper center")
        else:
            plt.figure(figsize=(7, 2))
            plt.subplots_adjust(left=0.1, bottom=0.22, right=0.9, top=0.8, wspace=0.2, hspace=0.5)
            plt.plot(db.doy, skew.A1, "k-", label="skew")
            plt.plot(db.doy, kurt.A1, "r--", label="kurt")
            ax1 = plt.gca()
            if db.table_name == "LAI":
                plt.ylim(-2, 2)
            plt.legend(loc="upper left")

            if (db.table_name.find("SW") == -1) & (db.table_name.find("Soil") == -1):
                ax1.set_xlim(122, 300)
            else:
                ax1.set_xlim(0, 300)

        ax1.set_xlabel("Day of year")
        ax1.minorticks_on()
        ax1.tick_params(which='both', direction='in')

        ax1.set_ylabel("skew or kurt")
        ax3 = ax1.twinx()
        ax3.set_xlim(ax1.get_xlim())
        ax3.set_ylim(ax1.get_ylim())
        ax3.set_yticks([0])
        ax3.set_yticklabels([])
        ax3.tick_params(which='both', direction='in')
        ax3.grid(which='major', axis='y', color='grey', linewidth=0.2, linestyle=':')

        if phenology:
            truth_phenology = phenology[0]
            ax2 = ax1.twiny()
            ax2.set_xlim(ax1.get_xlim())
            ax2.set_ylim(ax1.get_ylim())
            ax2.set_xticks(truth_phenology)
            ax2.set_xticklabels(["", "", "", "4", "5", "6", "7", "", "9", ""])
            ax2.set_xlabel("Phenology stage")
            ax2.tick_params(which='major', direction='in')
            ax2.grid(which='major', axis='x', color='grey', linewidth=0.2, linestyle=':')

        plt.savefig(db.table_name + "_skew.png", dpi=dict.dpi)
        plt.savefig(db.table_name + "_skew.eps")
        plt.close()
