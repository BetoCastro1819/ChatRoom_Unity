using UnityEngine;

public class Bullet : MonoBehaviour
{
	[SerializeField] float speed;
	public int damage;

    void FixedUpdate()
    {
		transform.position += transform.forward * speed * Time.fixedDeltaTime;
    }

	private void OnCollisionEnter(Collision other) 
	{
		Destroy(gameObject);	
	}
}
