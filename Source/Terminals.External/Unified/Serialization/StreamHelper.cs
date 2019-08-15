using System.Text;
using System.Drawing;

namespace Unified
{
    public class StreamHelper
    {
        public static string StreamToString(System.IO.Stream stream)
        {
            if (stream == null) 
                return null;

            return Encoding.UTF8.GetString(StreamToBytes(stream));
        }

        public static byte[] StreamToBytes(System.IO.Stream stream)
        {
            if (stream == null) return null;

            byte[] buffer = null;
            if (stream.Position > 0 && stream.CanSeek) stream.Seek(0, System.IO.SeekOrigin.Begin);
            buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int)buffer.Length);
            return buffer;
        }

        public static Icon ImageToIcon(Image image)
        {
            return Icon.FromHandle(((Bitmap)image).GetHicon());
        }

        public static Image IconToImage(Icon icon)
        {
            return Image.FromHbitmap(icon.ToBitmap().GetHbitmap());
        }
    }
}