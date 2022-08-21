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
        // I don't have enough time to implement this mini-game.
        // The point is that there is vertical parkour and there is lava.
        // You need to get to the top quickly before the lava kills a person.
        public string Name => "Паркур";
        public string Description => "[ВЕДУТСЯ РАБОТЫ. Могут быть биги или ивент не работает]";
        public string Color => "FF4242";
        public string CommandName => "parkour";
        public static Player Zombie { get; set; }
        public static Model Model { get; set; }
        public static Model LavaModel { get; set; }
        public static TimeSpan EventTime { get; set; }
        public int Votes { get; set; }

        public void OnStart()
        {
            Qurre.Events.Player.Join += OnJoin;
            OnEventStarted();
        }
        public void OnStop()
        {
            Qurre.Events.Player.Join -= OnJoin;
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
            Player.List.ToList().ForEach(player =>
            {
                player.Role = RoleType.ClassD;
                Timing.CallDelayed(2f, () =>
                {
                    player.Position = Model.GameObject.transform.position + new Vector3(-25.44f, 1.51f, -0.74f);
                });
            });
        }
        public IEnumerator<float> Cycle()
        {
            // Создание лавы
            LavaModel = new Model("lava", Model.GameObject.transform.position);
            LavaModel.AddPart(new ModelPrimitive(LavaModel, PrimitiveType.Cube, new Color32(255, 0, 0, 255), new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(100, 1, 100)));
            foreach (var prim in LavaModel.Primitives)
            {
                prim.GameObject.AddComponent<LavaComponent>();
            }

            while (Player.List.ToList().Count(r => r.Role != RoleType.Spectator) > 1)
            {
                Log.Info("Защёл в цикл");
                BroadcastPlayers($"<size=20><color=red><b>Живых: {Player.List.ToList().Count(r => r.Role != RoleType.Spectator)} Игроков</b></color></size>", 1);
                LavaModel.GameObject.transform.position += new Vector3(0, 0.1f, 0);
                Log.Info(LavaModel.GameObject.transform.position);
                yield return Timing.WaitForSeconds(1f);
                EventTime += TimeSpan.FromSeconds(1f);
            }
            if (Player.List.ToList().Count(r => r.Role != RoleType.Spectator) == 1)
            {
                BroadcastPlayers($"<size=80><color=red><b>Победитель\n{Player.List.ToList().First(r => r.Role != RoleType.Spectator).Nickname}</b></color></size>", 10);
            }
            else
            {
                BroadcastPlayers($"<size=70><color=red><b>Все утонули в Лаве (ахахахахах)</b></color></size>", 10);
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
            ev.Player.Role = RoleType.ClassD;
            Timing.CallDelayed(2f, () =>
            {
                ev.Player.Position = Model.GameObject.transform.position + new Vector3(-25.44f, 1.51f, -0.74f);
            });
        }
    }
}
