using AutoEvent.Interfaces;
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

namespace AutoEvent.Events
{
    internal class BallsEvent : IEvent
    {
        public string Name => "Дотронься Мячика";
        // I don't have enough time to implement this mini-game.
        // In theory, people should split into 3 teams and collect as many balls as possible across the map for a while.
        // People collect balls, as, for example, in Garry's mod.
        public string Description => "[Infinity Work!!!]";
        public string Color => "FFFF00";
        public string CommandName => "balls";
        public static Model Model { get; set; }
        public static Model Balls { get; set; }
        public static TimeSpan EventTime { get; set; }
        public int Votes { get; set; }

        public void OnStart()
        {
            Plugin.IsEventRunning = true;
            Qurre.Events.Player.Join += OnJoin;
            OnEventStarted();
        }
        public void OnStop()
        {
            Plugin.IsEventRunning = false;
            Qurre.Events.Player.Join -= OnJoin;
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
            if (Audio.Microphone.IsRecording) StopAudio();
            Timing.RunCoroutine(DestroyObjects(Model));
            Timing.RunCoroutine(CleanUpAll());
        }
        // Ивенты
        public void OnJoin(JoinEvent ev)
        {
            ev.Player.Role = RoleType.Spectator;
        }
    }
}
