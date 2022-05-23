# -*- coding: utf-8 -*-
"""
Created on Mon Sep 25 22:55:00 2017

@author: Yuxi Zhang
"""
# Plot for trial runnings of DA.


def settings():
    import sys
    script = sys.argv[0]
    script = script.replace("/", "\\")
    path = script.split('\\plot\\')
    sys.path.append(path[0])

    ensemble_no = 50
    get_phenology(ensemble_no, "EnKF")


def get_phenology(ensemble_size, db_file_name, do_export=True):
    db_file = db_file_name+".db"

    import numpy as np
    import read.apsimdb as read_db

    truth = read_db.ApsimDB("Truth")
    truth.read_phenology(db_file)

    openloop = read_db.ApsimDB("OpenLoop")
    openloop.read_phenology(db_file)

    ensembles = []
    for i in range(0, ensemble_size):
        get_data = read_db.ApsimDB("Ensemble"+str(i))
        get_data.read_phenology(db_file)
        ensembles.append(get_data)

    truth_phenology = [-99, -99, -99, -99, -99, -99, -99, -99, -99, -99]
    truth_phenology[0] = truth.Sowing
    truth_phenology[1] = truth.Germination
    truth_phenology[2] = truth.Emergence
    truth_phenology[3] = truth.EndOfJuvenile
    truth_phenology[4] = truth.FloralInitiation
    truth_phenology[5] = truth.Flowering
    truth_phenology[6] = truth.StartGrainFill
    truth_phenology[7] = truth.EndGrainFill
    truth_phenology[8] = truth.Maturity
    truth_phenology[9] = truth.HarvestRipe
    for i in range(0, len(truth_phenology) - 1):
        if not truth_phenology[i]:
            truth_phenology[i] = truth_phenology[i + 1]

    openloop_phenology = [-99, -99, -99, -99, -99, -99, -99, -99, -99, -99]
    openloop_phenology[0] = openloop.Sowing
    openloop_phenology[1] = openloop.Germination
    openloop_phenology[2] = openloop.Emergence
    openloop_phenology[3] = openloop.EndOfJuvenile
    openloop_phenology[4] = openloop.FloralInitiation
    openloop_phenology[5] = openloop.Flowering
    openloop_phenology[6] = openloop.StartGrainFill
    openloop_phenology[7] = openloop.EndGrainFill
    openloop_phenology[8] = openloop.Maturity
    openloop_phenology[9] = openloop.HarvestRipe
    for i in range(0, len(openloop_phenology) - 1):
        if not openloop_phenology[i]:
            openloop_phenology[i] = openloop_phenology[i + 1]

    ensemble_phenology = []
    for no in range(0, ensemble_size):
        phenology = [-99, -99, -99, -99, -99, -99, -99, -99, -99, -99]
        phenology[0] = ensembles[no].Sowing
        phenology[1] = ensembles[no].Germination
        phenology[2] = ensembles[no].Emergence
        phenology[3] = ensembles[no].EndOfJuvenile
        phenology[4] = ensembles[no].FloralInitiation
        phenology[5] = ensembles[no].Flowering
        phenology[6] = ensembles[no].StartGrainFill
        phenology[7] = ensembles[no].EndGrainFill
        phenology[8] = ensembles[no].Maturity
        phenology[9] = ensembles[no].HarvestRipe
        for i in range(0, len(phenology)-1):
            if not phenology[i]:
                phenology[i] = phenology[i+1]
        ensemble_phenology.append(phenology)

    ensemble_phenology = np.matrix(ensemble_phenology)
    phenology_mean = np.mean(ensemble_phenology, 0).A1
    phenology_diff_to_truth = np.subtract(phenology_mean, truth_phenology)
    phenology_std = np.std(ensemble_phenology, 0).A1

    phenology_str = ['Sowing', 'Germination', 'Emergence', 'EndOfJuvenile', 'FloralInitiation',
                     'Flowering', 'StartGrainFill', 'EndGrainFill', 'Maturity', 'HarvestRipe']

    if do_export:
        my_file = open("Phenology.csv", 'w')  # a= append, w=write, r=read
        sep = ','   # Separator

        my_file.write("Phenology")
        for row in range(0, len(truth_phenology)):
            my_file.write(sep + phenology_str[row])

        my_file.write("\nTruth")
        for row in range(0, len(truth_phenology)):
            my_file.write(sep + str(truth_phenology[row]))

        my_file.write("\nOpenLoop")
        for row in range(0, len(truth_phenology)):
            my_file.write(sep + str(openloop_phenology[row]))

        my_file.write("\nMean")
        for row in range(0, len(truth_phenology)):
            my_file.write(sep + str(phenology_mean[row]))

        my_file.write("\nDiff")
        for row in range(0, len(truth_phenology)):
            my_file.write(sep + str(phenology_diff_to_truth[row]))

        my_file.write("\nStd")
        for row in range(0, len(truth_phenology)):
            my_file.write(sep + str(phenology_std[row]))

        for no in range(0, ensemble_size):
            my_file.write("\n" + "Ensemble_" + str(no))
            for row in range(0, 10):
                my_file.write(sep + str(ensemble_phenology[no, row]))

        my_file.close()

    return truth_phenology, openloop_phenology, ensemble_phenology


# settings()
