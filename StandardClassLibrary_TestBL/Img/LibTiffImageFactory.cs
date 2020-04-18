using System;
using BitMiracle.LibTiff.Classic;
using System.IO;

namespace MARGO.BL.Img
{
    public interface IImageFactory
    {
        Image CreateImage(string name, ImageParts from);
    }

    internal sealed class LibTiffImageFactory : IImageFactory
    {
        public Image CreateImage(string name, ImageParts parts)
            => new Image(name, parts, this.Build);

        private MemoryStream Build(ImageParts parts)
        {
            var tiff = Tiff.ClientOpen("in-memory TIFF", "w", new MemoryStream(), new TiffStream());

            if (tiff is null)
                throw new IOException("Can not create in-memory TIFF!");

            try
            {
                int samplesPerPixel;
                Photometric photometric;
                switch (parts.Type)
                {
                    case ImageParts.Plan.Mono:
                        samplesPerPixel = 1;
                        photometric = Photometric.MINISBLACK;
                        break;

                    case ImageParts.Plan.RGB:
                    case ImageParts.Plan.Mapping:
                        samplesPerPixel = 3;
                        photometric = Photometric.RGB;
                        break;

                    default: throw new ArgumentException();
                }
                tiff.SetField(TiffTag.IMAGEWIDTH, parts.Width);
                tiff.SetField(TiffTag.IMAGELENGTH, parts.Height);
                tiff.SetField(TiffTag.SAMPLESPERPIXEL, samplesPerPixel);
                tiff.SetField(TiffTag.BITSPERSAMPLE, 8);
                tiff.SetField(TiffTag.ORIENTATION, Orientation.TOPLEFT);
                tiff.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                tiff.SetField(TiffTag.PHOTOMETRIC, photometric);
                tiff.SetField(TiffTag.ROWSPERSTRIP, 1);

                switch (parts.Type)
                {
                    case ImageParts.Plan.RGB:
                        this.WriteRGB(parts, tiff);
                        break;
                    case ImageParts.Plan.Mono:
                        this.WriteMono(parts, tiff);
                        break;
                    case ImageParts.Plan.Mapping:
                        this.WriteWithMapping(parts, tiff);
                        break;
                }

                //https://stackoverflow.com/questions/23162083/writing-a-multipaged-tif-file-from-memorystream-vs-filestream/23227792#23227792
                tiff.Flush();

                if (!tiff.WriteDirectory())
                    throw new IOException("The current directory was NOT written successfully!");

                return new MemoryStream(((MemoryStream)tiff.Clientdata()).GetBuffer(), false);
            }
            finally
            {
                // https://stackoverflow.com/questions/7274340/how-to-use-bit-miracle-libtiff-net-to-write-the-image-to-a-memorystream/8427942#8427942
                tiff.Close();
                tiff.Dispose();
            }
        }

        private void WriteMono(ImageParts parts, Tiff tiff)
        {
            var mono = parts.Chanels['M'].Span;

            byte[] rowData = new byte[tiff.ScanlineSize()];
            for (int rowIndex = 0; rowIndex < parts.Height; rowIndex++)
            {
                for (int pixelIndex = 0; pixelIndex < parts.Width; pixelIndex++)
                    rowData[pixelIndex] = mono[rowIndex * parts.Width + pixelIndex];

                if (!tiff.WriteScanline(rowData, rowIndex))
                    throw new IOException(@"Image data were NOT encoded and written successfully!");
            }
        }

        private void WriteRGB(ImageParts parts, Tiff tiff)
        {
            int samplesPerPixel = 3;

            var red = parts.Chanels['R'].Span;
            var green = parts.Chanels['G'].Span;
            var blue = parts.Chanels['B'].Span;

            byte[] rowData = new byte[tiff.ScanlineSize()];
            for (int rowIndex = 0; rowIndex < parts.Height; rowIndex++)
            {
                for (int pixelIndex = 0; pixelIndex < parts.Width; pixelIndex++)
                {
                    rowData[pixelIndex * samplesPerPixel + 0] = red[rowIndex * parts.Width + pixelIndex];
                    rowData[pixelIndex * samplesPerPixel + 1] = green[rowIndex * parts.Width + pixelIndex];
                    rowData[pixelIndex * samplesPerPixel + 2] = blue[rowIndex * parts.Width + pixelIndex];
                }

                if (!tiff.WriteScanline(rowData, rowIndex))
                    throw new IOException(@"Image data were NOT encoded and written successfully!");
            }
        }

        private void WriteWithMapping(ImageParts parts, Tiff tiff)
        {
            int samplesPerPixel = 3;

            var chanel = parts.Chanels['C'].Span;
            var mapping = parts.ColorMapping;

            byte[] rowData = new byte[tiff.ScanlineSize()];
            for (int rowIndex = 0; rowIndex < parts.Height; rowIndex++)
            {
                for (int pixelIndex = 0; pixelIndex < parts.Width; pixelIndex++)
                {
                    var argbColorBytes = BitConverter.GetBytes(mapping[chanel[rowIndex * parts.Width + pixelIndex]]);

                    rowData[pixelIndex * samplesPerPixel + 0] = argbColorBytes[1];
                    rowData[pixelIndex * samplesPerPixel + 1] = argbColorBytes[2];
                    rowData[pixelIndex * samplesPerPixel + 2] = argbColorBytes[3];
                }

                if (!tiff.WriteScanline(rowData, rowIndex))
                    throw new IOException(@"Image data were NOT encoded and written successfully!");
            }
        }
    }
}