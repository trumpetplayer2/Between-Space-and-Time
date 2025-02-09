using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tp2
{
    public class Parallax : MonoBehaviour
    {
        public GameObject[] parallaxLayers;
        public Vector2[] parallaxSpeeds;
        GameObject localPlayer;

        private void Start()
        {
            fetchLocalPlayer();
        }

        void fetchLocalPlayer()
        {
            localPlayer = Camera.main.gameObject;
            if (localPlayer == null)
            {
                //If local player not found, try again in a second
                Invoke("fetchLocalPlayer", 1);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (localPlayer == null) return;
            try
            {
                int max = Mathf.Min(parallaxLayers.Length, parallaxSpeeds.Length);
                for (int i = 0; i < max; i++)
                {
                    GameObject bg = parallaxLayers[i];
                    Vector2 speed = parallaxSpeeds[i];
                    bg.transform.localPosition = new Vector3(localPlayer.transform.position.x * speed.x, localPlayer.transform.position.y * speed.y, 0);
                }
            }
            catch { }
        }
    }
}
