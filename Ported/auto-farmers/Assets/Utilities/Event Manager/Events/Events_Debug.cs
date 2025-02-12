namespace Utilities.EventManager.Events
{
    /// <summary>
    /// Wrapper for all the events for different systems.
    /// Each string value has to be unique!
    /// </summary>
    public partial class Events
    {
        // Each string value has to be unique!
        public struct  Debug
        {
            public const string k_OnDebugingSettingChanged = "OnDebugingSettingChanged";
            //public const string k_OnStartGame = "OnStartGame";
        }
    }
}