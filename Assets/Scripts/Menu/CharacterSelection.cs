using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace tp2
{
    public class CharacterSelection : NetworkBehaviour
    {
        public static NetworkObject Atlas;
        public static NetworkObject Chroma;
        public NetworkObject aPrefab;
        public NetworkObject cPrefab;

        public void Start()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += autoselect;
            if(PlayerTypeExtensions.AtlasObject != null)
            {
                Atlas = PlayerTypeExtensions.AtlasObject.GetComponent<NetworkObject>();
            }
            else
            {
                Atlas = null;
            }
            if(PlayerTypeExtensions.ChromaObject != null)
            {
                Chroma = PlayerTypeExtensions.ChromaObject.GetComponent<NetworkObject>();
            }
            else
            {
                Chroma = null;
            }
            if(!(Chroma == null && Atlas == null))
            {
                hideMenuRpc();
            }
        }

        void autoselect(ulong owner)
        {
            if (this.NetworkObject.IsSpawned)
            {
                autoselectRpc(owner);
            }
        }

        private new void OnDestroy()
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= autoselect;
        }

        [Rpc(SendTo.Server)]
        public void autoselectRpc(ulong owner)
        {
            //Check if Host has selected character yet
            if (Atlas != null || Chroma != null)
            {
                if (Atlas != null)
                {
                    //Spawn Chroma, since Atlas is already taken
                    selectChromaRpc(owner);
                }
                else if (Chroma != null)
                {
                    //Spawn Atlas since Chroma is taken
                    selectAtlasRpc(owner);

                }
                else
                {
                    //Both Players are connected to this server, disconnect.
                    NetworkManager.DisconnectClient(owner, "Too many players");
                    SceneManager.LoadScene(0);
                }
            }
        }

        [Rpc(SendTo.Everyone)]
        public void informSelectRpc()
        {
            if(Chroma != null)
            {
                if (Chroma.OwnerClientId == this.NetworkManager.LocalClientId)
                    return;
            }
            if(Atlas != null)
            {
                if (Atlas.OwnerClientId == this.NetworkManager.LocalClientId)
                    return;
            }
            autoselectRpc(this.NetworkManager.LocalClientId);
        }

        public void selectChroma()
        {
            selectChromaRpc(this.NetworkManager.LocalClientId);
        }

        public void selectAtlas()
        {
            
            selectAtlasRpc(this.NetworkManager.LocalClientId);
        }
        
        //THIS SHOULD ONLY BE RUN BY SERVER
        public bool checkAvailable(PlayerType type)
        {
            switch (type)
            {
                case PlayerType.Atlas:
                    if(NetManager.instance.players[0] != null)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                case PlayerType.Chroma:
                    if (NetManager.instance.players[1] != null)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
            }
            return false;
            
        }

        [Rpc(SendTo.Everyone)]
        public void hideMenuRpc()
        {
            gameObject.SetActive(false);
        }

        [Rpc(SendTo.Server)]
        public void selectAtlasRpc(ulong owner)
        {
            if (!checkAvailable(PlayerType.Atlas)) return;
            //Spawn Atlas
            Atlas = Instantiate(aPrefab);
            Atlas.SpawnAsPlayerObject(owner);
            Atlas.GetComponent<InitializePlayer>().spawnPositionsRpc();
            //Hide the menu for all players
            hideMenuRpc();
            //Autoselect if other player is connected
            informSelectRpc();
        }

        [Rpc(SendTo.Server)]
        public void selectChromaRpc(ulong owner)
        {
            if (!checkAvailable(PlayerType.Chroma)) return;
            Chroma = Instantiate(cPrefab);
            Chroma.SpawnAsPlayerObject(owner);
            Chroma.GetComponent<InitializePlayer>().spawnPositionsRpc();
            //Hide the menu for all players
            hideMenuRpc();
            //Autoselect if other player is connected
            informSelectRpc();
        }
    }
}