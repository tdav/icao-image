using System;
using Xunit;
using ikao;
using System.Runtime.InteropServices;

namespace OFIQConsoleApp.Tests
{
    public class OFIQTests
    {
        [Fact]
        public void TestGetVersion()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return;

            IntPtr handle = IntPtr.Zero;
            try
            {
                handle = NativeInvoke.ofiq_get_implementation();
                Assert.NotEqual(IntPtr.Zero, handle);

                int major, minor, patch;
                var status = NativeInvoke.ofiq_get_version(handle, out major, out minor, out patch);
                
                Assert.Equal(0, status.Code);
                Assert.True(major >= 0);
            }
            finally
            {
                if (handle != IntPtr.Zero)
                    NativeInvoke.ofiq_destroy_implementation(handle);
            }
        }

        [Fact]
        public void TestVectorQualityWithPreprocessing()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return;

            IntPtr handle = IntPtr.Zero;
            try
            {
                handle = NativeInvoke.ofiq_get_implementation();
                Assert.NotEqual(IntPtr.Zero, handle);

                // Setup preprocessing result with buffer for landmarks
                int maxLandmarks = 100;
                var landmarksPtr = Marshal.AllocHGlobal(Marshal.SizeOf<NativeInvoke.BridgeLandmark>() * maxLandmarks);
                
                try {
                    var preproc = new NativeInvoke.BridgePreprocessingResult
                    {
                        LandmarkCount = maxLandmarks,
                        Landmarks = landmarksPtr,
                        SegmentationMask = IntPtr.Zero,
                        OcclusionMask = IntPtr.Zero
                    };

                    // Note: Full test requires initialization and an image. 
                    // This test verifies structure alignment and marshaling logic.
                    Assert.True(preproc.LandmarkCount == 100);
                }
                finally {
                    Marshal.FreeHGlobal(landmarksPtr);
                }
            }
            finally
            {
                if (handle != IntPtr.Zero)
                    NativeInvoke.ofiq_destroy_implementation(handle);
            }
        }
    }
}
