using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    public Transform target;

    public Vector3 offset;
    public bool lockX, lockY, lockZ;

    private Vector3 initPosition = Vector3.zero;
    public bool useLocalPosition, followRotation;
    // Start is called before the first frame update
    void Start()
    {
        initPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
       
    }
}
