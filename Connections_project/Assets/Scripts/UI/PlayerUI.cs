using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
	[SerializeField] GameObject serverHealthBar;
	[SerializeField] GameObject clientHealthBar;

	Slider healthBarSlider;
	bool onGameScreen;

    void Start()
    {
		serverHealthBar.SetActive(false);
		clientHealthBar.SetActive(false);

		NetworkScreen.Instance.OnGameScreenEvent += SetupHealthBar;
		onGameScreen = false;
	}

    void SetupHealthBar()
    {
        if (NetworkManager.Instance.isServer)
		{
			serverHealthBar.SetActive(true);
			healthBarSlider = serverHealthBar.GetComponent<Slider>();
		}
		else
		{
			clientHealthBar.SetActive(true);
			healthBarSlider = clientHealthBar.GetComponent<Slider>();
		}
		onGameScreen = true;
    }

	void Update()
	{
		if (!onGameScreen) return;

		if (NetworkManager.Instance.isServer)
			healthBarSlider.value = GameManager.Instance.serverShip.GetHealth() * 0.01f;
		else
			healthBarSlider.value = GameManager.Instance.clientShip.GetHealth() * 0.01f;
	}
}
