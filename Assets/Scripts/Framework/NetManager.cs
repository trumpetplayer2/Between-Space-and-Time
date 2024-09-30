using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using TMPro;
using UnityEngine.SceneManagement;

namespace tp2
{
    public class NetManager : MonoBehaviour
    {
        private static NetworkManager m_NetworkManager;
        private static UnityTransport m_UnityTransport;
        private static string address = "127.0.0.1";
        public static string lobby = "Lobby";
        public static Transform aStart;
        public static Transform cStart;
        public InitializePlayer[] players = new InitializePlayer[2] { null, null };
        public static NetManager instance;
        public float timeout = 10;
        NetworkTime lastKnownServerTime;
        bool wasConnected = false;
        bool chromaFinish = false;
        bool atlasFinish = false;
        bool loading = false;
        int currentScene = 0;
        public string[] SceneList;
        
         
        void Awake()
        {
            if(instance != null)
            {
                //Refresh instance if it already exists
                Destroy(instance);
            }
            instance = this;
            for(int i = 0; i < players.Length; i++)
            {
                players[i] = null;
            }
            m_NetworkManager = GetComponent<NetworkManager>();
            m_UnityTransport = GetComponent<UnityTransport>();
        }

        public void startHost()
        {
            //Start Host
            m_NetworkManager.StartHost();
            NetworkManager.Singleton.SceneManager.LoadScene(lobby, LoadSceneMode.Single);
            currentScene = 1;
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
            //Cap playercount
            if(m_NetworkManager.LocalClientId > 1)
            {
                m_NetworkManager.DisconnectClient(m_NetworkManager.LocalClientId, "Too many players");
            }

        }

        public void manualConnect(TMP_InputField input)
        {
            address = input.text.Trim();
            connectClient();
        }

        public void FixedUpdate()
        {
            if (!m_NetworkManager.IsConnectedClient)
            {
                if (wasConnected)
                {
                    Disconnect();
                    wasConnected = false;
                }
                return;
            }
            wasConnected = true;
            //Check how long its been since last ping in seconds
            lastKnownServerTime = m_NetworkManager.ServerTime;
            float ping = (m_NetworkManager.LocalTime - lastKnownServerTime).TimeAsFloat;
            //log("ping: " + ping);
            //If Ping passes timeout threshold, disconnect
            if(ping >= timeout)
            {
                log("Timedout! Ping was " + ping);
                m_NetworkManager.DisconnectClient(m_NetworkManager.LocalClientId, "Connection Timeout");
            }
            if(chromaFinish && atlasFinish)
            {
                nextScene();
                chromaFinish = false;
                atlasFinish = false;
                loading = true;
            }
        }


        private void Disconnect()
        {
            SceneManager.LoadScene(0);
            currentScene = 0;
        }

        public static void log(string s)
        {
            instance.logRpc(m_NetworkManager.LocalClientId, s);
        }

        [Rpc(SendTo.Server)]
        public void logRpc(ulong clientID, string s)
        {
            Debug.Log(clientID + ": " + s);
        }

        public void updateAtlasFinish(bool state)
        {
            if (loading) return;
            atlasFinish = state;
        }

        public void updateChromaFinish(bool state)
        {
            if (loading) return;
            chromaFinish = state;
        }

        
        public void updateScene(int scene)
        {
            
            setSceneRpc(SceneList[scene]);
        }

        [Rpc(SendTo.Server)]
        private void setSceneRpc(string scene)
        {
            if (!m_NetworkManager.IsServer) return;
            
            atlasFinish = false;
            chromaFinish = false;
            NetworkManager.Singleton.SceneManager.LoadScene(scene, LoadSceneMode.Single);
            loading = false;
        }

        public void nextScene()
        {
            if (!(atlasFinish && chromaFinish)) return;
            currentScene += 1;
            setSceneRpc(SceneList[currentScene]);
        }
    }
}