using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HierarchyUserWidget : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text m_UsernameText;

    public void SetUsername(string username)
    {
        m_UsernameText.SetText(username);
    }
}
    