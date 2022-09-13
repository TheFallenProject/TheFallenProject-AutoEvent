using AutoEvent.Functions;
using AutoEvent.Interfaces;
using InventorySystem.Items;
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
using UnityEngine;
using static AutoEvent.Functions.MainFunctions;
using Random = UnityEngine.Random;

namespace AutoEvent.Events
{
    internal class HideAndSeek : IEvent
    {
        public string Name => "Догонялки";
        public string Description => "[В работе!] Догонялки игроков.";
        public string Color => "FFFF00";
        public string CommandName => "hide";
        public static Model Model { get; set; }
        public static Model Ledders { get; set; }
        public static TimeSpan EventTime { get; set; }
        public int Votes { get; set; }

        public void OnStart()
        {
            Qurre.Events.Player.Join += OnJoin;
            Qurre.Events.Player.Spawn += OnSpawnEvent;
            OnEventStarted();
        }
        public void OnStop()
        {
            Qurre.Events.Player.Join -= OnJoin;
            Qurre.Events.Player.Spawn -= OnSpawnEvent;
            Timing.CallDelayed(10f, () => EventEnd());
        }
        public void OnEventStarted()
        {
            CreatingMapFromJson("HideAndSeek.json", new Vector3(145.18f, 945.26f, -122.97f), out var model);
            Model = model;

            LedderCreate();

            foreach (Player pl in Player.List)
            {
                pl.GameObject.AddComponent<BoxCollider>();
                pl.GameObject.AddComponent<BoxCollider>().size = new Vector3(3f, 1f, 3f);
            }
           // PlayAudio("FallGuys_FallForTheTeam.f32le", 15, true, "Догонялки");
            TeleportAndChangeRolePlayers(Player.List.ToList(), RoleType.ClassD, Model.GameObject.transform.position + new Vector3(-22.67f, 4.94f, 14.61f));
            // Запуск ивента
            Timing.RunCoroutine(TimeToStart(), "time_to_start");
        }
        public IEnumerator<float> TimeToStart()
        {
            foreach (Player player in Player.List)
            {
                player.GodMode = true;
                player.EnableEffect("Scp207");
                player.ChangeEffectIntensity("Scp207", 4);
            }

            for (int time = 10; time != 0; time--)
            {
                BroadcastPlayers($"<color=#D71868><b><i>{Name}</i></b></color>\n<color=#ABF000>До начала ивента осталось <color=red>{time}</color> секунд.</color>", 1);
                yield return Timing.WaitForSeconds(1f);
            }
            Timing.RunCoroutine(Cycle(), "hideandseek_time");
            yield break;
        }
        internal static Vector3 Getcampos(GameObject gameObject)
        {
            Scp049_2PlayerScript component = gameObject.GetComponent<Scp049_2PlayerScript>();
            Scp106PlayerScript component2 = gameObject.GetComponent<Scp106PlayerScript>();

            Vector3 forward = component.plyCam.transform.forward;
            Physics.Raycast(component.plyCam.transform.position, forward, out RaycastHit raycastHit, 40f, component2.teleportPlacementMask);
            Vector3 position = raycastHit.point;
            return position;
        }
        public IEnumerator<float> Cycle()
        {
            EventTime = new TimeSpan(0, 0, 30);

            InitRandomPlayer();

            while (EventTime.TotalSeconds != 0)
            {
                foreach (Player player in Player.List)
                {
                    Scp049_2PlayerScript component = player.GameObject.GetComponent<Scp049_2PlayerScript>();
                    Scp106PlayerScript component2 = player.GameObject.GetComponent<Scp106PlayerScript>();

                    Vector3 forward = component.plyCam.transform.forward;
                    Physics.Raycast(component.plyCam.transform.position, forward, out RaycastHit raycastHit, 40f, component2.teleportPlacementMask);
                    Vector3 pos = raycastHit.point;
                    player.ClearBroadcasts();
                    player.Broadcast($"<color=#D71868><b><i>{Name}</i></b></color>\n" +
                    $"<color=yellow>Осталось людей: <color=orange>{Player.List.Count(r => r.Role == RoleType.ClassD)}</color></color>\n" +
                    $"<color=yellow>Осталось секунд: <color=red>{EventTime.Minutes}:{EventTime.Seconds}</color></color>", 1);

                    // ShitCode awesome :(
                    foreach (Player anotherPlayer in Player.List)
                    {
                        if (player.Team == Team.CDP && anotherPlayer.Team == Team.MTF && player != anotherPlayer)
                        {
                            if(pos.x == anotherPlayer.GameObject.transform.position.x && pos.y == anotherPlayer.GameObject.transform.position.y && pos.z == anotherPlayer.GameObject.transform.position.z)
                            {
                                if (Vector3.Distance(player.GameObject.transform.position, anotherPlayer.GameObject.transform.position) < 3)
                            {
                                 BlockAndChangeRolePlayer(player, RoleType.NtfCaptain);
                                 player.ClearInventory();
                           
                                BlockAndChangeRolePlayer(anotherPlayer, RoleType.ClassD);
                                anotherPlayer.ClearInventory();
                                }
                            }
                            // if (Vector3.Distance(player.GameObject.transform.position, anotherPlayer.GameObject.transform.position) < 3)
                            // {
                            //     BlockAndChangeRolePlayer(player, RoleType.NtfCaptain);
                            //      player.ClearInventory();
                            //
                            //     BlockAndChangeRolePlayer(anotherPlayer, RoleType.ClassD);
                            //     anotherPlayer.ClearInventory();
                            //}
                        }
                    }
                }

                yield return Timing.WaitForSeconds(0.5f);
                EventTime -= TimeSpan.FromSeconds(0.5f);
            }
            foreach (Player player in Player.List.Where(r => r.Team == Team.MTF))
            {
                GrenadeDeath(player, "<color=red>Вы не успели.</color>");
            }
            // Итоги
            if (Player.List.Count(r => r.Role != RoleType.Spectator) == 1)
            {
                // Победитель
                var player = Player.List.ToList().First(r => r.Role != RoleType.Spectator);
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
                BroadcastPlayers($"<color=#D71868><b><i>{Name}</i></b></color>\n" +
                $"<color=yellow>Все игроки были убиты(</color>\n", 10);
            }
            OnStop();
            yield break;
        }
        public void LedderCreate()
        {
            Ledders = new Model("ledder", Model.GameObject.transform.position);
            Ledders.AddPart(new ModelPrimitive(Ledders, PrimitiveType.Cube, new Color32(255, 0, 0, 125), new Vector3(-38.7f, 17.72f, -10.62f), new Vector3(0, 90, 90), new Vector3(-11.07f, 1, 4.39f)));
            Ledders.AddPart(new ModelPrimitive(Ledders, PrimitiveType.Cube, new Color32(255, 0, 0, 125), new Vector3(-37.22f, 9.49f, 27.77f), new Vector3(0, -90, 90), new Vector3(-17.27f, 1, 4.39f)));
            Ledders.AddPart(new ModelPrimitive(Ledders, PrimitiveType.Cube, new Color32(255, 0, 0, 125), new Vector3(-35.31f, 4.52f, -2.52f), new Vector3(0, 180, 90), new Vector3(-21.8f, 1, 4.39f)));
            Ledders.AddPart(new ModelPrimitive(Ledders, PrimitiveType.Cube, new Color32(255, 0, 0, 125), new Vector3(-37.22f, 1.08f, 19.94f), new Vector3(0, -90, 90), new Vector3(-17.27f, 1, 4.39f)));
            Ledders.AddPart(new ModelPrimitive(Ledders, PrimitiveType.Cube, new Color32(255, 0, 0, 125), new Vector3(-11.34f, 6.18f, -17.15f), new Vector3(0, 0, 90), new Vector3(-24.15f, 1, 4.39f)));
            Ledders.AddPart(new ModelPrimitive(Ledders, PrimitiveType.Cube, new Color32(255, 0, 0, 125), new Vector3(-5.67f, 6.43f, -29.84f), new Vector3(0, 90, 90), new Vector3(-24.15f, 1, 4.39f)));
            Ledders.AddPart(new ModelPrimitive(Ledders, PrimitiveType.Cube, new Color32(255, 0, 0, 125), new Vector3(11f, 8.92f, 13.13f), new Vector3(0, 0, 90), new Vector3(-27.2f, 1, 4.39f)));
            Ledders.AddPart(new ModelPrimitive(Ledders, PrimitiveType.Cube, new Color32(255, 0, 0, 125), new Vector3(14.59f, 6.88f, 30.51f), new Vector3(0, 0, 90), new Vector3(-19.6f, 1, 4.39f)));
            Ledders.AddPart(new ModelPrimitive(Ledders, PrimitiveType.Cube, new Color32(255, 0, 0, 125), new Vector3(29f, 16.08f, 33.49f), new Vector3(0, 0, 90), new Vector3(-18f, 1, 4.39f)));
            Ledders.AddPart(new ModelPrimitive(Ledders, PrimitiveType.Cube, new Color32(255, 0, 0, 125), new Vector3(36.35f, 23.3f, 16.02f), new Vector3(0, 90, 90), new Vector3(-18f, 1, 4.39f)));
            Ledders.AddPart(new ModelPrimitive(Ledders, PrimitiveType.Cube, new Color32(255, 0, 0, 125), new Vector3(32.43f, 23.19f, -2.98f), new Vector3(0, 0, 90), new Vector3(-18f, 1, 4.39f)));
            Ledders.AddPart(new ModelPrimitive(Ledders, PrimitiveType.Cube, new Color32(255, 0, 0, 125), new Vector3(22.35f, 4.97f, -3.09f), new Vector3(0, 0, 90), new Vector3(-27.2f, 1, 4.39f)));
            Ledders.AddPart(new ModelPrimitive(Ledders, PrimitiveType.Cube, new Color32(255, 0, 0, 125), new Vector3(29.85f, 10.54f, -23.85f), new Vector3(0, 0, 90), new Vector3(-30.97f, 1, 4.39f)));
            Ledders.AddPart(new ModelPrimitive(Ledders, PrimitiveType.Cube, new Color32(255, 0, 0, 125), new Vector3(29.85f, 19.43f, -33.29f), new Vector3(0, 0, 90), new Vector3(-13.3f, 1, 4.39f)));
            Ledders.AddPart(new ModelPrimitive(Ledders, PrimitiveType.Cube, new Color32(255, 0, 0, 125), new Vector3(14.44f, 21.12f, -33.29f), new Vector3(0, 180, 90), new Vector3(-18f, 1, 4.39f)));

            foreach(var ledder in Ledders.Primitives)
            {
                ledder.GameObject.AddComponent<LedderComponent>();
            }
        }
        public void RestartEvent()
        {
            Timing.CallDelayed(10f, () =>
            {
                Timing.RunCoroutine(Cycle(), "hideandseek_time");
            });
        }
        public void InitRandomPlayer()
        {
            if (Player.List.Count() > 20)
            {
                AddNewPlayers(5);
            }
            else if (Player.List.Count() > 5 && Player.List.Count() <= 20)
            {
                AddNewPlayers(3);
            }
            else if (Player.List.Count() <= 5)
            {
                AddNewPlayers(1);
            }
        }
        public void AddNewPlayers(int count)
        {
            for (int i = 0; i < count; i++)
            {
                List<Player> list = Player.List.Where(r => r.Role != RoleType.Spectator).ToList();
                Player player = list.RandomItem();
                player.Role = RoleType.NtfCaptain;
                player.ClearInventory();
                player.EnableEffect("Scp207");
                player.ChangeEffectIntensity("Scp207", 4);
            }
        }
        public void GrenadeDeath(Player player, string Reason)
        {
            GrenadeFrag grenade = new GrenadeFrag(ItemType.GrenadeHE);
            grenade.FuseTime = 0.5f;
            grenade.Base.transform.localScale = new Vector3(0, 0, 0);
            grenade.MaxRadius = 0.5f;
            grenade.Spawn(player.Position);
            player.ClearInventory();
            player.Kill(Reason);
        }
        public void EventEnd()
        {
            foreach(Player player in Player.List)
            {
                player.GodMode = false;
                player.DisableAllEffects();
                player.GameObject.AddComponent<BoxCollider>().size = new Vector3(1f, 1f, 1f);
            }
            if (Audio.Microphone.IsRecording) StopAudio();
            Timing.RunCoroutine(DestroyObjects(Model));
            Timing.RunCoroutine(DestroyObjects(Ledders));
            Timing.RunCoroutine(CleanUpAll());
        }
        // Ивенты
        public void OnSpawnEvent(SpawnEvent ev)
        {
            ev.Player.BlockSpawnTeleport = true;
        }
        public void OnJoin(JoinEvent ev)
        {
            ev.Player.Role = RoleType.Spectator;
        }
    }
}
