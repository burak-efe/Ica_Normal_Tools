using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeTester : MonoBehaviour
{
    private ComputeBuffer buffer1;
    public Material Mat1;
    private void Start()
    {
        var b = new Vector3[24];
        for (int i = 8; i < 16; i++)
        {
            b[i] = new Vector3(0, 1, 0);
        }
        buffer1 = new ComputeBuffer(24, sizeof(float) * 3);
        buffer1.SetData(b);
        Mat1.SetBuffer("buffer", buffer1);
    }

    private void OnDestroy()
    {
        buffer1.Dispose();
    }
}
