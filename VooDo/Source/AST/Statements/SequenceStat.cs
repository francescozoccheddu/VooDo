using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using VooDo.Runtime;
using VooDo.Source.Utils;
using VooDo.Utils;

namespace VooDo.AST.Statements
{
    public sealed class SequenceStat : Stat
    {

        internal SequenceStat(params Stat[] _stats) : this((IEnumerable<Stat>) _stats)
        {

        }

        internal SequenceStat(IEnumerable<Stat> _stats)
        {
            Ensure.NonNull(_stats, nameof(_stats));
            Ensure.NonNullItems(_stats, nameof(_stats));
            Statements = new ReadOnlyCollection<Stat>(_stats.ToArray());
        }

        public IReadOnlyList<Stat> Statements { get; }

        #region Stat

        internal sealed override void Run(Env _env)
        {
            foreach (Stat stat in Statements)
            {
                stat.Run(_env);
            }
        }

        public override void Unsubscribe(HookManager _hookManager)
        {
            foreach (Stat stat in Statements)
            {
                stat.Unsubscribe(_hookManager);
            }
        }

        internal override HashSet<Name> GetVariables()
            => Tree.GetVariables(Statements).ToHashSet();

        #endregion

        #region ASTBase

        public sealed override string Code => $"{{\n{string.Join('\n', Statements.Select(_s => _s.IndentedCode()))}\n}}";


        public sealed override bool Equals(object _obj)
            => _obj is SequenceStat stat && Statements.Equals(stat.Statements);

        public sealed override int GetHashCode()
            => Statements.GetHashCode();

        #endregion

    }
}
