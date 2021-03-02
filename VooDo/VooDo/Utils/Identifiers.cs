using VooDo.Runtime.Implementation;

namespace VooDo.Utils
{
    internal abstract class Identifiers : TypedProgram<object>
    {

        private abstract class SpyEventHook : __VooDo_Reserved_EventHook
        {

            private SpyEventHook() { }

            internal const string notifyMethodName = nameof(__VooDo_Reserved_Notify);
            internal const string setSubscribedMethodName = nameof(__VooDo_Reserved_SetSubscribed);
            internal const string onEventMethodName = __VooDo_Reserved_onEventMethodName;

        }

        internal const string eventHookNotifyMethodName = SpyEventHook.notifyMethodName;
        internal const string eventHookOnEventMethodName = SpyEventHook.onEventMethodName;
        internal const string eventHookSetSubscribedMethodName = SpyEventHook.setSubscribedMethodName;
        internal const string notifyMethodName = SpyEventHook.notifyMethodName;
        internal const string runMethodName = nameof(__VooDo_Reserved_Run);
        internal const string typedRunMethodName = nameof(__VooDo_Reserved_TypedRun);
        internal const string tagPrefix = __VooDo_Reserved_tagPrefix;
        internal const string generatedHooksName = nameof(__VooDo_Reserved_GeneratedHooks);
        internal const string generatedEventHooksName = nameof(__VooDo_Reserved_GeneratedEventHooks);
        internal const string generatedVariablesName = nameof(__VooDo_Reserved_GeneratedVariables);
        internal const string subscribeHookMethodName = nameof(__VooDo_Reserved_SubscribeHook);
        internal const string scriptPrefix = __VooDo_Reserved_scriptPrefix;
        internal const string eventHookClassPrefix = __VooDo_Reserved_eventHookClassPrefix;
        internal const string eventHookClassName = nameof(__VooDo_Reserved_EventHook);
        internal const string subscribeEventMethodName = nameof(__VooDo_Reserved_SubscribeEvent);
        internal const string globalPrefix = __VooDo_Reserved_globalPrefix;
        internal const string reservedPrefix = __VooDo_Reserved_reservedPrefix;
        internal const string setControllerAndGetValueMethodName = nameof(__VooDo_Reserved_SetControllerAndGetValue);
        internal const string createVariableMethodName = nameof(__VooDo_Reserved_CreateVariable);

        private Identifiers()
        { }

    }
}
