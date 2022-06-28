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
    public class MoveComponent : MonoBehaviour
    {
        /*
        private BoxCollider collider;
        private void Start()
        {
            collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
        }
        void OnTriggerEnter(Collider other)
        {
            var pl = Player.Get(other.gameObject);
            //pl.EnableEffect(Qurre.API.Objects.EffectType.Stained, 1);
            Destroy(gameObject);
        }*/
        void Update()
        {
            transform.Rotate(0, 1f, 0);
        }
    }
}
