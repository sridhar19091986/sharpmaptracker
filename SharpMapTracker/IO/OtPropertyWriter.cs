using System.IO;
using System.Text;
using SharpTibiaProxy.Domain;

namespace SharpMapTracker.IO
{
    public class OtPropertyWriter : BinaryWriter
    {
        public OtPropertyWriter(Stream stream)
            : base(stream) { }

        public override void Write(string value)
        {
            Write((ushort)value.Length);
            Write(Encoding.Default.GetBytes(value));
        }

        public void Write(Location location)
        {
            Write((ushort)location.X);
            Write((ushort)location.Y);
            Write((byte)location.Z);
        }
    }
}
