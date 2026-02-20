using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.InteropServices;

namespace ikao
{
    class Program
    {       
        static void Main(string[] args)
        {
            if (args.Length < 1) { Console.WriteLine("Usage: ./OFIQConsoleApp <image_path> [config_dir] [config_file]"); return; }

            string imagePath = args[0];
            
            //string configDir = args.Length > 1 ? args[1] : "config";
            //string configFile = args.Length > 2 ? args[2] : "";

            string configDir =  ".";
            string configFile = "ofiq_config.jaxn";

            IntPtr handle = IntPtr.Zero;
            try
            {
                handle = NativeInvoke.ofiq_get_implementation();
                var initStatus = NativeInvoke.ofiq_initialize(handle, configDir, configFile);
                if (initStatus.Code != (int)NativeInvoke.ReturnCode.Success)
                {
                    Console.WriteLine($"Init error ({initStatus.Code}): {initStatus.GetInfo()}");
                    return;
                }

                int major, minor, patch;
                var versionStatus = NativeInvoke.ofiq_get_version(handle, out major, out minor, out patch);
                Console.WriteLine($"OFIQ version: {major}.{minor}.{patch}");
                NativeInvoke.free_status(versionStatus);

                using (var image = Image.Load<Bgr24>(imagePath))
                {
                    if (!image.DangerousTryGetSinglePixelMemory(out var memory))
                    {
                        throw new Exception("Could not get direct memory access to image pixels.");
                    }

                    var results = new NativeInvoke.BridgeAssessment[100]; // Запас для всех метрик
                    int count;
                    NativeInvoke.BridgeBoundingBox bbox;

                    Console.WriteLine($"Processing image: {imagePath} ({image.Width}x{image.Height})");
                    
                    NativeInvoke.BridgeReturnStatus status;
                    using (var handlePin = memory.Pin())
                    {
                        unsafe
                        {
                            status = NativeInvoke.ofiq_vector_quality(handle, (ushort)image.Width, (ushort)image.Height, 24, (IntPtr)handlePin.Pointer, results, out count, out bbox);
                        }
                    }

                    if (status.Code == (int)NativeInvoke.ReturnCode.Success)
                    {
                        Console.WriteLine($"\nFace found: X={bbox.X}, Y={bbox.Y}, W={bbox.Width}, H={bbox.Height}");
                        Console.WriteLine(new string('-', 80));
                        Console.WriteLine($"{"Measure Name",-30} | {"Raw Score",-15} | {"Scalar Score",-15}");
                        Console.WriteLine(new string('-', 80));

                        for (int i = 0; i < count; i++)
                        {
                            string measureName = Enum.IsDefined(typeof(NativeInvoke.QualityMeasure), results[i].Measure)
                                ? ((NativeInvoke.QualityMeasure)results[i].Measure).ToString()
                                : $"Unknown(0x{results[i].Measure:X})";

                            string raw = results[i].RawScore.ToString("F4");
                            string scalar = results[i].Code == 0 ? results[i].Scalar.ToString("F2") : "N/A";

                            Console.WriteLine($"{measureName,-30} | {raw,-15} | {scalar,-15}");
                        }
                        Console.WriteLine(new string('-', 80));
                    }
                    else
                    {
                        Console.WriteLine($"Quality assessment failed: {status.GetInfo()} (Code: {status.Code})");
                    }
                    NativeInvoke.free_status(status);
                }
            }
            catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
            finally { if (handle != IntPtr.Zero) NativeInvoke.ofiq_destroy_implementation(handle); }
        }
    }
}