﻿using System;
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
    internal class RunEvent : ICommand
    {
        public string Command => "ev_run";

        public string[] Aliases => null;

        public string Description => "Запускает ивент, берёт на себя 1 аргумент - командное название ивента.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player admin = Player.Get((sender as CommandSender).SenderId);
            if (!Plugin.CustomConfig.DonatorGroups.Contains(admin.GroupName))
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
            if (arguments.Count != 1)
            {
                response = $"Необходим только 1 аргумент - командное название ивента!";
            }
            var arr = GetTypesInNamespace(Assembly.GetExecutingAssembly(), "AutoEvent.Events");
            foreach (var type in arr)
            {
                if (type.GetProperty("CommandName") != null)
                {
                    var ev = Activator.CreateInstance(type);
                    try
                    {
                        if ((string)type.GetProperty("CommandName").GetValue(ev) == arguments.ElementAt(0)) //This is unsafe, but working with unknown data has it's drawbacks.
                        {
                            var eng = type.GetMethod("OnStart");
                            if (eng != null)
                            {
                                sender.Respond("Пытаюсь запустить ивент, OnStart не null...");
                                eng.Invoke(Activator.CreateInstance(type), null);
                                Round.Lock = true;
                                response = "Ивент найден, запускаю.";
                                return true;
                            }
                            response = "eng оказался нуллом. Каким-то образом, класс который был выбран не имеет в себе OnStart()";
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        response = $"Произошла ошибка при запуске ивента. Ошибка: {ex.Message}";
                    }
                }
            }
            response = "Ивент не найден, ничего не произошло.";
            return false;
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
