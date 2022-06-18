using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    public ParticleSystem sprayParticle;

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            sprayParticle.Play();
        else if (Input.GetMouseButtonUp(0))
            sprayParticle.Stop();
    }
}
