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
using Random = UnityEngine.Random;

namespace AutoEvent.Functions
{
    public class FootballComponent : MonoBehaviour
    {
        private SphereCollider collider;
        private Rigidbody rigid;
        private void Start()
        {
            collider = gameObject.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            rigid = gameObject.AddComponent<Rigidbody>();
            gameObject.AddComponent<Rigidbody>().isKinematic = false;
            gameObject.AddComponent<Rigidbody>().useGravity = true;
            gameObject.AddComponent<Rigidbody>().mass = 0.5f;
            /*var material = new PhysicMaterial("Proverka")
            {
                staticFriction = 0f,
                dynamicFriction = 0f,
                bounciness = 2f
            };
            gameObject.AddComponent<SphereCollider>().material = material;*/
        }
        void OnCollisionEnter(Rigidbody other)
        {
            //gameObject.AddComponent<Rigidbody>().velocity
        }
        void FixedUpdate()
        {
            //rigid.velocity = new Vector3(5, 0, 0);
        }
    }
}
