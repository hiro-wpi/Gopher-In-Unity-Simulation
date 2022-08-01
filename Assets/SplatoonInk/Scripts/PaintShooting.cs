using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintShooting : MonoBehaviour
{
    public ParticleSystem sprayParticle;
    public ParticleSystem collisionParticle;

    void Start()
    {  
    }

    void Update()
    {
    }

    public void PlayPainting()
    {
        sprayParticle.Play();
        collisionParticle.Play();
    }

    public void StopPainting()
    {
        sprayParticle.Stop();
        collisionParticle.Stop();
    }
}
