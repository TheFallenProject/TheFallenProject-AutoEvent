using AutoEvent.Functions;
using AutoEvent.Interfaces;
using MEC;
using Mirror;
using Qurre;
using Qurre.API;
using Qurre.API.Addons.Models;
using Qurre.API.Events;
using Qurre.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static AutoEvent.Functions.MainFunctions;
using Map = Qurre.API.Map;
using Player = Qurre.API.Player;
using Random = UnityEngine.Random;
using Server = Qurre.API.Server;

namespace AutoEvent.Events
{
    internal class Parkour : IEvent
    {
        public string Name => "Паркур";
        public string Description => "[В РАЗРАБОТКЕ!!!]";
        public string Color => "FF4242";
        public string CommandName => "parkour";
        public static Player Zombie { get; set; }
        public static Model Model { get; set; }
        public static Model LavaModel { get; set; }
        public static TimeSpan EventTime { get; set; }
        public int Votes { get; set; }

        public void OnStart()
        {
            Plugin.IsEventRunning = true;
            Qurre.Events.Player.Join += OnJoin;
            Qurre.Events.Player.Dead += OnDead;
            Qurre.Events.Player.Damage += OnDamage;
            OnEventStarted();
        }
        public void OnStop()
        {
            Plugin.IsEventRunning = false;
            Qurre.Events.Player.Join -= OnJoin;
            Qurre.Events.Player.Dead -= OnDead;
            Qurre.Events.Player.Damage -= OnDamage;
            Timing.CallDelayed(10f, () => EventEnd());
        }
        public void OnEventStarted()
        {
            // Обнуление Таймера
            EventTime = new TimeSpan(0, 0, 0);
            // Создание карты
            CreatingMapFromJson("Parkour.json", new Vector3(145.18f, 945.26f, -122.97f), out var model);
            Model = model;
            LavaModel = new Model("lava", Model.GameObject.transform.position);
            LavaModel.AddPart(new ModelPrimitive(LavaModel, PrimitiveType.Cube, new Color32(255, 0, 0, 255), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(100, 1, 100)));
            foreach (var prim in LavaModel.Primitives)
            {
                prim.GameObject.AddComponent<LavaComponent>();
            }
            //PlayAudio("Zombie.f32le", 20, true, "Zombie");
            Player.List.ToList().ForEach(player =>
            {
                player.Role = RoleType.ClassD;
                Timing.CallDelayed(2f, () =>
                {
                    player.Position = Model.GameObject.transform.position + new Vector3(-25.44f, 1.51f, -0.74f);
                });
            });
            //Timing.RunCoroutine(TimingBeginEvent($"Заражение", 15), "zombie_time");
        }
        public void EventEnd()
        {
            if (Audio.Microphone.IsRecording) StopAudio();
            Timing.RunCoroutine(DestroyObjects(Model));
            Timing.RunCoroutine(CleanUpAll());
        }
        // Ивенты
        public void OnDamage(DamageEvent ev)
        {
            if (ev.Attacker.Role == RoleType.Scp0492)
            {
                BlockAndChangeRolePlayer(ev.Target, RoleType.Scp0492);
            }
        }
        public void OnDead(DeadEvent ev)
        {
            ev.Target.Role = RoleType.Scp0492;
            Timing.CallDelayed(2f, () =>
            {
                ev.Target.Position = Model.GameObject.transform.position + new Vector3(-25.44f, 1.51f, -0.74f);
            });
        }
        public void OnJoin(JoinEvent ev)
        {
            ev.Player.Role = RoleType.Scp0492;
            Timing.CallDelayed(2f, () =>
            {
                ev.Player.Position = Model.GameObject.transform.position + new Vector3(-25.44f, 1.51f, -0.74f);
            });
        }
    }
}
