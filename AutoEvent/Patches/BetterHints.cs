using System.Collections.Generic;
using System.Linq;
using MEC;
using Hints;
using HarmonyLib;
using Qurre.API;

namespace AutoEvent.Patches
{
    /// <summary>Патч на Hint'ы</summary>
    [HarmonyPatch(typeof(Player), nameof(Player.ShowHint))]
    internal static class HintPatch
    {
        internal static bool Prefix(Player __instance, string text, float duration = 1f) => !EventManager.IsActive;
    }

    /// <summary>Менеджер улучшенных Hint'ов</summary>
    internal static class BetterHintsManager
    {
        /// <summary>Фикс для позиционирования Hint'ов по вертикали</summary>
        private const float FixHintVOffset = -11;

        //public static IReadOnlyDictionary<string, int> EventsVotes => EventManager.EventsVotes;

        /// <summary>
        /// Цикл обработки
        /// </summary>
        /*internal static void Cycle()
        {
            foreach (Player pl in Player.List)
            {
                try
                {
                    string str = "<line-height=0%>";
                    
                    // Hint Fix
                    str += $"<voffset={FixHintVOffset}em> </voffset>\n";

                    str += $"<align=\"right\"><voffset={FixHintVOffset + 20}em><b><color=#ACFF42>Ивенты</color></b></voffset></align>\n";
                    int i = 0;

                    // AutoEvents
                    if (EventsVotes != null)
                    {
                        foreach (var keys in EventsVotes.Keys)
                        {
                            if (keys == "Noname") continue;
                            str += $"<align=\"right\"><voffset={FixHintVOffset + 19 - i}em><b><color=yellow>{EventsVotes[keys]} {keys}</color></b></voffset></align>\n";
                            i++;
                        }
                    }

                    str += $"<align=\"right\"><voffset={FixHintVOffset + 19 - i}em><b><color=#FF4242>До запуска: {EventManager.LobbyTimer.Minutes}:{EventManager.LobbyTimer.Seconds}</color></b></voffset></align>\n";

                    for (i = 30; i < 40; i++) str += $"<voffset={i + FixHintVOffset}em> </voffset>\n";

                    // Отображаем Hint
                    pl.HintDisplay.Show(new TextHint(str + "</line-height>", new HintParameter[] { new StringHintParameter("") }, null, 1.1f));
                }
                catch { }
            }
        }*/
    }
}