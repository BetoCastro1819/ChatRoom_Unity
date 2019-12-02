using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
	public GameObject youWon;
	public GameObject youLost;

	public void Replay()
	{
		NetworkManager.Instance.CloseConnection();
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public void Quit()
	{
		Application.Quit();
	}
}
