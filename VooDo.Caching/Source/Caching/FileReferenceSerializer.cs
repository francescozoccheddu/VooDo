using System.Runtime.Serialization;

using VooDo.Compiling;

namespace VooDo.Caching
{

    public sealed class FileReferenceSerializer : ISerializer<Reference>
    {

        public static FileReferenceSerializer Instance { get; } = new FileReferenceSerializer();

        private FileReferenceSerializer() { }

        public Reference Deserialize(string _serializedValue)
            => Reference.FromFile(_serializedValue);

        public string Serialize(Reference _value)
        {
            string? path = _value.FilePath ?? _value.Assembly?.Location;
            if (path is null)
            {
                throw new SerializationException("Cannot serialize a Reference generated from memory");
            }
            return path;
        }

    }

}
