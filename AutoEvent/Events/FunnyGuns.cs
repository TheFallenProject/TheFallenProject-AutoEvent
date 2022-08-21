using AutoEvent.Interfaces;
using MEC;
using Qurre.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoEvent.Events
{
    internal class FunnyGuns : IEvent
    {
        /*
         Funny Guns (Created by Treeshold (aka Darcy Gaming))
         Please, do not change anything (UNLESS U KNOW WHAT U R DOIN!)
         please, dont.
         */
        public string CommandName => "funnyGuns";

        public string Name => "Funny Guns";

        public string Color => "FFFFFF";

        public string Description => "Тупой ивент Treeshold (aka Darcy Gaming) с ржачными пушками. [Это бета версия, сервер, конечно, не упадёт, но ивент \"может сделать брррррррррр\"]";

        //is this depricated?
        public int Votes { get; set; }

        public static List<EventClasses.FunnyGuns.IMutator> Muts = new List<EventClasses.FunnyGuns.IMutator>(10);

        /// <summary>
        /// Is Event Running?
        /// </summary>A
        public static bool EventActive = false;

        /// <summary>
        /// Stage 0 = Prep;
        /// Stage 1 = Damage is 0.5x, No Mutators;
        /// Stage 2 = Damage is 1x, 1 Mutator;
        /// Stage 3 = Damage is 1x, 2 Mutators, Surface Lockdown;
        /// Stage 4 = Damage is 1x, 3 Mutators, LCZ Lockdown;
        /// Stage 5 = Damage is 1x, No Mutators, HCZ Lockdown;
        /// Stage 6 = Damage in 1x, No Mutators, Everyone takes 2 damage per second.;
        /// </summary>
        public static int Stage;

        /// <summary>
        /// How many seconds until next stage?
        /// </summary>
        public static int SecondsBeforeNextStage;

        /// <summary>
        /// Can reinforcement be requested from intercom?
        /// </summary>
        public static bool RespawnAvaliable;

        /// <summary>
        /// How many CI are alive?
        /// </summary>
        public static uint AliveCHI = 0;

        /// <summary>
        /// How many NTF are alive?
        /// </summary>
        public static uint AliveNTF = 0;

        public static List<Qurre.API.Objects.ZoneType> ldzones = new List<Qurre.API.Objects.ZoneType>(5);


        private static string ver = "0.6.0.0-BETA";

        public void OnStart()
        {
            Qurre.Log.Info($"FunnyGuns (ver. {ver}) has been started.");
            EventClasses.FunnyGuns.Game.EngageEvent();
            ldzones.Clear();

            Qurre.Events.Player.Dies += PlayerDeath;
            Qurre.Events.Player.IcomSpeak += IntercomUsed;
            Qurre.Events.Map.DoorDamage += DoorDestruct;
            Qurre.Events.Player.DamageProcess += Hurting;
            EventClasses.FunnyGuns.Game.StageChangeEvent += StageChangeEvent;
            EventClasses.FunnyGuns.Game.StageChangeEvent += AllowReinforcements;

            foreach (var door in Qurre.API.Map.Doors)
            {
                if (door.Type == Qurre.API.Objects.DoorType.Surface_Gate)
                {
                    door.Open = false;
                    door.Locked = true;
                    break;
                }
            }

            Timing.RunCoroutine(ldZonesChecker(), "ldZonesChecker");
        }

        public void OnStop()
        {
            Timing.KillCoroutines("stagecont");
            Timing.KillCoroutines("hud");
            Timing.KillCoroutines("ldZonesChecker");
            Timing.KillCoroutines("instantDeath");
            ldzones.Clear();

            foreach (var mut in Muts)
            {
                mut.DisEngaged();
            }

            foreach (var door in Qurre.API.Map.Doors)
            {
                if (door.Type == Qurre.API.Objects.DoorType.Surface_Gate)
                {
                    door.Locked = false;
                    break;
                }
            }

            Muts.Clear();

            Qurre.Events.Player.Dies -= PlayerDeath;
            Qurre.Events.Player.IcomSpeak -= IntercomUsed;
            Qurre.Events.Map.DoorDamage -= DoorDestruct;
            Qurre.Events.Player.DamageProcess -= Hurting;
            EventClasses.FunnyGuns.Game.StageChangeEvent -= StageChangeEvent;
            EventClasses.FunnyGuns.Game.StageChangeEvent -= AllowReinforcements;

            foreach (var elev in Qurre.API.Map.Lifts)
            {
                if (elev.Type == Qurre.API.Objects.LiftType.GateA || elev.Type == Qurre.API.Objects.LiftType.GateB)
                    elev.Locked = false;
            }
            foreach (var elev in Qurre.API.Map.Lifts)
            {
                if (elev.Type == Qurre.API.Objects.LiftType.ElALeft || elev.Type == Qurre.API.Objects.LiftType.ElARight || elev.Type == Qurre.API.Objects.LiftType.ElBLeft || elev.Type == Qurre.API.Objects.LiftType.ElBRight)
                    elev.Locked = false;
            }
            foreach (var door in Qurre.API.Map.Doors)
            {
                if (door.Type == Qurre.API.Objects.DoorType.HCZ_Door)
                {
                    door.Locked = false;
                }
            }

            Qurre.Log.Info($"FunnyGuns (ver. {ver}) has been terminated.");
        }

        private IEnumerator<float> endgameChecker()
        {
            while (true)
            {
                int mtfs = Qurre.API.Player.List.Count(pl => pl.Team == Team.MTF);
                int cis = Qurre.API.Player.List.Count(pl => pl.Team == Team.CHI);
                if (mtfs == 0 || cis == 0)
                {
                    Plugin.ActiveEvent = null;
                    OnStop();

                    Map.ClearBroadcasts();
                    Map.Broadcast($"{(mtfs == 0 ? "Победа <color=green>хаоса</color>!" : "Победа <color=blue>мога</color>!")}", 10, true);
                }
            }
        }

        private void Hurting(Qurre.API.Events.DamageProcessEvent ev)
        {
            if (ev.Allowed && Stage == 1)
            {
                ev.Amount /= 2;
            }
            else if (ev.Allowed && Stage == 0)
            {
                ev.Allowed = false;
            }
        }

        private void DoorDestruct(Qurre.API.Events.DoorDamageEvent ev)
        {
            ev.Allowed = false;
        }

        private void AllowReinforcements(int stageNum)
        {
            if (stageNum % 2 == 0)
            {
                RespawnAvaliable = true;
            }
        }

        private void StageChangeEvent(int stageNum)
        {
            Qurre.API.Controllers.Cassie.Send(".g4", false, false, true);
            if (stageNum == 3)
            {
                //Surface LD
                foreach (var elev in Qurre.API.Map.Lifts)
                {
                    if (elev.Type == Qurre.API.Objects.LiftType.GateA || elev.Type == Qurre.API.Objects.LiftType.GateB)
                    elev.Locked = true;
                }
                ldzones.Add(Qurre.API.Objects.ZoneType.Surface);
            }
            else if (stageNum == 4) {
                //LCZ LD
                foreach (var elev in Qurre.API.Map.Lifts)
                {
                    if (elev.Type == Qurre.API.Objects.LiftType.ElALeft || elev.Type == Qurre.API.Objects.LiftType.ElARight || elev.Type == Qurre.API.Objects.LiftType.ElBLeft || elev.Type == Qurre.API.Objects.LiftType.ElBRight)
                        elev.Locked = true;
                }
                ldzones.Add(Qurre.API.Objects.ZoneType.Light);
            }
            else if (stageNum == 5)
            {
                //HCZ LD
                foreach (var door in Qurre.API.Map.Doors)
                {
                    if (door.Type == Qurre.API.Objects.DoorType.HCZ_Door)
                    {
                        door.Locked = true;
                        door.Open = false;
                    }
                }
                ldzones.Add(Qurre.API.Objects.ZoneType.Heavy);
            }
            else if (stageNum == 6)
            {
                Timing.RunCoroutine(instantDeath(), "instantDeath");
                foreach (var mut in Muts)
                {
                    mut.DisEngaged();
                }
                Muts.Clear();
            }
        }

        private IEnumerator<float> instantDeath()
        {
            while (true)
            {
                foreach (var pl in Qurre.API.Player.List)
                {
                    pl.Damage(1, "<color=red><b>Внезапная смерть.</b></color>");
                }
                yield return Timing.WaitForSeconds(0.25f);
            }
        }

        private IEnumerator<float> ldZonesChecker()
        {
            while (true)
            {
                foreach (var pl in Qurre.API.Player.List)
                {
                    if (ldzones.Contains(pl.Zone))
                    {
                        pl.EnableEffect(Qurre.API.Objects.EffectType.Blinded, 1);
                        pl.Damage(4f, "<color=red>Зона была отсечена.</color>");
                    }
                }
                yield return Timing.WaitForSeconds(0.25f);
            }
        }

        private void PlayerDeath(Qurre.API.Events.DiesEvent ev)
        {
            if (ev.Target.Team == Team.CHI)
            {
                EventClasses.FunnyGuns.HUD.CIRed += 2;
            }
            else if (ev.Target.Team == Team.MTF)
            {
                EventClasses.FunnyGuns.HUD.MTFRed += 2;
            }
        }

        private void IntercomUsed(Qurre.API.Events.IcomSpeakEvent ev)
        {
            ev.Allowed = false;
            if (RespawnAvaliable && (ev.Player.Team == Team.MTF || ev.Player.Team == Team.CHI))
            {
                Qurre.API.Controllers.Cassie.Send($"PITCH_0.2 .g3 PITCH_1 Attention . {(ev.Player.Team == Team.MTF ? "MTFUnit" : "Chaos Insurgency")} Has ordered more units", false, false, true);
                RespawnAvaliable = false;
                List<Qurre.API.Player> specs = new List<Qurre.API.Player>();
                var icomDoor = Qurre.API.Map.Doors.First(door => door.Type == Qurre.API.Objects.DoorType.Intercom);
                icomDoor.Open = true;
                icomDoor.Locked = true;
                Timing.CallDelayed(10f, () => icomDoor.Locked = false);
                foreach (var pl in Qurre.API.Player.List)
                {
                    //pl.Role == RoleType.Spectator
                    if (true)
                    {
                        specs.Add(pl);
                    }
                }
                for (int i = 0; i < 5; i++)
                {
                    if (specs.Count == 0)
                    {
                        break;
                    }
                    var pl = specs.RandomItem();
                    if (ev.Player.Team == Team.MTF)
                    {
                        pl.SetRole(RoleType.NtfSergeant);
                        pl.ClearInventory();
                        pl.AddItem(ItemType.KeycardNTFCommander);
                        pl.AddItem(ItemType.ArmorCombat);
                        pl.AddItem(ItemType.SCP500);
                        pl.AddItem(ItemType.Adrenaline);
                        pl.AddItem(ItemType.Medkit);
                        pl.AddItem(ItemType.GunE11SR);
                    }
                    else
                    {
                        pl.SetRole(RoleType.ChaosRifleman);
                        pl.ClearInventory();
                        pl.AddItem(ItemType.KeycardChaosInsurgency);
                        pl.AddItem(ItemType.ArmorCombat);
                        pl.AddItem(ItemType.SCP500);
                        pl.AddItem(ItemType.Adrenaline);
                        pl.AddItem(ItemType.Medkit);
                        pl.AddItem(ItemType.GunE11SR);
                    }
                    pl.GodMode = true;
                    Timing.CallDelayed(2f, () => { pl.Position = icomDoor.Position + new UnityEngine.Vector3(0, 1, 0); pl.GodMode = false; });
                    specs.Remove(pl);
                }
            }
        }
    }
}
