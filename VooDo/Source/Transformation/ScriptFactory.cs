using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using VooDo.Transformation;

namespace VooDo.Source.Transformation
{

    public sealed class ScriptFactory
    {

        public Type Type { get; }

        public ScriptFactory(Type _type)
        {
            if (_type is null)
            {
                throw new ArgumentNullException(nameof(_type));
            }
            if (!typeof(Script).IsAssignableFrom(_type))
            {
                throw new ArgumentException("Not a script type", nameof(_type));
            }
            if (_type.GetConstructor(Type.EmptyTypes) is null)
            {
                throw new ArgumentException("Script does not have a parameterless constructor", nameof(_type));
            }
            Type = _type;
        }

        public static ScriptFactory[] Create(CSharpCompilation _compilation)
        {
            if (_compilation.GetDiagnostics().Any(_d => _d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error))
            {
                throw new ArgumentException("Compilation has errors", nameof(_compilation));
            }
            Assembly assembly;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                EmitResult emitResult = _compilation.Emit(memoryStream);
                if (!emitResult.Success)
                {
                    throw new Exception("Emit failed");
                }
                assembly = Assembly.Load(memoryStream.ToArray());
            }
            Type baseType = typeof(Script);
            IEnumerable<Type> types = assembly.GetTypes().Where(_t => baseType.IsAssignableFrom(_t));
            return types.Select(_t => new ScriptFactory(_t)).ToArray();
        }

        public static ScriptFactory CreateSingle(CSharpCompilation _compilation)
        {
            ScriptFactory[] factories = Create(_compilation);
            if (factories.Length == 0)
            {
                throw new Exception("No script has been compiled");
            }
            if (factories.Length > 1)
            {
                throw new Exception("Multiple scripts have been compiled");
            }
            return factories[0];
        }

        public Script Create() => (Script) Activator.CreateInstance(Type);

    }

}
