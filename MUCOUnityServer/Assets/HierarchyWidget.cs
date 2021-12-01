using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HierarchyWidget : MonoBehaviour
{
    [SerializeField] private GameObject m_HierarchyElementPrefab;

    [SerializeField] private GameObject m_HierarchyList;

    private void Start()
    {
        Regenerate();
    }

    public void Regenerate()
    {
        // DESTROY THE CHILDREN!! HUU HAHAA AHHHHHHH!
        foreach (Transform child in m_HierarchyList.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        Instantiate(m_HierarchyElementPrefab, m_HierarchyList.transform);
        Instantiate(m_HierarchyElementPrefab, m_HierarchyList.transform);
    }
}
