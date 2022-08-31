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
using AutoEvent.Interfaces;

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
        public static IEvent ActiveEvent = null;
        public static Config CustomConfig { get; set; }
        public void RegisterEvents()
        {
            CustomConfig = new Config();
            CustomConfigs.Add(CustomConfig);
            if (!CustomConfig.IsEnable) return;

            Qurre.Events.Server.SendingRA += OnSendRA;
            Qurre.Events.Round.TeamRespawn += OnTeamRespawning;
            Qurre.Events.Round.End += OnRoundEnded;
            Qurre.Events.Round.Restart += OnRestart;
            Qurre.Events.Round.Waiting += OnWaiting;
        }
        public void UnregisterEvents()
        {
            CustomConfigs.Remove(CustomConfig);
            if (!CustomConfig.IsEnable) return;

            Qurre.Events.Server.SendingRA -= OnSendRA;
            Qurre.Events.Round.TeamRespawn -= OnTeamRespawning;
            Qurre.Events.Round.End -= OnRoundEnded;
            Qurre.Events.Round.Restart -= OnRestart;
            Qurre.Events.Round.Waiting -= OnWaiting;
        }
        public void OnSendRA(SendingRAEvent ev)
        {
            if (Plugin.ActiveEvent != null)
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
                            Server.Restart();
                        }
                    }
                }
            }
        }
        public void OnTeamRespawning(TeamRespawnEvent ev)
        {
            if (Plugin.ActiveEvent != null) ev.Allowed = false;
        }
        public void OnRoundEnded(RoundEndEvent ev)
        {
            Plugin.ActiveEvent = null;

            Round.LobbyLock = false;
            Round.Lock = false;
        }
        public void OnWaiting()
        {
            Plugin.ActiveEvent = null;

            Round.LobbyLock = false;
            Round.Lock = false;
        }
        public void OnRestart()
        {
            Plugin.ActiveEvent = null;

            Round.LobbyLock = false;
            Round.Lock = false;
        }
    }
}