# -*- coding: utf-8 -*-
"""
Created on Wed May 17 22:20:56 2018

@author: Yuxi Zhang

This script plot results of EnKF data assimilation using data from SQLite database.
Output including.

The curve "openloop" is replaced by the ensemble mean of a perturbed open-loop run.

"""


def plot(layer_depth, doy_step, x_diff, truth, openloop, prior, posterior, property, ensembles=[], is_ol=False):
    import matplotlib.pyplot as plt
    import matplotlib.axes as ax
    property = property.replace('_', '')
    (row, col) = posterior.shape
    step = doy_step - x_diff
    depth = []
    for lyr in range(0, len(layer_depth)):
        depth.append(sum(layer_depth[0:lyr]) + 0.5*layer_depth[lyr])

    plt.figure(figsize=(3, 7))
    if True:
        for ensemble in range(0, len(ensembles)):
            plt.plot(ensembles[ensemble][:, step].A1, depth, color='0.7', ls='-', linewidth=0.5)

    if property != 'SoilNO3':
        plt.plot(truth[:, step].A1, depth, color='k', ls='-', marker='o', markersize=2, linewidth=1, label="Truth")
        plt.plot(openloop[:, step].A1, depth, color='g', ls='-', marker='o', markersize=2, linewidth=1, label="Open-loop")
        # plt.plot(prior[:, step].A1, depth, color='cyan', ls='-', marker='o', markersize=2, linewidth=1, label="Prior")
        if not is_ol:
            plt.plot(posterior[:, step].A1, depth, color='b', ls='--', marker='o', markersize=2, linewidth=1, label="Posterior")

    plt.subplots_adjust(left=0.2, bottom=0.05, right=0.9, top=0.9, wspace=0.2, hspace=0.5)
    ax = plt.gca()
    ax.minorticks_on()
    ax.tick_params(which='both', direction='in')

    if property == 'SW':
        plt.xlabel("Soil water ($mm/mm$)")
        ax.set_xlim(0.08, 0.5)
        plt.text(0.1, 177, 'DoY=' + str(doy_step))
        plt.legend(loc='lower right')

    elif property == 'SoilNH4':
        plt.xlabel("Soil NH4 ($kg/ha$)")

        ax.set_xlim(-0.02, 0.7)
        ax.set_xticks([0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7])
        plt.text(0.01, 177, 'DoY=' + str(doy_step))
        plt.legend(loc='lower right')

    elif property == 'SoilNO3':
        ax.semilogx(truth[:, step].A1, depth, color='k', ls='-', marker='o', markersize=2, linewidth=1, label="Truth")
        ax.semilogx(openloop[:, step].A1, depth, color='g', ls='-', marker='o', markersize=2, linewidth=1, label="Open-loop")
        # ax.semilogx(prior[:, step].A1, depth, color='cyan', ls='-', marker='o', markersize=2, linewidth=1, label="Prior")
        if not is_ol:
            ax.semilogx(posterior[:, step].A1, depth, color='b', ls='--', marker='o', markersize=2, linewidth=1, label="Posterior")

        plt.xlabel("Soil NO3 ($kg/ha$)")
        ax.set_xlim(0.05, 200)
        plt.text(0.07, 177, 'DoY ' + str(doy_step))
        plt.legend(loc='lower right')

    ax.xaxis.set_label_position('top')

    plt.ylabel("Depth (cm)")
    ax.invert_yaxis()
    ax.set_ylim(180, 0)
    import numpy as np
    ax.set_yticks([0, 15, 30, 60, 90, 120, 150, 180], minor=False)
    ax.set_yticks(np.linspace(0, 180, num=37), minor=True)

    ax.grid(which='major', axis='y')
    ax.xaxis.set_tick_params(labeltop='on', labelbottom='off')
    #plt.savefig("Soil/" + property + str(doy_step) + ".png", dpi=100)
    import os
    if not os.path.exists(property):
        os.makedirs(property)
    plt.savefig(property + "/" + str(doy_step) + ".png", dpi=100)
    plt.close()

