using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{

    public float _speed = 1f;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0, 0, _speed * Time.deltaTime));
    }
}
