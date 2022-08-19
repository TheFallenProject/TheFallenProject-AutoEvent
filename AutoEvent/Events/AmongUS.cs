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

using InventorySystem.Items;
using Qurre.API.Controllers;
using Qurre.API.Controllers.Items;
using Qurre.API.Objects;
using PlayerEvents = Qurre.Events.Player;
namespace AutoEvent.Events
{
    class AmongUS : IEvent
    {
        public string CommandName => "among";
        public string Name => "AmongUS";
        public string Color => "FF4242";
        public static Model taskd1S { get; set; }
        public static Model Model { get; set; }
        public static Model taskd1F { get; set; }
        public static Model Bbody { get; set; }
        public static int takeitD1 = 0;
        public string Description => "Мафия (Запуская вы подтверждаете, что в случае краша сервера ВЫ несёте ответственность)";
        public static int tasks = 0;
        public int killers = 0;
        public int plcount = 1;
        public int pcount = 0;
        public static int kilid = 0;
        public static int shid = 0;
        public int Votes { get; set; }

        public void OnStart()
        {
            Qurre.Events.Player.Damage += OnDamage;
            Plugin.IsEventRunning = true;
            Qurre.Events.Player.Join += OnJoin;
            Qurre.Events.Player.Shooting += OnShootEvent;
            OnEventStarted();
        }

        public void OnStop()
        {
            Qurre.Events.Player.Damage += OnDamage;
            Plugin.IsEventRunning = false;
            Qurre.Events.Player.Shooting -= OnShootEvent;
            Qurre.Events.Player.Join -= OnJoin;
            Timing.CallDelayed(10f, () => EventEnd());
        }
        public void OnEventStarted()
        {
            BroadcastPlayers("<color=blue> Запускаем... </color>", 5);
            CreatingMapFromJson("Among.json", new Vector3(145.18f, 945.26f, -122.97f), out var model);
            Model = model;
            foreach (Player pl in Player.List)
            {
                pl.Role = Rolee.RandomItem();
                pl.ClearInventory();
                Timing.CallDelayed(2f, () => { pl.Position = RandomPosition(); });
                plcount += 1;
                pcount += 1;
            }
            Map.Broadcast("<color=green> Скоро всё начнётся </color> <color=red> (это не финальная версиия, в след будут задания!) </color>", 10);
            Timing.WaitForSeconds(10f);
            var ran = Random.Range(2, plcount);
            var sh = Random.Range(2, plcount);
            if (sh == ran)
            {
                if (ran == plcount) { sh = ran - 1; }
                else if (ran == 2) { sh = plcount; }
                else { sh = ran + 1; }
            }
            foreach (Player pl in Player.List)
            {
                if (pl.Id == ran)
                {
                    Timing.CallDelayed(10f, () => { pl.ShowHint("<color=red> Вы стали !УБИЙЦЕЙ! </color> \n" + "Убей их всех! (Пистолет скоро вам дадут)", 5); });
                    Timing.CallDelayed(10f, () => { pl.AddItem(ItemType.GunCOM18, 1); });
                    kilid = ran;
                }
                else if (pl.Id == sh)
                {
                    Timing.CallDelayed(10f, () => { pl.ShowHint(" <color=blue> Вы стали !ШЕРИФОМ! </color> \n" + "Убей убийцу!"); });
                    Timing.CallDelayed(10f, () => { pl.AddItem(ItemType.GunCOM18, 1); });
                    shid = pl.Id;
                }
                else
                {
                    Timing.CallDelayed(10f, () => { pl.ShowHint("<color=green> Вы мирый житель! </color> \n" + "Выживай!", 5); });
                }
            }
        }
        public void OnShootEvent(ShootingEvent ev)
        {
            ev.Shooter.ClearInventory();
            ev.Shooter.ShowHint("<color=red> жди 15 сек. пока я его перезарежаю </color>");
            Timing.CallDelayed(15f, () => { ev.Shooter.AddItem(ItemType.GunCOM18); });
        }

        public void EventEnd()
        {
            if (Audio.Microphone.IsRecording) StopAudio();
            Timing.RunCoroutine(DestroyObjects(Model));
            Timing.RunCoroutine(CleanUpAll());
        }
        public void OnDamage(DamageEvent ev)
        {
            if (ev.Attacker.Id == kilid)
            {
                pcount--;
                if (ev.Target.Id == shid)
                {
                    Map.Broadcast("<color=blue> Шериф убит! (пистолет передался рандомному игроку!) </color> ", 5);
                    foreach (Player pl in Player.List)
                    {
                        if (pl.Id == kilid)
                        {
                            continue;
                        }
                        else
                        {
                            if (pl.Role != RoleType.Spectator)
                            {
                                pl.ShakeScreen();
                                pl.ShowHint("<color=green> <Вам дали пистолет, убей убийцу!> </color>", 5);
                                pl.AddItem(ItemType.GunCOM18);
                                shid = pl.Id;
                                break;
                            }
                        }
                    }
                }
                if (pcount == 1)
                {
                    Map.ClearBroadcasts();
                    Map.Broadcast("<color=red> Убийца убил всех! </color>", 5);
                    OnStop();
                }
                ev.Target.Kill("Убит: Убийцей");
            }
            else if (ev.Attacker.Id == shid)
            {
                pcount--;
                if (ev.Target.Id == kilid)
                {
                    Map.Broadcast("<color=red> Убийца убит! </color>", 5);
                    OnStop();
                }
                else
                {
                    Map.Broadcast("<color=red> Шериф убил невиновного! И был убит! </color>", 5);
                    foreach (Player pl in Player.List)
                    {
                        if (pl.Id == kilid)
                        {
                            continue;
                        }
                        else
                        {
                            var ran = Random.Range(1, 2);
                            if (ran == 1)
                            {
                                continue;
                            }
                            else
                            {
                                pl.ShakeScreen();
                                pl.ShowHint("<color=red> <Вам дали пистолет, убей убийцу!> </color>", 5);
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                if (pcount == 1)
                {
                    Map.Broadcast("<color=red> Убийца убил всех! </color>", 5);
                    OnStop();
                }
            }
            ev.Target.ClearInventory();
            ev.Target.Kill();

        }
        public Vector3 RandomPosition()
        {
            Vector3 position = new Vector3(0, 0, 0);
            var rand = Random.Range(0, 14);
            switch (rand)
            { //?
                case 1: position = new Vector3(78f, 949f, -171f); break;
                case 2: position = new Vector3(78f, 949f, -120f); break;
                case 3: position = new Vector3(17f, 949f, -155f); break;
                case 4: position = new Vector3(17f, 949f, -120f); break;
                case 5: position = new Vector3(-32f, 949f, -105f); break;
                case 6: position = new Vector3(-10f, 949f, -80f); break;
                case 7: position = new Vector3(-36f, 949f, -59f); break;
                case 8: position = new Vector3(96f, 949f, -57f); break;
                case 9: position = new Vector3(119f, 949f, -86f); break;
                case 10: position = new Vector3(206f, 949f, -134f); break;
                case 11: position = new Vector3(143f, 949f, -82f); break;
                case 12: position = new Vector3(107f, 949f, -119f); break;
                case 13: position = new Vector3(178f, 949f, -118f); break;
            }
            return position;
        }
        public List<RoleType> Rolee = new List<RoleType>()
        {
            RoleType.ClassD,
            RoleType.NtfCaptain,
            RoleType.ChaosConscript,
            RoleType.Scientist
        };
        public void OnJoin(JoinEvent ev)
        {
            ev.Player.Role = RoleType.Spectator;
        }
    }
}