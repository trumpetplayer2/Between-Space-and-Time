using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace tp2
{
    public class HideOnStart : MonoBehaviour
    {
        SpriteRenderer[] renderers;
        public bool freezeAtZero = false;
        // Start is called before the first frame update
        void Start()
        {
            renderers = GetComponentsInChildren<SpriteRenderer>();
            if (renderers == null) Destroy(this);
            Invoke("hide", 0.001f);
        }

        private void Update()
        {
            if (freezeAtZero)
            {
                this.transform.position = new Vector3(0, 0, 0);
            }
        }
        public void hide()
        {
            foreach(SpriteRenderer r in renderers)
            {
                r.enabled = false;
            }
        }

        public void show()
        {
            foreach (SpriteRenderer r in renderers)
            {
                r.enabled = true;
            }
            Destroy(this);
        }

        public void OnDestroy()
        {
            foreach(NetworkObject child in GetComponentsInChildren<NetworkObject>())
            {
                if (child.transform.parent != this) continue;
                child.transform.parent = null;
            }
        }
    }
}
