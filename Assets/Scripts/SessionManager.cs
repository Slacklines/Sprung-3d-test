using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using UnityEngine.UI;
using System;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using TMPro;

public class SessionManager : MonoBehaviour
{
    public Button hostButton;
    public Button joinButton;

    public TMP_InputField roomCodeInput;

    private ISession currentSession;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        hostButton.interactable = false;
        joinButton.interactable = false;

        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        hostButton.interactable = true;
        joinButton.interactable = true;

        Debug.Log("Player ID: " + AuthenticationService.Instance.PlayerId);

    }

    // Update is called once per frame
    public async void HostGame()
    {
        try
        {
            var options = new SessionOptions {
                MaxPlayers = 8
            }
            .WithRelayNetwork();

            currentSession = await MultiplayerService.Instance.CreateSessionAsync(options);

            Debug.Log("Room Code: " + currentSession.Code);
            PlayerManager.instance.roomCode = currentSession.Code;

            NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);

        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinGame()
    {
        try
        {
            currentSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(roomCodeInput.text);
            PlayerManager.instance.roomCode = currentSession.Code;

            Debug.Log("Joined!");

        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
}
