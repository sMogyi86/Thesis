using System.Runtime.CompilerServices;

namespace MARGO.BL.Img
{
    public class ColorBuilder
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ConstructLITTLEEndian(byte a, byte r, byte g, byte b)
        {
            uint argb = 0;

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
        public uint ConstructBIGEndian(byte a, byte r, byte g, byte b)
        {
            uint argb = 0;

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
        public byte AFromLITTLEEndian(uint argb) => (byte)(argb >> 24);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte RFromLITTLEEndian(uint argb) => (byte)(argb >> 16);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GFromLITTLEEndian(uint argb) => (byte)(argb >> 8);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte BFromLITTLEEndian(uint argb) => (byte)argb;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte AFromBIGEndian(uint argb) => (byte)argb;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte RFromBIGEndian(uint argb) => (byte)(argb >> 8);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GFromBIGEndian(uint argb) => (byte)(argb >> 16);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte BFromBIGEndian(uint argb) => (byte)(argb >> 24);
    }
}