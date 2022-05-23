# Updated on 3 May, 2018


def ensemble_mean(ensemble_variable):
    ensemble_no = len(ensemble_variable)
    length = len(ensemble_variable[0])
    temp = []
    for i in range(0, length):
        mean = 0.0
        for j in range(0, ensemble_no):
            mean = mean + ensemble_variable[j][i][0]
        mean = mean / ensemble_no
        temp.append((mean,))
    return temp


def bias(data1, data2):
    length = len(data1)
    total = 0.0
    for i in range(0, length):
        total = total + data1[i][0] - data2[i][0]
    return total / length


def non_zero_bias(data1, data2):
    length = len(data1)
    length_new = 0
    total = 0.0
    for i in range(0, length):
        if (data1[i][0] != 0) & (data2[i][0] != 0):
            total = total + data1[i][0] - data2[i][0]
            length_new += 1
    return total / length_new


# Calculate the Nash-Sutcliffe (NS) value of data1 to data2.
def NS(data1, data2):
    length = len(data1)
    numerator = 0
    denominator = 0
    mean2 = 0

    for i in range(0, length):
        mean2 = mean2 + data2[i][0]
    mean2 = mean2 / length

    for i in range(0, length):
        numerator += (data1[i][0] - data2[i][0]) ** 2
        denominator += (data1[i][0] - mean2) ** 2
    return 1 - numerator / denominator


def none_zero_ns(data1, data2):
    length = len(data1)
    length_new = 0
    numerator = 0
    denominator = 0
    mean2 = 0

    for i in range(0, length):
        if (data1[i][0] != 0) & (data2[i][0] != 0):
            mean2 = mean2 + data2[i][0]
            length_new += 1
    mean2 = mean2 / length_new

    for i in range(0, length):
        if (data1[i][0] != 0) & (data2[i][0] != 0):
            numerator += (data1[i][0] - data2[i][0]) ** 2
            denominator += (data1[i][0] - mean2) ** 2
    return 1 - numerator / denominator


# Calculate the Root Mean Square Difference (RMSD) of data1 to data2.
def rmsd(data1, data2):
    length = len(data1)
    total = 0
    for i in range(0, length):
        total += (data1[i][0] - data2[i][0]) ** 2
    return (total / length) ** 0.5


def non_zero_rmsd(data1, data2):
    length = len(data1)
    length_new = 0
    total = 0
    for i in range(0, length):
        if (data1[i][0] != 0) & (data2[i][0] != 0):
            total += (data1[i][0] - data2[i][0]) ** 2
            length_new += 1
    return (total / length_new) ** 0.5


# Calculate the Ensemble Mean Root Mean Square Difference (MRMSD) of data1 to
# data2.
def mrmsd(data1, data2, EnsembleSize):
    length = len(data2)
    total = 0
    temp = 0
    for i in range(0, length):
        for j in range(0, EnsembleSize):
            total += (data1[j][i][0] - data2[i][0]) ** 2
        temp += (total / EnsembleSize) ** 0.5
    return temp / length


def non_zero_mrmsd(data1, data2, EnsembleSize):
    length = len(data2)
    length_new = 0
    total = 0
    temp1 = 0
    temp2 = 0
    for i in range(0, length):
        for j in range(0, EnsembleSize):
            temp1 = (data1[j][i][0] - data2[i][0]) ** 2
            total += temp1
        if (temp1 != 0) & (data2[i][0] != 0):
            temp2 += (total / EnsembleSize) ** 0.5
            length_new += 1
    return temp2 / length_new


# Calculate the unbiased Root Mean Square Difference (MRMSD) of data1 to
# data2.
# This method is not completed yet!!!!!!!!!!!!!
def ub_rmsd(data1, data2, EnsembleSize):
    length = len(data2)
    total = 0
    temp = 0
    for i in range(0, length):
        for j in range(0, EnsembleSize):
            total += (data1[j][i][0] - data2[i][0]) ** 2
        temp += (total / EnsembleSize) ** 0.5
    return temp / length


def non_zero_ub_rmsd(data1, data2, EnsembleSize):
    length = len(data2)
    length_new = 0
    total = 0
    temp1 = 0
    temp2 = 0
    for i in range(0, length):
        for j in range(0, EnsembleSize):
            temp1 = (data1[j][i][0] - data2[i][0]) ** 2
            total += temp1
        if (temp1 != 0) & (data2[i][0] != 0):
            temp2 += (total / EnsembleSize) ** 0.5
            length_new += 1
    return temp2 / length_new
