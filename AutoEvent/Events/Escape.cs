using AutoEvent.Functions;
using Interactables.Interobjects.DoorUtils;
using MEC;
using Qurre;
using Qurre.API;
using Qurre.API.Addons.Models;
using Qurre.API.Controllers;
using Qurre.API.Controllers.Items;
using Qurre.API.Events;
using Qurre.API.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static AutoEvent.Functions.MainFunctions;
using Random = UnityEngine.Random;

namespace AutoEvent
{
    internal class Escape : Interfaces.IEvent
    {
        public string Name => "Атомный Побег";
        public string Description => "";
        public string Color => "FFFF00";
        public static TimeSpan EventTime { get; set; }
        public int Votes { get; set; }

        public void OnStart()
        {
            Plugin.IsEventRunning = true;
            Qurre.Events.Alpha.Stopping += OnNukeDisable;
            Qurre.Events.Round.TeamRespawn += OnTeamRespawning;
            Qurre.Events.Server.SendingRA += OnSendRA;
            OnEventStarted();
        }
        public void OnStop()
        {
            Plugin.IsEventRunning = false;
            Qurre.Events.Alpha.Stopping -= OnNukeDisable;
            Qurre.Events.Round.TeamRespawn -= OnTeamRespawning;
            Qurre.Events.Server.SendingRA -= OnSendRA;
            Timing.CallDelayed(5f, () => EventEnd());
        }

        static void OnNukeDisable(AlphaStopEvent ev)
        {
            ev.Allowed = false;
        }

        public void OnEventStarted() // сделать ЛГБТ цвет
        {
            // Делаем всех д классами
            Player.List.ToList().ForEach(player =>
            {
                player.Role = RoleType.Scp173;
                player.EnableEffect(EffectType.Ensnared);
            });
            // Запуск боеголовки
            Alpha.Start();
            Alpha.TimeToDetonation = 80f;
            // Запуск музыки
            PlayAudio("Bomba_haus1.f32le", 20, false, "Escape");
            // Запуск ивента
            Timing.RunCoroutine(Cycle(), "escape_time");
        }
        public IEnumerator<float> Cycle()
        {
            // Обнуление таймера
            EventTime = new TimeSpan(0, 0, 0);
            // Отсчет обратного времени
            for (int time = 10; time > 0; time--)
            {
                BroadcastPlayers($"Атомный Побег\n" +
                    $"Успейте сбежать с комплекса пока он не взоврался!\n" +
                    $"<color=red>До начала побега: {(int)time} секунд</color>", 1);
                yield return Timing.WaitForSeconds(1f);
                EventTime += TimeSpan.FromSeconds(1f);
            }
            // Открываем все двери
            foreach (Door door in Map.Doors) door.Open = true;
            // Выключаем остановку
            Player.List.ToList().ForEach(player => player.DisableEffect(EffectType.Ensnared));
            // Отсчет времени
            while (Alpha.TimeToDetonation != 0)
            {
                foreach(Player player in Player.List)
                {
                    if (player.Room.Type == RoomType.EzGateA || player.Room.Type == RoomType.EzGateB) player.TeleportToRoom(RoomType.Surface);
                }
                BroadcastPlayers($"Атомный Побег\n" +
                    $"До взрыва: <color=red>{(int)Alpha.TimeToDetonation}</color> секунд", 1);
                yield return Timing.WaitForSeconds(1f);
                EventTime += TimeSpan.FromSeconds(1f);
            }
            OnStop();
            yield break;
        }
        // Подведение итогов ивента и возврат в лобби
        public void EventEnd()
        {
            BroadcastPlayers($"Атомный Побег\n" +
                $"<color=red>ПОБЕДА SCP</color>", 10);

                // Чистим трупы и оружия
                CleanUpAll();
                // Выключение музыки
                if (Audio.Microphone.IsRecording) StopAudio();
            // Рестарт Лобби
            // EventManager.Init();
           //s   Player.List.ToList().ForEach(player => player.Role = RoleType.Tutorial);
        }
        // Ивенты
        public void OnJoin(JoinEvent ev)
        {
            ev.Player.Role = RoleType.Scp173;
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
