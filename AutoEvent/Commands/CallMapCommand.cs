using System;
using System.Linq;
using CommandSystem;
using MEC;
using Qurre.API;
using Qurre.API.Addons.Models;
using UnityEngine;

namespace AutoEvent.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class CallMapCommand : ICommand
    {
        public string Command => "ev_map";
        public string[] Aliases => new string[] { };
        public string Description => "Вызвать карту из мини-игр: ev_map [Name.json] или [list] или [delete]";
        public Model Model { get; set; } = new Model("custom_map", new Vector3(0, 0, 0));
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
            if (arguments.Count != 1)
            {
                response = $"Введите название карты!";
                return false;
            }
            if (arguments.At(0) == "list")
            {
                response = $"Все карты: \n" +
                    $" - Zombie.json - Замок из Зомби Режима\n" +
                    $" - Battle.json - Мясная Заруба двух команд\n" +
                    $" - Bounce.json - 2 Команды из Вышибал\n" +
                    $" - Jail.json - карта Тюрьма Саймона\n" +
                    $" - Death.json - Большая плоская карта\n" +
                    $" - Glass.json - Длинная карта, но без поверхностей\n" +
                    $" - Lava.json - Карта на подобии Пабга, но без лавы\n" +
                    $" - Parkour.json - Паркур, чтобы люди залезали наверх\n";
                return true;
            }
            if (Model.Primitives.Count > 0)
            {
                Timing.RunCoroutine(Functions.MainFunctions.DestroyObjects(Model));
                if (arguments.At(0) == "delete")
                {
                    response = "Кастомная карта была удалена!";
                    return true;
                }
            }
            string name = arguments.At(0);
            Functions.MainFunctions.CreatingMapFromJson(name, new Vector3(140f, 930f, -122.97f), out var model);
            Model = model;
            response = $"<color=green>Карта создалась на улице (за стеной МТФ спавна)</color>";
            return true;
        }
    }
}
