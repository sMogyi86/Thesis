using System.IO;

namespace StandardClassLibrary_TestBL
{
    public class TestServices
    {
        public byte[] GetImageAsByteArray(string imageName)
        {
            byte[] image = { };

            using (Stream stream = new FileStream(imageName, FileMode.Open, FileAccess.Read))
            {
                var length = stream.Length;
                image = new byte[length];
                stream.Read(image, 0, (int)length);
            }

            return image;
        }
    }
}
