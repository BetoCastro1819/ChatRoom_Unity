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
			case (ushort)UserPacketType.Velocity:

				VelocityPacket velocityPacket = new VelocityPacket();
				velocityPacket.Deserialize(stream);

				//Debug.Log("Applying velocity received to " + gameObject.name);
				Vector3 velocityReceived = new Vector3(
					velocityPacket.payload.velocity.x,
					velocityPacket.payload.velocity.y,
					velocityPacket.payload.velocity.z
				);
				//rb.position += velocityReceived;

				transform.Translate(velocityReceived, Space.Self);

				if (NetworkManager.Instance.isServer)
					NetworkMessageManager.Instance.SendPosition(transform.position, (uint)objectID);

				break;

			case (ushort)UserPacketType.Position:
				PositionPacket positionPacket = new PositionPacket();
				positionPacket.Deserialize(stream);

				Vector3 position = new Vector3(
					positionPacket.payload.position.x,
					positionPacket.payload.position.y,
					positionPacket.payload.position.z
				);
				//rb.position = position;
				transform.position = position;

				break;

			case (ushort)UserPacketType.Shoot:
				Shoot();
				break;

			case (ushort)UserPacketType.Damage:
				DamagePacket damagePacket = new DamagePacket();
				damagePacket.Deserialize(stream);

				TakeDamage((int)damagePacket.payload.damage);
				break;

			case (ushort)UserPacketType.ShipDestroyed:

				//if (NetworkManager.Instance.isServer)
				//	NetworkMessageManager.Instance.SendShipdestroyedPacket((uint)objectID);

				Destroy(gameObject);
				break;
		}
	}

	void Update()
	{
		if (!NetworkManager.Instance.isServer) return;

		if (health <= 0) 
		{
			NetworkMessageManager.Instance.SendShipdestroyedPacket((uint)objectID);
			Destroy(gameObject);
		}
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

		Vector3 movPosition = Vector3.zero;
		if (Input.GetKey(KeyCode.LeftArrow))
		{
			movPosition += transform.right * speed * Time.fixedDeltaTime;
			NetworkMessageManager.Instance.SendVelocity(movPosition, (uint)objectID);
		}
		else if (Input.GetKey(KeyCode.RightArrow))
		{
			movPosition += -transform.right * speed * Time.fixedDeltaTime;
			NetworkMessageManager.Instance.SendVelocity(movPosition, (uint)objectID);
		}
		else if (Input.GetKey(KeyCode.DownArrow))
		{
			movPosition += transform.forward * speed * Time.fixedDeltaTime;
			NetworkMessageManager.Instance.SendVelocity(movPosition, (uint)objectID);
		}
		else if (Input.GetKey(KeyCode.UpArrow))
		{
			movPosition += -transform.forward * speed * Time.fixedDeltaTime;
			NetworkMessageManager.Instance.SendVelocity(movPosition, (uint)objectID);
		}
		//rb.position += movPosition;
		//transform.position += movPosition;
		transform.Translate(movPosition, Space.Self);
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
			NetworkMessageManager.Instance.SendDamagePacket((uint)bullet.damage, (uint)objectID);
		}
	}

	void TakeDamage(int damage)
	{
		health -= damage;
	}
}
