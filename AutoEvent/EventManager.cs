using AutoEvent.Functions;
using MEC;
using Qurre.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Qurre.API.Addons.Models;
using static AutoEvent.Functions.MainFunctions;
using Random = UnityEngine.Random;
using HarmonyLib;

namespace AutoEvent
{
    /// <summary>Менеджер ивентов</summary>
    internal static class EventManager
    {
        /// <summary>Автоивента активированы?</summary>
        public static bool IsActive = true;
        public static Harmony Harmony;

        public static int NumberOfEvents = 0;
        public static Interfaces.IEvent CurrentEvent;

        /// <summary>Загрузить ивент</summary>
        public static void LoadEvent(Interfaces.IEvent ev)
        {
            // залупа неработающая пошла она нахуй
            Interfaces.IEvent now = ev;
            CurrentEvent.OnStart();
            now.OnStart();
        }

        /// <summary>Инициализировать менеджер ивентов</summary>
        public static void Init()
        {
            // Счетчик проведенных ивентов
            if (NumberOfEvents < 10)
            {
                NumberOfEvents++;
                // Инициируем переменную CurrentEvent
                //CurrentEvent = null;
                // Делаем таймер на N секунд
                LobbyTimer = new TimeSpan(0, 0, 25); // 40
                // запускаем корутину Цикл
                Timing.RunCoroutine(Cycle(), "AutoEvents_Cycle");
                // Спавн лобби
                SpawnLobbyRoom();
                // Массовый телепорт игроков на лобби
                MainFunctions.TeleportAndChangeRolePlayers(Player.List.ToList(), RoleType.ClassD, EventManager.LobbyPosition + new Vector3(0, 6.67f, 0));
            }
            // если прошло много ивентов, то конец раунда
            else
            {
                //CurrentEvent = null;
                //EndEventParametres();
                Map.ClearBroadcasts();
                Map.Broadcast("<color=red><b><i>Конец Авто-Ивента</i></b></color>\n" +
                    "<color=yellow>Перезапуск обычного раунда...</color>", 10);
                Timing.CallDelayed(10f, () =>
                {
                    Server.Restart(); // Потом допилить и просто сделать Round.Restart();
                });
            }
        }

        /// <summary>Деинициализировать менеджер ивентов</summary>
        public static void Deinit()
        {
            // Обнуляем
            LobbyTimer = new TimeSpan(0, 0, 0);
            // Киляем корутину
            Timing.KillCoroutines("AutoEvents_Cycle");
            // Удаляем лобби
            Timing.CallDelayed(2f, () => LobbyModel.Destroy());
        }

        /// <summary>Цикл обработок</summary>
        private static IEnumerator<float> Cycle()
        {
            // Включаем патч
            Harmony = new Harmony("kotoxleb.autoevents");
            Harmony.PatchAll();

            while (true)
            {
                // Лобби
                //if (CurrentEvent == null)
                //{
                //    CheckPlayers();
                //}

                Patches.BetterHintsManager.Cycle();

                // Счетчик таймера
                LobbyTimer = new TimeSpan(0, LobbyTimer.Minutes, LobbyTimer.Seconds - 1);
                if (LobbyTimer.Minutes == 0 && LobbyTimer.Seconds == 0)
                {
                    //LoadEvent(LobbyEventCircles.Values.First(x => x.Name == EventsVotes.First().Key));
                    LobbyEventCircles.Values.First(x => x.Name == EventsVotes.First().Key).OnStart();
                    Deinit();
                    // Выключаем патч
                    Harmony.UnpatchAll();
                    yield break;
                }

                yield return Timing.WaitForSeconds(1f);
            }
        }

        #region LobbyRoom
        public static TimeSpan LobbyTimer;

        private static Model LobbyModel = new Model("checkplayers", new Vector3(0, 0, 0));
        internal static readonly Vector3 LobbyPosition = new Vector3(166.53f, 1047.55f, -62.21f);

        private const float CircleRadius = 6;
        internal static Dictionary<Vector3, Interfaces.IEvent> LobbyEventCircles = new Dictionary<Vector3, Interfaces.IEvent>()
        {
            // Заражение (Красный круг)
            { LobbyPosition + new Vector3(0, 0.5f, 20), new InfectionEvent() },
            // Вышибалы с Мячиком (Оранжевый круг)
            { LobbyPosition + new Vector3(-14, 0.25f, 14), new BounceEvent() },
            // Тюрьма Саймона (Жёлтый круг)
            { LobbyPosition + new Vector3(-20, 0.25f, 0), new JailEvent() },
            // Смертельная Вечеринка (Зелёный круг)
            { LobbyPosition + new Vector3(-14, 0.25f, -14), new DeathParty() },
            // Атомный Побег (Голубой круг)
            { LobbyPosition + new Vector3(0, 0.25f, -20), new Escape() },
            // Обычная Игра (Синий круг)
            { LobbyPosition + new Vector3(14, 0.25f, -14), new Event() },
            // Прыжок Веры (Фиолетовый круг)
            { LobbyPosition + new Vector3(20, 0.25f, 0), new Glass() },
            // Пол - это ЛАВА (Розовый круг)
            { LobbyPosition + new Vector3(14f, 0.25f, 14f), new Lava() }
        };
        public static IReadOnlyDictionary<string, int> EventsVotes
        {
            get
            {
                try
                {
                    var value = new Dictionary<string, int>();

                    foreach (var ev in EventManager.LobbyEventCircles.Values)
                    {
                        if (value.ContainsKey(ev.Name)) continue;
                        value.Add(ev.Name, ev.Votes);
                    }

                    value = value.OrderBy(x => value.Count - x.Value).ToDictionary(x => x.Key, x => x.Value);

                    return value;
                }
                catch (System.Exception e)
                {
                    Qurre.Log.Error($"{e.Message}\n{e}");
                }
                return null;
            }
        }

        /// <summary>Заспавнить лобби комнату</summary>
        private static void SpawnLobbyRoom()
        {
            CreatingMapFromJson("Lobby.json", LobbyPosition, out LobbyModel);
            var model = new ModelPrimitive(LobbyModel, PrimitiveType.Sphere, Color.white, new Vector3(0, 0, 0), Vector3.zero);
            LobbyModel.AddPart(model);
            model.GameObject.transform.parent = LobbyModel.GameObject.transform;
        }

        /// <summary>Проверить позиции игроков</summary>
        private static void CheckPlayers()
        {
            try
            {
                LobbyEventCircles.Values.ToList().ForEach(e => e.Votes = 0);

                foreach (Player p in Player.List)
                {
                    var pos = LobbyEventCircles.Keys.OrderBy((vec) => (vec - p.Transform.position).sqrMagnitude).First();
                    if (Vector3.Distance(p.Position, pos) < CircleRadius)
                    {
                        LobbyEventCircles[pos].Votes += 1;
                    }
                }
            }
            catch (Exception e)
            {
                Qurre.Log.Error(e.Message + "\n"+e);
            }
        }
        #endregion
    }
}