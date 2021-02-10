using System.IO;

using Common.Minio;

using ImageMagick;

namespace WeatherService.Smhi
{
    public class MagickImageReceiver : IStreamReceiver<MagickImage>
    {
        private MagickImage image;

        public void Receive(Stream _stream)
        {
            image = new MagickImage(_stream);
        }

        public MagickImage Get()
        {
            return image;
        }
    }
}