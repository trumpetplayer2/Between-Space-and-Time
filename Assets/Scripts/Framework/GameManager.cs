using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace tp2
{
    public class GameManager : NetworkBehaviour
    {
        public static GameManager instance;
        public NetworkVariable<bool> atlasInCutscene = new NetworkVariable<bool>(false);
        public NetworkVariable<bool> chromaInCutscene = new NetworkVariable<bool>(false);

        private void Awake()
        {
            if (instance != null)
            {
                //Refresh instance if it already exists
                Destroy(instance.gameObject);
            }
            instance = this;
            DontDestroyOnLoad(this);
            NetPlayer.paused = false;
        }

        private void Start()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        [Rpc(SendTo.Server)]
        public void finishDialogueRpc(PlayerType player, bool state)
        {
            switch (player)
            {
                case PlayerType.Atlas:
                    atlasInCutscene.Value = state;
                    break;
                case PlayerType.Chroma:
                    chromaInCutscene.Value = state;
                    break;
                case PlayerType.None:
                    atlasInCutscene.Value = state;
                    chromaInCutscene.Value = state;
                    break;
            }
        }

        public bool getAtlasInCutscene()
        {
            return atlasInCutscene.Value;
        }

        public bool getChromaInCutscene()
        {
            return chromaInCutscene.Value;
        }

        private void OnSceneUnloaded(Scene current)
        {
            if (NetManager.instance == null)
            {
                return;
            }
            foreach (InitializePlayer init in NetManager.instance.players)
            {
                if (init == null) continue;
                foreach (Box child in init.gameObject.GetComponentsInChildren<Box>(true))
                {
                    if (child != null)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
        }
    }
}
