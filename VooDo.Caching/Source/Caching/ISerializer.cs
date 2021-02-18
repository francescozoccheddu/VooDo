using System.IO;

namespace VooDo.Caching
{

    public interface ISerializer<TValue>
    {

        void Serialize(TValue _value, BinaryWriter _writer);
        TValue Deserialize(BinaryReader _reader);

    }

}
