using System;
using UnityEngine;
using UnityEngine.UI;
using System.Net;

public class NetworkScreen : MonoBehaviourSingleton<NetworkScreen>
{
	public GameObject networkEntity_server;
	public GameObject networkEntity_client;

    public Button connectBtn;
    public Button startServerBtn;
    public InputField portInputField;
    public InputField addressInputField;

	public event Action OnGameScreenEvent;

    protected override void Initialize()
    {
        connectBtn.onClick.AddListener(OnConnectBtnClick);
        startServerBtn.onClick.AddListener(OnStartServerBtnClick);
    }

    void OnConnectBtnClick()
    {
        IPAddress ipAddress = IPAddress.Parse(addressInputField.text);
        int port = System.Convert.ToInt32(portInputField.text);

        NetworkManager.Instance.StartClient(ipAddress, port);
        
        SwitchToChatScreen();
    }

    void OnStartServerBtnClick()
    {
        IPAddress ipAddress = IPAddress.Parse(addressInputField.text);
		int port = System.Convert.ToInt32(portInputField.text);

		NetworkManager.Instance.StartServer(port);
        SwitchToChatScreen();
    }

    void SwitchToChatScreen()
    {
        //ChatScreen.Instance.gameObject.SetActive(true);

		OnGameScreenEvent();

		networkEntity_server.SetActive(true);
		networkEntity_client.SetActive(true);

        this.gameObject.SetActive(false);
    }
}
