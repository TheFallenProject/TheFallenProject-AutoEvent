using Qurre.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MEC;
using Qurre.API.Addons.Models;
using Qurre.API.Objects;
using Qurre;

namespace AutoEvent.Functions
{
    public static class MainFunctions
    {
        /// <summary>Очистить всех игроков.</summary>
        public static void ClearAllPlayers()
        {
            Player.List.ToList().ForEach(x => x.Kill());
        }

        /// <summary>Переспавнить игрока без телепортиции на точку спавна.</summary>
        public static void BlockAndChangeRolePlayer(Player player, RoleType role)
        {
            player.BlockSpawnTeleport = true; // Блокирует телепортацию при спавне
            player.Role = role;               // Меняем роль
        }

        public static void TeleportPlayersToPosition(List<Player> players, Vector3 pos) // input -> Player.List.ToList(), Position
        {
            players.ForEach(player => player.Position = pos);
        }

        public static void TeleportAndChangeRolePlayers(List<Player> players, RoleType type, Vector3 pos) // input -> Player.List.ToList(), Position
        {
            players.ForEach(player =>
            {
                player.Role = type;
                Timing.CallDelayed(2f, () =>
                {
                    player.Position = pos;
                });
            });
        }

        /// <summary>Переспавнить игроков без телепортиции на точку спавна.</summary>
        public static void BlockAndChangeRolePlayers(List<Player> players, RoleType role)
        {
            players.ForEach(x => BlockAndChangeRolePlayer(x, role));
        }

        /// <summary>Проиграть аудиофайл</summary>
        public static void PlayAudio(string path, byte volume, bool loop, string eventName)
        {
            Audio.PlayFromFile(Path.Combine(Path.Combine(Qurre.PluginManager.PluginsDirectory, "Audio"), path), volume, false, loop, 1920, 48000, eventName);
        }

        /// <summary>Остановить прогирывание</summary>
        public static void StopAudio()
        {
            Audio.Microphone.Skip();
        }

        /// <summary>Написать броадкаст всем игрокам с предвадительной очисткой очереди</summary>
        public static void BroadcastPlayers(string message, ushort time)
        {
            Player.List.ToList().ForEach(player =>
            {
                player.ClearBroadcasts();
                player.Broadcast(message, time);
            });
        }

        /// <summary>Написать сообщение всем игрокам в консоль</summary>
        public static void ConsolePlayers(string message)
        {
            Player.List.ToList().ForEach(x => x.SendConsoleMessage(message, "white"));
        }

        /// <summary>Загрузить карту из JSON</summary>
        public static void CreatingMapFromJson(string path, Vector3 pos, out Model model)
        {
            SchematicUnity.SchematicUnity.Load(Path.Combine(Path.Combine(Qurre.PluginManager.PluginsDirectory, "Map"), path), pos, out Model _model);
            model = _model;
        }

        /// <summary>Подготовить игру к ивенту</summary>
        public static void StartEventParametres()
        {
            Round.LobbyLock = true;
            Round.Lock = true;
            Round.Start();
        }

        /// <summary>Подготовить игру к нормальному режиму</summary>
        public static void EndEventParametres()
        {
            Round.LobbyLock = false;
            Round.Lock = false;

            ClearAllPlayers();

            Round.End();
        }
        public static IEnumerator<float> TimingBeginBroadcastPlayers(string eventName, float time) // time = 30
        {
            while (time > 0)
            {
                Map.ClearBroadcasts();
                Map.Broadcast($"<color=#D71868><b><i>{eventName}</i></b></color>\n<color=#ABF000>До начала ивента осталось <color=red>{time}</color> секунд.</color>", 1);

                yield return Timing.WaitForSeconds(1f);
                time--;
            }
            yield break;
        }
        /// <summary>Переспавнить игроков без телепортиции на точку спавна.</summary>
        public static void EnablePlayersEffect(List<Player> players, EffectType effect)
        {
            players.ForEach(x => x.EnableEffect(effect));
        }
        public static void DisablePlayersEffect(List<Player> players, EffectType effect)
        {
            players.ForEach(x => x.DisableEffect(effect));
        }
        public static IEnumerator<float> CleanUpAll()
        {
            foreach(var ragdoll in Map.Ragdolls)
            {
                ragdoll.Destroy();
                yield return Timing.WaitForSeconds(0.01f);
            }
            foreach (var pickup in Map.Pickups)
            {
                pickup.Destroy();
                yield return Timing.WaitForSeconds(0.01f);
            }
            yield break;
        }
        public static IEnumerator<float> DestroyObjects(Model model)
        {
            Log.Info("Запуск удаления");
            foreach (var prim in model.Primitives)
            {
                GameObject.Destroy(prim.GameObject);
                yield return Timing.WaitForSeconds(0.1f);
            }
            foreach (var light in model.Lights)
            {
                GameObject.Destroy(light.GameObject);
                yield return Timing.WaitForSeconds(0.1f);
            }
            model.Destroy();
            yield break;
        }
        public static bool IsHuman(Player player) => player.Team != Team.SCP && player.Team != Team.RIP;
        public static int HumanCount = Player.List.Count(r => r.Team != Team.SCP && r.Team != Team.RIP);
    }
}