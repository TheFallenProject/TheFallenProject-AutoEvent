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
    public class GlassComponent : MonoBehaviour
    {
        private BoxCollider collider;
        private void Start()
        {
            collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
        }
        void OnTriggerEnter(Collider other)
        {
            var pl = Player.Get(other.gameObject);
            Destroy(gameObject);
        }
    }
}
