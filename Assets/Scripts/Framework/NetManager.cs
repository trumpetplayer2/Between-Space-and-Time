using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using TMPro;
using UnityEngine.SceneManagement;

public class NetManager : MonoBehaviour
{
    private static NetworkManager m_NetworkManager;
    private static UnityTransport m_UnityTransport;
    private static string address = "127.0.0.1";
    public static string lobby = "Lobby";
    public static Transform aStart;
    public static Transform cStart;

    void Awake()
    {
        m_NetworkManager = GetComponent<NetworkManager>();
        m_UnityTransport = GetComponent<UnityTransport>();
    }

    [Rpc(SendTo.Everyone)]
    public void startPositions()
    {
        
    }

    public void startHost()
    {        
        //Start Host
        m_NetworkManager.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene(lobby, LoadSceneMode.Single);
    }

    public void connectClient()
    {
        Debug.Log("Attempting to connect to " + address);
        if (m_UnityTransport != null)
        {
            try
            {
                m_UnityTransport.ConnectionData.Address = address;
            }
            catch { }
        }
        m_NetworkManager.StartClient();
    }

    public void manualConnect(TMP_InputField input)
    {
        address = input.text.Trim();
        connectClient();
    }
}
