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

    // Wandering, only for testing.
    [Header("Wandering (testing)")]
    [SerializeField] private bool m_EnableWandering = false;
    [SerializeField] private Vector3 m_WaderingBoundsSize = new Vector3(16.0f, 9.0f, 0.0f);
    [SerializeField] private Vector3 m_WaderingTargetLocation = new Vector3(0.0f, 0.0f, 0.0f);

    private void Update()
    {
        if (Vector3.Distance(transform.position, m_WaderingTargetLocation) < 0.5f)
        {
            m_WaderingTargetLocation = new Vector3(
                Random.Range(m_WaderingBoundsSize.x * -0.5f, m_WaderingBoundsSize.x * 0.5f),
                Random.Range(m_WaderingBoundsSize.y * -0.5f, m_WaderingBoundsSize.y * 0.5f),
                Random.Range(m_WaderingBoundsSize.z * -0.5f, m_WaderingBoundsSize.z * 0.5f));
        }

        if (m_EnableWandering)
        {
            transform.position += (m_WaderingTargetLocation - transform.position).normalized * m_MovementSpeed * Time.deltaTime;
        }
        else
        {
            transform.position += new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0.0f) * m_MovementSpeed * Time.deltaTime;
        }

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

    private void OnDrawGizmos()
    {
        Gizmos.DrawCube(Vector3.zero, m_WaderingBoundsSize);
    }
}
