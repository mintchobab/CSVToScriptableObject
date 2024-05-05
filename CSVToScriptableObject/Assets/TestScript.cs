using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    void Start()
    {
        string name = TestTableFirst.Instance[101].Name;
        string desc = TestTableFirst.Instance[101].Desc;
    }
}
