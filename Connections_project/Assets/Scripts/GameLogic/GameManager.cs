using UnityEngine;

public class GameManager : MonoBehaviourSingleton<GameManager>
{
	[SerializeField] GameObject gameOverCanvas;

	[SerializeField] Transform serverCameraPosition;
	[SerializeField] Transform clientCameraPosition;

	public ServerShip serverShip;
	public ClientShip clientShip;

    void Start()
    {
		NetworkScreen.Instance.OnGameScreenEvent += SetupCamera;
		gameOverCanvas.SetActive(false);
	}

	void SetupCamera()
	{
		Camera mainCamera = Camera.main;

		if (NetworkManager.Instance.isServer)
		{
			mainCamera.transform.position = serverCameraPosition.position;
			mainCamera.transform.rotation = serverCameraPosition.rotation;
		}
		else
		{
			mainCamera.transform.position = clientCameraPosition.position;
			mainCamera.transform.rotation = clientCameraPosition.rotation;
		}
	}

	void Update()
	{
		if (serverShip == null || clientShip == null)
		{
			gameOverCanvas.SetActive(true);
			GameOverUI gameOverUI = gameOverCanvas.GetComponent<GameOverUI>();

			if (serverShip == null)
			{
				if (NetworkManager.Instance.isServer)
					gameOverUI.youLost.SetActive(true);
				else
					gameOverUI.youWon.SetActive(true);
			} 
			else
			{
				if (NetworkManager.Instance.isServer)
					gameOverUI.youWon.SetActive(true);
				else
					gameOverUI.youLost.SetActive(true);
			}
		}
	}
}
