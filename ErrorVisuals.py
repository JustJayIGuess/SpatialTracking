from mpl_toolkits import mplot3d
import numpy as np
import matplotlib.pyplot as plt

with open(r"C:\Users\jaidy\source\repos\SpatialTracking\SpatialTracking\bin\Debug\netcoreapp3.1\LinearTrackingRoom.txt") as file:
    lines = file.readlines()

positions = [[float(line.split("),")[0][1:].split(", ")[i]) for i in range(3)] for line in lines]

cdata = [float(line.split("),")[2].strip().split(", ")[0]) for line in lines]
confidences = [float(line.split("),")[2].strip().split(", ")[1]) for line in lines]

xdata = [position[0] for position in positions]
ydata = [position[1] for position in positions]
zdata = [position[2] for position in positions]

#bp = plt.boxplot(cdata)
#print(np.nanmean(cdata))
#hb = plt.hexbin(xdata, ydata, C=cdata, gridsize = 50)
#plt.scatter(xdata, ydata, c=cdata, s=0.1)

fig, ax = plt.subplots(ncols=1)

hb = ax.hexbin(xdata, ydata, C=cdata, gridsize = 50)

cb = plt.colorbar(hb, ax=ax)
plt.show()
