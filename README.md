# Ica Normal Recalculation
A Normal and Tangent recalculaton library for Unity

## What problem does it solve?
When vertex positions change in any mesh, meshs normal data also should be recalculated to correct lightning. For this reason unity gives the Mesh.RecalculateNormals() funtion to us. <br />

But there is a problem, this method not counting vertices that same position on space. Which causes seams on uv island bounds and submesh bounds (when using multiple material).<br />
	
Also built in method not takes angle as an argument,so smooth all vertices no matter of how sharp is angle. Another downside this method not suitable for fix blendshape normals directly.<br />

![compare](https://github.com/burak-efe/Ica-Normal-Recalculation/assets/82805019/9fee8357-13d9-40f2-8e76-44c5d894b08a)

## Ica Normal Recalculation Provides 2 Normal Recalculation method
1: Bursted : Angle based method that uses job system <br />
	(Derived from https://medium.com/@fra3point/runtime-normals-recalculation-in-unity-a-complete-approach-db42490a5644 converted by using job system,mathematic and burst libraries, but not multithreaded due to limitations)<br />
2: Cached: Faster method that uses builtin method then normalizing duplicate vertices based on cached data of duplicates. This method smooth all vertices no matter of angle or sharp edge<br />

## And 2 way to use calculated data
1: Write to Mesh : This Write new normals directly to mesh asset like unity built in method. So its not suitable for Blendshapes<br />

2: Write to Material : This method needs a very basic custom shader whic included in the package. <br />
   This method compatible with meshes that require different normals but shared same mesh, like skinned mesh renderers that use blendshapes and shraing same model<br />
   
![2023-05-22 07_53_58-Window](https://github.com/burak-efe/Ica-Normal-Recalculation/assets/82805019/e6f4172c-49d5-4b53-ac9c-ab37722b85b2)

## Tips:
For BlendShaped Character Models > use Cached method and write to custom shader<br />
For Procedural Created Meshes > use Bursted method and write to mesh<br />

## About custom shader:<br />
NormalReceiver Shader just basic shader graph that sends custom normal and tangent data to material output. And can be used in all render pipelines.<br />

## Requirements
1: Burst Package <br />
2: Collections Package <br />
3: Mathematics Package <br />
4: Shader Graph Package (for custom material output shader) <br />

## Caveats
1: Meshes should be imported as Read/Write enabled. <br />
2: A scriptable object needed every mesh asset that use runtime recalculate component. <br />

## Planned Features:
Compute Shader Recalculation Method <br />
Using Job System on Cached Method <br />
Using Mesh Sharp edges for Cached Method <br />

