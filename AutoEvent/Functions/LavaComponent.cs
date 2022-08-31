using Qurre;
using Qurre.API;
using Qurre.API.Controllers;
using Qurre.API.Controllers.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AutoEvent.Functions
{
    public class LavaComponent : MonoBehaviour
    {
        private BoxCollider collider;
        private void Start()
        {
            collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
        }
        void OnTriggerStay(Collider other)
        {
            if (Player.Get(other.gameObject) is Player)
            {
                var pl = Player.Get(other.gameObject);
                if (pl.Role > 0)
                {
                    if (pl.Hp < 1)
                    {
                        GrenadeFrag grenade = new GrenadeFrag(ItemType.GrenadeHE);
                        grenade.FuseTime = 0.5f;
                        grenade.Base.transform.localScale = new Vector3(0, 0, 0);
                        grenade.MaxRadius = 0.5f;
                        grenade.Spawn(pl.Position);
                        pl.Kill("<color=red>Сгорел в Лаве!</color>");
                        pl.GameObject.GetComponent<BoxCollider>().enabled = false;
                    }
                    pl.Hp -= 0.5f;
                }
            }
        }
    }
}
