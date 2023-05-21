# IcaNormalRecalculation
A Normal and Tangent recalculaton library for Unity

What problem does it solve?
When vertex positions change in any mesh, meshs normal data also should be recalculated to correct lightning. For this reason unity gives the Mesh.RecalculateNormals() methodto us. 
But there is a problem, this method not counting vertices that same position on space. Which causes seams on uv island bounds and submesh bounds (when using multiple material).
Also built in method not takes angle as an argument,so smooth all vertices no matter of how sharp is angle. Another downside this method cannot be use to fix blendshape normals directly.

IcaNormalRecalculation Provides 2 Normal Recalculation method
1: Bursted : Angle based method that uses job system 
   (Derived from https://medium.com/@fra3point/runtime-normals-recalculation-in-unity-a-complete-approach-db42490a5644 converted by using job system,mathematic and burst libraries, but not multithreaded due to limitations)
2: Cached: Faster method that uses builtin method then normalizing duplicate vertices based on cached data of duplicates. This method smooth all vertices no matter of angle

And 2 way to use calculated data
1: Write to Mesh : This Write new normals directly to mesh asset like unity built in method. So its not suitable for Blendshapes
2: Write to Material : This method needs a very basic custom shader whic included in the package. 
   This method compatibale with meshes that require different normals but shared same mesh, like skinned mesh renderers that use blendshapes

Tips:
For BlendShaped Character Models > use fast method and write to custom shader
For Procedural Created Meshes > use Angle based method and write to mesh

About custom shader:
NormalReceiver Shader just basic shader graph that sends custom normal and tangent data to material output. And can be used in all render pipelines.


Planned Features:
Compute shader Recalculation

