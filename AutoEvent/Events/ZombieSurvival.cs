using AutoEvent.Functions;
using AutoEvent.Interfaces;
using MEC;
using Mirror;
using Qurre;
using Qurre.API;
using Qurre.API.Addons.Models;
using Qurre.API.Controllers;
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
    internal class ZombieSurvival : IEvent
    {
        public string Name => "Зомби-Выживание";
        public string Description => "Зомби должны заразить всех! А игроки должны убить их.";
        public string Color => "FF4242";
        public string CommandName => "survival";
        public static Model Model { get; set; }
        public static Model Teleport { get; set; }
        public static TimeSpan EventTime { get; set; }
        public int Votes { get; set; }

        public void OnStart()
        {
            Plugin.IsEventRunning = true;
            Qurre.Events.Player.Damage += OnDamage;
            Qurre.Events.Map.ScpDeadAnnouncement += OnScpDead;
            OnEventStarted();
        }
        public void OnStop()
        {
            Plugin.IsEventRunning = false;
            Qurre.Events.Player.Damage -= OnDamage;
            Qurre.Events.Map.ScpDeadAnnouncement -= OnScpDead;
            Timing.CallDelayed(10f, () => EventEnd());
        }
        public void OnEventStarted()
        {
            EventTime = new TimeSpan(0, 0, 0);

            CreatingMapFromJson("Zm_Dust_World.json", new Vector3(145.18f, 945.26f, -122.97f), out var model);
            Model = model;
            Teleport = new Model("tp", Model.GameObject.transform.position + new Vector3(40.561f, 15.456f, 22.42f));

            Player.List.ToList().ForEach(player =>
            {
                player.Scale = new Vector3(1, 1, 1);
                player.Role = RoleType.ClassD;
                Timing.CallDelayed(2f, () =>
                {
                    player.AddAhp(200, 200, 0, 0, 0, true);
                    player.AddItem(Guns.RandomItem());
                    player.AddItem(ItemType.GunCOM18);
                    player.Position = Model.GameObject.transform.position + new Vector3(-0.44f, 2.48f, -0.05f);
                });
            });
            Timing.RunCoroutine(TimingBeginEvent($"Зомби_Выживание", 10), "survival_time");
        }
        public IEnumerator<float> TimingBeginEvent(string eventName, float time)
        {
            PlayAudio("Countdown.f32le", 20, false, "Отсчёт");
            for (float _time = time; _time > 0; _time--)
            {
                BroadcastPlayers($"<color=#D71868><b><i>{eventName}</i></b></color>\n<color=#ABF000>До начала ивента осталось <color=red>{_time}</color> секунд.</color>", 1);
                yield return Timing.WaitForSeconds(1f);
            }
            DoZombie(Player.List.ToList().RandomItem(), true);
            Timing.RunCoroutine(EventBeginning(), "SpawnZombie");
            yield break;
        }
        public IEnumerator<float> EventBeginning()
        {
            while (Player.List.Count(r => r.Team != Team.SCP) > 0)
            {
                if (EventTime.TotalSeconds == 30)
                {
                    PlayAudio("ZombieSurvival.f32le", 10, true, "Зомби");
                }
                Teleports();
                Player.List.ToList().ForEach(player =>
                {
                    if (player.Role == RoleType.Spectator)
                    {
                        DoZombie(player, true);
                    }
                    player.ClearBroadcasts();
                    player.Broadcast($"<color=#D71868><b><i>{Name}</i></b></color>\n" +
                    $"<color=yellow>Осталось людей: <color=green>{Player.List.Count(r => r.Team != Team.SCP)}</color></color>\n" +
                    $"<color=yellow>Время ивента <color=red>{EventTime.Minutes}:{EventTime.Seconds}</color></color>", 2);
                });
                yield return Timing.WaitForSeconds(1f);
                EventTime += TimeSpan.FromSeconds(1f);
            }
            if (Player.List.Count(r => r.Team == Team.SCP) == 0)
            {
                BroadcastPlayers($"<color=yellow><color=#D71868><b><i>Люди</i></b></color> Победили!</color>\n" +
                $"<color=yellow>Время ивента <color=red>{EventTime.Minutes}:{EventTime.Seconds}</color></color>", 10);
            }
            else if (Player.List.Count(r => r.Team != Team.SCP) == 0)
            {
                BroadcastPlayers($"<color=red>Зомби Победили!</color>\n" +
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
        public void DoZombie(Player player, bool teleportToSpawn)
        {
            if (teleportToSpawn) player.Role = RoleType.Scp93953;
            else BlockAndChangeRolePlayer(player, RoleType.Scp93953);

            Timing.CallDelayed(2f, () =>
            {
                if (teleportToSpawn) player.Position = Model.GameObject.transform.position + new Vector3(-0.44f, 2.48f, -0.05f);
                player.Hp = 2500;
                player.DisableAllEffects();
            });
        }
        public void Teleports()
        {
            foreach(Player player in Player.List)
            {
                if (Vector3.Distance(Teleport.GameObject.transform.position, player.Position) < 3)
                {
                    player.Position = Model.GameObject.transform.position + new Vector3(-31.109f, 1.77f, -24.56f);
                }
            }
        }
        public List<ItemType> Guns = new List<ItemType>()
        {
            ItemType.GunFSP9,
            ItemType.GunCrossvec,
            ItemType.GunAK,
            ItemType.GunE11SR
        };
        // Ивенты
        public void OnDamage(DamageEvent ev)
        {
            if (ev.Attacker.Team == Team.SCP && ev.Attacker != ev.Target)
            {
                ev.Allowed = false;
                if (ev.Target.Ahp > 0) ev.Target.Ahp -= 100;
                else DoZombie(ev.Target, false);
            }
            else if (ev.Attacker.Team != Team.SCP)
            {
                if (ev.Target.Team == Team.SCP)
                {
                    var forward = ev.Attacker.Transform.forward;
                    ev.Target.Position += new Vector3(forward.x * 0.3f, forward.y * 0.3f, forward.z * 0.3f);
                }
            }
        }
        public void OnScpDead(ScpDeadAnnouncementEvent ev) => ev.Allowed = false;
    }
}
