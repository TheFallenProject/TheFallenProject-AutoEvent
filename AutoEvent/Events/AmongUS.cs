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
        public string CommandName => "amongus";
        public string Name => "AmongUS";
        public string Color => "FF4242";
        public static Model taskd1S { get; set; }
        public static Model Model { get; set; }
        public static Model taskd1F { get; set; }
        public static Model Bbody { get; set; }
        public static int takeitD1 = 0;
        public string Description => "Мафия (В разработке НО запускать можно!)";
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
            OnEventStarted();
        }

        public void OnStop()
        {
            Qurre.Events.Player.Damage += OnDamage;
            Plugin.IsEventRunning = false;
            Qurre.Events.Player.Join -= OnJoin;
            Timing.CallDelayed(10f, () => EventEnd());
        }
        public void OnEventStarted()
        {
            BroadcastPlayers("<color=blue> Запускаем... </color>", 5);
            CreatingMapFromJson("AmongUS.json", new Vector3(145.18f, 945.26f, -122.97f), out var model);
            Model = model;
            //Tasksd1();
            foreach (Player pl in Player.List)
            {
                pl.Role = Rolee.RandomItem();
                pl.ClearInventory();
                Timing.CallDelayed(2f, () => { pl.Position = RandomPosition(); });
                plcount += 1;
                pcount += 1;
            }
            Map.Broadcast("<color=green> Скоро всё начнётся (это не финальная версиия, в след будут задания!) </color>", 10);
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
                    Timing.CallDelayed(8f, () => { pl.ShowHint("<color=red> Вы стали !УБИЙЦЕЙ! </color> \n" + "Убей их всех! (Пистолет скоро вам дадут)", 5); });
                    Timing.CallDelayed(10f, () => { pl.AddItem(ItemType.GunCOM18, 1); });
                    kilid = ran;
                }
                else if (pl.Id == sh)
                {
                    Timing.CallDelayed(8f, () => { pl.ShowHint(" <color=blue> Вы стали !ШЕРИФОМ! </color> \n" + "Убей убийцу!"); });
                    Timing.CallDelayed(10f, () => { pl.AddItem(ItemType.GunCOM18, 1); });
                }
                else
                {
                    Timing.CallDelayed(8f, () => { pl.ShowHint("<color=green> Вы мирый житель! </color> \n" + "Выживай!", 5); });
                }
            }
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
                Log.Info("Убийца");
                if (ev.Target.Id == shid)
                {
                    Log.Info("ШЕРИФ УБИТ");
                    Map.Broadcast("<color=blue> <Шериф убит! (пистолет передался рандомному игроку!)> </color> ", 5);
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
                                pl.ShowHint("<color=green> <Вам дали пистолет, убей убийцу!> </color>", 5);
                                Log.Info("Пистолкет данн,");
                                break;
                            }
                        }
                    }
                }
                ev.Target.Kill("Убит: Убийцей");
            }
            else if (ev.Attacker.Id == shid)
            {
                Log.Info("Шериф");
                if (ev.Target.Id == kilid)
                {
                    Log.Info("Убийца убит");
                    Map.Broadcast("<color=red> <Убийца убит!> </color>", 5);
                }
                else
                {
                    Log.Info("Убит гражданский");
                    Map.Broadcast("<color=red> <Шериф убил невиновного! И был убит!> </color>", 5);
                }
            }
            else
            {
                Log.Info("граж");
            }
            ev.Target.Kill();

        }
        public Vector3 RandomPosition()
        {
            Vector3 position = new Vector3(0, 0, 0);
            var rand = Random.Range(0, 12);
            switch (rand)
            {
                case 0: position = new Vector3(101.94f, 950f, -155.8f); break;
                case 1: position = new Vector3(101.94f, 950f, -155.8f); break;
                case 2: position = new Vector3(107.63f, 950f, -87.72f); break;
                case 3: position = new Vector3(122.96f, 950f, -91.19f); break;
                case 4: position = new Vector3(134.65f, 950f, -85.92f); break;
                case 5: position = new Vector3(134.65f, 950f, -85.92f); break;
                case 6: position = new Vector3(134.65f, 950f, -85.92f); break;
                case 7: position = new Vector3(174.04f, 950f, -159.97f); break;
                case 8: position = new Vector3(122.26f, 950f, -125.35f); break;
                case 9: position = new Vector3(139.82f, 950f, -154.51f); break;
                case 10: position = new Vector3(143.57f, 950f, -109.93f); break;
                case 11: position = new Vector3(143.57f, 950f, -109.93f); break;
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