<img align="left" width="60" height="60" src="Icon\Icon.png" alt="VooDo Icon">

# VooDo

Created by [Francesco Zoccheddu](https://github.com/francescozoccheddu)
## Requirements
Work in progress… 🤥
## Installation
Work in progress… 🤥
## Usage
Work in progress… 🤥
## Limitations & future work
- `ImplicitGlobalTypeRewriter` fails if the type depends on another unresolved type.
- `EventHookRewriter` fails if the event's expression depends on another event access.
- `PropertyScriptGenerator` crashes when counting lines of code to report diagnostics.
- `PropertyScriptGenerator` does not support aliased or generic types yet.
- `PropertyBinding` lifecycle for `FrameworkElement` properties cannot be entirely controlled.
- `PropertyBinding`'s `Setter` should use `Reflection.Emit` or `DependencyObject.SetValue`.
- Conditional access syntax is not supported yet.
- Partial tuple deconstruction is not supported yet.
- String interpolation is not supported yet.
- Switch statement is not supported yet.
- Switch expression is not supported yet.
- C#9 patterns are not supported yet.
- Generators should allow custom `IHookInitializer`.
- Should use `Problem` instead of standard exceptions.
- Syntax tagging should be improved.
- `EventHookRewriter` does not support arguments yet.
- `_existingCompilation` argument is not validated.
- Rewriters do not always use `Session`'s `CancelationToken`.
- Roslyn should not be directly exposed in `HookInitializer` and `ReferenceFinder`.
- Ensure that converting the syntax tree to a string does not change the semantics.
- Enable multithreading support by using thread local `AnimatorFactory`, `BindingManager` and caches.
- Code quality sucks.