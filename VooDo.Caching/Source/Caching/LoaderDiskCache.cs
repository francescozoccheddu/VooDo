
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using VooDo.AST;
using VooDo.AST.Names;
using VooDo.Compiling;
using VooDo.Hooks;
using VooDo.Runtime;

namespace VooDo.Caching
{

    public sealed class LoaderDiskCache : ILoaderCache
    {

        private readonly struct Value
        {

            internal Value(Loader _loader, byte[] _assembly)
            {
                Loader = _loader;
                Assembly = _assembly;
            }

            public Loader Loader { get; }
            public byte[] Assembly { get; }

        }

        private readonly Dictionary<LoaderKey, Value> m_cache = new Dictionary<LoaderKey, Value>(64);
        private readonly IScriptCache m_scriptCache = new ScriptMemoryCache();
        private const int c_maxCount = 512;

        public ISerializer<Reference> ReferenceSerializer { get; }
        public ISerializer<IHookInitializer> HookInitializerSerializer { get; }

        public static void Delete(string _filePath)
            => File.Delete(_filePath);

        public string FilePath { get; }

        public LoaderDiskCache(string _filePath)
            : this(_filePath, FileReferenceSerializer.Instance, HookInitializerTypeNameSerializer.Instance) { }

        public LoaderDiskCache(string _filePath, ISerializer<Reference> _referenceSerializer, ISerializer<IHookInitializer> _hookInitializerSerializer)
        {
            FilePath = _filePath;
            ReferenceSerializer = _referenceSerializer;
            HookInitializerSerializer = _hookInitializerSerializer;
            if (File.Exists(_filePath))
            {
                Load();
            }
            else
            {
                Save();
            }
        }

        private IHookInitializer DeserializeHookInitializer(BinaryReader _reader)
        {
            int count = _reader.ReadInt32();
            if (count == -1)
            {
                return HookInitializerSerializer.Deserialize(_reader);
            }
            else
            {
                IHookInitializer[] children = new IHookInitializer[count];
                for (int i = 0; i < count; i++)
                {
                    children[i] = DeserializeHookInitializer(_reader);
                }
                return new HookInitializerList(children);
            }
        }

        private void SerializeHookInitializer(IHookInitializer _provider, BinaryWriter _writer)
        {
            if (_provider is HookInitializerList list)
            {
                _writer.Write(list.Count);
                foreach (IHookInitializer child in list)
                {
                    SerializeHookInitializer(child, _writer);
                }
            }
            else
            {
                _writer.Write(-1);
                HookInitializerSerializer.Serialize(_provider, _writer);
            }
        }

        public void Save()
        {
            using BinaryWriter writer = new BinaryWriter(File.Open(FilePath, FileMode.Create));
            writer.Write(m_cache.Count);
            foreach ((LoaderKey key, Value value) in m_cache)
            {
                writer.Write(key.scriptCode);
                writer.Write(key.returnTypeCode ?? "void");
                SerializeHookInitializer(key.HookInitializer, writer);
                writer.Write(key.References.Count);
                foreach (Reference reference in key.References)
                {
                    ReferenceSerializer.Serialize(reference, writer);
                }
                writer.Write(value.Assembly.Length);
                writer.Write(value.Assembly);
            }
        }

        public void Load()
        {
            using BinaryReader reader = new BinaryReader(File.Open(FilePath, FileMode.Open));
            int count = Math.Min(reader.ReadInt32(), c_maxCount);
            while (count-- > 0)
            {
                string scriptCode = reader.ReadString();
                string returnTypeCode = reader.ReadString();
                IHookInitializer hookInitializer = DeserializeHookInitializer(reader);
                Reference[] references = new Reference[reader.ReadInt32()];
                for (int r = 0; r < references.Length; r++)
                {
                    references[r] = ReferenceSerializer.Deserialize(reader);
                }
                byte[] assembly = reader.ReadBytes(reader.ReadInt32());
                Script script = m_scriptCache.GetOrParseScript(scriptCode);
                ComplexType? returnType = returnTypeCode == "void" ? null : ComplexType.Parse(returnTypeCode);
                LoaderKey key = LoaderKey.Create(script, references, returnType, hookInitializer);
                if (!m_cache.ContainsKey(key))
                {
                    m_cache[key] = new Value(Loader.FromAssembly(Assembly.Load(assembly)), assembly);
                }
            }
        }

        public void Clear(bool _keepOnDisk = false)
        {
            m_cache.Clear();
            if (!_keepOnDisk)
            {
                Delete(FilePath);
            }
        }

        public void Clear(LoaderKey _key)
            => m_cache.Remove(_key);

        public Loader GetOrCreateLoader(LoaderKey _key)
        {
            if (m_cache.TryGetValue(_key, out Value cached))
            {
                return cached.Loader;
            }
            else
            {
                byte[] assembly = Compilation.SucceedOrThrow(_key.Script, _key.CreateMatchingOptions()).EmitRawAssembly();
                Loader? loader = Loader.FromAssembly(Assembly.Load(assembly));
                m_cache.Add(_key, new Value(loader, assembly));
                Save();
                return loader;
            }
        }

    }

}
