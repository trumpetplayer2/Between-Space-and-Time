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
        /// <summary>
        /// Get the GameObject corresponding to a certain player type
        /// </summary>
        /// <param name="type">Player Type to search for</param>
        /// <returns>Player Object if exists, otherwise Null</returns>
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
        /// <summary>
        /// Reset values after disconnecting
        /// </summary>
        /// <param name="player">Client ID of the player disconnecting</param>
        public static void disconnect(ulong player)
        {
            PlayerType temp = getEnumOf(player);
            switch (temp)
            {
                case PlayerType.Atlas:
                    AtlasObject = null;
                    return;
                case PlayerType.Chroma:
                    ChromaObject = null;
                    return;
            }
            localPlayer = null;
        }
        /// <summary>
        /// Gets the local player type.
        /// </summary>
        /// <returns>The PlayerType of the Local Player</returns>
        public static PlayerType getLocalPlayerType()
        {
            return getEnumOf(localPlayer);
        }
        /// <summary>
        /// Get the local player
        /// </summary>
        /// <returns>The Gameobject corresponding to the local player</returns>
        public static GameObject getLocalPlayer()
        {
            return localPlayer;
        }
        /// <summary>
        /// Initialize local player based on if the player has a rigid body.
        /// </summary>
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
        }
        /// <summary>
        /// Try to fetch the PlayerType of an object
        /// </summary>
        /// <param name="obj">Object to check the Type of</param>
        /// <returns>Player Type based on layer</returns>
        public static PlayerType getEnumOf(GameObject obj)
        {
            if (obj == null) return PlayerType.None;
            if (!obj.tag.ToLower().Equals("player")) return PlayerType.None;
            switch (obj.layer)
            {
                case 8:
                case 10:
                case 13:
                case 6:
                    return PlayerType.Atlas;
                case 9:
                case 14:
                case 11:
                case 7:
                    return PlayerType.Chroma;
                default:
                    return PlayerType.None;
            }
        }
        /// <summary>
        /// Try to fetch the PlayerType of a layer
        /// </summary>
        /// <param name="i">Layer to check</param>
        /// <returns>Player Type based on layer</returns>
        public static PlayerType getEnumOf(int i)
        {
            switch (i)
            {
                case 6:
                    return PlayerType.Atlas;
                case 7:
                    return PlayerType.Chroma;
                default:
                    return PlayerType.None;
            }
        }
        /// <summary>
        /// Get the player type based off of the User ID
        /// </summary>
        /// <param name="pid">User ID to get Player Type of</param>
        /// <returns>Player Type based on the User ID</returns>
        public static PlayerType getEnumOf(ulong pid)
        {
            return getEnumOf(getObject(pid));
        }
        /// <summary>
        /// Returns user ID. If none available, return 10
        /// </summary>
        /// <param name="obj">Object to check</param>
        /// <returns>User ID of Player</returns>
        public static ulong getUserId(GameObject obj)
        {
            return getUserId(getTypeof(obj));
        }

        /// <summary>
        /// Returns user ID. If none available, return 10
        /// </summary>
        /// <param name="type">Player Type to check</param>
        /// <returns>User ID of Player</returns>
        public static ulong getUserId(PlayerType type)
        {
            switch (type)
            {
                case PlayerType.Atlas:
                    if (AtlasObject == null) return 10;
                    if (AtlasObject.GetComponent<NetworkObject>() == null) return 10;
                    return AtlasObject.GetComponent<NetworkObject>().OwnerClientId;
                case PlayerType.Chroma:
                    if (ChromaObject == null) return 10;
                    if (ChromaObject.GetComponent<NetworkObject>() == null) return 10;
                    return ChromaObject.GetComponent<NetworkObject>().OwnerClientId;
                default:
                    return 10;
            }
        }

        /// <summary>
        /// Get the Player Game Object based on User ID
        /// </summary>
        /// <param name="pid">User ID to get Player of</param>
        /// <returns>Gameobject corresponding to the player controlled by the User ID</returns>
        public static GameObject getObject(ulong pid)
        {
            if (AtlasObject.GetComponent<NetworkObject>().OwnerClientId == pid)
            {
                return AtlasObject;
            }
            if(ChromaObject.GetComponent<NetworkObject>().OwnerClientId == pid)
            {
                return ChromaObject;
            }
            return null;
        }
        /// <summary>
        /// Get the Player Type of an object
        /// </summary>
        /// <param name="obj">Object to fetch the Player Type of</param>
        /// <returns>Player Type of Obj</returns>
        public static PlayerType getTypeof(GameObject obj)
        {
            return getEnumOf(obj);
        }
        /// <summary>
        /// Get the PlayerType of a id
        /// </summary>
        /// <param name="val">0 for Atlas, 1 for Chroma, Anything else for None</param>
        /// <returns>Playertype of ID val</returns>
        public static PlayerType getTypeof(int val)
        {
            return getTypeFromValue(val);
        }
        /// <summary>
        /// I honestly dont know why this method exists, since this can be hardcoded, or fetched in a more efficient manner since I dont think this is used anywhere else
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static PlayerType getTypeFromValue(int val)
        {
            switch (val)
            {
                case 0:
                    return PlayerType.Atlas;
                case 1:
                    return PlayerType.Chroma;
                default:
                    return PlayerType.None;
            }
        }
        /// <summary>
        /// Get the box layer based on Player Type
        /// </summary>
        /// <param name="t">Type to get the layer of</param>
        /// <returns>An integer corresponding to the standard box layer</returns>
        public static int getBoxLayer(PlayerType t)
        {
            switch (t)
            {
                case PlayerType.Atlas:
                    return 15;
                case PlayerType.Chroma:
                    return 16;
                default:
                    return 12;
            }
        }

        public static PlayerType getFromBoxLayer(int i)
        {
            switch (i)
            {
                case 15:
                    return PlayerType.Atlas;
                case 16:
                    return PlayerType.Chroma;
                default:
                    return PlayerType.None;
            }
        }
        /// <summary>
        /// Fetch the game object based off of an arbritrary number. IDK why this exists
        /// </summary>
        /// <param name="val">0 = Atlas, 1 = Chroma</param>
        /// <returns>Player Object by ID</returns>
        public static GameObject getObject(int val)
        {
            switch (val)
            {
                case 0:
                    return AtlasObject;
                case 1:
                    return ChromaObject;
                default:
                    return null;
            }
        }
        /// <summary>
        /// Hardcoded ID fetcher. 
        /// </summary>
        /// <param name="obj">Game Object to fetch ID for</param>
        /// <returns>0 for Atlas, 1 for Chroma</returns>
        public static int getIDof(GameObject obj)
        {
            return getIDof(getTypeof(obj));
        }
        /// <summary>
        /// Get the ID of PlayerType type
        /// </summary>
        /// <param name="type">Player Type to check</param>
        /// <returns>0 for Atlas, 1 for Chroma</returns>
        public static int getIDof(PlayerType type)
        {
            switch (type)
            {
                case PlayerType.Atlas:
                    return 0;
                case PlayerType.Chroma:
                    return 1;
                default:
                    return -1;
            }
        }
        /// <summary>
        /// Check if the layer is only visible to a specific player
        /// </summary>
        /// <param name="Layer">Layer in question</param>
        /// <returns>Specific Player Type that can see it, else "None" for both</returns>
        public static PlayerType getPlayerVisible(int Layer)
        {
            PlayerType temp;
            switch (Layer)
            {
                case 8: case 10: case 13:
                    temp = PlayerType.Atlas;
                    break;
                case 9: case 11: case 14:
                    temp = PlayerType.Chroma;
                    break;
                default:
                    temp = PlayerType.None;
                    break;
            }
            //Check if Player exists, if not, return predefined value
            if (ChromaObject == null || AtlasObject == null) return temp;
            if (ChromaObject.GetComponent<NetPlayer>() == null || AtlasObject.GetComponent<NetPlayer>() == null) return temp;
            //Fetch Net Players
            NetPlayer ChromaPlayer = ChromaObject.GetComponent<NetPlayer>();
            NetPlayer AtlasPlayer = AtlasObject.GetComponent<NetPlayer>();
            //Check if Layer is in mask
            bool inChromaMask = ((ChromaPlayer.ChromaMask & (1 << Layer)) != 0);
            bool inAtlasMask = ((AtlasPlayer.AtlasMask & (1 << Layer)) != 0);
            if (inAtlasMask && !inChromaMask) return PlayerType.Atlas;
            if (inChromaMask && !inAtlasMask) return PlayerType.Chroma;
            return PlayerType.None;
        }

        public static bool isBoxLayer(GameObject obj)
        {
            return isBoxLayer(obj.layer);
        }

        public static bool isBoxLayer(int layer)
        {
            switch (layer)
            {
                case 10: case 11: case 12: case 15: case 16:
                    return true;
                default:
                    return false;
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
            if (NetManager.instance == null) return;
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

        [Rpc(SendTo.Owner)]
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
