using MEC;
using Qurre.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoEvent.Events.EventClasses.FunnyGuns
{
    internal class HUD
    {
        public static int MTFRed = 0;
        public static int CIRed = 0;

        static string SecondsEnding(int num)
        {
            int[] cases = { 2, 0, 1, 1, 1, 2 };
            string[] titles = { "секунда", "секунды", "секунд" };
            return titles[(num % 100 > 4 && num % 100 < 20) ? 2 : cases[(num % 10 < 5) ? num % 10 : 5]];
        }

        public static IEnumerator<float> HudCoroutine()
        {
            while (Events.FunnyGuns.EventActive)
            {
                bool isRed = Events.FunnyGuns.SecondsBeforeNextStage % 2 == 0;
                string msg = string.Empty;
                string msgDead = string.Empty;
                for (int i = 0; i < 12; i++)
                {
                    msg += "\n";
                    msgDead += "\n";
                }
                
                if (Events.FunnyGuns.Stage != 0)
                {
                    if (Events.FunnyGuns.RespawnAvaliable)
                    {
                        int specs = Qurre.API.Player.List.Count(pl => pl.Role == RoleType.Spectator);
                        if (specs > 5)
                            specs = 5;
                        if (specs > 0)
                        {
                            msg += $"<color=#c5ff84>Вызов подкрепления доступен. Используйте интерком для респавна {specs} членов вашей команды.</color>\n";
                        }
                        else
                        {
                            msg += $"<color=#c5ff84>Вызов подкрепления доступен, но нет спектаторов, которые бы могли быть возрождены. Отстаивайте интерком или потратьте его впустую.</color>\n";
                        }
                    }
                    if (Events.FunnyGuns.Stage == 1)
                    {
                        msg += $"<color=#c5ff84>1 Стадия: Урон снижен в 2 раза.</color>\n";
                    }
                    if (Events.FunnyGuns.Stage == 2)
                    {
                        msg += $"<color={(isRed ? "red" : "white")}>Закрытие поверхности в следующей стадии.</color>\n";
                    }
                    if (Events.FunnyGuns.Stage == 3)
                    {
                        msg += $"<color={(isRed ? "red" : "white")}>Закрытие лайт зоны в следующей стадии.</color>\n";
                    }
                    if (Events.FunnyGuns.Stage == 4)
                    {
                        msg += $"<color={(isRed ? "red" : "white")}>Закрытие хард зоны в следующей стадии.</color>\n";
                    }
                    if (Events.FunnyGuns.Stage == 5)
                    {
                        msg += $"<color={(isRed ? "red" : "white")}>Внезапная смерть и отключение всех мутаторов в следующей стадии.</color>\n";
                    }
                    string stageColor = "white";
                    switch (Events.FunnyGuns.Stage)
                    {
                        case 1:
                            stageColor = "white";
                            break;
                        case 2:
                            stageColor = "white";
                            break;
                        case 3:
                            stageColor = "white";
                            break;
                        case 4:
                            stageColor = "white";
                            break;
                        case 5:
                            stageColor = "white";
                            break;
                        case 6:
                            stageColor = "white";
                            break;
                    }

                    Events.FunnyGuns.AliveNTF = (uint)Qurre.API.Player.List.Count(pl => pl.Team == Team.MTF);
                    Events.FunnyGuns.AliveCHI = (uint)Qurre.API.Player.List.Count(pl => pl.Team == Team.CHI);
                    msg += $"Живы: {(MTFRed > 0 ? "<color=red>" : "")}{Events.FunnyGuns.AliveNTF}{(MTFRed > 0 ? "</color>" : "")} <color=blue>MTF</color>, {(CIRed > 0 ? "<color=red>" : "")}{Events.FunnyGuns.AliveCHI}{(CIRed > 0 ? "</color>" : "")} <color=green>CI</color>\n" +
                        $"Стадия <color={stageColor}>{Events.FunnyGuns.Stage}</color>, {(Events.FunnyGuns.Stage >= 6 ? "<color=red><b>Внезапная смерть</b></color>" : $"До следуюшей стадии {Events.FunnyGuns.SecondsBeforeNextStage} {SecondsEnding(Events.FunnyGuns.SecondsBeforeNextStage)}")}.";
                    if (Events.FunnyGuns.Muts.Count > 0)
                    {
                        msg += "\nАктивные мутаторы: ";
                        for (int i = 0; i < Events.FunnyGuns.Muts.Count; i++)
                        {
                            msg += Events.FunnyGuns.Muts.ElementAt(i).dispName;
                            if (i == Events.FunnyGuns.Muts.Count - 1)
                                msg += ".";
                            else
                                msg += ", ";
                        }
                    }
                }
                else
                {
                    // GET OUTTA HERE U PLACEHOLDER
                    // msg += $"<b>FG_BETA_TEST_PREP_STAGE ({Events.FunnyGuns.SecondsBeforeNextStage}s before stage 1)</b>";
                    msg += "<b>Funny Guns. Стадия подготовки.</b>\n<color=yellow>Суть ивента: </color>Ваша задача - уничтожить всю вражескую команду. Во время игры будут добавляться мутаторы и закрываться зоны. Также вы можете вызывать подкрепление до 5 игроков в интеркоме." +
                        " <b>Гейт на улице будет постоянно закрыт, единственный способ продвигаться - через комплекс.</b>\n";
                    msg += $"До следуюшей стадии {Events.FunnyGuns.SecondsBeforeNextStage} {SecondsEnding(Events.FunnyGuns.SecondsBeforeNextStage)}";
                    if (Events.FunnyGuns.fun < 10)
                    {
                        msg += "\n<color=gray>Hoppediah Plantar FTW</color>";
                    }
                }
                if (CIRed > 0)
                    CIRed--;
                if (MTFRed > 0)
                    MTFRed--;
                foreach (var pl in Player.List)
                {
                    pl.ShowHint(msg, 1);
                }
                yield return Timing.WaitForSeconds(0.5f);
            }
        }
    }
}
