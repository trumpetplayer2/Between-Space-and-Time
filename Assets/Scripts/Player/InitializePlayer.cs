using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace tp2
{
    public enum PlayerType
    {
        Atlas, Chroma
    }
    public class InitializePlayer : NetworkBehaviour
    {
        public PlayerType playerType;

        public void OnSceneLoaded()
        {
            spawnPositionsRpc();
        }

        [Rpc(SendTo.Server)]
        public void spawnPositionsRpc()
        {
            switch (playerType)
            {
                case PlayerType.Atlas:
                    transform.position = NetManager.aStart.position;
                    return;
                case PlayerType.Chroma:
                    transform.position = NetManager.cStart.position;
                    return;
            }
        }
    }
}
