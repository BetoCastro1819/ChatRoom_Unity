using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;

public class ClientShip : NetworkEntity
{
	[SerializeField] float simulatedLagInMs = 0.2f;

	Dictionary<uint, Vector3> inputs = new Dictionary<uint, Vector3>();
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
				rb.position += velocityReceived;

				if (NetworkManager.Instance.isServer)
					NetworkMessageManager.Instance.SendPosition(rb.position, (uint)objectID, velocityPacket.payload.sequence);

				break;

			case (ushort)UserPacketType.Position:
				PositionPacket positionPacket = new PositionPacket();
				positionPacket.Deserialize(stream);

				Vector3 position = new Vector3(
					positionPacket.payload.position.x,
					positionPacket.payload.position.y,
					positionPacket.payload.position.z
				);
				rb.position = position;
				//Debug.Log("Position before reconciliation: " + position);

				Vector3 reconciliationPosition = Vector3.zero;
				inputs.Remove(positionPacket.payload.sequence);
				for (uint currentInputKey = positionPacket.payload.sequence; currentInputKey < sequence; currentInputKey++)
				{
					if (inputs.ContainsKey(currentInputKey))
					{
						//Debug.Log("Removing input with ID " + currentInputKey);
						reconciliationPosition += inputs[currentInputKey];
					}
				}
				rb.position += reconciliationPosition;
				//Debug.Log("Position after reconciliation: " + rb.position);

				if (NetworkManager.Instance.isServer)
					StartCoroutine(SendServerResponseWithLag(rb.position, positionPacket.payload.sequence));

				break;

			case (ushort)UserPacketType.Shoot:
				Shoot();
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
		if (health <= 0) Destroy(gameObject);
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
			movPosition += -Vector3.right * speed * Time.fixedDeltaTime;
			NetworkMessageManager.Instance.SendVelocity(movPosition, (uint)objectID, sequence);

			Vector3 positionRequest = rb.position + movPosition;
			//Debug.Log("Saving position request for " + positionRequest + " with ID " + sequence);
			inputs.Add(sequence++, movPosition);
		}
		else if (Input.GetKey(KeyCode.RightArrow))
		{
			movPosition += Vector3.right * speed * Time.fixedDeltaTime;
			NetworkMessageManager.Instance.SendVelocity(movPosition, (uint)objectID, sequence);

			Vector3 positionRequest = rb.position + movPosition;
			//Debug.Log("Saving position request for " + positionRequest + " with ID " + sequence);
			inputs.Add(sequence++, movPosition);
		}
		else if (Input.GetKey(KeyCode.DownArrow))
		{
			movPosition += -Vector3.forward * speed * Time.fixedDeltaTime;
			NetworkMessageManager.Instance.SendVelocity(movPosition, (uint)objectID, sequence);

			Vector3 positionRequest = rb.position + movPosition;
			//Debug.Log("Saving position request for " + positionRequest + " with ID " + sequence);
			inputs.Add(sequence++, movPosition);
		}
		else if (Input.GetKey(KeyCode.UpArrow))
		{
			movPosition += Vector3.forward * speed * Time.fixedDeltaTime;
			NetworkMessageManager.Instance.SendVelocity(movPosition, (uint)objectID, sequence);

			Vector3 positionRequest = rb.position + movPosition;
			//Debug.Log("Saving position request for " + positionRequest + " with ID " + sequence);
			inputs.Add(sequence++, movPosition);
		}
		rb.position += movPosition;
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
