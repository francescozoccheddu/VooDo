
using System;

using VooDo.Runtime;

namespace VooDoTB
{

    public readonly struct Binding
    {

        public Binding(Env.Binding _source)
        {
            Value = _source.Eval.Value?.ToString() ?? "null";
            Type = _source.Eval.Type?.Name ?? "null";
            Name = _source.Name;
        }

        public Binding(Exception _exception)
        {
            Value = _exception.Message;
            Type = _exception.GetType().Name;
            Name = "Exception";
        }

        public string Value { get; }
        public string Type { get; }
        public string Name { get; }

    }

}
