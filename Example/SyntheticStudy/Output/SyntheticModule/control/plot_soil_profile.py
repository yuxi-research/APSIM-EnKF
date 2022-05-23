# -*- coding: utf-8 -*-
"""
Created on Wed Mar 28 14:20:56 2018

@author: Yuxi Zhang

This script plot soil water and N profiles.
"""


def settings():
    import sys
    script = sys.argv[0]
    script = script.replace("/", "\\")
    path = script.split('\\control\\')
    sys.path.append(path[0])
    ol_file = script.split('\\YuxiModule\\')[0] + '\OpenLoop\States'
    import os
    if not os.path.exists(ol_file+'.sqlite'):
        print('The file [' + ol_file + '] does not exist!')
    else:
        print('Open-loop file is  [' + ol_file + ']')
        print("Current directory is [" + os.getcwd() + "]")
        run(ol_file)


def run(ol_file):
    import read.sqlite as read_sql
    import plot.soil_profile as soil_profile
    import numpy as np

    properties = ["SW", "SoilNH4_", "SoilNO3_"]
    units = ["$mm/mm$", "$g/cm^2$", "$g/cm^2$"]
    layer_depth = [15, 15, 30, 30, 30, 30, 30]
    ensemble_size = 50

    for k in [0, 1, 2]:   # range(0, len(properties)):
        table_names = []
        for i in range(0, len(layer_depth)):
            table_names.append(properties[k] + str(i+1))
        truth = []
        openloop = []
        prior_mean = []
        posterior_mean = []
        ensembles_by_layers = []
        for i in range(0, len(table_names)):
            sql_enkf = read_sql.ReadSQLite()
            sql_enkf.read(table_names[i], ensemble_size, "States")
            doy = sql_enkf.doy

            truth.append(sql_enkf.truth)
            prior_mean.append(sql_enkf.prior_mean)
            posterior_mean.append(sql_enkf.posterior_mean)
            ensembles_by_layers.append(sql_enkf.posterior)

            sql_ol = read_sql.ReadSQLite()
            sql_ol.read(table_names[i], ensemble_size, ol_file)
            openloop.append(sql_ol.posterior_mean)

        ensembles = []
        for ensemble in range(0, ensemble_size):
            posterior_by_layers = []
            for i in range(0, len(table_names)):
                posterior_by_layers.append(ensembles_by_layers[i][ensemble, :].A1)
            posterior_by_layers = np.matrix(posterior_by_layers)
            ensembles.append(posterior_by_layers)

        x_diff = doy[0]
        truth = np.matrix(truth)
        openloop = np.matrix(openloop)
        prior_mean = np.matrix(prior_mean)
        posterior_mean = np.matrix(posterior_mean)

        for i in range(1, 366):
            doy_step = i
            soil_profile.plot(layer_depth, doy_step, x_diff, truth, openloop, prior_mean, posterior_mean, properties[k], ensembles)

        print("Plotting [" + properties[k] + "] profile done!")


settings()
