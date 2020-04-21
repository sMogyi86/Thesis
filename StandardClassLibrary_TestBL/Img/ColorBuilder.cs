using System.Runtime.CompilerServices;

namespace MARGO.BL.Img
{
    public class ColorBuilder
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ConstructLITTLEEndian(byte a, byte r, byte g, byte b)
        {
            int argb = 0;

            argb |= a;
            argb = (argb << 8);
            argb |= r;
            argb = (argb << 8);
            argb |= g;
            argb = (argb << 8);
            argb |= b;

            return argb;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ConstructBIGEndian(byte a, byte r, byte g, byte b)
        {
            int argb = 0;

            argb |= b;
            argb = (argb << 8);
            argb |= g;
            argb = (argb << 8);
            argb |= r;
            argb = (argb << 8);
            argb |= a;

            return argb;
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte AFromLITTLEEndian(int argb) => (byte)(argb >> 24);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte RFromLITTLEEndian(int argb) => (byte)(argb >> 16);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GFromLITTLEEndian(int argb) => (byte)(argb >> 8);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte BFromLITTLEEndian(int argb) => (byte)argb;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte AFromBIGEndian(int argb) => (byte)argb;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte RFromBIGEndian(int argb) => (byte)(argb >> 8);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GFromBIGEndian(int argb) => (byte)(argb >> 16);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte BFromBIGEndian(int argb) => (byte)(argb >> 24);
    }
}