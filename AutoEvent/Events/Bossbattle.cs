using AutoEvent.Functions;
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
    internal class Bossbattle : Interfaces.IEvent
    {
        public string Name => "Бой с боссом";

        public string Color => "0b6f00";

        public string Description => "";
        public Model Model { get; set; }
        public TimeSpan EventTime { get; set; }
        public int Votes { get; set; }

        public string CommandName => "bossbattle";

        public int i = 1;
        public static int hp = 5000;
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
            // Обнуление Таймера
            EventTime = new TimeSpan(0, 0, 0);
            // Создание карты
            CreatingMapFromJson("Battle.json", new Vector3(145.18f, 930f, -122.97f), out var model);
            Model = model;
            // Запуск музыки
            // PlayAudio("FallGuys_DnB.f32le", 10, true, "LavaAudio");
            // Создание и телепорт отрядов
            var cont = 0;
            foreach (Player player in Player.List)
            {
                cont++;
            }
            foreach(Player pl in Player.List)
            {
                if(i != cont)
                {
                    pl.ShowHint("<color=blue>ВЫ РЫЦАРЬ</color>");
                    pl.Role = RoleType.ClassD;
                    pl.AddItem(ItemType.ArmorHeavy);
                    pl.AddItem(ItemType.GunE11SR);
                    pl.AddItem(ItemType.Medkit, 6);
                    Timing.CallDelayed(2f, () =>
                    {
                        pl.Position = Model.GameObject.transform.position + new Vector3(44, 5, 5);
                    });
                    i++;                   
                }
                else if(i == cont)
                {
                    pl.ShowHint("<color=red>ВЫ БОСС</color>");
                    pl.Role = RoleType.Tutorial;
                    pl.Hp = hp;
                    pl.MaxHp = hp;
                    pl.AddItem(ItemType.ArmorHeavy);
                    pl.AddItem(ItemType.GunE11SR);
                    pl.AddItem(ItemType.ParticleDisruptor);
                    pl.AddItem(ItemType.Medkit, 5);
                    Timing.CallDelayed(2f, () =>
                    {
                        pl.Position = Model.GameObject.transform.position + new Vector3(-44, 5, -7);
                    });

                }
            }
            
            // Запуск ивента
            Timing.RunCoroutine(Cycle(), "battle_time");    
        }
        public IEnumerator<float> Cycle()
        {
            Player.List.ToList().ForEach(player => player.EnableEffect(EffectType.Ensnared));
            // Отсчет обратного времени
            for (int time = 10; time > 0; time--)
            {
                BroadcastPlayers($"<size=100><color=red>{time}</color></size>", 1);
                yield return Timing.WaitForSeconds(1f);
            }
            Player.List.ToList().ForEach(player => player.DisableEffect(EffectType.Ensnared));
            while (Player.List.Count(r => r.Team == Team.CDP) > 0 && Player.List.Count(r => r.Team == Team.TUT) > 0)
            {
                BroadcastPlayers($"<color=#D71868><b><i>Бой с боссом</i></b></color>\n" +
                $"<color=yellow><color=blue>{Player.List.Count(r => r.Team == Team.MTF)}</color> VS БОССА</color></color>\n" +
                $"<color=yellow>Время ивента <color=red>{EventTime.Minutes}:{EventTime.Seconds}</color></color>", 1);
                yield return Timing.WaitForSeconds(1f);
                EventTime += TimeSpan.FromSeconds(1f);
            }
            if (Player.List.Count(r => r.Team == Team.CDP) == 0)
            {
                BroadcastPlayers($"<color=#D71868><b><i>Бой с боссом</i></b></color>\n" +
                $"<color=yellow>ПОБЕДИЛ <color=green> БОСС</color></color>\n" +
                $"<color=yellow>Конец ивент: <color=red>{EventTime.Minutes}:{EventTime.Seconds}</color></color>", 10);
            }
            else if (Player.List.Count(r => r.Team == Team.TUT) == 0)
            {
                BroadcastPlayers($"<color=#D71868><b><i>Бой с боссом</i></b></color>\n" +
                $"<color=yellow>ПОБЕДИЛИ - <color=blue>{Player.List.Count(r => r.Team == Team.MTF)} РЫЦАРИ</color></color>\n" +
                $"<color=yellow>Конец ивент: <color=red>{EventTime.Minutes}:{EventTime.Seconds}</color></color>", 10);
            }
            OnStop();
            yield break;
        }
        public void EventEnd()
        {
            if (Audio.Microphone.IsRecording) StopAudio();
            Server.FriendlyFire = false;
            Timing.RunCoroutine(DestroyObjects(Model));
            Timing.RunCoroutine(CleanUpAll());
        }
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