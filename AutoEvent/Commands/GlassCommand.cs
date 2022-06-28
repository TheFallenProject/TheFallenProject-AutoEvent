using System;
using System.Linq;
using CommandSystem;
using Qurre.API;

namespace AutoEvent.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class GlassCommand : ICommand
    {
        public string Command => "ev_glass";
        public string[] Aliases => new string[] { };
        public string Description => "Создать авто-ивент Прыжок Веры: ev_glass";
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
            new Glass().OnStart();
            response = $"Игра началась";
            return true;
        }
    }
}
