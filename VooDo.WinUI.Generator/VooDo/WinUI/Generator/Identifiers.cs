namespace VooDo.WinUI.Generator
{

    public static class Identifiers
    {

        private const string c_thisVariableName = "this";
        private const string c_rootVariableName = "root";

        public static class PropertyScripts
        {
            public const string scriptPrefix = "PropertyScript_";
            public const string thisVariableName = c_thisVariableName;
            public const string rootVariableName = c_rootVariableName;
            public const string propertyTag = "Property";
            public const string objectTag = "Object";
            public const string codeTag = "Code";
            public const string pathTag = "Path";
            public const string xamlPathTag = "XamlPath";
            internal const string markupObjectName = "Property";
            internal const string markupCodePropertyName = "Code";
            internal const string markupPathPropertyName = "Path";
            internal const string markupObjectNamespace = "VooDo.WinUI.Xaml";
            internal const string attachmentPropertyName = "Binding";
            internal const string attachmentPropertyOwnerName = "Object";
            internal const string attachmentPropertyOwnerNamespace = "VooDo.WinUI.Xaml";
        }

        public static class ClassScripts
        {
            public const string scriptPrefix = "ClassScript_";
            public const string scriptPathTag = "ScriptPath";
            public const string tagTag = "Tag";
            public const string thisVariableName = c_thisVariableName;
            internal const string tagOption = "Tag";
            internal const string xamlClassOption = "XamlClass";
            internal const string xamlPathOption = "XamlPath";
        }

        internal const string dependencyPropertyHookName = "DependencyPropertyHook";
        internal const string notifyPropertyChangedHookName = "NotifyPropertyChangedHook";
        internal const string hooksNamespace = "VooDo.WinUI.Hooks";
        internal const string vooDoWinUiName = "VooDo.WinUI";
        internal const string winUiName = "Microsoft.WinUI";
        internal const string dependencyObjectFullName = "Microsoft.UI.Xaml.DependencyObject";
        internal const string usingsOption = "VooDoUsings";
        internal const string xamlPresentationBaseNamespace = "Microsoft.UI.Xaml";
        internal const string xamlPresentationNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        internal const string xamlUsingNamespacePrefix = "using:";
        internal const string xamlFileExtension = ".xaml";
        internal const string scriptFileExtension = ".voodo";

    }

}
