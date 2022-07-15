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
    internal class HideAndSeek : IEvent
    {
        public string Name => "Догонялки";
        public string Description => "[В работе!]";
        public string Color => "FFFF00";
        public string CommandName => "hideandseek";
        public static Model Model { get; set; }
        public static TimeSpan EventTime { get; set; }
        public int Votes { get; set; }

        public void OnStart()
        {
            Plugin.IsEventRunning = true;
            Qurre.Events.Player.Join += OnJoin;
            Qurre.Events.Player.DamageProcess += OnDamageProcess;
            OnEventStarted();
        }
        public void OnStop()
        {
            Plugin.IsEventRunning = false;
            Qurre.Events.Player.Join -= OnJoin;
            Qurre.Events.Player.DamageProcess -= OnDamageProcess;
            Timing.CallDelayed(5f, () => EventEnd());
        }
        public void OnEventStarted()
        {
            CreatingMapFromJson("HideAndSeek.json", new Vector3(145.18f, 945.26f, -122.97f), out var model);
            Model = model;
            //PlayAudio("FallGuys_FallRoll.f32le", 15, true, "Death");
            TeleportAndChangeRolePlayers(Player.List.ToList(), RoleType.ClassD, Model.GameObject.transform.position + new Vector3(-22.67f, 4.94f, 14.61f));
            // Запуск ивента
            Timing.RunCoroutine(TimeToStart(), "time_to_start");
        }
        public IEnumerator<float> TimeToStart()
        {
            foreach (Player player in Player.List)
            {
                player.GodMode = true;
                player.EnableEffect("MovementBoost");
                player.ChangeEffectIntensity("MovementBoost", 4);
            }

            for (int time = 10; time != 0; time--)
            {
                BroadcastPlayers($"<color=#D71868><b><i>{Name}</i></b></color>\n<color=#ABF000>До начала ивента осталось <color=red>{time}</color> секунд.</color>", 1);
                yield return Timing.WaitForSeconds(1f);
            }
            Timing.RunCoroutine(Cycle(), "hideandseek_time");
            yield break;
        }
        public IEnumerator<float> Cycle()
        {
            EventTime = new TimeSpan(0, 60, 0);

            InitRandomPlayer();

            while (EventTime.TotalSeconds != 0)
            { // таймер и сделать смерть от гранаты
                BroadcastPlayers($"<color=#D71868><b><i>{Name}</i></b></color>\n" +
                $"<color=yellow>Осталось людей: <color=orange>{Player.List.Count(r => r.Role == RoleType.ClassD)}</color></color>\n" +
                $"<color=yellow>Осталось секунд: <color=red>{EventTime.Minutes}:{EventTime.Seconds}</color></color>", 1);

                foreach (Player player in Player.List.Where(r => r.Team == Team.MTF))
                {
                    foreach (Player anyPlayer in Player.List.Where(r => r.Team == Team.CDP))
                    {
                        if (Vector3.Distance(player.GameObject.transform.position, anyPlayer.GameObject.transform.position) < 1)
                        {
                            BlockAndChangeRolePlayer(player, RoleType.ClassD);
                            player.ShowHint("<color=orange>Вы стали Убегающим</color>", 2);
                            player.ClearInventory();

                            BlockAndChangeRolePlayer(anyPlayer, RoleType.NtfCaptain);
                            player.ShowHint("<color=red>Вы стали Догоняющим</color>", 2);
                            anyPlayer.ClearInventory();
                        }
                    }
                }
                yield return Timing.WaitForSeconds(1f);
                EventTime -= TimeSpan.FromSeconds(1f);
            }
            foreach(Player player in Player.List.Where(r => r.Team == Team.MTF))
            {
                GrenadeDeath(player, "<color=red>Вы не успели.</color>");
            }
            // Итоги
            if (Player.List.Count(r => r.Role != RoleType.Spectator) == 1)
            {
                // Победитель
                var player = Player.List.ToList().First(r => r.Role != RoleType.Spectator);
                player.Hp = 1000;
                BroadcastPlayers($"<color=#D71868><b><i>{Name}</i></b></color>\n" +
                $"<color=yellow>ПОБЕДИТЕЛЬ - <color=red>{player.Nickname}</color></color>", 10);
            }
            else if (Player.List.Count(r => r.Role != RoleType.Spectator) > 1)
            {
                // Перезапуск
                BroadcastPlayers($"<color=#D71868><b><i>{Name}</i></b></color>\n" +
                $"<color=yellow>Ещё осталось <color=orange>{Player.List.Count(r => r.Role == RoleType.ClassD)}</color> игроков</color>", 10);
                RestartEvent();
                yield break;
            }
            else
            {
                // Ничья
                BroadcastPlayers($"<color=#D71868><b><i>{Name}</i></b></color>\n" +
                $"<color=yellow>Все игроки были убиты(</color>\n", 10);
            }
            OnStop();
            yield break;
        }
        public void RestartEvent()
        {
            // Ожидаем рестарта
            Timing.CallDelayed(10f, () =>
            {
                // Запуск Ивента
                Timing.RunCoroutine(Cycle(), "hideandseek_time");
            });
        }
        public void InitRandomPlayer()
        {
            if (Player.List.Count() > 25)
            {
                AddNewPlayers(5);
            }
            else if (Player.List.Count() > 15)
            {
                AddNewPlayers(3);
            }
            else if (Player.List.Count() >= 5)
            {
                AddNewPlayers(1);
            }
        }
        public void AddNewPlayers(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Player player = Player.List.ToList().RandomItem();
                BlockAndChangeRolePlayer(player, RoleType.NtfCaptain);
                player.ClearInventory();
            }
        }
        public void GrenadeDeath(Player player, string Reason)
        {
            GrenadeFrag grenade = new GrenadeFrag(ItemType.GrenadeHE);
            grenade.FuseTime = 0.5f;
            grenade.Base.transform.localScale = new Vector3(0, 0, 0);
            grenade.MaxRadius = 0.5f;
            grenade.Spawn(player.Position);
            player.Kill(Reason);
        }
        public void EventEnd()
        {
            foreach(Player player in Player.List)
            {
                player.GodMode = false;
                player.DisableAllEffects();
            }

            if (Audio.Microphone.IsRecording) StopAudio();
            Timing.RunCoroutine(DestroyObjects(Model));
            Timing.RunCoroutine(CleanUpAll());
        }
        // Ивенты
        public void OnDamageProcess(DamageProcessEvent ev)
        {
            if (ev.DamageType == DamageTypes.Falldown)
            {
                ev.Amount = 30;
                if (ev.Target.Hp <= 1)
                {
                    GrenadeDeath(ev.Target, "<color=red>Умер от падения</color>");
                }
            }
        }
        public void OnJoin(JoinEvent ev)
        {
            ev.Player.Role = RoleType.Spectator;
        }
    }
}
