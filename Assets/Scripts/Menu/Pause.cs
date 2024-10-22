using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace tp2
{
    public class Pause : MonoBehaviour
    {
        public GameObject menu;
        // Update is called once per frame
        void Update()
        {
            //If no players, Pause wont work
            if (PlayerTypeExtensions.getObject(PlayerType.Atlas) == null && PlayerTypeExtensions.getObject(PlayerType.Chroma) == null) { return; }
            if (Input.GetButtonDown("Pause"))
            {
                toggleMenu();
            }
        }

        public void toggleMenu()
        {
            toggleMenu(!menu.activeSelf);
        }

        public void toggleMenu(bool state)
        {
            menu.SetActive(state);
        }

        public void switchScenes(int scene)
        {
            NetManager.instance.updateScene(scene);
        }

        public void disconnect()
        {
            try
            {
                NetManager.instance.DisconnectPlayer(PlayerTypeExtensions.getLocalPlayer().GetComponent<NetworkObject>());
            }
            catch { };
        }
    }
}
