using LczArmoryDoorBlock.Configs;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;

namespace LczArmoryDoorBlock
{
    public sealed class Plugin
    {
        [PluginConfig]
        public Config Config;

        [PluginEntryPoint("LczArmoryDoorBlock", "1.0.0", "[REDACTED]", "Memento Mori ~ Dev")]
        public void Run()
        {
            if (!Config.IsEnabled)
            {
                return;
            }

            EventManager.RegisterEvents<EventHandlers>(this);
        }
    }
}
