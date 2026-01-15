using UnityEngine;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using System.Threading.Tasks;
using Unity.Netcode;
using TMPro;
public class MainMenuController : MonoBehaviour
{

    [SerializeField] GameObject mainMenuParent;
    [SerializeField] GameObject lobbyParent;
    [SerializeField] GameObject LoadingParent;
    [SerializeField] TMP_Text LobbyRoomCodeDisplay;
    [SerializeField] TMP_InputField roomcodeInput;

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
        Loader.instance.gameObject.SetActive(false);

        //LoadingParent.SetActive(false);

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



