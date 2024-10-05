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
            if(PlayerTypeExtensions.AtlasObject.GetComponent<Rigidbody2D>() != null)
            {
                localPlayer = PlayerTypeExtensions.AtlasObject;
            }else if (PlayerTypeExtensions.ChromaObject.GetComponent<Rigidbody2D>() != null)
            {
                localPlayer = PlayerTypeExtensions.ChromaObject;
            }
            else
            {
                Debug.LogWarning("Error Finding Local Player");
                Destroy(this);
            }
        }

        // Update is called once per frame
        void Update()
        {
            int max = Mathf.Min(parallaxLayers.Length, parallaxSpeeds.Length);
            for(int i = 0; i < max; i++)
            {
                GameObject bg = parallaxLayers[i];
                Vector2 speed = parallaxSpeeds[i];
                bg.transform.localPosition = new Vector3(localPlayer.transform.position.x * speed.x, localPlayer.transform.position.y * speed.y, 0);
            }
        }
    }
}
