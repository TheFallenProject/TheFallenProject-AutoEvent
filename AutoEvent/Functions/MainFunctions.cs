using MEC;
using Qurre;
using Qurre.API;
using Qurre.API.Addons.Models;
using Qurre.API.Controllers.Items;
using Qurre.API.Objects;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

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
            player.BlockSpawnTeleport = true;
            player.Role = role;
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
        /// <summary>Очистка мусора после ивента.</summary>
        public static IEnumerator<float> CleanUpAll()
        {
            foreach (Pickup pickup in Map.Pickups)
            {
                pickup.Base.DestroySelf();
            }
            foreach (Ragdoll ragdoll in UnityEngine.Object.FindObjectsOfType<Ragdoll>())
            {
                Object.Destroy(ragdoll.gameObject);
            }
            yield break;
        }
        /// <summary>Очистка карты после ивента.</summary>
        public static IEnumerator<float> DestroyObjects(Model model)
        {
            foreach (var prim in model.Primitives)
            {
                GameObject.Destroy(prim.GameObject);
            }
            foreach (var light in model.Lights)
            {
                GameObject.Destroy(light.GameObject);
            }
            model.Destroy();
            yield break;
        }
    }
}