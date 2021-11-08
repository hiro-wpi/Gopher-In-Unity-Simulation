using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformController : MonoBehaviour
{
    public float speed = 1.0f;
    public float angularSpeed = 1.5f;

    private Transform tf;
    private float xMove;
    private float zMove;
    private Vector3 forwardDirection;

    // Start is called before the first frame update
    void Start()
    {
        tf = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        // Get key input
        xMove = Input.GetAxisRaw("Horizontal");
        zMove = Input.GetAxisRaw("Vertical");
        forwardDirection = transform.forward * zMove;

        tf.Translate(speed * forwardDirection.normalized * Time.deltaTime);
        tf.Rotate(angularSpeed * Vector3.up * Time.deltaTime);
    }
}
