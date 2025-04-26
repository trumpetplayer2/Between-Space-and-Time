using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tp2
{
    public class GravityZone : MonoBehaviour
    {
        public float GravityScale = 1.8f;
        Dictionary<Rigidbody2D, float> map = new Dictionary<Rigidbody2D, float>();
        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent<Rigidbody2D>(out var Body)) return;
            map.Add(Body, Body.gravityScale);
            Body.gravityScale = GravityScale;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent<Rigidbody2D>(out var Body)) return;
            Body.gravityScale = map[Body];
            map.Remove(Body);
        }
        //Prevent issue if in gravity zone and is destroyed or disabled somehow
        private void OnDisable ()
        {
            foreach(Rigidbody2D Body in map.Keys)
            {
                Body.gravityScale = map[Body];
                map.Remove(Body);
            }
        }
    }
}
