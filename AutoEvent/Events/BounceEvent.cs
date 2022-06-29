 using AutoEvent.Functions;
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
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static AutoEvent.Functions.MainFunctions;
using Random = UnityEngine.Random;

namespace AutoEvent
{
    internal class BounceEvent : Interfaces.IEvent
    {
        public string Name => "Вышибалы с Мячиком";
        public string Description => "";
        public string Color => "FFFF00";
        public static Model Model { get; set; }
        public static TimeSpan EventTime { get; set; }
        public int Votes { get; set; }

        public void OnStart()
        {
            Plugin.IsEventRunning = true;
            Qurre.Events.Player.Join += OnJoin;
            Qurre.Events.Player.Damage += OnDamage;
            Qurre.Events.Round.TeamRespawn += OnTeamRespawning;
            Qurre.Events.Server.SendingRA += OnSendRA;
            OnEventStarted();
        }
        public void OnStop()
        {
            Plugin.IsEventRunning = false;
            Qurre.Events.Player.Join -= OnJoin;
            Qurre.Events.Player.Damage -= OnDamage;
            Qurre.Events.Round.TeamRespawn -= OnTeamRespawning;
            Qurre.Events.Server.SendingRA -= OnSendRA;
            Timing.CallDelayed(10f, () => EventEnd());
        }
        public void OnEventStarted()
        {
            EventTime = new TimeSpan(0, 0, 0);

            CreatingMapFromJson("Bounce.json", new Vector3(145.18f, 945.26f, -122.97f), out var model);
            Model = model;

            PlayAudio("PVZ_Moongrains.f32le", 20, true, "Bounce");

            // Создание и телепорт отрядов
            var count = 0;
            foreach (Player player in Player.List)
            {
                if (count % 2 == 0)
                {
                    player.Role = RoleType.NtfSergeant;
                    Timing.CallDelayed(2f, () =>
                    {
                        player.Position = Model.GameObject.transform.position + RandomPosition(true);
                        player.AllItems.ToList().ForEach(item => player.RemoveItem(item));
                    });
                }
                else
                {
                    player.Role = RoleType.ChaosConscript;
                    Timing.CallDelayed(2f, () =>
                    {
                        player.Position = Model.GameObject.transform.position + RandomPosition(false);
                        player.AllItems.ToList().ForEach(item => player.RemoveItem(item));
                    });
                }
                count++;
            }
            // Запуск ивента
            Timing.RunCoroutine(TimingBeginEvent($"Вышибалы", 15), "bounce_time");
        }
        // Отсчет до начала ивента
        public IEnumerator<float> TimingBeginEvent(string eventName, float time) // не используется но нужно
        {
            // Отсчёт
            for (float _time = time; _time > 0; _time--)
            {
                BroadcastPlayers($"<color=#D71868><b><i>{eventName}</i></b></color>\n" +
                    $"<color=#ABF000>До начала ивента осталось <color=red>{_time}</color> секунд.</color>", 1);
                yield return Timing.WaitForSeconds(1f);
            }
            // Запуск корутины начала ивента
            Timing.RunCoroutine(BallManager(), "BallStarted");
            yield break;
        }
        // Тайминг, который каждую секунду выдает мячик, а также считает время
        public IEnumerator<float> BallManager()
        {
            while (Player.List.Count(r=> r.Team == Team.MTF) > 0 && Player.List.Count(r => r.Team == Team.CHI) > 0)
            {
                BroadcastPlayers($"<color=#D71868><b><i>Вышибалы</i></b></color>\n" +
                $"<color=yellow><color=blue>{Player.List.Count(r => r.Team == Team.MTF)}</color> VS <color=green>{Player.List.Count(r => r.Team == Team.CHI)}</color></color>\n" +
                $"<color=yellow>Время ивента <color=red>{EventTime.Minutes}:{EventTime.Seconds}</color></color>", 1);

                foreach (Player player in Player.List)
                {
                    if ((player.Team == Team.MTF || player.Team == Team.CHI) && EventTime.Seconds % 5 == 0)
                    {
                        player.AddItem(ItemType.SCP018);
                    }
                }
                EventTime += TimeSpan.FromSeconds(1f);
                yield return Timing.WaitForSeconds(1f);
            }
            OnStop();
            yield break;
        }
        // Подведение итогов ивента и возврат в лобби
        public void EventEnd()
        {
            if (Player.List.Count(r => r.Team == Team.MTF) == 0)
            {
                BroadcastPlayers($"<color=#D71868><b><i>Вышибалы</i></b></color>\n" +
                $"<color=yellow>ПОБЕДИЛИ - <color=green>{Player.List.Count(r => r.Team == Team.CHI)} ХАОС</color></color>\n" +
                $"<color=yellow>Конец ивент: <color=red>{EventTime.Minutes}:{EventTime.Seconds}</color></color>", 10);
            }
            else if (Player.List.Count(r => r.Team == Team.CHI) == 0)
            {
                BroadcastPlayers($"<color=#D71868><b><i>Вышибалы</i></b></color>\n" +
                $"<color=yellow>ПОБЕДИЛИ - <color=blue>{Player.List.Count(r => r.Team == Team.MTF)} МОГ</color></color>\n" +
                $"<color=yellow>Конец ивент: <color=red>{EventTime.Minutes}:{EventTime.Seconds}</color></color>", 10);
            }
            // Ожидание рестарта лобби допустим внезапный рестарт негативно встретится, а тут подведение итогов ивента
                // Первым мы чистим трупы и оружия
                CleanUpAll();

                // Выключение музыки
                if (Audio.Microphone.IsRecording) StopAudio();
                // Рестарт Лобби
                // EventManager.Init();
                // Очистка карты Ивента
                // Затем чистим карту
                Log.Info("Запуск удаления");
                NetworkServer.UnSpawn(Model.GameObject);
                Timing.RunCoroutine(DestroyObjects(Model));
               // Player.List.ToList().ForEach(player => player.Role = RoleType.Tutorial);
        }
        public Vector3 RandomPosition(bool isMTF)
        {
            Vector3 position = new Vector3(0, 0, 0);
            var rand = Random.Range(0, 5);
            if (isMTF)
            {
                switch (rand)
                {
                    case 0: position = new Vector3(28.6f, 5.31f, 0.99f); break;
                    case 1: position = new Vector3(9.66f, 1.19f, 21.11f); break;
                    case 2: position = new Vector3(9.66f, 1.19f, 8.93f); break;
                    case 3: position = new Vector3(9.66f, 1.19f, -3.65f); break;
                    case 4: position = new Vector3(9.66f, 1.19f, -20.08f); break;

                }
            }
            else switch (rand)
                {
                    case 0: position = new Vector3(-27.46f, 5.59f, 0.99f); break;
                    case 1: position = new Vector3(-11.38f, 1.19f, -20.08f); break;
                    case 2: position = new Vector3(-11.38f, 1.19f, -3.65f); break;
                    case 3: position = new Vector3(-11.38f, 1.19f, 8.93f); break;
                    case 4: position = new Vector3(-11.38f, 1.19f, 21.11f); break;
                }
            return position;
        }
        // Ивенты
        public void OnDamage(DamageEvent ev)
        {
            if (ev.DamageType == DamageTypes.Scp018)
            {
                ev.Allowed = false;
                DamageTeleport(ev);
            }
        }
        public void DamageTeleport(DamageEvent ev)
        {
            // Изменяем роль при смерти и тепаем на вышку
            if (ev.Target.Team == Team.MTF)
            {
                ev.Target.Role = RoleType.Tutorial;
                Timing.CallDelayed(2f, () =>
                {
                    ev.Target.Position = Model.GameObject.transform.position + new Vector3(26.35f, 20.72f, 0.99f);
                });
            }
            else
            {
                ev.Target.Role = RoleType.Tutorial;
                Timing.CallDelayed(2f, () =>
                {
                    ev.Target.Position = Model.GameObject.transform.position + new Vector3(-27.46f, 20.72f, 0.99f);
                });
            }
        }
        public void OnJoin(JoinEvent ev)
        {
            ev.Player.Role = RoleType.Tutorial;
            Timing.CallDelayed(2f, () =>
            {
                ev.Player.Position = Model.GameObject.transform.position + new Vector3(-27.46f, 20.72f, 0.99f);
            });
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
