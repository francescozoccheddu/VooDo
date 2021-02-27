using VooDo.Runtime.Implementation;

namespace VooDo.Utils
{
    internal abstract class Identifiers : TypedProgram<object>
    {

        internal const string runMethodName = nameof(__VooDo_Reserved_Run);
        internal const string typedRunMethodName = nameof(__VooDo_Reserved_TypedRun);
        internal const string tagPrefix = __VooDo_Reserved_tagPrefix;
        internal const string generatedHooksName = nameof(__VooDo_Reserved_GeneratedHooks);
        internal const string generatedVariablesName = nameof(__VooDo_Reserved_GeneratedVariables);
        internal const string subscribeHookMethodName = nameof(__VooDo_Reserved_SubscribeHook);
        internal const string scriptPrefix = __VooDo_Reserved_scriptPrefix;
        internal const string eventPollMethodPrefix = __VooDo_Reserved_eventPollMethodPrefix;
        internal const string eventSubscribePrefix = __VooDo_Reserved_eventSubscribeMethodPrefix;
        internal const string globalPrefix = __VooDo_Reserved_globalPrefix;
        internal const string reservedPrefix = __VooDo_Reserved_reservedPrefix;
        internal const string setControllerAndGetValueMethodName = nameof(__VooDo_Reserved_SetControllerAndGetValue);
        internal const string createVariableMethodName = nameof(__VooDo_Reserved_CreateVariable);

        private Identifiers()
        { }

    }
}
