using AutoEvent.Functions;
using AutoEvent.Interfaces;
using MEC;
using Mirror;
using Qurre;
using Qurre.API;
using Qurre.API.Addons.Models;
using Qurre.API.Events;
using Qurre.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static AutoEvent.Functions.MainFunctions;
using Map = Qurre.API.Map;
using Player = Qurre.API.Player;
using Random = UnityEngine.Random;
using Server = Qurre.API.Server;

using InventorySystem.Items;
using Qurre.API.Controllers;
using Qurre.API.Controllers.Items;
using Qurre.API.Objects;
using PlayerEvents = Qurre.Events.Player;
namespace AutoEvent.Events
{
    class AmongUS : IEvent
    {
        public string CommandName => "among";
        public string Name => "AmongUS";
        public string Color => "FF4242";
        public static Model taskd { get; set; }
        public static Model Model { get; set; }
        public static Model Bbody { get; set; }
        public static Model Door { get; set; }
        public static Model Screens { get; set; }
        public TimeSpan EventTime { get; set; }
        public static int takeitD1 = 0;
        public string Description => "Мафия (Запуская вы подтверждаете, что в случае краша сервера ВЫ несёте ответственность)";
        public static int tasks = 0;
        public string killrol;
        public int krcount = -1;
        public int plcount = 1;
        public int pcount = 0;
        public static int kilid = 0;
        public static int shid = 0;
        public int Votes { get; set; }
        public List<bool> Tasks = new List<bool>()
        {
             
        };
        public void OnStart()
        {
            kilid = 0;
            shid = 0;
            pcount = 0;
            plcount = 1;
            Qurre.Events.Player.Damage += OnDamage;
            Qurre.Events.Player.Join += OnJoin;
            Qurre.Events.Player.Shooting += OnShootEvent;
            Qurre.Events.Player.Leave += OnLeave;
            Voice.PressAltChat += OnAlt;
            OnEventStarted();
        }

        public void OnStop()
        {
            Qurre.Events.Player.Damage += OnDamage;
            Plugin.ActiveEvent = null;
            Qurre.Events.Player.Shooting -= OnShootEvent;
            Qurre.Events.Player.Join -= OnJoin;
            Qurre.Events.Player.Leave -= OnLeave;
            Timing.CallDelayed(10f, () => EventEnd());
        }
        public void OnAlt(PressAltChatEvent ev)
        {
            
        }
        public void OnEventStarted()
        {
            foreach (Player pl in Player.List)
            {
                pl.GameObject.AddComponent<BoxCollider>();
                pl.GameObject.AddComponent<BoxCollider>().size = new Vector3(3f, 1f, 3f);
            }
            BroadcastPlayers("<color=blue> -=-Among-=- </color>", 5);
            CreatingMapFromJson("Among.json", new Vector3(145.18f, 945.26f, -122.97f), out var model);
            Model = model;
            foreach (Player pl in Player.List)
            {
                pl.Role = Rolee.RandomItem();
                pl.ClearInventory();
                Timing.CallDelayed(2f, () => { pl.Position = RandomPosition(); });
                Log.Info($"Id игрока: {pl.Id} | Имя: {pl.Nickname} | pos: {pl.Position}");
                if(pl.Position.y < 800)
                {
                    pl.Position = new Vector3(78f, 949f, -171f);
                }
                plcount += 1;
                pcount += 1;
            }
            Map.Broadcast("<color=green> Скоро всё начнётся... </color> <color=red> v1.1 </color>", 10);
            Timing.WaitForSeconds(10f);
            var ran = Random.Range(2, plcount);
            var sh = Random.Range(2, plcount);
            if (sh == ran)
            {
                if (ran == plcount) { sh = ran - 1; }
                else if (ran == 2) { sh = plcount; }
                else { sh = ran + 1; }
            }
            foreach (Player pl in Player.List)
            {
                if (pl.Id == ran)
                {
                    Timing.CallDelayed(10f, () => { pl.ShowHint("<color=red> Вы стали !ХАКЕРОМ! </color> \n" + "Взломай терминалы чтобы ПОБЕДИТЬ", 10); });
                    Timing.CallDelayed(10f, () => { pl.AddItem(ItemType.GunCOM18, 1); });
                    kilid = ran;
                    killrol = pl.RoleName;
                    
                }
                else if (pl.Id == sh)
                {
                    Timing.CallDelayed(10f, () => { pl.ShowHint(" <color=blue> Вы из группы зачистки </color> \n" + "Убей хакеров!", 10); });
                    Timing.CallDelayed(10f, () => { pl.AddItem(ItemType.GunCOM18, 1); });
                    shid = pl.Id;
                }
                else
                {
                    Timing.CallDelayed(10f, () => { pl.ShowHint("<color=green> Вы РАБОЧИЙ! </color> \n" + "Работай и ищи хакеров", 10); });
                }
            }
            TrigCreate();
            DoorCreate();
            ScreenCreate();
        }
        public void OnShootEvent(ShootingEvent ev)
        {
            ev.Shooter.ClearInventory();
            ev.Shooter.ShowHint($"Перезарядка... 15 секунд");
            //Why do you store time IN A VARIABLE? THERE ARE NO OTHER REFERENCES TO IT OTHER THAN HERE?
            Timing.CallDelayed(15f, () => { ev.Shooter.AddItem(ItemType.GunCOM18); });
        }
        public void EventEnd()
        {
            if (Audio.Microphone.IsRecording) StopAudio();
            Timing.RunCoroutine(DestroyObjects(Model));
            Timing.RunCoroutine(CleanUpAll());
        }
        public void OnLeave(LeaveEvent ev)
        {
            if(ev.Player.Id == kilid)
            {
                Log.Info("Убийца вышел");
                Map.ClearBroadcasts();
                Map.Broadcast("Убийца вышел, конец ивента", 5);
                OnStop();
            }
            else if(ev.Player.Id == shid)
            {
                Log.Info("Шериф вышел");
                Map.ClearBroadcasts();
                Map.Broadcast("Шериф вышел, пистолет у мирного", 5);
                var ran = Random.Range(2, plcount);
                if (ran == kilid || ran == shid)
                {
                    while (ran != kilid && ran != shid)
                    {
                        ran = Random.Range(2, plcount);
                    }
                }
                foreach (Player pl in Player.List)
                {
                    if (ran == pl.Id)
                    {
                        pl.ShowHint("<color=yellow> Вам дали пистолет, убей убийцу! </color>");
                        pl.AddItem(ItemType.GunCOM18, 1);
                        shid = pl.Id;
                    }
                }
            }
        }
        public void OnDamage(DamageEvent ev)
        {

        }
        public Vector3 RandomPosition()
        {
            Vector3 position = new Vector3(0, 0, 0);
            var rand = Random.Range(0, 14);
            switch (rand)
            { //?
                case 1: position = new Vector3(78f, 949f, -171f); break;
                case 2: position = new Vector3(78f, 949f, -120f); break;
                case 3: position = new Vector3(17f, 949f, -155f); break;
                case 4: position = new Vector3(17f, 949f, -120f); break;
                case 5: position = new Vector3(-32f, 949f, -105f); break;
                case 6: position = new Vector3(-10f, 949f, -80f); break;
                case 7: position = new Vector3(-36f, 949f, -59f); break;
                case 8: position = new Vector3(96f, 949f, -57f); break;
                case 9: position = new Vector3(119f, 949f, -86f); break;
                case 10: position = new Vector3(206f, 949f, -134f); break;
                case 11: position = new Vector3(143f, 949f, -82f); break;
                case 12: position = new Vector3(107f, 949f, -119f); break;
                case 13: position = new Vector3(178f, 949f, -118f); break;
            }
            return position;
        }
        public List<RoleType> Rolee = new List<RoleType>()
        {
            RoleType.ClassD,
            RoleType.NtfCaptain,
            RoleType.ChaosConscript,
            RoleType.Scientist
        };
        public void TrigCreate()
        {
            taskd = new Model("trig", Model.GameObject.transform.position);
            taskd.AddPart(new ModelPrimitive(taskd, PrimitiveType.Cube, new Color32(255, 0, 1, 125), new Vector3(0.47f, 2.14f, -13.58f), new Vector3(0, 0, 0), new Vector3(2.657938f, 3.390858f, 1.928785f)));
            taskd.AddPart(new ModelPrimitive(taskd, PrimitiveType.Cube, new Color32(255, 0, 2, 125), new Vector3(-67.67f, 2.14f, -56.33f), new Vector3(0, 0, 0), new Vector3(2.657938f, 3.390858f, 1.928785f)));
            taskd.AddPart(new ModelPrimitive(taskd, PrimitiveType.Cube, new Color32(255, 0, 3, 125), new Vector3(-126.11f, 2.14f, -44.36f), new Vector3(0, 0, 0), new Vector3(2.657938f, 3.390858f, 1.928785f)));
            taskd.AddPart(new ModelPrimitive(taskd, PrimitiveType.Cube, new Color32(255, 0, 4, 125), new Vector3(-149.01f, 2.14f, -3.02f), new Vector3(0, 0, 0), new Vector3(2.657938f, 3.390858f, 1.928785f)));
            taskd.AddPart(new ModelPrimitive(taskd, PrimitiveType.Cube, new Color32(255, 0, 5, 125), new Vector3(-96.16f, 2.14f, 10.437f), new Vector3(0, 0, 0), new Vector3(2.657938f, 3.390858f, 1.928785f)));
            taskd.AddPart(new ModelPrimitive(taskd, PrimitiveType.Cube, new Color32(255, 0, 6, 125), new Vector3(-40.75f, 2.14f, 46.2f), new Vector3(0, 0, 0), new Vector3(2.657938f, 3.390858f, 1.928785f)));
            foreach (var trig in taskd.Primitives)
            {
                trig.GameObject.AddComponent<Trigger>();
            }
        }
        public void ScreenCreate()
        {
            Screens = new Model("screen", Model.GameObject.transform.position);
            Screens.AddPart(new ModelPrimitive(taskd, PrimitiveType.Cube, new Color32(255, 0, 0, 125), new Vector3(0.6083f, 2.831f, -14.14f), new Vector3(0, 0, 0), new Vector3(2.246967f, 1.708155f, 0.4473f)));
            Screens.AddPart(new ModelPrimitive(taskd, PrimitiveType.Cube, new Color32(255, 0, 0, 125), new Vector3(-67.59f, 2.831f, -56.91601f), new Vector3(0, 0, 0), new Vector3(2.657938f, 1.708155f, 1.928785f)));
            Screens.AddPart(new ModelPrimitive(taskd, PrimitiveType.Cube, new Color32(255, 0, 0, 125), new Vector3(-126.24f, 2.831f, -44.64f), new Vector3(0, 0, 0), new Vector3(2.657938f, 1.708155f, 1.928785f)));
            Screens.AddPart(new ModelPrimitive(taskd, PrimitiveType.Cube, new Color32(255, 0, 0, 125), new Vector3(-148.93f, 2.831f, -3.307f), new Vector3(0, 0, 0), new Vector3(2.657938f, 1.708155f, 1.928785f)));
            Screens.AddPart(new ModelPrimitive(taskd, PrimitiveType.Cube, new Color32(255, 0, 0, 125), new Vector3(-96.1f, 2.831f, 9.893997f), new Vector3(0, 0, 0), new Vector3(2.657938f, 1.708155f, 1.928785f)));
            Screens.AddPart(new ModelPrimitive(taskd, PrimitiveType.Cube, new Color32(255, 0, 0, 125), new Vector3(-40.75f, 2.831f, 45.849f), new Vector3(0, 0, 0), new Vector3(2.657938f, 1.708155f, 1.928785f)));
            foreach (var screen in Screens.Primitives)
            {
                screen.GameObject.AddComponent<Screens>();
            }
        }
        public void DoorCreate()
        {
            Door = new Model("door", Model.GameObject.transform.position);
            Door.AddPart(new ModelPrimitive(taskd, PrimitiveType.Cube, new Color32(27, 70, 19, 255), new Vector3(0.6083f, 2.831f, -14.14f), new Vector3(0, 0, 0), new Vector3(2.246967f, 1.708155f, 0.4473f)));
            foreach (var door in Door.Primitives)
            {
                door.GameObject.AddComponent<Doorr>();
            }
        }
        public void OnJoin(JoinEvent ev)
        {
            ev.Player.Role = RoleType.Spectator;
        }
    }
    class Taskcreate1S : MonoBehaviour 
    {
        
        public static bool istask1 = false;
        private BoxCollider collider;
        void Start()
        {
            collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
        }
        void OnTriggerStay(Collider other)
        {
            var pl = Player.Get(other.gameObject);
            if (istask1 == false)
            {
                pl.AddItem(ItemType.KeycardJanitor, 3);
                pl.ShowHint("<color=green> Вы взяли задание, отнеси это в зал А </color>", 5);
            }
            else if (istask1 == true)
            {
                pl.ShowHint("<color=red> Вы его сделали </color>", 5);
            }
        }
    }
    internal class Doorr : MonoBehaviour
    {
       private BoxCollider collider;
       private void Start()
       {
          collider = gameObject.AddComponent<BoxCollider>();
          collider.isTrigger = true;
          Log.Info("дверь");
       }
    }
    internal class Screens : MonoBehaviour
    {
        private BoxCollider collider;
        private void Start()
        {
            collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            Log.Info("экран");
        }
    }
    internal class Trigger : MonoBehaviour
    {
        private BoxCollider collider;
        private void Start()
        {
            collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            Log.Info("Триггер");
        }
        void OnTriggerStay(Collider other)
        {
            Log.Info("триггггггггггг");
            var pl = Player.Get(other.gameObject);
        }
    }
}

