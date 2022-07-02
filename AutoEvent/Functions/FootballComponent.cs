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
        private SphereCollider sphere;
        private Rigidbody rigid;
        private void Start()
        {
            sphere = gameObject.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius = 1.1f;
            rigid = gameObject.AddComponent<Rigidbody>();
            rigid.isKinematic = false;
            rigid.useGravity = true;
            rigid.mass = 0.1f;
            rigid.drag = 0.1f;
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
            Log.Info("Проверка");
        }
        void FixedUpdate()
        {
            //rigid.velocity = new Vector3(5, 0, 0);
        }
    }
}
