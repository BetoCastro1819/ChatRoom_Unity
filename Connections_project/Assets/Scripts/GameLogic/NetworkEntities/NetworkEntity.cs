using System.IO;
using UnityEngine;

public enum ObjectIDs
{
	ServerShip = 2,
	ClientShip = 3,
}

public abstract class NetworkEntity : MonoBehaviour
{
	[SerializeField] protected float speed = 10;

	protected ObjectIDs objectID;

	protected Rigidbody rb;

	protected virtual void Start()
    {
		rb = GetComponent<Rigidbody>();
    }

    protected abstract void OnReceiveDataEvent(uint packetID, ushort packetTypeID, Stream stream);
}