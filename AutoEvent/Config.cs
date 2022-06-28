using Qurre.API.Addons;
using System.ComponentModel;

namespace AutoEvent
{
    public class Config : IConfig
    {
        [Description("Plugin Name")]
        public string Name { get; set; } = "AutoEvent";

        [Description("Enable the plugin?")]
        public bool IsEnable { get; set; } = true;
        [Description("Запись количества прошедших раундов")]
        public int RoundCount { get; set; } = 0;
    }
}