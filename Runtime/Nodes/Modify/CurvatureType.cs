namespace TerrainGraph
{
    public enum CurvatureType
    {
        /// <summary>
        /// Plan curvature measures topographic convergence or divergence.
        /// Is positive for diverging flows on ridges and negative converging flows in valleys.
        /// </summary>
        Plan,

        /// <summary>
        /// Same as plan curvature but multiplied by the sine of the slope angle.
        /// Does not take on extremely large values when slope is small.
        /// aka Tangential curvature.
        /// </summary>
        Horizontal,

        /// <summary>
        /// Vertical curvature measures the rate of change of the slope.
        /// Is negative for slope increasing downhill and positive for slope decreasing dowhill.
        /// aka profile curvature.
        /// </summary>
        Vertical,

        /// <summary>
        /// Mean curvature represents convergence and relative deceleration with equal weights.
        /// </summary>
        Mean,

        /// <summary>
        /// Gaussian curvature retains values in each point on the surface after
        /// its bending without breaking, stretching, and compressing.
        /// </summary>
        Gaussian,

        Minimal,
        Maximal,
        Unsphericity,
        Rotor,
        Difference,
        HorizontalExcess,
        VerticalExcess,
        Ring,
        Accumulation
    }
}