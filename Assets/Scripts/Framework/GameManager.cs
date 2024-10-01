using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

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
                Destroy(instance);
            }
            instance = this;
            DontDestroyOnLoad(this);
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
            Debug.Log(atlasInCutscene.Value + " " + chromaInCutscene.Value);
        }

        public bool getAtlasInCutscene()
        {
            return atlasInCutscene.Value;
        }

        public bool getChromaInCutscene()
        {
            return chromaInCutscene.Value;
        }
    }
}