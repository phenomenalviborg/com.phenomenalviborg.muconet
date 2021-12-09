using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Phenomenal.MUCONet;

public class HierarchyWidget : MonoBehaviour
{
    [Header("Server")]
    [SerializeField] private MUCOUnityServer m_ServerManager;

    [Header("Runtime Data")]
    [SerializeField] private GameObject m_HierarchyElementPrefab;

    [Header("UI Elements")]
    [SerializeField] private GameObject m_HierarchyList;

    private void Start()
    {
        //m_ServerManager.Server.OnClientConnectedEvent += Regenerate;
        //m_ServerManager.Server.OnClientDisconnectedEvent += Regenerate;
    }

    public void Regenerate()
    {
        // DESTROY THE CHILDREN!! HUU HAHAA AHHHHHHH!
        foreach (Transform child in m_HierarchyList.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        // Add all current users
        foreach (MUCOServer.MUCOClientInfo clientInfo in m_ServerManager.Server.ClientInfo.Values)
        {
            GameObject userWidgetGameObject = Instantiate(m_HierarchyElementPrefab, m_HierarchyList.transform);
            HierarchyUserWidget userWidget = userWidgetGameObject.GetComponent<HierarchyUserWidget>();
            userWidget.SetUsername($"USER_{clientInfo.UniqueIdentifier}");
        }
    }
}
