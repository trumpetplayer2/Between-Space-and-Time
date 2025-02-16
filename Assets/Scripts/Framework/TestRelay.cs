using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Authentication;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;

namespace tp2
{
    public class TestRelay : MonoBehaviour
    {

        // Start is called before the first frame update
        async void Start()
        {
            await UnityServices.InitializeAsync();
            if (AuthenticationService.Instance.IsSignedIn) return;
            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("Signed In as " + AuthenticationService.Instance.PlayerId);
            };
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        public async void CreateRelay()
        {
            try
            {
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);
                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                    allocation.RelayServer.IpV4,
                    (ushort) allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData
                    );
                NetManager.instance.joinCode = joinCode;
                NetworkManager.Singleton.StartHost();
                NetManager.instance.startHost();
                
            }catch(RelayServiceException e)
            {
                Debug.Log(e);
            }
        }

        public void JoinRelay(TMP_InputField input)
        {
            JoinRelay(input.text.Trim());
        }

        async void JoinRelay(string joinCode)
        {
            try
            {
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                    joinAllocation.RelayServer.IpV4,
                    (ushort) joinAllocation.RelayServer.Port,
                    joinAllocation.AllocationIdBytes,
                    joinAllocation.Key,
                    joinAllocation.ConnectionData,
                    joinAllocation.HostConnectionData
                    );

                NetworkManager.Singleton.StartClient();
            }catch(RelayServiceException e)
            {
                Debug.Log(e);
            }
        }
    }
}
