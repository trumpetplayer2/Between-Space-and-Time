using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace tp2
{
    public static class PlayerTypeExtensions
    {
        public static GameObject AtlasObject;
        public static GameObject ChromaObject;
        public static GameObject getObject (this PlayerType type)
        {
            switch (type)
            {
                case PlayerType.Atlas:
                    return AtlasObject;
                case PlayerType.Chroma:
                    return ChromaObject;
                default: return null;
            }
        }

        public static PlayerType getEnumOf(GameObject obj)
        {
            if (!obj.tag.ToLower().Equals("player")) return PlayerType.None;
            switch (obj.layer)
            {
                case 6:
                    return PlayerType.Atlas;
                case 7:
                    return PlayerType.Chroma;
                default:
                    return PlayerType.None;
            }
        }
    }
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
                    PlayerTypeExtensions.AtlasObject = this.gameObject;
                    break;
                case PlayerType.Chroma:
                    NetManager.instance.players[1] = null;
                    PlayerTypeExtensions.ChromaObject = this.gameObject;
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

        [Rpc(SendTo.Server)]
        public void updateCameraRpc()
        {
            Vector3 temp = new Vector3(0,0,0);
            switch (playerType)
            {
                case PlayerType.Atlas:
                    temp = NetManager.aStart.position;
                    NetManager.instance.players[0] = this;
                    break;
                case PlayerType.Chroma:
                    temp = NetManager.cStart.position;
                    NetManager.instance.players[1] = this;
                    break;
            }
            gameObject.GetComponent<NetPlayer>().updateCameraTrackerRpc(temp);
        }
    }
}
