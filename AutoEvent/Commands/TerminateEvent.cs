using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AutoEvent.Functions;
using CommandSystem;
using Qurre.API;

namespace AutoEvent.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class TerminateEvent : ICommand
    {
        public string Command => "ev_stop";

        public string[] Aliases => null;

        public string Description => "Принудительно останавливает активный ивент.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player admin = Player.Get((sender as CommandSender).SenderId);
            if (Plugin.CustomConfig.DonatorGroups.Contains(admin.GroupName))
            {
                response = $"<color=red>Вы не можете это использовать!</color>";
                return false;
            }
            if (!Round.Started)
            {
                response = $"Раунд ещё не начался!";
                return false;
            }
            if (Plugin.ActiveEvent == null)
            {
                response = $"Мини-игра не проводится!";
                return false;
            }
            if (arguments.Count != 0)
            {
                response = $"Лишние аргументы!";
                return false;
            }
            Plugin.ActiveEvent.OnStop();
            Plugin.ActiveEvent = null;
            response = "Мы попросили ивент сделать суицид.";
            return true;
        }

        static private Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            return
              assembly.GetTypes()
                      .Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal))
                      .ToArray();
        }
    }
}
