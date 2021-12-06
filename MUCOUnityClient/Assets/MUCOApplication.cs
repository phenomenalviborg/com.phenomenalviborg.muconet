using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MUCOApplication : MonoBehaviour
{
    private void Awake()
    {
        Application.runInBackground = true;
    }
}
