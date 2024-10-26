using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace tp2
{
    public class Pause : MonoBehaviour
    {
        public GameObject menu;
        public UnityEngine.UI.Button lobbyButton;

        private void Start()
        {
            if (lobbyButton == null) return;
            if (PlayerTypeExtensions.getLocalPlayer() == null) return;
            NetworkObject temp = PlayerTypeExtensions.getLocalPlayer().GetComponent<NetworkObject>();
            if (temp == null) return;
            lobbyButton.interactable = temp.IsOwnedByServer;
        }

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
                //PlayerTypeExtensions.getLocalPlayer().GetComponent<NetworkObject>()
                NetManager.instance.DisconnectPlayer();
            }
            catch { };
        }
    }
}
