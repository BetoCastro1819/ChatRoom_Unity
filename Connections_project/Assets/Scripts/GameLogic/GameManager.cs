using UnityEngine;

public class GameManager : MonoBehaviour
{
	[SerializeField] Transform serverCameraPosition;
	[SerializeField] Transform clientCameraPosition;

    void Start()
    {
		NetworkScreen.Instance.OnGameScreenEvent += SetupCamera;
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
}
