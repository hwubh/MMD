using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace MMD_URP 
{
    class MeshCreationInfo
    {
        public class Pack
        {
            public int material_index; //マテリアル
            public int[] plane_indices;    //面
            public int[] vertices;     //頂点
        }
        public Pack[] value;
        public int[] all_vertices;         //総頂点
        public Dictionary<int, int> reassign_dictionary;  //頂点リアサインインデックス用辞書
    }

    public partial class Convertor
    {
        Mesh CreateMesh(PMXObject pmx) 
        {
            Mesh mesh;

            var triangles = pmx.SurfaceList;
            var materialList = pmx.MaterialList;
            var materialCount = materialList.Length;
            var vertexList = pmx.VertexList;
            var vertexCount = vertexList.Length;
            MeshCreationInfo result = new MeshCreationInfo();

            // Only create one mesh renderer, we assume that the index buffer is 32 bit. https://docs.unity3d.com/ScriptReference/Mesh-indexFormat.html
            // Assign vertices to each material(submesh)
            int indexStart = 0;
            var subMeshDescriptor = new SubMeshDescriptor[materialCount];
            for (int i = 0; i < materialCount; i++)
            {
                int indexCount = materialList[i].surfaceCount;
                var topology = GetMeshTopology(materialList[i].flag);
                subMeshDescriptor[i] = new SubMeshDescriptor(indexStart, indexCount, topology);
                indexStart += indexCount;
            }

            var vertices = new int[vertexCount];
            var reassign_dictionary = new Dictionary<int, int>(vertexCount);
            for (int i = 0; i < vertexCount; ++i)
            {
                vertices[i] = i;
                reassign_dictionary[i] = i;
            }
            result.all_vertices = vertices;
            result.reassign_dictionary = reassign_dictionary;

            mesh = CreateMesh(result, vertexList);

            return mesh;
        }

        MeshTopology GetMeshTopology(byte flag) 
        {
            if (flag | (byte) PMXMaterial.Flag.PointDrawing == )
                return MeshTopology.Points;
            if (flag == PMXMaterial.Flag.LineDrawing)
                return MeshTopology.Lines;

            return MeshTopology.Triangles;
        }

        MeshCreationInfo.Pack[] CreateSubMeshes(int materialCount, PMXMaterial[] materialList, int[] triangles)
        {
            int plane_start = 0;
            var packs = new MeshCreationInfo.Pack[materialCount];
            for (int i = 0; i < materialCount; i++)
            {
                MeshCreationInfo.Pack pack = new MeshCreationInfo.Pack();
                pack.material_index = i;
                int plane_count = materialList[i].surfaceCount;
                pack.plane_indices = (new ArraySegment<int>(triangles, plane_start, plane_count)).ToArray();
                pack.vertices = pack.plane_indices.Distinct().ToArray();
                plane_start += plane_count;
                packs[i] = pack;
            }
            return packs;
        }

        Mesh CreateMesh(MeshCreationInfo result, PMXVertex[] vertexList)
        {
            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            var vertices = result.all_vertices;
            var vertexCount = vertices.Length;
            var tempVertices = new Vector3[vertexCount];
            var tempNormals = new Vector3[vertexCount];
            var tempUVs = new Vector2[vertexCount];
            var tempColors = new Color[vertexCount];
            NativeList<BoneWeight1> bonesWeight = new NativeList<BoneWeight1>(0, AllocatorManager.Temp);
            NativeList<byte> bonesPerVertex = new NativeList<byte>(0, AllocatorManager.Temp);
            Color color = new Color(0f, 0f, 0f, 0f);
            for (int j = 0; j < vertexCount; j++)
            {
                tempVertices[j] = new Vector3();
                tempVertices[j] = vertexList[vertices[j]].pos * 1.0f;
                tempNormals[j] = new Vector3();
                tempNormals[j] = vertexList[vertices[j]].normal;
                tempUVs[j] = new Vector2();
                tempUVs[j] = vertexList[vertices[j]].uv;
                tempColors[j] = new Color();
                color.a = vertexList[vertices[j]].edgeScale * 0.25f;
                tempColors[j] = color;
                var PMXboneWeight = vertexList[vertices[j]].boneWeight;
                var boneCount = PMXboneWeight.boneCount;
                bonesPerVertex.Add(boneCount);
                for (int a = 0; a < boneCount; a++)
                {
                    var boneWeights = PMXboneWeight.boneWeights;
                    bonesWeight.Add(boneWeights[a]);
                }
            };
            mesh.vertices = tempVertices;
            mesh.normals = tempNormals;
            mesh.uv = tempUVs;
            mesh.colors = tempColors;
            mesh.SetBoneWeights(bonesPerVertex, bonesWeight);
            var materialCount = result.value.Length;
            mesh.subMeshCount = materialCount;
            for (int j = 0; j < materialCount; ++j)
            {
                int[] indices = result.value[j].plane_indices.Select(x => result.reassign_dictionary[x]).ToArray();
                mesh.SetTriangles(indices, j);
            }
            return mesh;
        }
    }
}
