using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SrpBatcherTest : MonoBehaviour
{
    public Material Mat1;
    public Material Mat2;

    private ComputeBuffer _commonBuffer;
    void Start()
    {
        _commonBuffer = new ComputeBuffer(100, sizeof(float) * 3);
        Mat1.SetBuffer("normalsOutBuffer",_commonBuffer);
        Mat2.SetBuffer("normalsOutBuffer",_commonBuffer);
    }

    private void OnDestroy()
    {
        _commonBuffer.Dispose();
    }
}
