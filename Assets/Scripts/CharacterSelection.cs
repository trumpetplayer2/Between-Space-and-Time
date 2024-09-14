using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelection : NetworkBehaviour
{
    public static NetworkObject Atlas;
    public static NetworkObject Chroma;
    public NetworkObject aPrefab;
    public NetworkObject cPrefab;

    public void Start()
    {
        autoselect();
    }

    public void autoselect()
    {
        //Check if Host has selected character yet
        if (Atlas != null || Chroma != null)
        {
            if (Atlas != null)
            {
                //Spawn Chroma, since Atlas is already taken
                selectChromaRpc(this.OwnerClientId);
            }
            else if (Chroma != null)
            {
                //Spawn Atlas since Chroma is taken
                selectAtlasRpc(this.OwnerClientId);
                
            }
            else
            {
                //Both Players are connected to this server, disconnect.
                NetworkManager.DisconnectClient(this.OwnerClientId);
                SceneManager.LoadScene(0);
            }
        }
    }

    [Rpc(SendTo.NotMe)]
    public void informSelectRpc()
    {
        //If the owner is neither character, autoselect
        if(!(Atlas.OwnerClientId == this.OwnerClientId || Chroma.OwnerClientId == this.OwnerClientId))
        {
            autoselect();
        }
    }

    public void selectChroma()
    {
        selectChromaRpc(this.OwnerClientId);
    }

    public void selectAtlas()
    {
        selectAtlasRpc(this.OwnerClientId);
    }

    [Rpc(SendTo.Server)]
    public void selectAtlasRpc(ulong owner)
    {
        //Spawn Atlas
        Atlas = NetworkManager.SpawnManager.InstantiateAndSpawn(aPrefab, owner);
        //If other player isn't spawned, and another client is connected, try to select
        this.gameObject.SetActive(false);
    }
    
    [Rpc(SendTo.Server)]
    public void selectChromaRpc(ulong owner)
    {
        Chroma = NetworkManager.SpawnManager.InstantiateAndSpawn(cPrefab, owner);
        this.gameObject.SetActive(false);
    }
}
