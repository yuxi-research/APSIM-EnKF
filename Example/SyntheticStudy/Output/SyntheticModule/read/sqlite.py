# -*- coding: utf-8 -*-
"""
Created on Wed Mar 28 13:13:41 2018

@author: Yuxi Zhang

Read data of single table from SQLite database by table_name (results of APSIM data assimilation)
Stored in Matrix
2-D State variables:
    Matrix Row: Ensemble number
    Matrix Col: Time step
1-D DOY, Truth, Obs, Mean:
    Vector by Col
"""


class ReadSQLite:
    def _init_(self):
        self.sql_name = []
        self.ensemble_size = []
        self.table_name = []

    def read(self, table_name, ensemble_size, sql_name):
        self.sql_name = sql_name
        self.ensemble_size = ensemble_size
        self.table_name = table_name
        self.id = []
        self.doy = []
        self.truth = []
        self.openloop = []
        self.obs = []
        self.prior_mean = []
        self.posterior_mean = []

        import sqlite3 as lite
        import numpy as np
        con = lite.connect(self.sql_name + ".sqlite")
        cur = con.cursor()

        #        Define column names.
        columns = ["ID", "DOY", "Truth", "PriorMean", "Obs", "PosteriorMean", "PosteriorOpenLoop"]
        for number in range(0, ensemble_size):
            prior_str = "PriorEnsemble" + str(number)
            posterior_str = "PosteriorEnsemble" + str(number)
            obs_str = "ObsEnsemble" + str(number)
            columns.append(prior_str)
            columns.append(posterior_str)
            columns.append(obs_str)

        #       Load SQLite from Assimilation
        temp_prior = []
        temp_posterior = []
        temp_perturbed_obs = []

        for j in range(0, len(columns)):
            str1 = "SELECT " + columns[j] + " FROM " + table_name
            cur.execute(str1)
            col = cur.fetchall()
            col_new = []
            for temp in range(0, len(col)):
                col_new.append(col[temp][0])

            if columns[j] == "id":
                self.id = col_new
            elif columns[j] == "DOY":
                self.doy = col_new
            elif columns[j] == "Truth":
                self.truth = col_new
            elif columns[j] == "PosteriorOpenLoop":
                self.openloop = col_new
            elif columns[j] == "Obs":
                self.obs = col_new
            elif columns[j].find("Prior") != -1 & columns[j].find("Mean") == -1:
                temp_prior.append(col_new)
            elif columns[j] == "PriorMean":
                self.prior_mean = col_new
            elif columns[j].find("Posterior") != -1 & columns[j].find("Mean") == -1:
                temp_posterior.append(col_new)
            elif columns[j] == "PosteriorMean":
                self.posterior_mean = col_new
            elif columns[j].find("Obs") != -1 & columns[j].find("Ensemble") != -1:
                temp_perturbed_obs.append(col_new)

        self.prior = np.matrix(temp_prior)
        self.posterior = np.matrix(temp_posterior)
        self.perturbed_obs = np.matrix(temp_perturbed_obs)

        if (table_name == "GrainWt") or (table_name == "GrainN"):
            self.truth, temp1, temp2 = transfer(np.matrix(self.truth))
            self.prior_mean, self.prior_harvest_day, self.prior = transfer(self.prior)
            self.posterior_mean, self.posterior_harvest_day, self.posterior = transfer(self.posterior)

        #print("Reading [" + sql_name +"." + table_name + "] done!")


# Transfer prior and posterior yield to be the maximum, return the time series of the yield
# with the averaged yield value and harvest date.
def transfer(matrix_in):
    import numpy as np
    matrix = matrix_in.copy()
    (row, col) = matrix.shape
    index_max = []
    for i in range(0, row):
        temp_max = 0
        temp_index = 0

        for j in range(0, col):
            if matrix[i, j] > temp_max:
                temp_max = matrix[i, j]
                temp_index = j

        index_max.append(temp_index)
        harvest = np.mean(index_max)
        harvest = int(np.ceil(harvest))

        for j in range(0, col - 1):
            if (matrix[i, j] > temp_max/2) & (matrix[i, j + 1] == 0):
                matrix[i, j + 1] = temp_max

    mean_out = np.mean(matrix, 0).A1

    if False:
        import matplotlib.pyplot as plt
        for i in range(0, row):
            plt.plot(matrix[i, :].A1, color='pink')
        plt.plot(mean_out, color='red')
        plt.show()

    # Set values after harvest to zero.
    #for j in range(harvest + 1, col):
     #   mean_out[j] = 0
    return mean_out, harvest, matrix

