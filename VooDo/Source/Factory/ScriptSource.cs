using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

using VooDo.Factory.Syntax;
using VooDo.Parsing;
using VooDo.Transformation;
using VooDo.Utils;

namespace VooDo.Factory
{

    public sealed class ScriptSource : IEquatable<ScriptSource>
    {

        public static Reference RuntimeReference { get; }
            = Reference.FromAssembly(Assembly.GetExecutingAssembly(), Identifiers.referenceAlias);

        public static ScriptSource FromScript(string _source)
        {
            if (_source == null)
            {
                throw new ArgumentNullException(nameof(_source));
            }
            return FromCSharp(Parser.ParseAnyScript(_source));
        }

        public static ScriptSource FromCSharp(string _source)
        {
            if (_source == null)
            {
                throw new ArgumentNullException(nameof(_source));
            }
            return FromCSharp(SyntaxFactory.ParseCompilationUnit(_source));
        }

        public static ScriptSource FromCSharp(CompilationUnitSyntax _syntax)
        {
            if (_syntax == null)
            {
                throw new ArgumentNullException(nameof(_syntax));
            }
            CompilationUnitSyntax source = OriginRewriter.RewriteFromFullSpan(_syntax);
            ImmutableArray<Diagnostic> diagnostics = BodyValidator.GetSyntaxDiagnostics(_syntax);
            if (diagnostics.Length > 0)
            {
                throw new ArgumentException("Invalid code", nameof(_syntax));
            }
            ImmutableHashSet<Namespace> usingDirectives = _syntax.Usings
                .Where(_u => _u.Alias == null && _u.StaticKeyword.IsKind(SyntaxKind.None))
                .Select(_u => Namespace.FromSyntax(_u.Name))
                .ToImmutableHashSet();
            ImmutableHashSet<QualifiedType> usingStaticDirectives = _syntax.Usings
                .Where(_u => _u.Alias == null && _u.StaticKeyword.IsKind(SyntaxKind.StaticKeyword))
                .Select(_u => QualifiedType.FromSyntax(_u.Name))
                .ToImmutableHashSet();
            ImmutableDictionary<Identifier, Namespace> usingAliasDirectives = _syntax.Usings
                .Where(_u => _u.Alias == null && _u.StaticKeyword.IsKind(SyntaxKind.StaticKeyword))
                .ToImmutableDictionary(_u => Identifier.FromSyntax(_u.Alias.Name.Identifier), _u => Namespace.FromSyntax(_u.Name));
            return new ScriptSource(source, ImmutableArray.Create(RuntimeReference), ImmutableArray.Create<Global>(), usingDirectives, usingStaticDirectives, usingAliasDirectives);
        }

        private ScriptSource(
            CompilationUnitSyntax _source,
            ImmutableArray<Reference> _references,
            ImmutableArray<Global> _extraGlobals,
            ImmutableHashSet<Namespace> _usingDirectives,
            ImmutableHashSet<QualifiedType> _usingStaticDirectives,
            ImmutableDictionary<Identifier, Namespace> _usingAliasDirectives)
        {
            if (_source == null)
            {
                throw new ArgumentNullException(nameof(_source));
            }
            if (_references == null)
            {
                throw new ArgumentNullException(nameof(_references));
            }
            if (_extraGlobals == null)
            {
                throw new ArgumentNullException(nameof(_extraGlobals));
            }
            if (_usingDirectives == null)
            {
                throw new ArgumentNullException(nameof(_usingDirectives));
            }
            if (_usingStaticDirectives == null)
            {
                throw new ArgumentNullException(nameof(_usingStaticDirectives));
            }
            if (_usingAliasDirectives == null)
            {
                throw new ArgumentNullException(nameof(_usingAliasDirectives));
            }
            if (_references.AnyNull())
            {
                throw new ArgumentException("Null reference", nameof(_references));
            }
            if (_extraGlobals.AnyNull())
            {
                throw new ArgumentException("Null additional global", nameof(_extraGlobals));
            }
            if (_usingDirectives.AnyNull())
            {
                throw new ArgumentException("Null using directive", nameof(_usingDirectives));
            }
            if (_usingStaticDirectives.AnyNull())
            {
                throw new ArgumentException("Null using static directive", nameof(_usingStaticDirectives));
            }
            if (_usingAliasDirectives.Keys.AnyNull() || _usingAliasDirectives.Values.AnyNull())
            {
                throw new ArgumentException("Null using alias directive key or value", nameof(_usingAliasDirectives));
            }
            if (_extraGlobals.Select(_g => _g.Name).AnyDuplicate())
            {
                throw new ArgumentException("Duplicate global name");
            }
            SourceSyntax = _source;
            References = Reference.Merge(_references);
            m_aliasMap = References
                .SelectMany(_r => _r.Aliases.Select(_a => KeyValuePair.Create(_a, _r)))
                .ToImmutableDictionary();
            Aliases = m_aliasMap.Keys.ToImmutableHashSet();
            ExtraGlobals = _extraGlobals;
            UsingDirectives = _usingDirectives;
            UsingAliasDirectives = _usingAliasDirectives;
            UsingStaticDirectives = _usingStaticDirectives;
        }

        public CompiledScript Compile(QualifiedType _returnType = null)
        {
            if (!HasRuntimeReference)
            {
                throw new InvalidOperationException($"No runtime library reference with alias ${Identifiers.referenceAlias}");
            }
            CSharpCompilationOptions options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            ImmutableArray<MetadataReference> references = References.Select(_r => _r.GetMetadataReference()).ToImmutableArray();
            {
                SyntaxTree tree = CSharpSyntaxTree.Create(TransformedSyntax);
                CSharpCompilation compilation = CSharpCompilation.Create(null, new[] { tree }, references, options);
                SemanticModel semantics = compilation.GetSemanticModel(tree);
                CompilationUnitSyntax rootWithDecl = GlobalVariableDeclarationRewriter.Rewrite(semantics, out ImmutableArray<Global> globals);
                CompilationUnitSyntax rootWithAccess = null;
            }
            return null;
        }

        private readonly ImmutableDictionary<Identifier, Reference> m_aliasMap;
        private CompilationUnitSyntax m_transformedSyntax;

        private CompilationUnitSyntax TransformSource()
        {
            return SourceSyntax
                .WithExterns(Aliases.Select(_a => SyntaxFactory.ExternAliasDirective()))
        }

        public CompilationUnitSyntax SourceSyntax { get; }
        public CompilationUnitSyntax TransformedSyntax
            => m_transformedSyntax ?? (m_transformedSyntax = TransformSource());
        public ImmutableHashSet<Identifier> Aliases { get; }
        public ImmutableArray<Reference> References { get; }
        public ImmutableArray<Global> ExtraGlobals { get; }
        public ImmutableHashSet<Namespace> UsingDirectives { get; }
        public ImmutableHashSet<QualifiedType> UsingStaticDirectives { get; }
        public ImmutableDictionary<Identifier, Namespace> UsingAliasDirectives { get; }
        public bool HasRuntimeReference
            => Reference.AreSameMetadata(GetReferenceByAlias(Identifiers.referenceAlias), RuntimeReference);

        public Reference GetReferenceByAlias(Identifier _alias)
            => m_aliasMap.TryGetValue(_alias, out Reference reference) ? reference : null;

        public ScriptSource WithAdditionalReferences(params Reference[] _references)
            => WithAdditionalReferences((IEnumerable<Reference>) _references);

        public ScriptSource WithAdditionalExtraGlobals(params Global[] _extraGlobals)
            => WithAdditionalExtraGlobals((IEnumerable<Global>) _extraGlobals);

        public ScriptSource WithAdditionalUsingDirectives(params Namespace[] _usingDirectives)
            => WithAdditionalUsingDirectives((IEnumerable<Namespace>) _usingDirectives);

        public ScriptSource WithAdditionalUsingStaticDirectives(params QualifiedType[] _usingStaticDirectives)
            => WithAdditionalUsingStaticDirectives((IEnumerable<QualifiedType>) _usingStaticDirectives);

        public ScriptSource WithAdditionalUsingAliasDirectives(params (Identifier, Namespace)[] _usingAliasDirectives)
            => WithAdditionalUsingAliasDirectives(_usingAliasDirectives.Select(_e => KeyValuePair.Create(_e.Item1, _e.Item2)));

        public ScriptSource WithAdditionalReferences(IEnumerable<Reference> _references)
            => WithReferences(References.AddRange(_references));

        public ScriptSource WithAdditionalExtraGlobals(IEnumerable<Global> _extraGlobals)
            => WithExtraGlobals(ExtraGlobals.AddRange(_extraGlobals));

        public ScriptSource WithAdditionalUsingDirectives(IEnumerable<Namespace> _usingDirectives)
            => WithUsingDirectives(UsingDirectives.Union(_usingDirectives));

        public ScriptSource WithAdditionalUsingStaticDirectives(IEnumerable<QualifiedType> _usingStaticDirectives)
            => WithUsingStaticDirectives(UsingStaticDirectives.Union(_usingStaticDirectives));

        public ScriptSource WithAdditionalUsingAliasDirectives(IEnumerable<KeyValuePair<Identifier, Namespace>> _usingAliasDirectives)
            => WithUsingAliasDirectives(UsingAliasDirectives.AddRange(_usingAliasDirectives));

        public ScriptSource WithReferences(params Reference[] _references)
            => WithReferences((IEnumerable<Reference>) _references);

        public ScriptSource WithExtraGlobals(params Global[] _extraGlobals)
            => WithExtraGlobals((IEnumerable<Global>) _extraGlobals);

        public ScriptSource WithUsingDirectives(params Namespace[] _usingDirectives)
            => WithUsingDirectives((IEnumerable<Namespace>) _usingDirectives);

        public ScriptSource WithUsingStaticDirectives(params QualifiedType[] _usingStaticDirectives)
            => WithUsingStaticDirectives((IEnumerable<QualifiedType>) _usingStaticDirectives);

        public ScriptSource WithUsingAliasDirectives(params (Identifier, Namespace)[] _usingAliasDirectives)
            => WithUsingAliasDirectives(_usingAliasDirectives.Select(_e => KeyValuePair.Create(_e.Item1, _e.Item2)));

        public ScriptSource WithReferences(IEnumerable<Reference> _references)
            => new ScriptSource(SourceSyntax, _references.ToImmutableArray(), ExtraGlobals, UsingDirectives, UsingStaticDirectives, UsingAliasDirectives);

        public ScriptSource WithExtraGlobals(IEnumerable<Global> _extraGlobals)
            => new ScriptSource(SourceSyntax, References, _extraGlobals.ToImmutableArray(), UsingDirectives, UsingStaticDirectives, UsingAliasDirectives);

        public ScriptSource WithUsingDirectives(IEnumerable<Namespace> _usingDirectives)
            => new ScriptSource(SourceSyntax, References, ExtraGlobals, _usingDirectives.ToImmutableHashSet(), UsingStaticDirectives, UsingAliasDirectives);

        public ScriptSource WithUsingStaticDirectives(IEnumerable<QualifiedType> _usingStaticDirectives)
            => new ScriptSource(SourceSyntax, References, ExtraGlobals, UsingDirectives, _usingStaticDirectives.ToImmutableHashSet(), UsingAliasDirectives);

        public ScriptSource WithUsingAliasDirectives(IEnumerable<KeyValuePair<Identifier, Namespace>> _usingAliasDirectives)
            => new ScriptSource(SourceSyntax, References, ExtraGlobals, UsingDirectives, UsingStaticDirectives, _usingAliasDirectives.ToImmutableDictionary());

        public override int GetHashCode()
            => Identity.CombineHash(
                SourceSyntax,
                Identity.CombineHashes(References),
                Identity.CombineHashes(ExtraGlobals),
                Identity.CombineHashes(UsingDirectives),
                Identity.CombineHashes(UsingStaticDirectives),
                Identity.CombineHashes(UsingAliasDirectives));

        public override bool Equals(object _obj) => Equals(_obj as ScriptSource);
        public bool Equals(ScriptSource _other) => _other != null
            && SourceSyntax.IsEquivalentTo(_other.SourceSyntax)
            && References.Equals(_other.References)
            && ExtraGlobals.Equals(_other.ExtraGlobals)
            && UsingDirectives.SetEquals(_other.UsingDirectives)
            && UsingStaticDirectives.SetEquals(_other.UsingStaticDirectives)
            && UsingAliasDirectives.SequenceEqual(_other.UsingAliasDirectives);

        public static bool operator ==(ScriptSource _left, ScriptSource _right) => Identity.AreEqual(_left, _right);
        public static bool operator !=(ScriptSource _left, ScriptSource _right) => !(_left == _right);

    }

}
