# SpatialTracking
 Spatial tracking software with a few architectures:
 * *Linear Tracking* - for **fixed cameras and moving trackers**; this method creates cartesian equations of lines intersecting at the tracked point and finds intersections with associated confidence values to find overall best prediction of tracked point.
 * *Circular Tracking* - for **fixed trackers and moving camera**; this method uses some circle geometry to associate the tracked point (the camera/s) with points on circles, which are then intersected to find the best prediction of the tracked point. Note: this method is succeptible to noise.
 * *Vector Tracking* - for **fixed trackers and moving camera**; this method associates tracking markers with 3D lines (described by offset and direction vectors), whose approximate intersection (the lines likely do not actually intersect in 3D) is found through a linear algebra approach to LSM optimisation - will elaborate further if this pans out.

## Vector Tracking
Each vector, $d_{i}$, represents a ray starting at the camera's position and pointing toward the tracked point. These are paired with the position vectors of each camera, $a_{i}$, to give a set of 3D lines:
$$
\vec{r}_{i} = \vec{a}_{i} + t\vec{d}_{i}.
$$
These direction vectors are normalised to simplify the following process:
$$
\hat{d}_{i} = \frac{1}{\left\lVert\vec{d}_{i}\right\rVert}\vec{d}_i.
$$
Consider one line and an arbitrary point, $\vec{x}$. Construct a right-angled triangle with the two sides being parallel and perpendicular to the line, and the hypotenuse running from $\vec{x}$ to $\vec{a}$. Note that $d$ here refers to the normalised $\hat{d}$ vector.
![[Tracking Diagram Bare|300]]
The length of the parallel side is the scalar resolute of the hypotenuse vector, $\vec{x} - \vec{a}$ projected onto the direction of the line, $\hat{d}$, and the hypotenuse is the magnitude of $\vec{x}-\vec{a}$. The remaining side is the scalar distance to the line from the point, $s$.
![[Tracking Diagram Labelled 1|300]]
Using the Pythagorean Theorem, we now have an expression for the square distance from a point to a line.
$$
\begin{align*}
a^{2}+b^{2} &= c^{2}\\
a^{2} &= c^{2} - b^{2}\\
s^{2} &= \left\lVert\vec{x}-\vec{a}\right\rVert^{2} - \left((\vec{x}-\vec{a})\cdot\hat{d}\right)^{2}
\end{align*}
$$
Now consider the function, $D(\vec{x})$, that gives the sum of the squared distances to each line.
$$
\begin{align*}
D(\vec{x}) &= \sum_{i}\left[\left\lVert\vec{x}-\vec{a_{i}}\right\rVert^{2} - \left((\vec{x}-\vec{a_{i}})\cdot\hat{d_{i}}\right)^{2}\right]
\end{align*}
$$
>It is useful to think about $\left\lVert\vec{x}-\vec{a_{i}}\right\rVert^{2}$ instead as its dot product with itself, $(\vec{x}-\vec{a_{i}})\cdot(\vec{x}-\vec{a_{i}})$.

We expect this function to have a minimum at the point which is "closest" to being an intersection of all lines. This minimum can be found by first taking the derivative of $D(\vec{x})$ w.r.t $\vec{x}$.
$$
\begin{align*}
D'(\vec{x}) &= \sum_{i}\left[2(\vec{x}-\vec{a_{i}})-2\left( (\vec{x}-\vec{a_{i}})\cdot\hat{d_{i}} \right)\hat{d_{i}}\right]
\end{align*}
$$
This is set to zero to find local extrema.
$$
\begin{align*}
0 &= \sum_{i}\left[2(\vec{x}-\vec{a_{i}})-2\left( (\vec{x}-\vec{a_{i}})\cdot\hat{d_{i}} \right)\hat{d_{i}}\right]\\
0 &= 2\sum\limits_{i}\left[ (\vec{x}-\vec{a_{i}}) - \hat{d_{i}}\left(\hat{d_{i}}(\vec{x}-\vec{a_{i}})\right) \right]\\
0 &= \sum\limits_{i}\left[ (\vec{x}-\vec{a_{i}}) - \hat{d_{i}}\left(\hat{d_{i}}(\vec{x}-\vec{a})\right) \right]
\end{align*}
$$
The vectors can now be thought of as matrices, with dot products of the column vector $\vec{x}$ being represented as $\vec{x}^{T}\vec{x}$. This allows the expression to be simplified to a closed-form solution for $s$.
$$
\begin{align*}
0 &= \sum\limits_{i}\left[ I(\vec{x}-\vec{a_{i}}) - \hat{d_{i}}\hat{d_{i}}^{T}(\vec{x}-\vec{a_{i}}) \right]\\
0 &= \sum\limits_{i}\left[ \left(I - \hat{d_{i}}\hat{d_{i}}^{T}\right)(\vec{x}-\vec{a_{i}}) \right]\\
\sum\limits_{i}\left(I - \hat{d_{i}}\hat{d_{i}}^{T}\right) \vec{a_{i}} &= \sum\limits_{i} \left(I - \hat{d_{i}}\hat{d_{i}}^{T}\right) \vec{x}\\
\sum\limits_{i}\left(I - \hat{d_{i}}\hat{d_{i}}^{T}\right) \vec{a_{i}} &= \vec{x}\sum\limits_{i} \left(I - \hat{d_{i}}\hat{d_{i}}^{T}\right)\\
\vec{x} &= \left[\sum\limits_{i} \left(I - \hat{d_{i}}\hat{d_{i}}^{T}\right)\right]^{-1}\sum\limits_{i} \left(I - \hat{d_{i}}\hat{d_{i}}^{T}\right)\vec{a_{i}}
\end{align*}
$$
This value of $\vec{x}$ was used as the predicted object position.
