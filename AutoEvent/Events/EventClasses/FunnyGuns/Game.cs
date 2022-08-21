using MEC;
using Qurre.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

public delegate void StageChange(int stageNum);

namespace AutoEvent.Events.EventClasses.FunnyGuns
{
    internal class Game
    {
        static bool unfairSpawn = false;
        public static event StageChange StageChangeEvent;

        public static void EngageEvent()
        {
            Events.FunnyGuns.Stage = 0;
            Events.FunnyGuns.SecondsBeforeNextStage = 30;
            Events.FunnyGuns.EventActive = true;
            Events.FunnyGuns.RespawnAvaliable = false;

            int NTFTickets = 0;
            int CITickets = 0;
            if (Qurre.API.Player.List.Count() % 2 == 0)
            {
                unfairSpawn = false;
                NTFTickets = Qurre.API.Player.List.Count() / 2;
                CITickets = Qurre.API.Player.List.Count() / 2;
            }
            else
            {
                //NTF Always has one extra, therefore, balancing nessesary.
                unfairSpawn = true;
                NTFTickets = Qurre.API.Player.List.Count() / 2 + 1;
                CITickets = Qurre.API.Player.List.Count() / 2;
            }

            foreach (var pl in Qurre.API.Player.List)
            {
                bool selNTF = UnityEngine.Random.Range(0, 1) < 1;
                if (selNTF && NTFTickets > 0)
                {
                    NTFTickets--;
                    pl.SetRole(RoleType.NtfSergeant);
                    pl.ClearInventory();
                    pl.AddItem(ItemType.Flashlight);
                    pl.AddItem(ItemType.KeycardNTFCommander);
                    pl.AddItem(ItemType.ArmorCombat);
                    pl.AddItem(ItemType.SCP500);
                    pl.AddItem(ItemType.Adrenaline);
                    pl.AddItem(ItemType.Medkit);
                    pl.AddItem(ItemType.GunE11SR);
                }
                else if (selNTF)
                {
                    CITickets--;
                    pl.SetRole(RoleType.ChaosRifleman);
                    pl.ClearInventory();
                    pl.AddItem(ItemType.Flashlight);
                    pl.AddItem(ItemType.KeycardChaosInsurgency);
                    pl.AddItem(ItemType.ArmorCombat);
                    pl.AddItem(ItemType.SCP500);
                    pl.AddItem(ItemType.Adrenaline);
                    pl.AddItem(ItemType.Medkit);
                    pl.AddItem(ItemType.GunE11SR);
                }
                else if (!selNTF && CITickets > 0)
                {
                    CITickets--;
                    pl.SetRole(RoleType.ChaosRifleman);
                    pl.ClearInventory();
                    pl.AddItem(ItemType.Flashlight);
                    pl.AddItem(ItemType.KeycardChaosInsurgency);
                    pl.AddItem(ItemType.ArmorCombat);
                    pl.AddItem(ItemType.SCP500);
                    pl.AddItem(ItemType.Adrenaline);
                    pl.AddItem(ItemType.Medkit);
                    pl.AddItem(ItemType.GunE11SR);
                }
                else if (!selNTF)
                {
                    NTFTickets--;
                    pl.SetRole(RoleType.NtfSergeant);
                    pl.ClearInventory();
                    pl.AddItem(ItemType.Flashlight);
                    pl.AddItem(ItemType.KeycardNTFCommander);
                    pl.AddItem(ItemType.ArmorCombat);
                    pl.AddItem(ItemType.SCP500);
                    pl.AddItem(ItemType.Adrenaline);
                    pl.AddItem(ItemType.Medkit);
                    pl.AddItem(ItemType.GunE11SR);
                }
            }

            //All players are spawned by now, doing funny stuff...
            Timing.RunCoroutine(StageController(), "stagecont");
            Timing.RunCoroutine(HUD.HudCoroutine(), "hud");
        }

        public static IEnumerator<float> InstantDeath()
        {
            while (Events.FunnyGuns.EventActive)
            {
                if (Events.FunnyGuns.Stage >= 6)
                {
                    foreach (var pl in Qurre.API.Player.List)
                    {
                        pl.Damage(1f, "<b><color=red>Внезапная смерть</color></b>");
                    }
                }
                yield return Timing.WaitForSeconds(0.25f);
            }
        }

        private static IEnumerator<float> StageController()
        {
            while (Events.FunnyGuns.EventActive)
            {
                if (Events.FunnyGuns.SecondsBeforeNextStage > 0)
                {
                    if (Events.FunnyGuns.Stage < 6)
                    {
                        Events.FunnyGuns.SecondsBeforeNextStage--;
                    }
                }
                else
                {
                    Events.FunnyGuns.Stage++;
                    Events.FunnyGuns.SecondsBeforeNextStage = 50;

                    try
                    {
                        StageChangeEvent.Invoke(Events.FunnyGuns.Stage);
                    }
                    catch (Exception ex)
                    {
                        Qurre.Log.Info("Stage changed, but nothing was subscribed to stage change event. How strange!");
                    }

                    if (Events.FunnyGuns.Stage >= 2 && Events.FunnyGuns.Stage < 5)
                    {
                        AssignRandomMutator();
                    }
                }
                yield return Timing.WaitForSeconds(1f);
            }
            Qurre.Log.Info($"dead");
        }

        static private Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            return
              assembly.GetTypes()
                      .Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal))
                      .ToArray();
        }

        private static void AssignRandomMutator()
        {
            var types = GetTypesInNamespace(Assembly.GetExecutingAssembly(), "AutoEvent.Events.EventClasses.FunnyGuns.Muts");
            int tries = 50;
            while (tries > 0)
            {
                int randsel = UnityEngine.Random.Range(0, types.Length);
                var type = types[randsel];
                if (type.GetInterface("IMutator") is null)
                {
                    Qurre.Log.Error($"Selected mutator ({type.Name}) does not inherit from IMutator (AutoEvent.Events.EventClasses.FunnyGuns.Muts.IMutator) interface. This is an issue, report it to the developers!");
                }
                else
                {
                    Qurre.Log.Info($"Mutator {type.Name} is safe (all methods/properties are present), Checking for eligility to engage.");
                    var mut_obj = Activator.CreateInstance(type);
                    bool duplicate = false;
                    foreach (var list_mut in Events.FunnyGuns.Muts)
                    {
                        if (list_mut.devName == (string)type.GetProperty("devName").GetValue(mut_obj))
                        {
                            duplicate = true;
                            break;
                        }
                    }
                    if (duplicate)
                    {
                        Qurre.Log.Warn("Mutator has been already engaged. Trying another one");
                    }
                    else
                    {
                        if (type.GetMethod("DoIWantToEngage").Invoke(mut_obj, null).Equals(true)) //Blindly trusting, that this method returns bool (it would be pretty bad if it didn't)
                        {
                            Qurre.Log.Info("All checks have been passed and we are ready to engage the mutator!");
                            try
                            {
                                type.GetMethod("Engaged").Invoke(mut_obj, null);
                            }
                            catch (Exception ex)
                            {
                                Qurre.Log.Error($"An exception occured, when mutator was engaging. Exception (+stack trace): {ex}");
                                return;
                            }
                            Events.FunnyGuns.Muts.Add((IMutator)mut_obj);
                            Map.Broadcast($"Был запущен мутатор \"{type.GetProperty("dispName").GetValue(mut_obj)}\"\n<size=18>{type.GetProperty("description").GetValue(mut_obj)}</size>", 10);
                            Qurre.Log.Info("Mutator has been engaged and is now operational!");
                            return;
                        }
                        else
                        {
                            Qurre.Log.Warn("Mutator has reported that it doesn't want to engage. Trying another one");
                        }
                    }
                }
                tries--;
            }
            Qurre.Log.Error($"Mutator assignment failed! Ran out of tries.");
        }
    }
}
