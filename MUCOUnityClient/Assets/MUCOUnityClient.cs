using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Phenomenal.MUCONet;

public class MUCOUnityClient : MonoBehaviour
{
    [SerializeField] private string m_SeverAddress = "127.0.0.1";
    [SerializeField] private int m_ServerPort = 1000;

    public MUCOClient Client { get; private set; } = null;

    public enum ServerPackets : int
    {
        HelloFromServer
    }

    public enum ClientPackets : int
    {
        HelloFromClient,
        UpdateTransform
    }

    private void Start()
    {
        MUCOLogger.LogEvent += Log;

        Client = new MUCOClient();
        Client.RegisterPacketHandler((int)ServerPackets.HelloFromServer, HandleHelloFromServer);
        Client.Connect(m_SeverAddress, m_ServerPort);

        Client.OnConnectedEvent += OnConnected;
    }

    private void OnApplicationQuit()
    {
        Client.Disconnect();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.D))
        {

        }

    }

    private void HandleHelloFromServer(MUCOPacket packet)
    {
        Debug.Log("HelloFromServer");
    }

    private static void Log(MUCOLogMessage message)
    {
        Debug.Log(message.Message);
    }

    #region Events
    private void OnConnected()
    {
        Debug.Log("OnConnected()");
    }
    #endregion

}
