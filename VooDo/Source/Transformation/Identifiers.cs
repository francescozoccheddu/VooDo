
namespace VooDo.Transformation
{

    public static class Identifiers
    {

        private const string c_prefix = "VooDo_";
        public const string referenceAlias = c_prefix + "VooDo";
        public const string controllerOfMacro = c_prefix + "controllerof";
        public const string globalVariableType = c_prefix + "global";
        public const string globalsClass = c_prefix + "Globals";
        public const string globalsField = c_prefix + "globals";
        public const string globalVariableFormat = c_prefix + "variable_{0}";
        public const string eventDataClass = c_prefix + "EventData";
        public const string eventHookBaseClass = c_prefix + "GenericEventHook";
        public const string eventHookClassFormat = c_prefix + "EventHook_{0}";
        public const string eventHookFireMethod = c_prefix + "Fire";
        public const string getEventMethodFormat = c_prefix + "GetEvent_{0}";
        public const string localFormat = c_prefix + "local_{0}";

    }

}
