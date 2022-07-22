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

    class LedderComponent : MonoBehaviour
    {
        private BoxCollider collider;
        private void Start()
        {
            collider = gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            Log.Info("Успешно");
        }
        void OnTriggerEnter(Collider other)
        {
            Log.Info("косание");
            var pl = Player.Get(other.gameObject);
            pl.Position = new Vector3(pl.Position.x, pl.Position.y + 1, pl.Position.z);
        }
    }
}
