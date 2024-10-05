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
        public static GameObject localPlayer;
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

        public static PlayerType getLocalPlayerType()
        {
            return getEnumOf(localPlayer);
        }

        public static void initializeLocalPlayer()
        {
            if (AtlasObject != null)
            {
                if (AtlasObject.GetComponent<Rigidbody2D>() != null)
                {
                    localPlayer = AtlasObject;
                }
            }
            if (ChromaObject != null)
            {
                if (ChromaObject.GetComponent<Rigidbody2D>() != null)
                {
                    localPlayer = ChromaObject;
                }
            }
            if(localPlayer == null)
            {
                Debug.LogWarning("Error Finding Local Player");
            }
        }

        public static PlayerType getEnumOf(GameObject obj)
        {
            if (obj == null) return PlayerType.None;
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
            updatePlayerList();
        }

        public override void OnDestroy()
        {
            switch (playerType)
            {
                case PlayerType.Atlas:
                    NetManager.instance.players[0] = null;
                    PlayerTypeExtensions.AtlasObject = null;
                    break;
                case PlayerType.Chroma:
                    NetManager.instance.players[1] = null;
                    PlayerTypeExtensions.ChromaObject = null;
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
                    PlayerTypeExtensions.AtlasObject = this.gameObject;
                    break;
                case PlayerType.Chroma:
                    transform.position = NetManager.cStart.position;
                    NetManager.instance.players[1] = this;
                    PlayerTypeExtensions.ChromaObject = this.gameObject;
                    break;
            }
        }

        public void updatePlayerList()
        {
            switch (playerType)
            {
                case PlayerType.Atlas:
                    PlayerTypeExtensions.AtlasObject = this.gameObject;
                    break;
                case PlayerType.Chroma:
                    PlayerTypeExtensions.ChromaObject = this.gameObject;
                    break;
            }
            //Attempt to initialize local player
            PlayerTypeExtensions.initializeLocalPlayer();
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
