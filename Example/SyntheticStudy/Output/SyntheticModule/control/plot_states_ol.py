# -*- coding: utf-8 -*-
"""
Created on Wed Mar 28 14:20:56 2018

@author: Yuxi Zhang

This script plot results of OpenLoop simulation using data from SQLite database.
Output including:
    1. State variables
    2. Histogram according to:
        LiY_integrated_2014
        De Lannoy,2006_Assessment
    3. Ensemble spread, skewness, kurtosis.
    3. Statistics

"""



# Add this module to path environment.
def add_module():
    import os
    import sys
    script = sys.argv[0]
    script = script.replace("/", "\\")
    path = script.split('\\control\\')
    sys.path.append(path[0])


add_module()


def plot(ol_file):
    import read.sqlite as read_sql
    import plot.states_stats as states_stats
    import plot.states_plot as states_plot
    import plot.get_phenology as phenology
    import settings.apsim_dict as dict
    import settings.fwrite as fwrite

    table_names = dict.table_names
    ensemble_size = 50

    # set_type, set_xtick, set_ytick, set_ylabel, set_legend, exp_eps
    set_default = ('default', True, True, True, False, True)

    pheno = phenology.get_phenology(ensemble_size, "OpenLoop", True)
    fwrite.format1()

    for i in range(0, len(table_names)):
        sql_ol = read_sql.ReadSQLite()
        sql_ol.read(table_names[i], ensemble_size, ol_file)
        states_plot.plot(sql_ol, sql_ol, set_default, phenology=pheno, is_simple=False, is_ol=True)
        states_stats.calc_stat(sql_ol, phenology=pheno, do_plot=True)
        print("Plotting [" + table_names[i] + "] done!")


import settings.set_path as set_path
plot(set_path.ol())

