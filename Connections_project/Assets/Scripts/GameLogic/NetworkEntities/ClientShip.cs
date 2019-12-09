using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;

public class ClientShip : NetworkEntity
{
	[SerializeField] GameObject explosionEffect;
	[SerializeField] float simulatedLagInMs = 0.2f;

	Dictionary<uint, Vector3> inputs = new Dictionary<uint, Vector3>();

	uint lastSequenceServerReceived = 0;
	uint sequence = 0;

    protected override void Start()
    {
		base.Start();

		objectID = ObjectIDs.ClientShip;
		PacketManager.Instance.AddListener((uint)objectID, OnReceiveDataEvent);
    }

	protected override void OnReceiveDataEvent(uint packetID, ushort packetTypeID, Stream stream)
	{
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
					NetworkMessageManager.Instance.SendPosition(transform.position, (uint)objectID, velocityPacket.payload.sequence);

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
				//Debug.Log("Position before reconciliation: " + position);


				if (positionPacket.payload.sequence > lastSequenceServerReceived)
				{
					lastSequenceServerReceived = positionPacket.payload.sequence;

					Vector3 reconciliationPosition = Vector3.zero;
					for (uint currentInputKey = positionPacket.payload.sequence; currentInputKey < sequence; currentInputKey++)
					{
						if (inputs.ContainsKey(currentInputKey))
						{
							//Debug.Log("Removing input with ID " + currentInputKey);
							reconciliationPosition += inputs[currentInputKey];
							inputs.Remove(positionPacket.payload.sequence);
						}
					}
					transform.position = position;
				}

				if (NetworkManager.Instance.isServer)
				{
					//NetworkMessageManager.Instance.SendPosition(transform.position, (uint)objectID, sequence);
					//StartCoroutine(SendServerResponseWithLag(transform.position, positionPacket.payload.sequence));
				}

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

				if (NetworkManager.Instance.isServer)
					NetworkMessageManager.Instance.SendShipdestroyedPacket((uint)objectID);

				Instantiate(explosionEffect, transform.position, Quaternion.identity);
				Destroy(gameObject);
				break;
		}
	}

	IEnumerator SendServerResponseWithLag(Vector3 position, uint sequence)
	{
		yield return new WaitForSeconds(simulatedLagInMs);
		NetworkMessageManager.Instance.SendPosition(position, (uint)objectID, sequence);
	} 

	void Update()
	{
		if (health <= 0)
		{
			NetworkMessageManager.Instance.SendShipdestroyedPacket((uint)objectID);
			//Destroy(gameObject);
		} 
	}

    void FixedUpdate()
    {
		if (NetworkManager.Instance.isServer) return;

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
			movPosition += -transform.right * speed * Time.fixedDeltaTime;
			NetworkMessageManager.Instance.SendVelocity(movPosition, (uint)objectID, sequence);

			//Vector3 positionRequest = rb.position + movPosition;
			//Debug.Log("Saving position request for " + positionRequest + " with ID " + sequence);
			inputs.Add(sequence++, movPosition);
		}
		else if (Input.GetKey(KeyCode.RightArrow))
		{
			movPosition += transform.right * speed * Time.fixedDeltaTime;
			NetworkMessageManager.Instance.SendVelocity(movPosition, (uint)objectID, sequence);

			//Vector3 positionRequest = rb.position + movPosition;
			//Debug.Log("Saving position request for " + positionRequest + " with ID " + sequence);
			inputs.Add(sequence++, movPosition);
		}
		else if (Input.GetKey(KeyCode.DownArrow))
		{
			movPosition += -transform.forward * speed * Time.fixedDeltaTime;
			NetworkMessageManager.Instance.SendVelocity(movPosition, (uint)objectID, sequence);

			//Vector3 positionRequest = rb.position + movPosition;
			//Debug.Log("Saving position request for " + positionRequest + " with ID " + sequence);
			inputs.Add(sequence++, movPosition);
		}
		else if (Input.GetKey(KeyCode.UpArrow))
		{
			movPosition += transform.forward * speed * Time.fixedDeltaTime;
			NetworkMessageManager.Instance.SendVelocity(movPosition, (uint)objectID, sequence);

			//Vector3 positionRequest = rb.position + movPosition;
			//Debug.Log("Saving position request for " + positionRequest + " with ID " + sequence);
			inputs.Add(sequence++, movPosition);
		}
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

			//TakeDamage(bullet.damage);	
		}
	}

	void TakeDamage(int damage)
	{
		health -= damage;
	}
}
