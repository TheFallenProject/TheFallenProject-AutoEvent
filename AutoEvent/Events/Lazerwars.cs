using AutoEvent.Functions;
using AutoEvent.Interfaces;
using Interactables.Interobjects.DoorUtils;
using MEC;
using Mirror;
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
    internal class Lazerwars : IEvent
    {
        public string CommandName => "lazer";
        public string Name => "Лазерый FreeforAll";
        public string Color => "FFFF00";
        public string Description => "Вы окажитесь в лабиринте! А дальше будь что будет.";
        public Model Model { get; set; }
        public TimeSpan EventTime { get; set; }
        public int Votes { get; set; }

        public void OnStart()
        {
            Plugin.IsEventRunning = true;
            Qurre.Events.Player.Join += OnJoin;
            onst();
        }
        public void OnStop()
        {
            Plugin.IsEventRunning = false;
            Qurre.Events.Player.Join -= OnJoin;
            Timing.CallDelayed(5f, () => EventEnd());
        }
        public void onst()
        {
            CreatingMapFromJson("Lab.json", new Vector3(145.18f, 930f, -122.97f), out var model);
            Model = model;
            foreach (Player pl in Player.List)
            {
                pl.Role = RoleType.ClassD;
                pl.FriendlyFire = true;
                Timing.CallDelayed(3f, () => { pl.Position = RandomPosition(); });
                pl.ShakeScreen();
                Timing.CallDelayed(5f, () => { pl.ShowHint("\n" + "\n" + "<color=red> !УБЕЙ ИХ ВСЕХ! </color>", 5); });
                pl.AddItem(ItemType.ParticleDisruptor, 3);
                pl.AddItem(ItemType.Medkit, 5);
            }
            Timing.RunCoroutine(Cycle(), "lazer_time");
        }
        public Vector3 RandomPosition()
        {
            Vector3 position = new Vector3(0, 0, 0);
            var rand = Random.Range(0, 11);
            switch (rand)
            {
                case 0: position = new Vector3(101.94f, 950f, -155.8f); break;
                case 1: position = new Vector3(101.94f, 950f, -155.8f); break;
                case 2: position = new Vector3(101.94f, 950f, -155.8f); break;
                case 3: position = new Vector3(122.96f, 950f, -91.19f); break;
                case 4: position = new Vector3(134.65f, 950f, -85.92f); break;
                case 5: position = new Vector3(134.65f, 950f, -85.92f); break;
                case 6: position = new Vector3(174.04f, 950f, -159.97f); break;
                case 7: position = new Vector3(122.26f, 950f, -125.35f); break;
                case 8: position = new Vector3(139.82f, 950f, -154.51f); break;
                case 9: position = new Vector3(143.57f, 950f, -109.93f); break;
                case 10: position = new Vector3(144.07f, 950f, -119.53f); break;
            }
            return position;
        }
        public IEnumerator<float> Cycle()
        {
            while (Player.List.Count(r => r.Team == Team.CDP) > 1)
            {
                BroadcastPlayers($"<color=#D71868><b><i>Лазерый FreeforAll</i></b></color>\n" +
                $"<color=blue>Осталось {Player.List.Count(r => r.Team == Team.CDP)} </color>\n" +
                $"<color=yellow>Время ивента <color=red>{EventTime.Minutes}:{EventTime.Seconds}</color></color>", 1);
                yield return Timing.WaitForSeconds(1f);
                EventTime += TimeSpan.FromSeconds(1f);
            }
            if (Player.List.Count(r => r.Team == Team.CDP) == 1)
            {
                foreach (Player pl in Player.List)
                {
                    if (pl.Role == RoleType.ClassD)
                    {
                        BroadcastPlayers($"<color=#D71868><b><i>Лазерый FreeforAll</i></b></color>\n" +
                        $"<color=yellow>ПОБЕДИЛ - <color=green>{pl.Nickname}</color></color>\n" +
                        $"<color=yellow>Конец ивента: <color=red>{EventTime.Minutes}:{EventTime.Seconds}</color></color>", 10);
                    }
                }
            }
            else if (Player.List.Count(r => r.Team == Team.CDP) == 0)
            {
                BroadcastPlayers($"<color=#D71868><b><i>Лазерый FreeforAll</i></b></color>\n" +
                $"<color=green> !ПОБЕДИЛА ДРУЖБА! </color>\n" +
                $"<color=yellow>Конец ивента: <color=red>{EventTime.Minutes}:{EventTime.Seconds}</color></color>", 10);
            }
            OnStop();
            yield break;
        }
        public void OnJoin(JoinEvent ev)
        {
            ev.Player.Role = RoleType.Spectator;
        }
        public void EventEnd()
        {
            if (Audio.Microphone.IsRecording) StopAudio();
            Server.FriendlyFire = false;
            Timing.RunCoroutine(DestroyObjects(Model));
            Timing.RunCoroutine(CleanUpAll());
        }
    }
}