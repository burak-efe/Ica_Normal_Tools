StructuredBuffer<float3> buffer;

void buffer_float(float ID, out float3 outf3)
{
    outf3 = float3(0, 0, 0);
    
    #ifndef SHADERGRAPH_PREVIEW
    outf3 = buffer[ID];
    #endif
}

