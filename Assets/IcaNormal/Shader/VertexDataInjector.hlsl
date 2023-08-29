#pragma warning (disable : 3571)
#pragma warning (disable : 3206)

StructuredBuffer<float3> normalsOutBuffer;
StructuredBuffer<float4> tangentsOutBuffer;

void GetVertexNormals_float(float ID, out float3 Normal, out float3 Tangent)
{
    Normal = float3(0, 0, 0);
    Tangent = float3(0, 0, 0);
    
    #ifndef SHADERGRAPH_PREVIEW
    Normal = normalsOutBuffer[ID];
    Tangent = tangentsOutBuffer[ID].xyz; 
    #endif
}

