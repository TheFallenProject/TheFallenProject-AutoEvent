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
    internal class Battle : IEvent
    {
        public string Name => "Мясная Заруба";
        public string Description => "Битва, в которой одна из команд должна одолеть другую.";
        public string Color => "FFFF00";
        public string CommandName => "battle";
        public Model Model { get; set; }
        public TimeSpan EventTime { get; set; }
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
            EventTime = new TimeSpan(0, 0, 0);

            CreatingMapFromJson("Battle.json", new Vector3(145.18f, 930f, -122.97f), out var model);
            Model = model;

            PlayAudio("MGS4.f32le", 10, true, "БИТВА");
            var count = 0;
            foreach (Player player in Player.List)
            {
                if (count % 2 == 0)
                {
                    CreateSoldier(true, player);
                }
                else
                {
                    CreateSoldier(false, player);
                }
                count++;
            }
            Timing.RunCoroutine(Cycle(), "battle_time");
        }
        public IEnumerator<float> Cycle()
        {
            Player.List.ToList().ForEach(player => player.EnableEffect(EffectType.Ensnared));
            for (int time = 10; time > 0; time--)
            {
                BroadcastPlayers($"<size=100><color=red>{time}</color></size>", 1);
                yield return Timing.WaitForSeconds(1f);
            }
            Player.List.ToList().ForEach(player => player.DisableEffect(EffectType.Ensnared));
            while (Player.List.Count(r => r.Team == Team.MTF) > 0 && Player.List.Count(r => r.Team == Team.CHI) > 0)
            {
                BroadcastPlayers($"<color=#D71868><b><i>Заруба</i></b></color>\n" +
                $"<color=yellow><color=blue>{Player.List.Count(r => r.Team == Team.MTF)}</color> VS <color=green>{Player.List.Count(r => r.Team == Team.CHI)}</color></color>\n" +
                $"<color=yellow>Время ивента <color=red>{EventTime.Minutes}:{EventTime.Seconds}</color></color>", 1);
                yield return Timing.WaitForSeconds(1f);
                EventTime += TimeSpan.FromSeconds(1f);
            }
            if (Player.List.Count(r => r.Team == Team.MTF) == 0)
            {
                BroadcastPlayers($"<color=#D71868><b><i>Заруба</i></b></color>\n" +
                $"<color=yellow>ПОБЕДИЛИ - <color=green>{Player.List.Count(r => r.Team == Team.CHI)} ХАОС</color></color>\n" +
                $"<color=yellow>Конец ивент: <color=red>{EventTime.Minutes}:{EventTime.Seconds}</color></color>", 10);
            }
            else if (Player.List.Count(r => r.Team == Team.CHI) == 0)
            {
                BroadcastPlayers($"<color=#D71868><b><i>Заруба</i></b></color>\n" +
                $"<color=yellow>ПОБЕДИЛИ - <color=blue>{Player.List.Count(r => r.Team == Team.MTF)} МОГ</color></color>\n" +
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
        public void CreateSoldier(bool isMtf, Player player)
        {
            if (isMtf)
            {
                player.Role = RoleType.NtfSergeant;
                player.AllItems.ToList().ForEach(item => player.RemoveItem(item));
                Timing.CallDelayed(2f, () =>
                {
                    player.Position = Model.GameObject.transform.position + new Vector3(44, 5, 5);
                });
            }
            else
            {
                player.Role = RoleType.ChaosConscript;
                player.AllItems.ToList().ForEach(item => player.RemoveItem(item));
                Timing.CallDelayed(2f, () =>
                {
                    player.Position = Model.GameObject.transform.position + new Vector3(-44, 5, -7);
                });
            }
            switch (Random.Range(0, 3))
            {
                case 0:
                    {
                        player.Broadcast("Вы играете за ПЕХОТУ", 6);
                        player.AddItem(ItemType.GunE11SR);
                        player.AddItem(ItemType.Medkit, 2);
                        player.AddItem(ItemType.GrenadeHE, 2);
                        player.AddItem(ItemType.ArmorCombat);
                        player.AddItem(ItemType.SCP1853);
                        player.AddItem(ItemType.Adrenaline);
                    }
                    break;
                case 1:
                    {
                        player.Broadcast("Вы играете за МЕДИКА", 6);
                        player.AddItem(ItemType.GunShotgun);
                        player.AddItem(ItemType.Medkit, 5);
                        player.AddItem(ItemType.ArmorCombat);
                        player.AddItem(ItemType.SCP500);
                    }
                    break;
                case 2:
                    {
                        player.Broadcast("Вы играете за ТАНК", 6);
                        player.AddAhp(100, 500);
                        player.AddItem(ItemType.GunLogicer);
                        player.AddItem(ItemType.ArmorHeavy);
                        player.AddItem(ItemType.GrenadeHE, 2);
                        player.AddItem(ItemType.SCP500, 2);
                        player.AddItem(ItemType.SCP1853);
                        player.AddItem(ItemType.Medkit);
                    }
                    break;
            }
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
