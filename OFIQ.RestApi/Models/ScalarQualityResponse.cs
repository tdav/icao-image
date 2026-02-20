namespace OFIQ.RestApi.Models
{
    /// <summary>
    /// Response containing the scalar quality score.
    /// </summary>
    public class ScalarQualityResponse
    {
        /// <summary>
        /// A scalar value assessment of image quality [0, 100]. 
        /// -1.0 indicates a failure to calculate the score.
        /// </summary>
        public double ScalarQuality { get; set; }
    }
}
