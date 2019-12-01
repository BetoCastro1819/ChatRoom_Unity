using UnityEngine;

public class NetworkEntity : MonoBehaviour
{
	[SerializeField] float speed = 10;

	uint objectID = 1;
	Rigidbody rb;

    void Start()
    {
		rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
		float horizontal = Input.GetAxis("Horizontal");
		float vertical = Input.GetAxis("Vertical");

		Vector3 velocity = new Vector3(horizontal, 0, vertical);

		rb.velocity = velocity * speed;
    }
}
