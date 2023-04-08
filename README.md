# SpatialTracking
 Spatial tracking software with a few architectures:
 * 'Linear Tracking' - for fixed cameras and moving trackers; this method creates cartesian equations of lines intersecting at the tracked point and finds intersections with associated confidence values to find overall best prediction of tracked point.
 * 'Circular Tracking' - for fixed trackers and moving camera; this method uses some circle geometetry to associate the tracked point (the camera/s) with points on circles, which are then intersected to find the best prediction of the tracked point. Note: this method is suceptible to noise.
 * 'Vector Tracking' (W.I.P.) - for fixed trackers and moving camera; this method associates tracking markers with 3D lines (described by offset and direction vectors), whose approximate intersection (the lines likely do not actually intersect in 3D) is found through a linear algebra approach to LSM optimisation - will elaborate further if this pans out.
