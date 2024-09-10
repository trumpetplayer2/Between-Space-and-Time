using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;

public class NetManager : MonoBehaviour
{
    private static NetworkManager m_NetworkManager;
    private static UnityTransport m_UnityTransport;
    private static string address = "127.0.0.1";

    void Awake()
    {
        m_NetworkManager = GetComponent<NetworkManager>();
        m_UnityTransport = GetComponent<UnityTransport>();
    }

    public void startHost()
    {
        m_NetworkManager.StartHost();
    }

    public void connectClient()
    {
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
}
