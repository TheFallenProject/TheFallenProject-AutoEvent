using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoEvent.Events.EventClasses.FunnyGuns.Muts
{
    internal class LowVisibility : IMutator
    {
        public string devName => "lowVisibility";

        public string dispName => "<color=orange>Густой туман</color>";

        public string description => "Видимость снижена.";

        public void DisEngaged()
        {
            Qurre.Events.Player.ItemUsed -= UsedSCP500;
            Qurre.Events.Player.RoleChange -= RoleChange;
            foreach (var pl in Qurre.API.Player.List)
                pl.DisableEffect(Qurre.API.Objects.EffectType.Amnesia);
        }

        public bool DoIWantToEngage()
        {
            return true;
        }

        public void Engaged()
        {
            foreach (var pl in Qurre.API.Player.List)
                pl.EnableEffect(Qurre.API.Objects.EffectType.Amnesia);
            Qurre.Events.Player.ItemUsed += UsedSCP500;
            Qurre.Events.Player.RoleChange += RoleChange;
        }

        private void RoleChange(Qurre.API.Events.RoleChangeEvent ev)
        {
            if (ev.NewRole != RoleType.Spectator)
            {
                Timing.CallDelayed(5f, () => ev.Player.EnableEffect(Qurre.API.Objects.EffectType.Amnesia));
            }
        }

        private void UsedSCP500(Qurre.API.Events.ItemUsedEvent ev)
        {
            if (ev.Item.TypeId == ItemType.SCP500)
            {
                Timing.CallDelayed(5f, () => ev.Player.EnableEffect(Qurre.API.Objects.EffectType.Amnesia));
            }
        }
    }
}
