using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoEvent.Events.EventClasses.FunnyGuns.Muts
{
    internal class damageX2 : IMutator
    {
        public string devName => "damage2x";

        public string dispName => "<color=red>Повышенный урон</color>";

        public string description => "Урон <color=red>увеличен</color> в <color=yellow>2</color> раза!";

        public void DisEngaged()
        {
            Qurre.Events.Player.DamageProcess -= Hurting;
        }

        public bool DoIWantToEngage()
        {
            return true;
        }

        public void Engaged()
        {
            Qurre.Events.Player.DamageProcess += Hurting;
        }

        private void Hurting(Qurre.API.Events.DamageProcessEvent ev)
        {
            if (ev.Allowed)
            {
                ev.Amount *= 2;
            }
        }
    }
}
