using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    void Start() {}

    void Update() {}

    public float AddAndMultiplyRegular(float a, float b)
    {
        return (a + b) * (a + b);
    }


    public void AddAndMultiply(float a, float b, Action<float> callback)
    {
        StartCoroutine(AddAndMultiplyCoroutine(a, b, callback));
    }

    private IEnumerator AddAndMultiplyCoroutine(float a, float b, Action<float> callback)
    {
        yield return new WaitForSeconds(1f);

        callback( (a + b) * (a + b) );
    }


    public void Call()
    {
        float result = AddAndMultiplyRegular(2, 3);
        Debug.Log(result);


        AddAndMultiply(2, 3, CallBackFun);
    }

    private void CallBackFun(float result)
    {
        Debug.Log(result);
    }
}
