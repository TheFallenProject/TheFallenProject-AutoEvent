using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoEvent.Events.EventClasses.FunnyGuns.Muts
{
    internal class o5ForEveryone : IMutator
    {
        public string devName => "keycardInflation";

        public string dispName => "<color=orange>Ключ от всех дверей</color>";

        public string description => "У <color=yellow>каждого</color> игрока теперь есть <color=yellow>ключ-карта совета O5</color>. К будущим юнитам подкрепления данный мутатор не применятся.";

        public void DisEngaged()
        {
            
        }

        public bool DoIWantToEngage()
        {
            return true;
        }

        public void Engaged()
        {
            foreach (var pl in Qurre.API.Player.List)
            {
                pl.AddItem(ItemType.KeycardO5);
            }
        }
    }
}
