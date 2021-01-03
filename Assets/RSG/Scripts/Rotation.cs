using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotation : MonoBehaviour
{
    public float rotation;
    void Update()
    {
        transform.Rotate(0, 0, rotation);
    }
}