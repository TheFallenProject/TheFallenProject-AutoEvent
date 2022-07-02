using MEC;
using Mirror;
using Qurre;
using Qurre.API;
using Qurre.API.Addons.Models;
using Qurre.API.Controllers.Items;
using Qurre.API.Events;
using Qurre.API.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AutoEvent.Functions.MainFunctions;
using Random = UnityEngine.Random;

namespace AutoEvent
{
    internal class BallsEvent : Interfaces.IEvent
    {
        public string Name => "Дотронься Мячика";
        public string Description => "";
        public string Color => "FFFF00";
        public static Model Model { get; set; }
        public static Model Balls { get; set; }
        public static TimeSpan EventTime { get; set; }
        public int Votes { get; set; }
        public void OnStart()
        {
            Plugin.IsEventRunning = true;
            Qurre.Events.Round.TeamRespawn += OnTeamRespawning;
            Qurre.Events.Server.SendingRA += OnSendRA;
            OnEventStarted();
        }
        public void OnStop()
        {
            Plugin.IsEventRunning = false;
            Qurre.Events.Round.TeamRespawn -= OnTeamRespawning;
            Qurre.Events.Server.SendingRA -= OnSendRA;
            Timing.CallDelayed(5f, () => EventEnd());
        }
        public void OnEventStarted()
        {
            CreatingMapFromJson("Death.json", new Vector3(145.18f, 945.26f, -122.97f), out var model);
            Model = model;
            //PlayAudio("FallGuys_FallRoll.f32le", 15, true, "Death");
            Balls = new Model("balls", new Vector3(0, 0, 0));
            TeleportAndChangeRolePlayers(Player.List.ToList(), RoleType.ClassD, Model.GameObject.transform.position);
            // Запуск ивента
            Timing.RunCoroutine(Cycle(), "ballevent_time");
        }
        public IEnumerator<float> Cycle()
        {
            // Обнуление таймера
            EventTime = new TimeSpan(0, 0, 0);
            while (Round.Started)
            {
                Balls.AddPart(new ModelPrimitive(
                    Balls,
                    PrimitiveType.Sphere,
                    UnityEngine.Color.red,
                    Model.GameObject.transform.position + new Vector3(0, 5, 0),
                    new Vector3(0, 0, 0),
                    new Vector3(1, 1, 1),
                    false
                ));
                yield return Timing.WaitForSeconds(1f);
            }
            OnStop();
            yield break;
        }
        public void EventEnd()
        {
            CleanUpAll();
            if (Audio.Microphone.IsRecording) StopAudio();
            Log.Info("Запуск удаления");
            NetworkServer.UnSpawn(Model.GameObject);
            Timing.RunCoroutine(DestroyObjects(Model));
            //Player.List.ToList().ForEach(player => player.Role = RoleType.Tutorial);
            // Рестарт Лобби
            // EventManager.Init();
        }
        // Ивенты
        public void OnJoin(JoinEvent ev)
        {
            ev.Player.Role = RoleType.Spectator;
        }
        public void OnTeamRespawning(TeamRespawnEvent ev)
        {
            if (Plugin.IsEventRunning) ev.Allowed = false;
        }
        public void OnSendRA(SendingRAEvent ev)
        {
            if (Plugin.IsEventRunning)
            {
                if (Plugin.DonatorGroups.Contains(ev.Player.GroupName))
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
                            EventManager.Harmony.UnpatchAll();
                            Server.Restart();
                        }
                    }
                }
            }
        }
    }
}
