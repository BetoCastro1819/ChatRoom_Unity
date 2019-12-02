using UnityEngine;

public class GameManager : MonoBehaviour
{
	[SerializeField] GameObject gameOverCanvas;

	[SerializeField] Transform serverCameraPosition;
	[SerializeField] Transform clientCameraPosition;

	[SerializeField] GameObject serverShip;
	[SerializeField] GameObject clientShip;

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
		}
	}
}
