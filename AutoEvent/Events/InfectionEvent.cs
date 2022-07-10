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
    internal class InfectionEvent : IEvent
    {
        public string Name => "Заражение Зомби";
        public string Description => "Зомби режим, целью которого заразить всех игроков.";
        public string Color => "FF4242";
        public string CommandName => "zombie";
        public static Player Zombie { get; set; }
        public static Model Model { get; set; }
        public static TimeSpan EventTime { get; set; }
        public int Votes { get; set; }

        public void OnStart()
        {
            Plugin.IsEventRunning = true;
            Qurre.Events.Player.Join += OnJoin;
            Qurre.Events.Player.Dead += OnDead;
            Qurre.Events.Player.Damage += OnDamage;
            Qurre.Events.Round.TeamRespawn += OnTeamRespawning;
            Qurre.Events.Server.SendingRA += OnSendRA;
            OnEventStarted();
        }
        public void OnStop()
        {
            Plugin.IsEventRunning = false;
            Qurre.Events.Player.Join -= OnJoin;
            Qurre.Events.Player.Dead -= OnDead;
            Qurre.Events.Player.Damage -= OnDamage;
            Qurre.Events.Round.TeamRespawn -= OnTeamRespawning;
            Qurre.Events.Server.SendingRA -= OnSendRA;
            Timing.CallDelayed(10f, () => EventEnd());
        }
        public void OnEventStarted()
        {
            // Обнуление Таймера
            EventTime = new TimeSpan(0, 0, 0);
            // Создание карты
            CreatingMapFromJson("Zombie.json", new Vector3(145.18f, 945.26f, -122.97f), out var model);
            Model = model;
            // Запуск музыки
            if (Random.Range(0, 2) == 0) PlayAudio("Zombie.f32le", 20, true, "Zombie");
            else PlayAudio("Zombie1.f32le", 10, true, "Zombie");
            Player.List.ToList().ForEach(player =>
            {
                player.Role = RoleType.ClassD;
                Timing.CallDelayed(2f, () =>
                {
                    player.Position = Model.GameObject.transform.position + new Vector3(-25.44f, 1.51f, -0.74f);
                });
            });
            Timing.RunCoroutine(TimingBeginEvent($"Заражение", 15), "zombie_time");
        }
        // Отсчет до начала ивента
        public IEnumerator<float> TimingBeginEvent(string eventName, float time)
        {
            for (float _time = time; _time > 0; _time--)
            {
                BroadcastPlayers($"<color=#D71868><b><i>{eventName}</i></b></color>\n<color=#ABF000>До начала ивента осталось <color=red>{_time}</color> секунд.</color>", 1);
                yield return Timing.WaitForSeconds(1f);
            }
            SpawnZombie();
            yield break;
        }
        // Спавн зомби
        public void SpawnZombie()
        {
            Zombie = Player.List.ToList().RandomItem();
            BlockAndChangeRolePlayer(Zombie, RoleType.Scp0492);
            Timing.RunCoroutine(EventBeginning(), "SpawnZombie");
        }
        // Ивент начался - отсчет времени и колво людей
        public IEnumerator<float> EventBeginning()
        {
            while (Player.List.Count(r => r.Role == RoleType.ClassD) > 1)
            {
                BroadcastPlayers($"<color=#D71868><b><i>Заражение</i></b></color>\n" +
                    $"<color=yellow>Осталось людей: <color=green>{Player.List.Count(r => r.Role == RoleType.ClassD)}</color></color>\n" +
                    $"<color=yellow>Время ивента <color=red>{EventTime.Minutes}:{EventTime.Seconds}</color></color>", 2);

                yield return Timing.WaitForSeconds(1f);
                EventTime += TimeSpan.FromSeconds(1f);
            }
            Timing.RunCoroutine(DopTime(), "EventBeginning");
            yield break;
        }
        // Если останется один человек, то обратный отсчет
        public IEnumerator<float> DopTime()
        {
            for (int doptime = 30; doptime > 0; doptime--)
            {
                if (Player.List.Count(r => r.Role == RoleType.ClassD) == 0) break;

                BroadcastPlayers($"Дополнительное время: {doptime}\n" +
                $"<color=yellow>Остался <b><i>Последний</i></b> человек!</color>\n" +
                $"<color=yellow>Время ивента <color=red>{EventTime.Minutes}:{EventTime.Seconds}</color></color>", 2);

                yield return Timing.WaitForSeconds(1f);
                EventTime += TimeSpan.FromSeconds(1f);
            }
            if (Player.List.Count(r => r.Role == RoleType.ClassD) == 0)
            {
                BroadcastPlayers($"<color=red>Зомби Победили!</color>\n" +
                $"<color=yellow>Время ивента <color=red>{EventTime.Minutes}:{EventTime.Seconds}</color></color>", 10);
            }
            else
            {
                BroadcastPlayers($"<color=yellow><color=#D71868><b><i>Люди</i></b></color> Победили!</color>\n" +
                $"<color=yellow>Время ивента <color=red>{EventTime.Minutes}:{EventTime.Seconds}</color></color>", 10);
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
