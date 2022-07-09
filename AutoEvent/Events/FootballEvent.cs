using AutoEvent.Functions;
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
    internal class FootballEvent : IEvent
    {
        public string Name => "Футбольчик";
        public string Description => "";
        public string Color => "FFFF00";
        public string CommandName => "football";
        public static Model Model { get; set; }
        public static Model Ball { get; set; }
        public static TimeSpan EventTime { get; set; }
        public int Votes { get; set; }
        public int BluePoints { get; set; } = 0;
        public int RedPoints { get; set; } = 0;

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
            CreatingMapFromJson("Football.json", new Vector3(145.18f, 945.26f, -122.97f), out var model);
            Model = model;
            PlayAudio("FallGuys_DnB.f32le", 7, true, "Футбол");
            Ball = new Model("balls", new Vector3(0, 0, 0));
            Ball.AddPart(new ModelPrimitive(Ball, PrimitiveType.Sphere, UnityEngine.Color.red, Model.GameObject.transform.position + new Vector3(0, 5f, 0), new Vector3(0, 0, 0), new Vector3(1, 1, 1)));
            foreach (var ball in Ball.Primitives)
            {
                ball.GameObject.AddComponent<FootballComponent>();
            }
            var count = 0;
            foreach (Player player in Player.List)
            {
                if (count % 2 == 0)
                {
                    player.Role = RoleType.NtfCaptain;
                    player.ClearInventory();
                }
                else
                {
                    player.Role = RoleType.ClassD;
                    player.ClearInventory();
                }
                Timing.CallDelayed(2f, () =>
                {
                    player.Position = Model.GameObject.transform.position + new Vector3(0, 5, 0);
                });
                count++;
            }
            // Запуск ивента
            Timing.RunCoroutine(Cycle(), "ballevent_time");
        }
        public IEnumerator<float> Cycle()
        {
            // Обнуление таймера
            EventTime = new TimeSpan(0, 10, 0);
            BluePoints = 0;
            RedPoints = 0;
            // Запуск
            while (BluePoints < 3 && RedPoints < 3 && EventTime.TotalSeconds > 0)
            {
                foreach (Player player in Player.List)
                {
                    var text = string.Empty;
                    if (player.Role == RoleType.NtfCaptain)
                    {
                        text += "<color=cyan>Вы играете за Синюю Команду</color>\n";
                    }
                    else
                    {
                        text += "<color=red>Вы играете за Красную Команду</color>\n";
                    }
                    // Проверка расстояния между игроком и мячом
                    if (Vector3.Distance(Ball.Primitives[0].Primitive.Position, player.Position) < 5)
                    {
                        Ball.Primitives[0].GameObject.TryGetComponent<Rigidbody>(out Rigidbody rig);
                        rig.AddForce(player.Transform.forward + new Vector3(0, 0.5f, 0), ForceMode.Impulse);
                    }
                    player.ClearBroadcasts();
                    player.Broadcast(text + $"<color=blue>{BluePoints}</color> VS <color=red>{RedPoints}</color>\n" +
                        $"Время до конца: {EventTime.Minutes}:{EventTime.Seconds}", 1);
                }
                // Проверка выпадения мяча за карту
                if (Ball.Primitives[0].Primitive.Position.y < Model.GameObject.transform.position.y - 10f)
                {
                    Ball.Primitives[0].Primitive.Position = Model.GameObject.transform.position + new Vector3(0, 5f, 0);
                }
                // Проверка попадания мяча в синии ворота
                if (Vector3.Distance(Ball.Primitives[0].Primitive.Position, new Vector3(98.47f, 949.87f, -122.48f)) < 5)
                {
                    Ball.Primitives[0].Primitive.Position = Model.GameObject.transform.position + new Vector3(0, 5f, 0);
                    RedPoints++;
                }
                // Проверка попадания мяча в красные ворота
                if (Vector3.Distance(Ball.Primitives[0].Primitive.Position, new Vector3(191.71f, 949.87f, -123.13f)) < 5)
                {
                    Ball.Primitives[0].Primitive.Position = Model.GameObject.transform.position + new Vector3(0, 5f, 0);
                    BluePoints++;
                }
                yield return Timing.WaitForSeconds(1f);
                EventTime -= TimeSpan.FromSeconds(1f);
            }
            // Голов в синие ворота больше чем красных
            if (BluePoints > RedPoints)
            {
                BroadcastPlayers($"<color=blue>ПОБЕДА СИНИХ!</color>", 10);
            }
            // Голов красные ворота больше чем синих
            else if (RedPoints > BluePoints)
            {
                BroadcastPlayers($"<color=red>ПОБЕДА КРАСНЫХ!</color>", 10);
            }
            // Ничья
            else
            {
                BroadcastPlayers($"<color=#808080>Ничья</color>\n<color=blue>{BluePoints}</color> VS <color=red>{RedPoints}</color>", 10);
            }
            OnStop();
            yield break;
        }
        public void EventEnd()
        {
            if (Audio.Microphone.IsRecording) StopAudio();
            Ball.Destroy();
            Timing.RunCoroutine(DestroyObjects(Model));
            Timing.RunCoroutine(CleanUpAll());
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
