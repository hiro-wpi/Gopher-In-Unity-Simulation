using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyController : MonoBehaviour
{
    public Rigidbody rb;
    
    public float speed = 1.0f;
    public float angularSpeed = 1.5f;
    private float angularSpeedDegree;

    private float xMove;
    private float zMove;
    private Vector3 forwardDirection;
    private Vector3 rotationVector;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // Get key input
        xMove = Input.GetAxisRaw("Horizontal");
        zMove = Input.GetAxisRaw("Vertical");

        forwardDirection = transform.forward * zMove;
        rotationVector = new Vector3(0, xMove, 0);
        angularSpeedDegree = angularSpeed * 180f / (float)Mathf.PI;
    }

    void FixedUpdate()
    {

        rb.MovePosition(transform.position + 
                        forwardDirection.normalized * Time.fixedDeltaTime * speed);

        Quaternion deltaRotation = Quaternion.Euler(rotationVector * 
                                                    angularSpeedDegree * Time.fixedDeltaTime);
        rb.MoveRotation(rb.rotation * deltaRotation);
    }
}
