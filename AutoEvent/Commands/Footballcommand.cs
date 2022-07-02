using System;
using System.Linq;
using CommandSystem;
using Qurre.API;

namespace AutoEvent.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Footballcommand : ICommand
    {
        public string Command => "ev_football";
        public string[] Aliases => new string[] { };
        public string Description => "Создать авто-ивент Футбол: ev_football";
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player admin = Player.Get((sender as CommandSender).SenderId);
            if (Plugin.DonatorGroups.Contains(admin.GroupName))
            {
                response = $"<color=red>Вы не можете это использовать!</color>";
                return false;
            }
            if (!Round.Started)
            {
                response = $"Раунд ещё не начался!";
                return false;
            }
            if (Plugin.IsEventRunning)
            {
                response = $"Мини-Игра уже проводится!";
                return false;
            }
            Functions.MainFunctions.StartEventParametres();
            new FootballEvent().OnStart();
            response = $"Игра началась";
            return true;
        }
    }
}
