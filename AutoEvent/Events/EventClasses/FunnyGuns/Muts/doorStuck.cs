using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoEvent.Events.EventClasses.FunnyGuns.Muts
{
    internal class doorStuck : IMutator
    {
        public string devName => "badDoors";

        public string dispName => "<color=orange>Двери заклинило</color>";

        public string description => "Двери <color=red>не открываются с первого раза</color>, поэтому по ним нужно спамить <b>E</b>, пока они не откроются. Для дверей с ключ-картами нужны ключк-карты.";

        public void DisEngaged()
        {
            Qurre.Events.Player.InteractDoor -= doorInteract;
        }

        public bool DoIWantToEngage()
        {
            return true;
        }

        public void Engaged()
        {
            Qurre.Events.Player.InteractDoor += doorInteract;
        }

        private void doorInteract(Qurre.API.Events.InteractDoorEvent ev)
        {
            if (ev.Allowed)
            {
                bool shouldOpen = UnityEngine.Random.Range(1, 10) == 1;
                if (!shouldOpen)
                    ev.Allowed = false;
            }
        }
    }
}
