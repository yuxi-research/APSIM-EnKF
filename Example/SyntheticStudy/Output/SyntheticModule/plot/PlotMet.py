# Plot weather perturbation on a figure.

import numpy as np

ensembleSize = 50
met = "O:/Simulation_Output/Ch6_SyntheticStudy/Synthetic_Final/1_Copy_for_Journal/Met"

originMet = met+"/Weather.met"
metfiles = met+"/Weather_Ensemble{0}.met"
rainFig = met+"/rain.png"
radnFig = met+"/radn.png"
tempFig = met+"/temp.png"
evapFig = met+"/evap.png"
weatherFig = met+"/FigS2 Weather"


slide1 = 0
size = 360

X = [T1,T2,T3,T4,T5,T6,T7] = np.loadtxt(originMet,skiprows=(20),usecols=(1,2,3,4,5,6,7),unpack=True)
x = T1
Truth = [T2,T3,T4,T5,T6,T7]

mat1 = np.array
mat = list([])

#   read data as: met[ensemble size][col=4=variable number][row=days=2192]
for i in range(0,ensembleSize+1):
    if i < ensembleSize:
        metfile = metfiles.replace('{0}', str(i))
    else:
        metfile = metfiles.replace('Weather_Ensemble{0}', "OpenLoop")

    #   radn maxt mint rain pan vp
    mat1 = [col1,col2,col3,col4,col5,col6] = np.loadtxt(metfile,skiprows=(20),usecols=(2,3,4,5,6,7),unpack=True)
    mat.append(mat1)

# plot truth and ensembles, show axis

import matplotlib.pyplot as plt
plt.rcParams['pdf.fonttype'] = 42

plt.figure(figsize=(7, 6))
plt.subplots_adjust(left=0.1, bottom=0.1, right=0.95, top=0.95, wspace=0.2, hspace=0.25)

## radn
plt.subplot(3,1,1)
for i in range(0,ensembleSize):
    plt.plot(x[slide1:size],mat[i][0][slide1:size],color="silver", linewidth=1)

plt.plot(x[slide1:size],Truth[0][slide1:size],color="black", linewidth=1,label="Truth", zorder=10)
plt.plot(x[slide1:size],mat[ensembleSize][0][slide1:size],color="green", linewidth=1,label="Degraded data",zorder=5)
plt.plot(x[slide1:size],mat[ensembleSize-1][0][slide1:size],color="silver", linewidth=1,label="Purturbed data (N=50)", zorder=0)

plt.ylabel("Radn ($MJ/m^2$)")
plt.legend(loc='upper left')
plt.xlim(131,305)

## temp
plt.subplot(3,1,2)

for i in range(0,ensembleSize):
    plt.plot(x[slide1:size],mat[i][1][slide1:size],color="silver", linewidth=1)
    plt.plot(x[slide1:size],mat[i][2][slide1:size],color="silver", linewidth=1)

plt.plot(x[slide1:size],mat[ensembleSize-1][1][slide1:size],color="silver", linewidth=1)
plt.plot(x[slide1:size],mat[ensembleSize-1][2][slide1:size],color="silver", linewidth=1)
plt.plot(x[slide1:size],mat[ensembleSize][1][slide1:size],color="green", linewidth=1)
plt.plot(x[slide1:size],mat[ensembleSize][2][slide1:size],color="green", linewidth=1)
plt.plot(x[slide1:size],Truth[1][slide1:size],color="black", linewidth=1)
plt.plot(x[slide1:size],Truth[2][slide1:size],color="black", linewidth=1)

plt.ylabel("Temp ($^oC$)")
plt.xlim(131,305)

## rain

plt.subplot(3,1,3)

for i in range(0,ensembleSize):
    plt.plot(x[slide1:size],mat[i][3][slide1:size],color="silver", linewidth=1)

plt.plot(x[slide1:size],mat[ensembleSize-1][3][slide1:size],color="silver", linewidth=1)
plt.plot(x[slide1:size],mat[ensembleSize][3][slide1:size],color="green", linewidth=1)
plt.plot(x[slide1:size],Truth[3][slide1:size],color="black", linewidth=1)

plt.ylabel("Rain (mm)")
plt.xlabel("Day of year")

plt.ylim(-5,75)
plt.xlim(131,305)

plt.savefig(weatherFig + '.png', dpi=600)
plt.savefig(weatherFig + '.pdf')
#plt.show()
