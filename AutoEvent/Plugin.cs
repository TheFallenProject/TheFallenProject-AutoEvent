using Qurre;
using System;
using Qurre.Events;
using Qurre.API.Events;
using System.Collections.Generic;
using System.Linq;
using SchematicUnity;
using UnityEngine;
using MEC;
using System.IO;
using Qurre.API;
using HarmonyLib;
using Round = Qurre.API.Round;
using Server = Qurre.API.Server;
using Map = Qurre.API.Map;

namespace AutoEvent
{
    public class Plugin : Qurre.Plugin
    {
        public override string Developer => "KoT0XleB#4663 | TreesHold | AlexanderK";
        public override string Name => "AutoEvent";
        public override Version Version => new Version(2, 0, 0);
        public override void Enable() => RegisterEvents();
        public override void Disable() => UnregisterEvents();
        public override int Priority => int.MaxValue;
        // Проводить ли в этом раунде ивенты?
        public static bool NeedDoLobby = false;

        /// <summary>
        /// PLEASE REMEMBER TO SET IT TO FALSE
        /// </summary>
        public static bool IsEventRunning = false;
        public static Config CustomConfig { get; set; }
        public void RegisterEvents()
        {
            CustomConfig = new Config();
            CustomConfigs.Add(CustomConfig);
            if (!CustomConfig.IsEnable) return;

            Qurre.Events.Round.Start += OnRoundStarted;
            if (NeedDoLobby)
            {
                Qurre.Events.Player.Join += OnJoin;
            }
        }
        public void UnregisterEvents()
        {
            CustomConfigs.Remove(CustomConfig);
            if (!CustomConfig.IsEnable) return;

            Qurre.Events.Round.Start -= OnRoundStarted;
            if (NeedDoLobby)
            {
                Qurre.Events.Player.Join -= OnJoin;
            }
        }
        public void DoLobby()
        {
            string configPath = Path.Combine(Qurre.PluginManager.CustomConfigsDirectory, $"AutoEvent-{Qurre.API.Server.Port}.yaml");
            char[] file = File.ReadAllText(configPath).ToCharArray();

            for (int i = 0; i < file.Length; i++)
            {
                if (char.IsDigit(file[i]))
                {
                    var value = int.Parse(file[i].ToString());
                    if (value < 3) // 0 1 2 > 3 < 0
                    {
                        // Прибавляем единицу
                        value++;
                        // АвтоИвент выключен
                        NeedDoLobby = false;
                    }
                    else
                    {
                        // Обнуляем счетчик
                        value = 5; // <-
                        // Включаем АвтоИвент
                        NeedDoLobby = true;
                        // Инициализируем менеджер ивентов
                        //EventManager.Init();
                        // Изменяем параметры сервера на блокировку раунда
                        AutoEvent.Functions.MainFunctions.StartEventParametres();
                    }
                    file[i] = Convert.ToChar(value.ToString());
                }
            }
            File.WriteAllText(configPath, new string(file));
        }
        /// <summary>При запуске раунда</summary>
        public void OnRoundStarted()
        {
            //DoLobby();
        }
        /// <summary>При подключении нового игрока</summary>
        public void OnJoin(JoinEvent ev)
        {
            if (Round.Started) // EventManager.CurrentEvent == null
            {
                ev.Player.Role = RoleType.ClassD;
                Timing.CallDelayed(2f, () =>
                {
                    ev.Player.Position = EventManager.LobbyPosition + new Vector3(0, 6.67f, 0);
                    return;
                });
            }
        }
        public static List<string> DonatorGroups = new List<string>()
        {
            "owner",
            "gladcat",
            "admin",
            "vip"
        };
    }
}