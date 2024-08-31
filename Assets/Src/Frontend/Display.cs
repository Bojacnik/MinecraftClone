using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

using Src.Backend;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

namespace Src.Frontend
{
    public static class Display
    {
        public static void DisplayMesh(List<Chunk> chunks)
        {
            var mesh = CreateMesh(chunks);
            var objekt = new GameObject
            {
                name = "Chunk"
            };
            
            var meshFilter = objekt.AddComponent<MeshFilter>();
            var meshRenderer = objekt.AddComponent<MeshRenderer>();
            //var meshCollider = objekt.AddComponent<MeshCollider>();
            meshFilter.mesh = mesh.Item1;
            objekt.transform.position = Vector3.zero;
            meshRenderer.material = FindMaterial("Stone");
            //meshCollider.sharedMesh = mesh.Item1;
        }
        
        // Mask for the sides of a block
        [Flags]
        private enum Side
        {
            None = 0,
            Left = 1,
            Right = 2,
            Front = 4,
            Back = 8,
            Top = 16,
            Bottom = 32,
            All = 63
        }
        
        private static Side GetVisibleFaces(Block block, Side mask, Chunk currChunk, List<Chunk> chunks)
        {
            var result = Side.None;
            if (block.Type == BlockType.Air)
            {
                return result;
            }
            
            var x = block.X;
            var y = block.Y;
            var z = block.Z;

            var chunkX = currChunk.X;
            var chunkZ = currChunk.Z;

            
            if (mask.HasFlag(Side.Left))
            {
                // if block is on the left edge of the chunk
                if (x == 0)
                {
                    // find chunk to the left
                    var leftChunk = chunks.Find(chunk => chunk.X == chunkX - 1 && chunk.Z == chunkZ);
                    // if there is no chunk to the left, the block is visible
                    
                    if (leftChunk == null)
                    {
                        result |= Side.Left;
                    }
                    else {
                        // find the block to the left of the current block
                        var leftBlock = leftChunk?.Blocks[Chunk.GetBlockIndexInChunk((int)Chunk.CHUNK_SIZE - 1, y, z)];
                        // if there is no block to the left, the block is visible
                        if (leftBlock == null || leftBlock.Type == BlockType.Air)
                            result |= Side.Left;
                    }
                }
                else {
                    var leftblock = currChunk.Blocks[Chunk.GetBlockIndexInChunk(x - 1, y, z)];
                    if (leftblock == null || leftblock.Type == BlockType.Air)
                        result |= Side.Left;
                }
            }
            if (mask.HasFlag(Side.Right))
            {
                if (x == Chunk.CHUNK_SIZE - 1)
                {
                    var rightChunk = chunks.Find(chunk => chunk.X == chunkX + 1 && chunk.Z == chunkZ);
                    if (rightChunk == null)
                    {
                        result |= Side.Right;
                    }
                    var rightBlock = rightChunk?.Blocks[Chunk.GetBlockIndexInChunk(0, y, z)];
                    if (rightBlock == null || rightBlock.Type == BlockType.Air)
                        result |= Side.Right;
                }
                else {
                    var rightblock = currChunk.Blocks[Chunk.GetBlockIndexInChunk(x + 1, y, z)];
                    if (rightblock == null || rightblock.Type == BlockType.Air)
                        result |= Side.Right;
                }
            }
            if (mask.HasFlag(Side.Front))
            {
                if (z == 0) {
                    var frontChunk = chunks.Find(chunk => chunk.X == chunkX && chunk.Z == chunkZ - 1);
                    if (frontChunk == null)
                    {
                        result |= Side.Front;
                    } else {
                        var frontBlock = frontChunk?.Blocks[Chunk.GetBlockIndexInChunk(x, y, (int)Chunk.CHUNK_SIZE - 1)];
                        if (frontBlock == null || frontBlock.Type == BlockType.Air)
                            result |= Side.Front;
                    }
                } else {
                    var frontBlock = currChunk.Blocks[Chunk.GetBlockIndexInChunk(x, y, z - 1)];
                    if (frontBlock == null || frontBlock.Type == BlockType.Air)
                        result |= Side.Front;
                }
            }
            if (mask.HasFlag(Side.Back))
            {
                if (z == Chunk.CHUNK_SIZE - 1)
                {
                    var backChunk = chunks.Find(chunk => chunk.X == chunkX && chunk.Z == chunkZ + 1);
                    if (backChunk == null)
                    {
                        result |= Side.Back;
                    }
                    else {
                        var backBlock = backChunk?.Blocks[Chunk.GetBlockIndexInChunk(x, y, 0)];
                        if (backBlock == null || backBlock.Type == BlockType.Air)
                            result |= Side.Back;
                    }
                } else {
                    var backBlock = currChunk.Blocks[Chunk.GetBlockIndexInChunk(x, y, z + 1)];
                    if (backBlock == null || backBlock.Type == BlockType.Air)
                        result |= Side.Back;
                }
            }
            if (mask.HasFlag(Side.Top))
            {
                if (y == Chunk.MAX_HEIGHT - 1)
                {
                    result |= Side.Top;
                } else {
                    var topBlock = currChunk.Blocks[Chunk.GetBlockIndexInChunk(x, y + 1, z)];
                    if (topBlock == null || topBlock.Type == BlockType.Air)
                        result |= Side.Top;
                }
            }
            if (mask.HasFlag(Side.Bottom))
            {
                if (y == 0)
                {
                    result |= Side.Bottom;
                } else { 
                    var bottomBlock = currChunk.Blocks[Chunk.GetBlockIndexInChunk(x, y - 1, z)];
                    if (bottomBlock == null || bottomBlock.Type == BlockType.Air)
                        result |= Side.Bottom;
                }
            }
            return result;
        }

        private static (Mesh, Material[]) CreateMesh(List<Chunk> chunks)
        {
            var chunksAsArray = chunks.ToArray();
            foreach (var chunk in chunks)
            {
                var result = BinaryCulledMesher.CreateChunkMesh(chunk, chunksAsArray);
                foreach (var point in result.Item1.vertices)
                {
                    DrawSphere(new Vector4(point.x, point.y, point.z, 1), 0.5f, Color.red);
                }
                return (result.Item1, null);
            }

            return (null, null);
        }
        
        [Obsolete]
        public static void DrawSphere(Vector4 pos, float radius, Color color)
        {
            Vector4[] v = MakeUnitSphere(16);
            int len = MakeUnitSphere(16).Length / 3;
            for (int i = 0; i < len; i++)
            {
                var sX = pos + radius * v[0 * len + i];
                var eX = pos + radius * v[0 * len + (i + 1) % len];
                var sY = pos + radius * v[1 * len + i];
                var eY = pos + radius * v[1 * len + (i + 1) % len];
                var sZ = pos + radius * v[2 * len + i];
                var eZ = pos + radius * v[2 * len + (i + 1) % len];
                Debug.DrawLine(sX, eX, color);
                Debug.DrawLine(sY, eY, color);
                Debug.DrawLine(sZ, eZ, color);
            }
        }
        
        [Obsolete]
        private static Vector4[] MakeUnitSphere(int len)
        {
            Debug.Assert(len > 2);
            var v = new Vector4[len * 3];
            for (int i = 0; i < len; i++)
            {
                var f = i / (float)len;
                float c = Mathf.Cos(f * (float)(Math.PI * 2.0));
                float s = Mathf.Sin(f * (float)(Math.PI * 2.0));
                v[0 * len + i] = new Vector4(c, s, 0, 1);
                v[1 * len + i] = new Vector4(0, c, s, 1);
                v[2 * len + i] = new Vector4(s, 0, c, 1);
            }
            return v;
        }

        private static int addTriangles(Side visibleFaces, List<int> triangles, int index, int triangleCount)
        {
            // front face
            if (visibleFaces.HasFlag(Side.Front))
            {
                triangles.Add(index + 0);
                triangles.Add(index + 2);
                triangles.Add(index + 1);
                                
                triangles.Add(index + 2);
                triangles.Add(index + 3);
                triangles.Add(index + 1);
                                
                triangleCount += 2;
            }
                            
            // back face
            if (visibleFaces.HasFlag(Side.Back))
            {
                triangles.Add(index + 5);
                triangles.Add(index + 6);
                triangles.Add(index + 4);

                triangles.Add(index + 5);
                triangles.Add(index + 7);
                triangles.Add(index + 6);
                                
                triangleCount += 2;
            }
                            
            // left face
            if (visibleFaces.HasFlag(Side.Left))
            {
                triangles.Add(index + 8);
                triangles.Add(index + 9);
                triangles.Add(index + 10);

                triangles.Add(index + 10);
                triangles.Add(index + 9);
                triangles.Add(index + 11);
                                
                triangleCount += 2;
            }

            // right face
            if (visibleFaces.HasFlag(Side.Right))
            {
                triangles.Add(index + 12);
                triangles.Add(index + 14);
                triangles.Add(index + 13);

                triangles.Add(index + 14);
                triangles.Add(index + 15);
                triangles.Add(index + 13);
                                
                triangleCount += 2;
            }

            // top face
            if (visibleFaces.HasFlag(Side.Top))
            {
                triangles.Add(index + 16);
                triangles.Add(index + 18);
                triangles.Add(index + 17);

                triangles.Add(index + 18);
                triangles.Add(index + 19);
                triangles.Add(index + 17);
                                
                triangleCount += 2;
            }

            // bottom face
            if (visibleFaces.HasFlag(Side.Bottom))
            {
                triangles.Add(index + 20);
                triangles.Add(index + 21);
                triangles.Add(index + 22);

                triangles.Add(index + 22);
                triangles.Add(index + 21);
                triangles.Add(index + 23);
                                
                triangleCount += 2;
            }

            return triangleCount;
        }
        private static void addVertices(List<Vector3> vertices, Vector3 globalPosition)
        {
            // front face
            vertices.Add(globalPosition);
            vertices.Add(globalPosition + new Vector3(1, 0, 0));
            vertices.Add(globalPosition + new Vector3(0, 1, 0));
            vertices.Add(globalPosition + new Vector3(1, 1, 0));
                            
            // back face
            vertices.Add(globalPosition + new Vector3(0, 0, 1));
            vertices.Add(globalPosition + new Vector3(1, 0, 1));
            vertices.Add(globalPosition + new Vector3(0, 1, 1));
            vertices.Add(globalPosition + new Vector3(1, 1, 1));
                            
            // left face
            vertices.Add(globalPosition);
            vertices.Add(globalPosition + new Vector3(0, 0, 1));
            vertices.Add(globalPosition + new Vector3(0, 1, 0));
            vertices.Add(globalPosition + new Vector3(0, 1, 1));
                            
            // right face
            vertices.Add(globalPosition + new Vector3(1, 0, 0));
            vertices.Add(globalPosition + new Vector3(1, 0, 1));
            vertices.Add(globalPosition + new Vector3(1, 1, 0));
            vertices.Add(globalPosition + new Vector3(1, 1, 1));
                            
            // top face
            vertices.Add(globalPosition + new Vector3(0, 1, 0));
            vertices.Add(globalPosition + new Vector3(1, 1, 0));
            vertices.Add(globalPosition + new Vector3(0, 1, 1));
            vertices.Add(globalPosition + new Vector3(1, 1, 1));
                            
            // bottom face
            vertices.Add(globalPosition);
            vertices.Add(globalPosition + new Vector3(1, 0, 0));
            vertices.Add(globalPosition + new Vector3(0, 0, 1));
            vertices.Add(globalPosition + new Vector3(1, 0, 1));
        }

        private static void addUVs(List<Vector2> uv)
        {
            // front uv
            uv.Add(new Vector2(0, 0));
            uv.Add(new Vector2(1, 0));
            uv.Add(new Vector2(0, 1));
            uv.Add(new Vector2(1, 1));
                            
            // back uv
            uv.Add(new Vector2(1, 0));
            uv.Add(new Vector2(0, 0));
            uv.Add(new Vector2(1, 1));
            uv.Add(new Vector2(0, 1));
                            
            // left uv
            uv.Add(new Vector2(1, 0));
            uv.Add(new Vector2(0, 0));
            uv.Add(new Vector2(1, 1));
            uv.Add(new Vector2(0, 1));
                            
            // right uv
            uv.Add(new Vector2(0, 0));
            uv.Add(new Vector2(1, 0));
            uv.Add(new Vector2(0, 1));
            uv.Add(new Vector2(1, 1));
                            
            // top uv
            uv.Add(new Vector2(0, 0));
            uv.Add(new Vector2(1, 0));
            uv.Add(new Vector2(0, 1));
            uv.Add(new Vector2(1, 1));
                            
            // bottom uv
            uv.Add(new Vector2(1, 0));
            uv.Add(new Vector2(0, 0));
            uv.Add(new Vector2(1, 1));
            uv.Add(new Vector2(0, 1));
        }


        private static Material FindMaterial(string name)
        {
            return (Material)Resources.Load(name, typeof(Material));
        }
        
        private static Material FindMaterialOrNone(string name)
        {
            var material = (Material)Resources.Load(name, typeof(Material));
            return material != null ? material : FindMaterial("None");
        }
    }
}