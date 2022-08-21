using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoEvent.Events.EventClasses.FunnyGuns.Muts
{
    internal class PassiveRegen : IMutator
    {
        public string devName => "passiveRegen";

        public string dispName => "<color=green>Пассивная регенерация</color>";

        public string description => "Все игроки восстанавливают <color=green>2хп</color> в <color=yellow>секунду</color>.";

        public void DisEngaged()
        {
            Timing.KillCoroutines("passiveregen");
        }

        public bool DoIWantToEngage()
        {
            return true;
        }

        public void Engaged()
        {
            foreach (var pl in Qurre.API.Player.List)
            {
                Timing.RunCoroutine(passiveRegenCoroutine(pl), "passiveregen");
            }
        }

        private IEnumerator<float> passiveRegenCoroutine(Qurre.API.Player pl)
        {
            while (true)
            { 
                pl.Heal(1f, false);
                yield return Timing.WaitForSeconds(0.5f);
            }
        }
    }
}
