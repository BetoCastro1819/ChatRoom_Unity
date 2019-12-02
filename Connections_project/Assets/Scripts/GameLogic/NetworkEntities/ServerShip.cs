using System.IO;
using UnityEngine;

public class ServerShip : NetworkEntity
{
	protected override void Start()
	{
		base.Start();

		objectID = ObjectIDs.ServerShip;
		PacketManager.Instance.AddListener((uint)objectID, OnReceiveDataEvent);
	}

	protected override void OnReceiveDataEvent(uint packetID, ushort packetTypeID, Stream stream)
	{
		//Debug.Log("Applying velocity received to " + gameObject.name);

		switch (packetTypeID)
		{
			case (ushort)UserPacketType.Position:
				PositionPacket positionPacket = new PositionPacket();
				positionPacket.Deserialize(stream);

				Vector3 position = new Vector3(
					positionPacket.payload.position.x,
					positionPacket.payload.position.y,
					positionPacket.payload.position.z
				);
				rb.position = position;
				break;

			case (ushort)UserPacketType.Shoot:
				Shoot();
				break;
		}
	}

	void Update()
	{
		if (health <= 0) Destroy(gameObject);
	}

    void FixedUpdate()
    {
		if (!NetworkManager.Instance.isServer) return;

		HandleMovementInput();
		HandleShootInput();
	}

	void HandleMovementInput()
	{
		float horizontal = Input.GetAxisRaw("Horizontal");
		float vertical = Input.GetAxisRaw("Vertical");

		Vector3 velocity = new Vector3(-horizontal, 0, -vertical);
		rb.position += velocity * speed * Time.fixedDeltaTime;

		NetworkMessageManager.Instance.SendPosition(rb.position, (uint)objectID);
	}

	void HandleShootInput()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			Shoot();
			NetworkMessageManager.Instance.SendShootPacket((uint)objectID);
		}
	}

	void Shoot()
	{
		Instantiate(bulletPrefab, shootPosition.position, shootPosition.rotation);
	}

	void OnCollisionEnter(Collision other)
	{
		if (other.collider.CompareTag("Bullet"))
		{
			Bullet bullet = other.gameObject.GetComponent<Bullet>();
			TakeDamage(bullet.damage);
		}
	}

	void TakeDamage(int damage)
	{
		health -= damage;
	}
}
