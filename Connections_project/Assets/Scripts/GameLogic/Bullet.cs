using System.IO;
using UnityEngine;

public class Bullet : MonoBehaviour
{
	[SerializeField] GameObject explosionEffect;

	public float speed = 10f;
	public int damage;

    void FixedUpdate()
    {
		Vector3 velocity = transform.forward * speed * Time.fixedDeltaTime;
		transform.position += velocity;
    }

	private void OnCollisionEnter(Collision other) 
	{
		Instantiate(explosionEffect, transform.position, Quaternion.identity);
		Destroy(gameObject);	
	}
}
