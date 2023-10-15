using System.Text.RegularExpressions;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class MenuControl : MonoBehaviour
{
    [SerializeField] TMP_Text m_IPAddressText;

    [SerializeField] string m_LobbySceneName = "Lobby";
    [SerializeField] GameObject _hCTScreen;

    void Awake()
    {
        _hCTScreen = GameObject.Find("HCT Canvas");
        _hCTScreen.SetActive(false);
    }

    public void StartGame()
    {
        var utpTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        if (utpTransport)
        {
            utpTransport.SetConnectionData(Sanitize(m_IPAddressText.text), 7777);
        }
        if (NetworkManager.Singleton.StartHost())
        {
            SceneTransitionHandler.sceneTransitionHandler.RegisterCallbacks();
            SceneTransitionHandler.sceneTransitionHandler.SwitchScene(m_LobbySceneName);
        }
        else
        {
            Debug.LogError("Failed to start host.");
        }
    }

    public void JoinGame()
    {
        var utpTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        if (utpTransport)
        {
            utpTransport.SetConnectionData(Sanitize(m_IPAddressText.text), 7777);
        }
        if (!NetworkManager.Singleton.StartClient())
        {
            Debug.LogError("Failed to start client.");
        }
    }
    
    static string Sanitize(string dirtyString)
    {
        // sanitize the input for the ip address
        return Regex.Replace(dirtyString, "[^A-Za-z0-9.]", "");
    }

    void Update() // Delete after testing
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _hCTScreen.SetActive(true);
        }
    }

    public void AcceptButtonClicked() => _hCTScreen.gameObject.SetActive(false);
}
