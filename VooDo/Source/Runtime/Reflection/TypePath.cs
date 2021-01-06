﻿using System;
using System.Collections.Generic;
using System.Linq;

using VooDo.AST;
using VooDo.Runtime.Meta;
using VooDo.Utils;

namespace VooDo.Runtime.Reflection
{

    public sealed class TypePath : IMemberProvider, IGeneric
    {

        public TypePath(params Name[] _path) : this((IEnumerable<Name>) _path)
        { }

        public TypePath(IEnumerable<Name> _path)
        {
            Ensure.NonNull(_path, nameof(_path));
            Path = _path.ToList().AsReadOnly();
            if (Path.Count == 0)
            {
                throw new ArgumentException("Empty path", nameof(_path));
            }
            Ensure.NonNullItems(Path, nameof(_path));
        }

        public IReadOnlyList<Name> Path { get; }

        public string QualifiedName => string.Join('.', Path);

        public sealed override bool Equals(object _obj) => _obj is TypePath ns && Path.SequenceEqual(ns.Path);

        public sealed override int GetHashCode() => Identity.CombineHash(Path.ToArray());

        public sealed override string ToString() => QualifiedName;

        public TypePath Child(Name _name)
        {
            Ensure.NonNull(_name, nameof(_name));
            return new TypePath(Path.Concat(new Name[] { _name }));
        }

        public Type AsGenericTypeDefinition(int _argumentsCount)
        {
            if (_argumentsCount > 0)
            {
                return Type.GetType($"{QualifiedName}`{_argumentsCount}");
            }
            else
            {
                throw new ArgumentException("Non-positive arguments count", nameof(_argumentsCount));
            }
        }

        public Type AsGenericType(Type[] _arguments)
        {
            Ensure.NonNull(_arguments, nameof(_arguments));
            Ensure.NonNullItems(_arguments, nameof(_arguments));
            Type definition = AsGenericTypeDefinition(_arguments.Length);
            return definition.MakeGenericType(_arguments);
        }

        Eval IGeneric.Specialize(Env _env, Type[] _arguments)
        {
            Ensure.NonNull(_arguments, nameof(_arguments));
            Ensure.NonNullItems(_arguments, nameof(_arguments));
            return new Eval(new TypeWrapper(AsGenericType(_arguments)));
        }

        Eval IMemberProvider.EvaluateMember(Env _env, Name _name)
        {
            Ensure.NonNull(_name, nameof(_name));
            TypePath path = Child(_name);
            Type type = path.AsType;
            return new Eval(type != null ? (object) new TypeWrapper(type) : path);
        }

        public Type AsType => Type.GetType(QualifiedName);

    }

}
