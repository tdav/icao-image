namespace OFIQ.RestApi.Models
{
    /// <summary>
    /// X and Y coordinates of a facial landmark.
    /// </summary>
    public class LandmarkDto
    {
        public short X { get; set; }
        public short Y { get; set; }
    }

    /// <summary>
    /// Extended quality analysis including facial landmarks.
    /// </summary>
    public class PreprocessingQualityResponse : VectorQualityResponse
    {
        /// <summary>
        /// Detected facial landmarks (98 points).
        /// </summary>
        public List<LandmarkDto> Landmarks { get; set; } = new();
    }
}
