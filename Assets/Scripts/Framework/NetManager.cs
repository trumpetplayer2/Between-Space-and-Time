using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using TMPro;
using UnityEngine.SceneManagement;
using System;

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
        public GameObject menu;
         
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
            SceneManager.sceneLoaded += OnSceneLoad;
            m_NetworkManager.OnClientDisconnectCallback += onDisconnect;
        }

        void onDisconnect(ulong clientID)
        {
            //Get Player Object
            GameObject player = PlayerTypeExtensions.getObject(clientID);
            if (player == null) return;
            //Remove player from list
            PlayerTypeExtensions.disconnect(clientID);
            //Destroy it
            Destroy(player);
            //Someone disconnected from server, send back to lobby
            setSceneRpc(lobby);
        }

        public void DisconnectPlayer(NetworkObject player) 
        {
            if (player.IsOwnedByServer)
            {
                m_NetworkManager.Shutdown();
                SceneManager.LoadScene(0);
            }
            else
            {
                m_NetworkManager.DisconnectClient(player.OwnerClientId);
            }
        }

        void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            atlasFinish = false;
            chromaFinish = false;
            loading = false;
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
            log("Attempting to connect to " + address);
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
            //if(ping >= timeout)
            //{
            //    log("Timedout! Ping was " + ping);
            //    m_NetworkManager.DisconnectClient(m_NetworkManager.LocalClientId, "Connection Timeout");
            //}
            if(PingTracker.instance != null)
            {
                //Send Ping in ms to PingTracker
                PingTracker.instance.updatePing(ping * 1000);
            }
            if(chromaFinish && atlasFinish && !loading)
            {
                loading = true;
                if (menu != null)
                {
                    openSelectionMenu();
                }
                else
                {
                    nextScene();
                }
            }
        }

        private void openSelectionMenu()
        {
            if (!m_NetworkManager.IsHost) return;
            if (menu == null) return;
            //Open Menu
            menu.SetActive(true);
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
            logRpc(clientID + ": " + s);
        }

        [Rpc(SendTo.Server)]
        public void logRpc(string s)
        {
            if (Console.instance == null) return;
            Console.instance.Log(s);
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
            if (!m_NetworkManager.IsServer) return;
            updateCurSceneRpc(scene);
            setSceneRpc(SceneList[scene]);
        }

        [Rpc(SendTo.NotServer)]
        void updateCurSceneRpc(int scene)
        {
            currentScene = scene;
        }

        [Rpc(SendTo.Server)]
        private void setSceneRpc(string scene)
        {
            if (!m_NetworkManager.IsServer) return;
            for(int i = 0; i < SceneList.Length; i++)
            {
                if (SceneList[i].ToLower().Equals(scene.ToLower())) currentScene = i; continue;
            }
            atlasFinish = false;
            chromaFinish = false;
            NetworkManager.Singleton.SceneManager.LoadScene(scene, LoadSceneMode.Single);
        }

        public void nextScene()
        {
            if (!(atlasFinish && chromaFinish)) return;
            currentScene += 1;
            setSceneRpc(SceneList[currentScene]);
        }

        public void endLoading()
        {
            loading = false;
        }
    }
}