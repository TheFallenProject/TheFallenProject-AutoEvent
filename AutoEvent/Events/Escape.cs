using AutoEvent.Functions;
using AutoEvent.Interfaces;
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

namespace AutoEvent.Events
{
    internal class Escape : IEvent
    {
        public string Name => "Атомный Побег";
        public string Description => "Сбегите с комплекса Печеньками на сверхзвуковой скорости!";
        public string Color => "FFFF00";
        public string CommandName => "escape";
        public static TimeSpan EventTime { get; set; }
        public int Votes { get; set; }

        public void OnStart()
        {
            Plugin.IsEventRunning = true;
            Qurre.Events.Alpha.Stopping += OnNukeDisable;
            Qurre.Events.Player.Join += OnJoin;
            OnEventStarted();
        }
        public void OnStop()
        {
            Plugin.IsEventRunning = false;
            Qurre.Events.Alpha.Stopping -= OnNukeDisable;
            Qurre.Events.Player.Join -= OnJoin;
            Timing.CallDelayed(5f, () => EventEnd());
        }

        public void OnEventStarted()
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
            PlayAudio("Bomba_haus1.f32le", 20, false, "Escape");
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
        public void EventEnd()
        {
            BroadcastPlayers($"Атомный Побег\n" +
            $"<color=red>ПОБЕДА SCP</color>", 10);

            if (Audio.Microphone.IsRecording) StopAudio();
            Timing.RunCoroutine(CleanUpAll());
        }
        // Ивенты
        public void OnJoin(JoinEvent ev)
        {
            Timing.CallDelayed(2f, () => ev.Player.Role = RoleType.Scp173 );
        }
        static void OnNukeDisable(AlphaStopEvent ev)
        {
            ev.Allowed = false;
        }
    }
}
