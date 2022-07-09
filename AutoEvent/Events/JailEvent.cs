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
    internal class JailEvent : Interfaces.IEvent
    {
        public string Name => "Тюрьма Саймона";
        public string Description => "";
        public string Color => "FFFF00";

        public static string DayWeek = string.Empty;
        public static Model Maps { get; set; }
        public static Model Button { get; set; }
        public static Model Doors { get; set; }
        public static Model JailerDoors { get; set; }
        public static Dictionary<GameObject, float> JailerDoorsTime { get; set; } = new Dictionary<GameObject, float>();
        public static Model Spawners { get; set; }
        public static TimeSpan EventTime { get; set; }
        public static bool isDoorsOpen = false;
        public int Votes { get; set; }

        public string CommandName => "jailbreak";

        public void OnStart()
        {
            Plugin.IsEventRunning = true;
            Qurre.Events.Player.RoleChange += OnChangeRole;
            Qurre.Events.Player.Shooting += OnShootEvent;
            Qurre.Events.Player.InteractLocker += OnInteractLocker;
            Qurre.Events.Player.PickupItem += OnPickupItem;
            Qurre.Events.Round.TeamRespawn += OnTeamRespawning;
            Qurre.Events.Server.SendingRA += OnSendRA;
            OnEventStarted();
        }
        public void OnStop()
        {
            Plugin.IsEventRunning = false;
            Qurre.Events.Player.RoleChange -= OnChangeRole;
            Qurre.Events.Player.Shooting -= OnShootEvent;
            Qurre.Events.Player.InteractLocker -= OnInteractLocker;
            Qurre.Events.Player.PickupItem -= OnPickupItem;
            Qurre.Events.Round.TeamRespawn -= OnTeamRespawning;
            Qurre.Events.Server.SendingRA -= OnSendRA;
            Timing.CallDelayed(5f, () => EventEnd());
        }

        public void OnEventStarted()
        {
            // Создание карты
            CreatingMapFromJson("Jail.json", new Vector3(145.18f, 945.26f, -122.97f), out var model);
            Maps = model;
            // включить огонь по своим
            Server.FriendlyFire = true;
            // Создание кнопки
            Button = new Model("button", Maps.GameObject.transform.position + new Vector3(28.26f, 10.852f, 16.188f));
            // Создание дверей
            CreatePrisonerDoors();
            // Создание спавнеров оружия
            CreateSpawner();
            // Создание дверей у охраны
            CreateJailerDoors();
            // Создаем мячик
            // CreateBall();
            // Запуск музыки
            /*Timing.CallDelayed(5, () =>
            {
                PlayAudio("Instruction.f32le", 40, false, "Instruction");
            });*/
            // Новый раунд ивента
            WaitingEvent();
        }
        public void WaitingEvent()
        {
            // День недели новый
            DayWeek = RandomMessage.RandomItem();
            // Разделение игроков на тюремщиков и зеков
            for (int i = 0; i <= Player.List.Count() / 10; i++) // = убрал Правило 1 на 10 человек
            {
                var jailer = Player.List.ToList().RandomItem();
                jailer.Role = RoleType.NtfCaptain;
                Timing.CallDelayed(2f, () =>
                {
                    jailer.Position = Maps.GameObject.transform.position + RandomPosition(true);
                });
            }
            foreach (Player player in Player.List)
            {
                // Спавн
                if (player.Role != RoleType.NtfCaptain)
                {
                    player.Role = RoleType.ClassD;
                    Timing.CallDelayed(2f, () =>
                    {
                        player.Position = Maps.GameObject.transform.position + RandomPosition(false);
                    });
                }
            }
            // Очистка предметов у охраны
            // Запуск ивента
            Timing.RunCoroutine(Cycle(), "jail_time");
        }
        public IEnumerator<float> Cycle()
        {
            // Обнуление таймера
            EventTime = new TimeSpan(0, 0, 0);
            // Отсчет обратного времени
            for (int time = 10; time > 0; time--)
            {
                BroadcastPlayers($"<color=yellow>Ивент <color=red><b><i>Тюрьма Саймона</i></b></color>\n" +
                    $"До начала: <color=red>{time}</color> секунд</color>", 1);
                yield return Timing.WaitForSeconds(1f);
            }
            // Пока игроки с обоих команд живы
            while (Player.List.Count(r => r.Role == RoleType.ClassD) > 0 && Player.List.Count(r => r.Team == Team.MTF) > 0)
            {
                foreach (var door in JailerDoors.Primitives)
                {
                    if (JailerDoorsTime.ContainsKey(door.GameObject))
                    {
                        if (JailerDoorsTime[door.GameObject] <= 0)
                        {
                            door.GameObject.transform.position -= new Vector3(0f, -5f, 0f);
                            JailerDoorsTime.Remove(door.GameObject);
                        }
                        else JailerDoorsTime[door.GameObject] -= 0.5f;
                    }
                    foreach (Player player in Player.List)
                    {
                        if (Vector3.Distance(door.GameObject.transform.position, player.Position) < 3)
                        {
                            door.GameObject.transform.position += new Vector3(0f, -5f, 0f);

                            if (!JailerDoorsTime.ContainsKey(door.GameObject))
                            {
                                JailerDoorsTime.Add(door.GameObject, 2f);
                            }
                        }
                    }
                }
                EventTime = new TimeSpan(0, EventTime.Minutes, EventTime.Seconds + 1);
                BroadcastPlayers($"<size=20><color=red>Тюрьма Саймона</color>\n" +
                    $"<color=yellow>День: <color=cyan>{DayWeek}</color></color>\n" +
                    $"<color=yellow>Колво Зеков: <color=red>{Player.List.Count(r => r.Role == RoleType.ClassD)}</color></color>\n" +
                    $"<color=red>{EventTime.Minutes}:{EventTime.Seconds}</color></size>", 1);
                yield return Timing.WaitForSeconds(1f);
            }
            if (Player.List.Count(r => r.Team == Team.MTF) == 0)
            {
                BroadcastPlayers($"<color=red><b><i>Победа Заключенных</i></b></color>\n" +
                    $"<color=red>{EventTime.Minutes}:{EventTime.Seconds}</color>", 10);
            }
            if (Player.List.Count(r => r.Role == RoleType.ClassD) == 0)
            {
                BroadcastPlayers($"<color=blue><b><i>Победа Охранников</i></b></color>\n" +
                    $"<color=red>{EventTime.Minutes}:{EventTime.Seconds}</color>", 10);
            }
            // Конец под ивента и рестарт
            OnStop();
            yield break;
        }
        // Подведение итогов ивента и возврат в лобби
        public void EventEnd()
        {
            if (Audio.Microphone.IsRecording) StopAudio();
            // Очистка времени
            EventTime = new TimeSpan(0, 0, 0);
            DayWeek = string.Empty;
            JailerDoorsTime.Clear();
            isDoorsOpen = false;
            Server.FriendlyFire = false;

            Button.Destroy();
            Spawners.Destroy();
            Timing.RunCoroutine(DestroyObjects(Maps));
            Timing.RunCoroutine(DestroyObjects(Doors));
            Timing.RunCoroutine(DestroyObjects(JailerDoors));
            Timing.RunCoroutine(CleanUpAll());
        }
        public List<string> RandomMessage = new List<string>()
        {
            "Понедельник",
            "Вторник",
            "Среда",
            "Четверг",
            "Пятница",
            "Суббота"
        };
        // Манипуляции с дверьми и примитивами
        public Vector3 RandomPosition(bool isJailer)
        {
            Vector3 position = new Vector3(0, 0, 0);
            if (isJailer)
            {
                var rand = Random.Range(0, 2);
                switch (rand)
                {
                    case 0: position = new Vector3(12.194f, 1.56f, -2.59f); break;
                    case 1: position = new Vector3(18.39f, 1.56f, -2.59f); break;
                }
            }
            else
            {
                var rand = Random.Range(0, 15);
                switch (rand)
                {
                    case 0: position = new Vector3(6.31f, 2.13f, 44.975f); break;
                    case 1: position = new Vector3(15.63f, 2.13f, 44.975f); break;
                    case 2: position = new Vector3(27.14f, 2.13f, 44.975f); break;
                    case 3: position = new Vector3(37.21f, 2.13f, 44.975f); break;
                    case 4: position = new Vector3(48.36f, 2.13f, 44.975f); break;
                    case 5: position = new Vector3(4.04f, 7.13f, 44.975f); break;
                    case 6: position = new Vector3(14.11f, 7.13f, 44.975f); break;
                    case 7: position = new Vector3(24.7f, 7.13f, 44.975f); break;
                    case 8: position = new Vector3(37.4f, 7.13f, 44.975f); break;
                    case 9: position = new Vector3(48.36f, 7.13f, 44.975f); break;
                    case 10: position = new Vector3(4.04f, 12.18f, 44.975f); break;
                    case 11: position = new Vector3(13.38f, 12.18f, 44.975f); break;
                    case 12: position = new Vector3(25.52f, 12.18f, 44.975f); break;
                    case 13: position = new Vector3(36.96f, 12.18f, 44.975f); break;
                    case 14: position = new Vector3(48.04f, 12.18f, 44.975f); break;
                }
            }
            return position;
        }
        public List<Vector3> DoorsPosition = new List<Vector3>()
        {
            // 1 этаж
            new Vector3(8.83f, 2.91f, 41.325f),
            new Vector3(20.36f, 2.91f, 41.325f),
            new Vector3(31.42f, 2.91f, 41.325f),
            new Vector3(42.75f, 2.91f, 41.325f),
            new Vector3(53.72f, 2.91f, 41.325f),
            // 2 этаж
            new Vector3(8.83f, 8.026f, 41.325f),
            new Vector3(20.36f, 8.026f, 41.325f),
            new Vector3(31.42f, 8.026f, 41.325f),
            new Vector3(42.75f, 8.026f, 41.325f),
            new Vector3(53.72f, 8.026f, 41.325f),
            // 3 этаж
            new Vector3(8.83f, 13.08f, 41.325f),
            new Vector3(20.36f, 13.08f, 41.325f),
            new Vector3(31.42f, 13.08f, 41.325f),
            new Vector3(42.75f, 13.08f, 41.325f),
            new Vector3(53.72f, 13.08f, 41.325f),
        };
        public void CreatePrisonerDoors()
        {
            Doors = new Model("PrisonerDoors", new Vector3(145.18f, 945.26f, -122.97f));
            foreach (var pos in DoorsPosition)
            {
                Doors.AddPart(new ModelPrimitive(Doors, PrimitiveType.Cube, new Color32(0, 0, 0, 200), pos, Vector3.zero, new Vector3(3.88f, 4.55f, 1)));
            }
        }
        public void CreateJailerDoors()
        {
            JailerDoors = new Model("PrisonerDoors", new Vector3(145.18f, 945.26f, -122.97f));
            JailerDoors.AddPart(new ModelPrimitive(JailerDoors, PrimitiveType.Cube, new Color32(255, 0, 0, 125), new Vector3(44.254f, 3f, -10.252f), new Vector3(0f, 90f, 0f), new Vector3(6.68f, 5.43f, 0.64f)));
            JailerDoors.AddPart(new ModelPrimitive(JailerDoors, PrimitiveType.Cube, new Color32(255, 0, 0, 125), new Vector3(25.18f, 3f, -10.252f), new Vector3(0f, 90f, 0f), new Vector3(6.68f, 5.43f, 0.64f)));
            JailerDoors.AddPart(new ModelPrimitive(JailerDoors, PrimitiveType.Cube, new Color32(63, 45, 45, 128), new Vector3(15.524f, 3f, -4.61f), new Vector3(0f, 0f, 0f), new Vector3(4.59f, 5.43f, 0.64f)));
            // Двери на улицу или на паркур
            JailerDoors.AddPart(new ModelPrimitive(JailerDoors, PrimitiveType.Cube, new Color32(0, 255, 0, 125), new Vector3(55.31f, 3f, -24.08f), new Vector3(0f, 90f, 0f), new Vector3(6.68f, 5.43f, 0.64f)));
            JailerDoors.AddPart(new ModelPrimitive(JailerDoors, PrimitiveType.Cube, new Color32(0, 255, 0, 125), new Vector3(-10.36f, 3f, 10.04f), new Vector3(0f, 90f, 0f), new Vector3(6.68f, 5.43f, 0.64f)));
        }
        public static void CreateSpawner()
        {
            Spawners = new Model("Spawner", new Vector3(145.18f, 945.26f, -122.97f));
            Spawners.AddPart(new ModelLocker(Spawners, LockerPrefabs.RifleRack, new Vector3(15.82f, 1.2f, 3.53f), new Vector3(0f, 180f, 0f), new Vector3(1f, 1f, 1f)));
            Spawners.AddPart(new ModelLocker(Spawners, LockerPrefabs.RifleRack, new Vector3(15.82f, 0f, 3.53f), new Vector3(0f, 180f, 0f), new Vector3(1f, 1f, 1f)));
            Spawners.AddPart(new ModelLocker(Spawners, LockerPrefabs.RifleRack, new Vector3(15.82f, 2.4f, 3.53f), new Vector3(0f, 180f, 0f), new Vector3(1f, 1f, 1f)));
            Spawners.AddPart(new ModelLocker(Spawners, LockerPrefabs.LargeGun, new Vector3(24.83f, 1.02f, -18.76f), new Vector3(0f, -90f, 0f), new Vector3(-1.5f, 1.5f, 1.5f)));
            Spawners.AddPart(new ModelLocker(Spawners, LockerPrefabs.RegularMedkit, new Vector3(6.1f, -1.5f, -16.66f), new Vector3(0f, 90f, 0f), new Vector3(2f, 2f, 2f)));
            Spawners.AddPart(new ModelLocker(Spawners, LockerPrefabs.RegularMedkit, new Vector3(6.1f, -1.5f, -19.78f), new Vector3(0f, 90f, 0f), new Vector3(2f, 2f, 2f)));
            Spawners.AddPart(new ModelLocker(Spawners, LockerPrefabs.RegularMedkit, new Vector3(6.1f, -1.5f, -13.45f), new Vector3(0f, 90f, 0f), new Vector3(2f, 2f, 2f)));
            Spawners.AddPart(new ModelLocker(Spawners, LockerPrefabs.Pedestal500, new Vector3(20.29f, 0f, -0.25f), new Vector3(0f, -90f, 0f), new Vector3(1f, 1f, 1f)));
        }
        // Ивенты
        public void OnJoin(JoinEvent ev)
        {
            ev.Player.Role = RoleType.Spectator;
            ev.Player.ClearBroadcasts();
            ev.Player.Broadcast("<color=yellow>Привет, Игрок!\n" +
                "Сейчас проходит ивент <color=red>'Тюрьма'</color>" +
                "Ты мёртв, подожди некоторое время.</color>", 10);
        }
        public void OnShootEvent(ShootingEvent ev)
        {
            if (Vector3.Distance(ev.Shooter.LookingAt.transform.position, Button.GameObject.transform.position) < 3)
            {
                // открытие или закрытие дверей
                if (isDoorsOpen)
                {
                    Doors.GameObject.transform.position += new Vector3(3.13f, 0f, 0f);
                    isDoorsOpen = false;
                }
                else
                {
                    Doors.GameObject.transform.position -= new Vector3(3.13f, 0f, 0f);
                    isDoorsOpen = true;
                }
            }
        }
        public void OnChangeRole(RoleChangeEvent ev)
        {
            if (ev.NewRole == RoleType.ClassD)
            {
                ev.Player.Role = RoleType.ClassD;
                Timing.CallDelayed(2f, () =>
                {
                    ev.Player.Position = Maps.GameObject.transform.position + RandomPosition(false);
                });
            }
            else if (ev.NewRole == RoleType.NtfPrivate || ev.NewRole == RoleType.NtfSergeant || ev.NewRole == RoleType.NtfSpecialist || ev.NewRole == RoleType.NtfCaptain)
            {
                ev.Player.Role = RoleType.NtfCaptain;
                Timing.CallDelayed(2f, () =>
                {
                    ev.Player.Position = Maps.GameObject.transform.position + RandomPosition(true);
                });
            }
        }
        public void OnInteractLocker(InteractLockerEvent ev)
        {
            /*
            switch(ev.Locker.Type)
            {
                case LockerType.RifleRack: ev.Player.AddItem(ItemType.GunE11SR); break;
                case LockerType.Pedestal: ev.Player.AddItem(ItemType.GunCOM18); break;
                case LockerType.RegularMedkit: ev.Player.Hp = ev.Player.MaxHp; break;
                case LockerType.LargeGun:
                    {
                        ev.Player.AddItem(ItemType.ArmorCombat);
                        ev.Player.Ahp = ev.Player.MaxAhp;
                        break;
                    }
            }
            ev.Allowed = false;
            */
        }
        public void OnPickupItem(PickupItemEvent ev)
        {
            if (ev.Pickup.Type == ItemType.Coin)
            {
                ev.Allowed = false;
            }
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
