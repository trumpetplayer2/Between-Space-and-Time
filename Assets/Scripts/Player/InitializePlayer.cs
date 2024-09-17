using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace tp2
{
    public enum PlayerType
    {
        None, Atlas, Chroma
    }
    public class InitializePlayer : NetworkBehaviour
    {
        public PlayerType playerType;
        
        public void OnSceneLoaded()
        {
            spawnPositionsRpc();
        }

        public void Start()
        {
            spawnPositionsRpc();
        }

        public override void OnDestroy()
        {
            switch (playerType)
            {
                case PlayerType.Atlas:
                    NetManager.instance.players[0] = null;
                    break;
                case PlayerType.Chroma:
                    NetManager.instance.players[1] = null;
                    break;
            }
        }

        [Rpc(SendTo.Server)]
        public void spawnPositionsRpc()
        {
            switch (playerType)
            {
                case PlayerType.Atlas:
                    transform.position = NetManager.aStart.position;
                    NetManager.instance.players[0] = this;
                    return;
                case PlayerType.Chroma:
                    transform.position = NetManager.cStart.position;
                    NetManager.instance.players[1] = this;
                    return;
            }
        }
    }
}
