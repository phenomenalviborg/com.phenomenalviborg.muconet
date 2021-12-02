using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Phenomenal.MUCONet;

public class MUCOUser : MonoBehaviour
{

    [SerializeField] private float m_MovementSpeed = 5.0f;

    [SerializeField] private MUCOUnityClient m_ClientNetworkManager = null;

    [SerializeField] private float m_MinimumReplicationTransformDelta = 0.05f;
    private Vector3 m_LastReplicatedPosition = new Vector3(0,0,0);
    
    private void Update()
    {
        transform.position += new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0.0f) * m_MovementSpeed * Time.deltaTime;
    
        // Replicate Movement
        if (Vector3.Distance(transform.position, m_LastReplicatedPosition) > m_MinimumReplicationTransformDelta)
        {
            Debug.Log("Replicating...");

            MUCOPacket packet = new MUCOPacket((int)MUCOUnityClient.ClientPackets.UpdateTransform);
            packet.WriteFloat(transform.position.x);
            packet.WriteFloat(transform.position.y);
            m_ClientNetworkManager.Client.SendPacket(packet);
         
            m_LastReplicatedPosition = transform.position;
        }

    }
}
