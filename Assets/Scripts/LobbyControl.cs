﻿using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class LobbyControl : NetworkBehaviour
{
    [SerializeField] string m_InGameSceneName = "InGame";
    
    // Minimum player count required to transition to next level
    [SerializeField] int m_MinimumPlayerCount = 2;

    [SerializeField] TMP_Text LobbyText;
    bool m_AllPlayersInLobby;

    Dictionary<ulong, bool> m_ClientsInLobby;
    string m_UserLobbyStatusText;

    public override void OnNetworkSpawn()
    {
        m_ClientsInLobby = new Dictionary<ulong, bool>();
        
        //Always add ourselves to the list at first
        m_ClientsInLobby.Add(NetworkManager.LocalClientId, false);

        //If we are hosting, then handle the server side for detecting when clients have connected
        //and when their lobby scenes are finished loading.
        if (IsServer)
        {
            m_AllPlayersInLobby = false;

            //Server will be notified when a client connects
            NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
            SceneTransitionHandler.sceneTransitionHandler.OnClientLoadedScene += ClientLoadedScene;
        }

        //Update our lobby
        GenerateUserStatsForLobby();

        SceneTransitionHandler.sceneTransitionHandler.SetSceneState(SceneTransitionHandler.SceneStates.Lobby);
    }

    void OnGUI()
    {
        if (LobbyText != null) 
            LobbyText.text = m_UserLobbyStatusText;
    }

    /// <summary>
    ///     GenerateUserStatsForLobby
    ///     Psuedo code for setting player state
    ///     Just updating a text field, this could use a lot of "refactoring"  :)
    /// </summary>
    void GenerateUserStatsForLobby()
    {
        m_UserLobbyStatusText = string.Empty;
        foreach (var clientLobbyStatus in m_ClientsInLobby)
        {
            ulong playerNumber = clientLobbyStatus.Key + 1;
            m_UserLobbyStatusText += "Player " + playerNumber + "          ";
            if (clientLobbyStatus.Value)
                m_UserLobbyStatusText += "(READY)\n";
            else
                m_UserLobbyStatusText += "(NOT READY)\n";
        }
    }

    /// <summary>
    ///     UpdateAndCheckPlayersInLobby
    ///     Checks to see if we have at least 2 or more people to start
    /// </summary>
    void UpdateAndCheckPlayersInLobby()
    {
        m_AllPlayersInLobby = m_ClientsInLobby.Count >= m_MinimumPlayerCount;

        foreach (var clientLobbyStatus in m_ClientsInLobby)
        {
            SendClientReadyStatusUpdatesClientRpc(clientLobbyStatus.Key, clientLobbyStatus.Value);
            if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientLobbyStatus.Key))

                //If some clients are still loading into the lobby scene then this is false
                m_AllPlayersInLobby = false;
        }

        CheckForAllPlayersReady();
    }

    /// <summary>
    ///     ClientLoadedScene
    ///     Invoked when a client has loaded this scene
    /// </summary>
    /// <param name="clientId"></param>
    void ClientLoadedScene(ulong clientId)
    {
        if (IsServer)
        {
            if (!m_ClientsInLobby.ContainsKey(clientId))
            {
                m_ClientsInLobby.Add(clientId, false);
                GenerateUserStatsForLobby();
            }

            UpdateAndCheckPlayersInLobby();
        }
    }

    /// <summary>
    ///     OnClientConnectedCallback
    ///     Since we are entering a lobby and Netcode's NetworkManager is spawning the player,
    ///     the server can be configured to only listen for connected clients at this stage.
    /// </summary>
    /// <param name="clientId">client that connected</param>
    void OnClientConnectedCallback(ulong clientId)
    {
        if (IsServer)
        {
            if (!m_ClientsInLobby.ContainsKey(clientId)) m_ClientsInLobby.Add(clientId, false);
            GenerateUserStatsForLobby();

            UpdateAndCheckPlayersInLobby();
        }
    }

    /// <summary>
    ///     SendClientReadyStatusUpdatesClientRpc
    ///     Sent from the server to the client when a player's status is updated.
    ///     This also populates the connected clients' (excluding host) player state in the lobby
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="isReady"></param>
    [ClientRpc]
    void SendClientReadyStatusUpdatesClientRpc(ulong clientId, bool isReady)
    {
        if (!IsServer)
        {
            if (!m_ClientsInLobby.ContainsKey(clientId))
                m_ClientsInLobby.Add(clientId, isReady);
            else
                m_ClientsInLobby[clientId] = isReady;
            GenerateUserStatsForLobby();
        }
    }

    /// <summary>
    ///     CheckForAllPlayersReady
    ///     Checks to see if all players are ready, and if so launches the game
    /// </summary>
    void CheckForAllPlayersReady()
    {
        if (m_AllPlayersInLobby)
        {
            var allPlayersAreReady = true;
            foreach (var clientLobbyStatus in m_ClientsInLobby)
                if (!clientLobbyStatus.Value)

                    //If some clients are still loading into the lobby scene then this is false
                    allPlayersAreReady = false;

            //Only if all players are ready
            if (allPlayersAreReady)
            {
                //Remove our client connected callback
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;

                //Remove our scene loaded callback
                SceneTransitionHandler.sceneTransitionHandler.OnClientLoadedScene -= ClientLoadedScene;

                //Transition to the ingame scene
                SceneTransitionHandler.sceneTransitionHandler.SwitchScene(m_InGameSceneName);
            }
        }
    }

    /// <summary>
    ///     PlayerIsReady
    ///     Tied to the Ready button in the InvadersLobby scene
    /// </summary>
    public void PlayerIsReady()
    {
        m_ClientsInLobby[NetworkManager.Singleton.LocalClientId] = true;
        if (IsServer)
        {
            UpdateAndCheckPlayersInLobby();
        }
        else
        {
            OnClientIsReadyServerRpc(NetworkManager.Singleton.LocalClientId);
        }

        GenerateUserStatsForLobby();
    }

    /// <summary>
    ///     OnClientIsReadyServerRpc
    ///     Sent to the server when the player clicks the ready button
    /// </summary>
    /// <param name="clientid">clientId that is ready</param>
    [ServerRpc(RequireOwnership = false)]
    void OnClientIsReadyServerRpc(ulong clientid)
    {
        if (m_ClientsInLobby.ContainsKey(clientid))
        {
            m_ClientsInLobby[clientid] = true;
            UpdateAndCheckPlayersInLobby();
            GenerateUserStatsForLobby();
        }
    }
}
