using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoEvent.Events.EventClasses.FunnyGuns.Muts
{
    internal class LightsOut : IMutator
    {
        public string devName => "lightsOut";

        public string dispName => "<color=yellow>Нет света</color>";

        public string description => "По всему комплексу <color=yellow>отключен свет</color>. <color=yellow>Ищите фонарики</color> или <color=yellow>ставьте их на оружие</color>.";

        public void DisEngaged()
        {
            Qurre.API.Controllers.Lights.TurnOff(0);
        }

        public bool DoIWantToEngage()
        {
            return true;
        }

        public void Engaged()
        {
            Qurre.API.Controllers.Lights.TurnOff(9999);
        }
    } 
}
