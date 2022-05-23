# -*- coding: utf-8 -*-
"""
Created on Thu Mar 29 17:30:10 2018

@author: Yuxi Zhang

a, B, C are 2D matrices or array
m, N are vectors (1x? or ?x1 matrices), arrays or lists
"""

import numpy as np


###################
# statistics of ensembles at each time step
###################

def ensp(a, axis=0):
    # Variation
    import numpy as np
    #    print(a)
    a_mean = np.mean(a, axis)
    #    print(a_mean)
    return msd(a, a_mean, axis)


def ensk(a, m, axis=0):
    import numpy as np
    a_mean = np.mean(a, axis)
    temp = np.power(a_mean - m, 2)
    return temp


def msd(a, m, axis=0):
    import numpy as np
    temp = np.power(a - m, 2)
    return np.mean(temp, axis)


def skew(a, axis=0):
    import numpy as np
    a_mean = np.mean(a, axis)
    a_ensp = msd(a, a_mean, axis)
    temp = np.power(a_ensp, 0.5)
    temp = np.divide(a - a_mean, temp)
    temp = np.mean(np.power(temp, 3), axis)
    return temp


def kurt(a, axis=0):
    a_mean = np.mean(a, axis)
    a_ensp = msd(a, a_mean, axis)
    temp = np.power(a_ensp, 0.5)
    temp = np.divide(a - a_mean, temp)
    temp = np.mean(np.power(temp, 4) - 3, axis)
    return temp


###################
# averaged to each ensemble and time step
###################


def mrmsd(a, m, axis=0):  # Ensemble root mean square error
    temp = np.mean(np.power(a - m, 2), axis)
    temp = np.power(temp, 0.5)
    temp = np.mean(temp)
    #    temp = np.mean(temp, 1 - axis)
    return temp


###################
# Between ensemble mean and obs
###################


def rmsd(a, m, axis=0):  # Root mean square difference between ensemble mean and obs
    a = np.mean(a, axis)
    temp = np.mean(np.power(a - m, 2))
    temp = np.power(temp, 0.5)
    return temp


def ns(a, m, axis=0):  # Nash-Sutcliffe coefficient between ensemble mean and obs
    a = np.mean(a, axis)
    temp1 = np.power(a - m, 2)
    #    print(temp1)
    temp2 = np.power(m - np.mean(m), 2)
    #    print(temp2)
    return 1 - np.sum(temp1) / np.sum(temp2)


def bias(a, m, axis=0):  # Bias between ensemble mean and obs
    a = np.mean(a, axis)
    return np.mean(a - m)


#    return np.mean(a - m, 1 - axis)


def test():
    a = np.matrix([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12]).reshape([3, 4])
    l = np.matrix([3, 4, 5, 2])
    m = np.matrix([1, 1, 1, 2])
    n = np.matrix([1, 1, 2]).reshape([3, 1])
    print(ns(a, m, 0))


def test1():
    a = np.matrix([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12]).reshape([4, 6])
    m = np.matrix([0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11]).reshape([1, 12])
    print(a[:, 2:-1])
    print(m[:, 2:-1])


# test1()
