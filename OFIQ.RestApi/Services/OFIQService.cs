using ikao;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace OFIQ.RestApi.Services
{
    public interface IOFIQService
    {
        Task<double> GetScalarQualityAsync(Stream imageStream);
        Task<(NativeInvoke.BridgeAssessment[] assessments, NativeInvoke.BridgeBoundingBox bbox)> GetVectorQualityAsync(Stream imageStream);
        Task<(NativeInvoke.BridgeAssessment[] assessments, NativeInvoke.BridgeBoundingBox bbox, NativeInvoke.BridgeLandmark[] landmarks)> GetPreprocessingResultsAsync(Stream imageStream);
    }

    public class OFIQService : IOFIQService, IDisposable
    {
        private readonly IntPtr _handle;
        private readonly ILogger<OFIQService> _logger;
        private readonly string _configDir = ".";
        private readonly string _configFile = "ofiq_config.jaxn";

        public OFIQService(ILogger<OFIQService> logger, IConfiguration configuration)
        {
            _logger = logger;
            
            _logger.LogInformation("Initializing OFIQ library...");
            _handle = NativeInvoke.ofiq_get_implementation();
            
            if (_handle == IntPtr.Zero)
            {
                throw new Exception("Failed to get OFIQ implementation handle.");
            }

            var status = NativeInvoke.ofiq_initialize(_handle, _configDir, _configFile);
            if (status.Code != (int)NativeInvoke.ReturnCode.Success)
            {
                string error = status.GetInfo();
                NativeInvoke.free_status(status);
                throw new Exception($"Failed to initialize OFIQ: {error} (Code: {status.Code})");
            }
            NativeInvoke.free_status(status);
            _logger.LogInformation("OFIQ library initialized successfully.");
        }

        public async Task<double> GetScalarQualityAsync(Stream imageStream)
        {
            var (assessments, _) = await GetVectorQualityInternalAsync(imageStream, false);
            // UnifiedQualityScore is usually the first or can be found by enum
            foreach (var a in assessments)
            {
                if (a.Measure == (int)NativeInvoke.QualityMeasure.UnifiedQualityScore)
                    return a.Scalar;
            }
            return -1;
        }

        public async Task<(NativeInvoke.BridgeAssessment[] assessments, NativeInvoke.BridgeBoundingBox bbox)> GetVectorQualityAsync(Stream imageStream)
        {
            return await GetVectorQualityInternalAsync(imageStream, false);
        }

        public async Task<(NativeInvoke.BridgeAssessment[] assessments, NativeInvoke.BridgeBoundingBox bbox, NativeInvoke.BridgeLandmark[] landmarks)> GetPreprocessingResultsAsync(Stream imageStream)
        {
            using var image = await Image.LoadAsync<Bgr24>(imageStream);
            if (!image.DangerousTryGetSinglePixelMemory(out var memory))
            {
                throw new Exception("Could not get direct memory access to image pixels.");
            }

            var results = new NativeInvoke.BridgeAssessment[100];
            int count;
            NativeInvoke.BridgeBoundingBox bbox;
            int maxLandmarks = 100;
            var landmarks = new NativeInvoke.BridgeLandmark[maxLandmarks];

            NativeInvoke.BridgeReturnStatus status;
            using (var handlePin = memory.Pin())
            {
                unsafe
                {
                    var preproc = new NativeInvoke.BridgePreprocessingResult
                    {
                        LandmarkCount = maxLandmarks,
                        Landmarks = (IntPtr)Unsafe.AsPointer(ref landmarks[0]),
                        SegmentationMask = IntPtr.Zero,
                        OcclusionMask = IntPtr.Zero
                    };

                    status = NativeInvoke.ofiq_vector_quality_with_preprocessing(
                        _handle, (ushort)image.Width, (ushort)image.Height, 24, 
                        (IntPtr)handlePin.Pointer, results, out count, out bbox, ref preproc);
                    
                    maxLandmarks = preproc.LandmarkCount;
                }
            }

            if (status.Code != (int)NativeInvoke.ReturnCode.Success)
            {
                string error = status.GetInfo();
                NativeInvoke.free_status(status);
                throw new Exception($"OFIQ Vector Quality with Preprocessing failed: {error}");
            }
            NativeInvoke.free_status(status);

            return (results.Take(count).ToArray(), bbox, landmarks.Take(maxLandmarks).ToArray());
        }

        private async Task<(NativeInvoke.BridgeAssessment[] assessments, NativeInvoke.BridgeBoundingBox bbox)> GetVectorQualityInternalAsync(Stream imageStream, bool includePreproc)
        {
            using var image = await Image.LoadAsync<Bgr24>(imageStream);
            if (!image.DangerousTryGetSinglePixelMemory(out var memory))
            {
                throw new Exception("Could not get direct memory access to image pixels.");
            }

            var results = new NativeInvoke.BridgeAssessment[100];
            int count;
            NativeInvoke.BridgeBoundingBox bbox;

            NativeInvoke.BridgeReturnStatus status;
            using (var handlePin = memory.Pin())
            {
                unsafe
                {
                    status = NativeInvoke.ofiq_vector_quality(_handle, (ushort)image.Width, (ushort)image.Height, 24, (IntPtr)handlePin.Pointer, results, out count, out bbox);
                }
            }

            if (status.Code != (int)NativeInvoke.ReturnCode.Success)
            {
                string error = status.GetInfo();
                NativeInvoke.free_status(status);
                throw new Exception($"OFIQ Vector Quality failed: {error}");
            }
            NativeInvoke.free_status(status);

            return (results.Take(count).ToArray(), bbox);
        }

        public void Dispose()
        {
            if (_handle != IntPtr.Zero)
            {
                _logger.LogInformation("Destroying OFIQ implementation...");
                NativeInvoke.ofiq_destroy_implementation(_handle);
            }
        }
    }
}
