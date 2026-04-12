using UnityEngine;
using Mirror;
using TMPro;

public class MultiplayerMenu : MonoBehaviour
{
    public TMP_InputField ipInput;

    public void HostGame()
    {
        NetworkManager.singleton.StartHost();
    }

    public void JoinGame()
    {
        string ip = ipInput.text;
        if (string.IsNullOrEmpty(ip)) ip = "localhost";

        NetworkManager.singleton.networkAddress = ip;
        NetworkManager.singleton.StartClient();
    }
}