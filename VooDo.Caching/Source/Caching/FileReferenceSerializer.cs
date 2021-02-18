using System.IO;
using System.Runtime.Serialization;

using VooDo.AST.Names;
using VooDo.Compiling;

namespace VooDo.Caching
{

    public sealed class FileReferenceSerializer : ISerializer<Reference>
    {

        public static FileReferenceSerializer Instance { get; } = new FileReferenceSerializer();

        private FileReferenceSerializer() { }


        public Reference Deserialize(BinaryReader _reader)
        {
            string path = _reader.ReadString();
            Identifier[] aliases = new Identifier[_reader.ReadInt32()];
            for (int a = 0; a < aliases.Length; a++)
            {
                aliases[a] = _reader.ReadString();
            }
            return Reference.FromFile(path, aliases);
        }

        public void Serialize(Reference _value, BinaryWriter _writer)
        {
            string? path = _value.FilePath ?? _value.Assembly?.Location;
            if (path is null)
            {
                throw new SerializationException("Cannot serialize a Reference generated from memory");
            }
            _writer.Write(path);
            _writer.Write(_value.Aliases.Count);
            foreach (Identifier alias in _value.Aliases)
            {
                _writer.Write(alias.ToString());
            }
        }

    }

}
