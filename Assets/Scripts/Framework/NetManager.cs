using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Events;

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
        public static bool debug = true;
        public static UnityEvent networkUpdate = new UnityEvent();
        float networkUpdateMS = 50;
        float timeSinceLastUpdate = 0;
         
        void Awake()
        {
            if(instance != null)
            {
                //Refresh instance if it already exists
                Destroy(this.gameObject);
                return;
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
            if (m_NetworkManager == null) return;
            if(clientID == m_NetworkManager.LocalClientId) { userDisconnect(); };
            if (m_NetworkManager.ShutdownInProgress) return;
            GameObject player;
            try
            {
                //Get Player Object
                player = PlayerTypeExtensions.getObject(clientID);
                if (player == null) return;
            }
            catch
            {
                return;
            }
            //Remove player from list
            PlayerTypeExtensions.disconnect(clientID);
            //Destroy it
            Destroy(player);
            //Someone disconnected from server, send back to lobby
            setSceneRpc(lobby);
            NetPlayer.paused = false;
        }

        private void FixedUpdate()
        {
            timeSinceLastUpdate += Time.fixedDeltaTime;
            if (timeSinceLastUpdate * 1000 >= networkUpdateMS)
            {
                networkUpdate.Invoke();
                timeSinceLastUpdate = 0;
            }
        }
        void userDisconnect()
        {
            //Destroy object, reset instance, and load menu
            m_NetworkManager.Shutdown();
            SceneManager.LoadScene(0);
            PlayerTypeExtensions.AtlasObject = null;
            PlayerTypeExtensions.ChromaObject = null;
            CharacterSelection.Atlas = null;
            CharacterSelection.Chroma = null;
            PlayerTypeExtensions.localPlayer = null;
        }

        public void DisconnectPlayer() 
        {
            userDisconnect();
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
            m_NetworkManager.SceneManager.LoadScene(lobby, LoadSceneMode.Single);
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

        public void Update()
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
            if (menu == null) return;
            NetPlayer.paused = true;
            if (!m_NetworkManager.IsHost) { 
                clientSelectionMenu(); 
                return; 
            }
            //Open Menu
            menu.SetActive(true);
        }

        void clientSelectionMenu()
        {
            LevelSelect select = menu.GetComponent<LevelSelect>();
            if (select == null) return;
            menu.SetActive(true);
            select.showClient();
        }

        private void Disconnect()
        {
            SceneManager.LoadScene(0);
            currentScene = 0;
        }

        public static void log(string s)
        {
            if(!debug)
            {
                return;
            }
            try
            {
                instance.logRpc(m_NetworkManager.LocalClientId, s);
            }
            catch {}
        }

        [Rpc(SendTo.Server)]
        public void logRpc(ulong clientID, string s)
        {
            logRpc("[Client " + clientID + "] " + s);
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
            NetPlayer.paused = false;
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
            try
            {
                m_NetworkManager.SceneManager.LoadScene(scene, LoadSceneMode.Single);
            }
            catch(Exception e)
            {
                log("Failed to change level!" + e.StackTrace);
            }
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