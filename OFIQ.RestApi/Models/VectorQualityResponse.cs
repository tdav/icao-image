namespace OFIQ.RestApi.Models
{
    /// <summary>
    /// Coordinates of the detected face region.
    /// </summary>
    public class BoundingBoxDto
    {
        public short X { get; set; }
        public short Y { get; set; }
        public short Width { get; set; }
        public short Height { get; set; }
    }

    /// <summary>
    /// Result of a specific quality measure.
    /// </summary>
    public class AssessmentResultDto
    {
        /// <summary>
        /// Name of the quality measure (e.g., Sharpness, EyesOpen).
        /// </summary>
        public string MeasureName { get; set; } = string.Empty;
        /// <summary>
        /// Raw value computed by the algorithm.
        /// </summary>
        public double RawScore { get; set; }
        /// <summary>
        /// Normalized scalar value [0, 100].
        /// </summary>
        public double ScalarScore { get; set; }
        /// <summary>
        /// Return code for the specific measure (0 = Success).
        /// </summary>
        public int Code { get; set; }
    }

    /// <summary>
    /// Full set of quality metrics and face coordinates.
    /// </summary>
    public class VectorQualityResponse
    {
        /// <summary>
        /// Detected face region.
        /// </summary>
        public BoundingBoxDto BoundingBox { get; set; } = new();
        /// <summary>
        /// Collection of all calculated quality measures.
        /// </summary>
        public List<AssessmentResultDto> Assessments { get; set; } = new();
    }
}
