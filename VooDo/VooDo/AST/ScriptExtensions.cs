using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using VooDo.AST.Directives;
using VooDo.AST.Expressions;
using VooDo.AST.Names;
using VooDo.AST.Statements;
using VooDo.Compiling.Emission;
using VooDo.Utils;

namespace VooDo.AST
{

    public static class ScriptExtensions
    {

        public static ImmutableHashSet<Namespace> GetUsingNamespaces(this Script _script)
            => _script.Usings.OfType<UsingNamespaceDirective>().Where(_u => !_u.HasAlias).Select(_u => _u.Namespace).ToImmutableHashSet();

        public static ImmutableDictionary<Identifier, Namespace> GetUsingAliasNamespaces(this Script _script)
            => _script.Usings.OfType<UsingNamespaceDirective>().Where(_u => _u.HasAlias).ToImmutableDictionary(_u => _u.Alias!, _u => _u.Namespace);

        public static ImmutableArray<GlobalPrototype> GetGlobalPrototypes(this Script _script)
            => _script.DescendantNodes(_c => Tree.IsStatementAncestor(_c) || Tree.IsExpressionAncestor(_c), Tree.ETraversal.BreadthFirst)
                .SelectMany(_c => _c switch
                {
                    GlobalExpression expr => new[] { new GlobalPrototype(new Global(false, ComplexTypeOrVar.Var, null, expr.Initializer), expr) },
                    GlobalStatement stat => stat.SelectMany(_d => _d.Declarators.Select(_l => new GlobalPrototype(new Global(stat.IsConstant, _d.Type, _l.Name, _l.Initializer), _l))),
                    _ => Enumerable.Empty<GlobalPrototype>()
                }).ToImmutableArray();

        public static Script AddUsingStaticTypes(this Script _script, params QualifiedType[] _types)
            => AddUsingStaticTypes(_script, (IEnumerable<QualifiedType>) _types);

        public static Script AddUsingStaticTypes(this Script _script, IEnumerable<QualifiedType> _types)
            => _script with
            {
                Usings = _script.Usings.AddRange(_types.Select(_t => new UsingStaticDirective(_t)))
            };

        public static Script AddUsingNamespaces(this Script _script, params Namespace[] _namespaces)
            => AddUsingNamespaces(_script, (IEnumerable<Namespace>) _namespaces);

        public static Script AddUsingNamespaces(this Script _script, IEnumerable<Namespace> _namespaces)
            => AddUsingDirectives(_script, _namespaces.Select(_n => new UsingNamespaceDirective(_n)));

        public static Script AddUsingAliasNamespace(this Script _script, Identifier _alias, Namespace _namespace)
            => AddUsingAliasNamespaces(_script, new[] { new KeyValuePair<Identifier, Namespace>(_alias, _namespace) }.ToImmutableDictionary());

        public static Script AddUsingAliasNamespaces(this Script _script, IDictionary<Identifier, Namespace> _namespaces)
            => AddUsingDirectives(_script, _namespaces.Select(_n => new UsingNamespaceDirective(_n.Key, _n.Value)));

        public static Script AddUsingDirectives(this Script _script, params UsingDirective[] _directives)
            => AddUsingDirectives(_script, (IEnumerable<UsingDirective>) _directives);

        public static Script AddUsingDirectives(this Script _script, IEnumerable<UsingDirective> _directives)
            => _script with
            {
                Usings = _script.Usings.AddRange(_directives)
            };

        public static Script AddGlobals(this Script _script, params Global[] _globals)
            => AddGlobals(_script, (IEnumerable<Global>) _globals);

        public static Script AddGlobals(this Script _script, IEnumerable<Global> _globals)
        {
            if (_globals.Select(_g => _g.Name).AnyNull())
            {
                throw new ArgumentException("Global name cannot be null", nameof(_globals));
            }
            IEnumerable<GlobalStatement> statements = _globals.Select(_g =>
                new GlobalStatement(
                    _g.IsConstant,
                    ImmutableArray.Create(
                    new DeclarationStatement(
                        _g.Type,
                        ImmutableArray.Create(
                            new DeclarationStatement.Declarator(
                                _g.Name!,
                                _g.Initializer))))));
            return _script with
            {
                Statements = _script.Statements.InsertRange(0, statements)
            };
        }
    }

}
