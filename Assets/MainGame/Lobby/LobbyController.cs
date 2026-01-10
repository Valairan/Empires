using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyController : MonoBehaviour
{
    [SerializeField] GameObject lobbyParent;
    [SerializeField] GameObject mainMenuParent;
    [SerializeField] GameObject LoadingParent;

    [SerializeField] TMP_InputField roomcodeInput;
    [SerializeField] TMP_Text LobbyRoomCodeDisplay;

    public async void createRoom()
    {
        mainMenuParent.SetActive(false);
        LoadingParent.SetActive(true);
        string joinCode = await StartHostWithRelay(8, "dtls");
        if (joinCode.Length > 0)
        {
            LoadingParent.SetActive(false);
            LobbyRoomCodeDisplay.text = joinCode;
            lobbyParent.SetActive(true);
            return;
        }

        mainMenuParent.SetActive(true);
        LoadingParent.SetActive(false);

    }

    public async void joinRoom()
    {
        if (roomcodeInput.text.Length > 0)
        {
            if (await StartClientWithRelay(roomcodeInput.text.ToString().Trim(), "dtls"))
            {
                LoadingParent.SetActive(false);
                //NetworkManager.Singleton.
                lobbyParent.SetActive(true);
                return;
            }
        }
    }

    public async Task<string> StartHostWithRelay(int maxConnections, string connectionType)
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        var allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));
        var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        return NetworkManager.Singleton.StartHost() ? joinCode : null;
    }

    public async Task<bool> StartClientWithRelay(string joinCode, string connectionType)
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));
        return !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();
    }
}


public enum playerColors
{

}