using Harbard;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace EINAR5Integration.Services.CompressServices

{
    public class Compressor
    {
        public async Task InitializeFFmpeg()
        {
            try
            {
                string ffmpegExecutablesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services", "FFmpegBinaries");
                await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, ffmpegExecutablesPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading FFmpeg: {ex.Message}");
            }
        }

        public  void CompressImage(EventImage eventImage, int quality = 75)
        {
            if (eventImage.data == null)
            {
                // Handle null data case
                return;
            }

            using (var inputStream = new MemoryStream(eventImage.data))
            {
                using (var image = Image.Load(inputStream))
                {

                    var encoder = new JpegEncoder
                    {
                        Quality = quality
                    };

                    using (var outputStream = new MemoryStream())
                    {
                        image.Save(outputStream, encoder);
                        eventImage.data = outputStream.ToArray();
                    }
                    
                    eventImage.width = image.Width;
                    eventImage.height = image.Height;
                }
            }
        }

        public async Task CompressVideo(EventVideo eventVideo)
        {
            if (eventVideo?.data == null)
            {
                // Handle null data case
                return;
            }

            var tempInputPath = Path.GetTempFileName();
            File.WriteAllBytes(tempInputPath, eventVideo.data);

            var tempOutputPath = Path.GetTempFileName();

            try
            {
                // Perform the video compression
                var conversion = await FFmpeg.Conversions.FromSnippet.ToMp4(tempInputPath, tempOutputPath);
                conversion.SetPreset(ConversionPreset.UltraFast); // Example to set preset for faster encoding, adjust as needed
                await conversion.Start();

                eventVideo.data = File.ReadAllBytes(tempOutputPath);
                eventVideo.format = "mp4";
            }
            finally
            {
                // Clean up temporary files
                if (File.Exists(tempInputPath))
                {
                    File.Delete(tempInputPath);
                }
                if (File.Exists(tempOutputPath))
                {
                    File.Delete(tempOutputPath);
                }
            }
        }

    }
}
