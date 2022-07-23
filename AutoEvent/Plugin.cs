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
using AutoEvent.Functions;

namespace AutoEvent
{
    public class Plugin : Qurre.Plugin
    {
        public override string Developer => "KoT0XleB#4663 $ TreesHold $ AlexanderK $ ГIеJIbмeнь#3519";
        public override string Name => "AutoEvent";
        public override Version Version => new Version(2, 0, 0);
        public override void Enable() => RegisterEvents();
        public override void Disable() => UnregisterEvents();
        public override int Priority => int.MaxValue;
        public static bool NeedDoLobby = false;
        public static bool IsEventRunning = false;
        public static Config CustomConfig { get; set; }
        public void RegisterEvents()
        {
            CustomConfig = new Config();
            CustomConfigs.Add(CustomConfig);
            if (!CustomConfig.IsEnable) return;

            Qurre.Events.Round.Start += OnRoundStarted;
            Qurre.Events.Server.SendingRA += OnSendRA;
            Qurre.Events.Round.TeamRespawn += OnTeamRespawning;
            Qurre.Events.Round.End += OnRoundEnded;
            Qurre.Events.Round.Restart += OnRestart;
            Qurre.Events.Round.Waiting += OnWaiting;
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
            Qurre.Events.Server.SendingRA -= OnSendRA;
            Qurre.Events.Round.TeamRespawn -= OnTeamRespawning;
            Qurre.Events.Round.End -= OnRoundEnded;
            Qurre.Events.Round.Restart -= OnRestart;
            Qurre.Events.Round.Waiting -= OnWaiting;
            if (NeedDoLobby)
            {
                Qurre.Events.Player.Join -= OnJoin;
            }
        }
        // I wanted to implement a Lobby room.
        // In the beginning it was a very good idea for players to choose a mini-game,
        // but they always took only one Zombie Mode.
        // And in the future, the implementation killed my nerves.
        // I decided to refuse.
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
                        MainFunctions.StartEventParametres();
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
        public void OnSendRA(SendingRAEvent ev)
        {
            if (Plugin.IsEventRunning)
            {
                if (Plugin.CustomConfig.DonatorGroups.Contains(ev.Player.GroupName))
                {
                    ev.Allowed = false;
                    ev.Success = false;
                    ev.ReplyMessage = "Сейчас проводится Ивент!";
                }
                else if (ev.Name.ToLower() == "server_event")
                {
                    if (ev.Args.Count() == 1)
                    {
                        if (ev.Args[0].ToLower() == "round_restart")
                        {
                            ev.Allowed = false;
                            MainFunctions.EndEventParametres();
                            EventManager.Harmony.UnpatchAll();
                            Server.Restart();
                        }
                    }
                }
            }
        }
        public void OnTeamRespawning(TeamRespawnEvent ev)
        {
            if (Plugin.IsEventRunning) ev.Allowed = false;
        }
        // независимо от включения или выключения плагина, блокировки раунда и лобби не будет
        public void OnRoundEnded(RoundEndEvent ev)
        {
            Plugin.IsEventRunning = false;

            Round.LobbyLock = false;
            Round.Lock = false;
        }
        public void OnWaiting()
        {
            Plugin.IsEventRunning = false;

            Round.LobbyLock = false;
            Round.Lock = false;
        }
        public void OnRestart()
        {
            Plugin.IsEventRunning = false;

            Round.LobbyLock = false;
            Round.Lock = false;
        }
    }
}