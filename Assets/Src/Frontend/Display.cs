using System.Collections.Generic;
using UnityEngine;

using Src.Backend;

namespace Src.Frontend
{
    public class Display
    {
        private static Chunk FindMostLeft(List<Chunk> chunks)
        {
            var mostLeft = chunks[0];
            foreach (var chunk in chunks)
            {
                if (chunk.X < mostLeft.X)
                {
                    mostLeft = chunk;
                }
            }
            return mostLeft;
        }
        
        private static Chunk FindMostBottom(List<Chunk> chunks)
        {
            var mostBottom = chunks[0];
            foreach (var chunk in chunks)
            {
                if (chunk.Z < mostBottom.Z)
                {
                    mostBottom = chunk;
                }
            }
            return mostBottom;
        }
        
        private static Vector3 FindMostLeftBottomChunk(List<Chunk> chunks)
        {
            var mostLeft = FindMostLeft(chunks);
            var mostBottom = FindMostBottom(chunks);
            
            if (mostLeft == mostBottom)
            {
                return new Vector3(mostLeft.X * Chunk.CHUNK_SIZE, 0, mostBottom.Z * Chunk.CHUNK_SIZE);
            }
            return new Vector3(mostLeft.X * Chunk.CHUNK_SIZE, 0, mostBottom.Z - 1 * Chunk.CHUNK_SIZE);
        }
        
        // Mask for the sides of a block
        private enum Side
        {
            None = 0,
            Left = 1,
            Right = 2,
            Front = 4, 
            Back = 8,
            Top = 16,
            Bottom = 32
        }
        
        private static bool IsBlockVisible(Block block, Side mask, Chunk currChunk, List<Chunk> chunks)
        {
            if (block.Type == BlockType.Air)
            {
                return false;
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
                        return true;
                    }
                    // find the block to the left of the current block
                    var leftBlock = leftChunk.Blocks[Chunk.GetBlockIndexInChunk((int)Chunk.CHUNK_SIZE - 1, y, z)];
                    // if there is no block to the left, the block is visible
                    return leftBlock == null || leftBlock.Type == BlockType.Air;
                }

                var leftblock = currChunk.Blocks[Chunk.GetBlockIndexInChunk(x - 1, y, z)];
                return leftblock == null || leftblock.Type == BlockType.Air;
            }
            if (mask.HasFlag(Side.Right))
            {
                if (x == Chunk.CHUNK_SIZE - 1)
                {
                    var rightChunk = chunks.Find(chunk => chunk.X == chunkX + 1 && chunk.Z == chunkZ);
                    if (rightChunk == null)
                    {
                        return true;
                    }
                    var rightBlock = rightChunk.Blocks[Chunk.GetBlockIndexInChunk(0, y, z)];
                    return rightBlock == null || rightBlock.Type == BlockType.Air;
                }
                var rightblock = currChunk.Blocks[Chunk.GetBlockIndexInChunk(x + 1, y, z)];
                return rightblock == null || rightblock.Type == BlockType.Air;
            }
            if (mask.HasFlag(Side.Front))
            {
                if (z == Chunk.CHUNK_SIZE - 1)
                {
                    var frontChunk = chunks.Find(chunk => chunk.X == chunkX && chunk.Z == chunkZ + 1);
                    if (frontChunk == null)
                    {
                        return true;
                    }
                    var frontBlock = frontChunk.Blocks[Chunk.GetBlockIndexInChunk(x, y, 0)];
                    return frontBlock == null || frontBlock.Type == BlockType.Air;
                }
                var frontblock = currChunk.Blocks[Chunk.GetBlockIndexInChunk(x, y, z + 1)];
                return frontblock == null || frontblock.Type == BlockType.Air;
            }
            if (mask.HasFlag(Side.Back))
            {
                if (z == 0) {
                    var backChunk = chunks.Find(chunk => chunk.X == chunkX && chunk.Z == chunkZ - 1);
                    if (backChunk == null)
                    {
                        return true;
                    }
                    var backBlock = backChunk.Blocks[Chunk.GetBlockIndexInChunk(x, y, (int)Chunk.CHUNK_SIZE - 1)];
                    return backBlock == null || backBlock.Type == BlockType.Air;
                }
                var backblock = currChunk.Blocks[Chunk.GetBlockIndexInChunk(x, y, z - 1)];
                return backblock == null || backblock.Type == BlockType.Air;
            }

            if (mask.HasFlag(Side.Top))
            {
                if (y == Chunk.MAX_HEIGHT - 1)
                {
                    return true;
                }
                var topBlock = currChunk.Blocks[Chunk.GetBlockIndexInChunk(x, y + 1, z)];
                return topBlock == null || topBlock.Type == BlockType.Air;
            }
            
            if (mask.HasFlag(Side.Bottom))
            {
                if (y == 0)
                {
                    return false;
                }
                var bottomBlock = currChunk.Blocks[Chunk.GetBlockIndexInChunk(x, y - 1, z)];
                return bottomBlock == null || bottomBlock.Type == BlockType.Air;
            }

            return false;
        }

        private static Mesh CreateMesh(List<Chunk> chunks)
        {
            var mesh = new Mesh();
            
            var meshVertices = new List<Vector3>();
            var meshTriangles = new List<int>();
            //var meshUVs = new Vector2[mesh.vertices.Length];

            
            var verticesIndex = 0;
            for (var chunkIndex = 0; chunkIndex < chunks.Count; chunkIndex++)
            {
                var chunk = chunks[chunkIndex];
                var blockVertices = new Vector3[8];
                var blockTriangles = new int[6 * 2 * 3];
                //var blockUVs = new Vector2[8];
                
                for (var blockIndex = 0; blockIndex < chunk.Blocks.Length; blockIndex++)
                {
                    var block = chunk.Blocks[blockIndex];
                    if (block.Type == BlockType.Air)
                    {
                        continue;
                    }

                    var x = block.X + chunk.X * Chunk.CHUNK_SIZE;
                    var y = block.Y;
                    var z = block.Z + chunk.Z * Chunk.CHUNK_SIZE;
                    
                    blockVertices[0] = new Vector3(x, y, z);
                    blockVertices[1] = new Vector3(x, y, z + 1);
                    blockVertices[2] = new Vector3(x, y + 1, z);
                    blockVertices[3] = new Vector3(x, y + 1, z + 1);
                    blockVertices[4] = new Vector3(x + 1, y, z);
                    blockVertices[5] = new Vector3(x + 1, y, z + 1);
                    blockVertices[6] = new Vector3(x + 1, y + 1, z);
                    blockVertices[7] = new Vector3(x + 1, y + 1, z + 1);
                    

                    verticesIndex += 8;
                    
                    // Left face
                    if (IsBlockVisible(block, Side.Left, chunk, chunks)) {
                        blockTriangles[0] = 0 + verticesIndex - 8; 
                        blockTriangles[1] = 1 + verticesIndex - 8;
                        blockTriangles[2] = 2 + verticesIndex - 8;
                    
                        blockTriangles[3] = 2 + verticesIndex - 8;
                        blockTriangles[4] = 1 + verticesIndex - 8;
                        blockTriangles[5] = 3 + verticesIndex - 8;
                    }

                    
                    // Right face
                    if (IsBlockVisible(block, Side.Right, chunk, chunks))
                    {
                        blockTriangles[6] = 4 + verticesIndex - 8;
                        blockTriangles[7] = 6 + verticesIndex - 8;
                        blockTriangles[8] = 5 + verticesIndex - 8;
                    
                        blockTriangles[9] = 6 + verticesIndex - 8;
                        blockTriangles[10] = 7 + verticesIndex - 8;
                        blockTriangles[11] = 5 + verticesIndex - 8;
                    }
                    
                    // Bottom face
                    if (IsBlockVisible(block, Side.Bottom, chunk, chunks))
                    {
                        blockTriangles[12] = 0 + verticesIndex - 8;
                        blockTriangles[13] = 4 + verticesIndex - 8;
                        blockTriangles[14] = 1 + verticesIndex - 8;
                    
                        blockTriangles[15] = 4 + verticesIndex - 8;
                        blockTriangles[16] = 5 + verticesIndex - 8;
                        blockTriangles[17] = 1 + verticesIndex - 8;
                    }

                    // Top face
                    if (IsBlockVisible(block, Side.Top, chunk, chunks))
                    {
                        blockTriangles[18] = 2 + verticesIndex - 8;
                        blockTriangles[19] = 3 + verticesIndex - 8;
                        blockTriangles[20] = 6 + verticesIndex - 8;
                    
                        blockTriangles[21] = 3 + verticesIndex - 8;
                        blockTriangles[22] = 7 + verticesIndex - 8;
                        blockTriangles[23] = 6 + verticesIndex - 8;
                    }
                    
                    // Back face
                    if (IsBlockVisible(block, Side.Back, chunk, chunks))
                    {
                        blockTriangles[24] = 0 + verticesIndex - 8;
                        blockTriangles[25] = 2 + verticesIndex - 8;
                        blockTriangles[26] = 4 + verticesIndex - 8;
                    
                        blockTriangles[27] = 2 + verticesIndex - 8;
                        blockTriangles[28] = 6 + verticesIndex - 8;
                        blockTriangles[29] = 4 + verticesIndex - 8;
                    }
                    
                    // Front face
                    if (IsBlockVisible(block, Side.Front, chunk, chunks))
                    {
                        blockTriangles[30] = 1 + verticesIndex - 8;
                        blockTriangles[31] = 5 + verticesIndex - 8;
                        blockTriangles[32] = 3 + verticesIndex - 8;
                    
                        blockTriangles[33] = 5 + verticesIndex - 8;
                        blockTriangles[34] = 7 + verticesIndex - 8;
                        blockTriangles[35] = 3 + verticesIndex - 8;
                    }
                    
                    meshVertices.AddRange(blockVertices);
                    meshTriangles.AddRange(blockTriangles);
                    //meshUVs = new Vector2[meshVertices.Count];
                }
            }
            
            mesh.vertices = meshVertices.ToArray();
            mesh.triangles = meshTriangles.ToArray();
            

            mesh.Optimize();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            return mesh;
        }
        
        public static void DisplayMesh(List<Chunk> chunks)
        {
            var mesh = CreateMesh(chunks);
            var objekt = new GameObject();
            
            var meshFilter = objekt.AddComponent<MeshFilter>();
            var meshRenderer = objekt.AddComponent<MeshRenderer>();
            var meshCollider = objekt.AddComponent<MeshCollider>();
            meshFilter.mesh = mesh;
            objekt.transform.position = Vector3.zero;
            meshRenderer.material = FindMaterial("Stone");
            meshCollider.sharedMesh = mesh;
        }

        public static void DisplayChunk(Chunk chunk)
        {
            foreach (var block in chunk.Blocks)
            {
                DisplayBlock(block.X, block.Y, block.Z, block.Type, chunk.X, chunk.Z);
            }
        }
        
        public static void DisplayBlock(long x, long y, long z, BlockType type, int chunkx, int chunkz)
        {
            if (type == BlockType.Air)
            {
                return;
            }
            
            var objekt = new GameObject();
            var meshFilter = objekt.AddComponent<MeshFilter>();
            var meshRenderer = objekt.AddComponent<MeshRenderer>();
            var meshCollider = objekt.AddComponent<MeshCollider>();
            //objekt.AddComponent<Shader>();

            var mesh = new Mesh();
            meshFilter.mesh = mesh;
        
            objekt.transform.position = new Vector3(x + chunkx * Chunk.CHUNK_SIZE, y, z + chunkz * Chunk.CHUNK_SIZE);

            // Define the vertices. A square has 4 vertices.
            var vertices = new Vector3[8];
            vertices[0] = new Vector3(0, 0, 0);
            vertices[1] = new Vector3(0, 0, 1);
            vertices[2] = new Vector3(0, 1, 0);
            vertices[3] = new Vector3(0, 1, 1);
            vertices[4] = new Vector3(1, 0, 0);
            vertices[5] = new Vector3(1, 0, 1);
            vertices[6] = new Vector3(1, 1, 0);
            vertices[7] = new Vector3(1, 1, 1);
        
            // Define the triangles. A square has 2 triangles.
            var triangles = new int[6*2*3];
            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 2;

            triangles[3] = 2;
            triangles[4] = 1;
            triangles[5] = 3;

            triangles[6] = 4;
            triangles[7] = 6;
            triangles[8] = 5;

            triangles[9] = 6;
            triangles[10] = 7;
            triangles[11] = 5;

            triangles[12] = 0;
            triangles[13] = 4;
            triangles[14] = 1;

            triangles[15] = 4;
            triangles[16] = 5;
            triangles[17] = 1;

            triangles[18] = 2;
            triangles[19] = 3;
            triangles[20] = 6;

            triangles[21] = 3;
            triangles[22] = 7;
            triangles[23] = 6;

            triangles[24] = 0;
            triangles[25] = 2;
            triangles[26] = 4;

            triangles[27] = 2;
            triangles[28] = 6;
            triangles[29] = 4;

            triangles[30] = 1;
            triangles[31] = 5;
            triangles[32] = 3;

            triangles[33] = 5;
            triangles[34] = 7;
            triangles[35] = 3;
        
            // Assign the vertices and triangles to the mesh
            mesh.vertices = vertices;
            mesh.triangles = triangles;

            var material = type switch
            {
                BlockType.Stone => FindMaterialOrNone("Stone"),
                BlockType.Grass => FindMaterialOrNone("Grass"),
                BlockType.Dirt => FindMaterialOrNone("Dirt"),
                BlockType.Bedrock => FindMaterialOrNone("Bedrock"),
                _ => FindMaterial("None")
            };
            meshRenderer.material = material;
        
            meshCollider.sharedMesh = mesh;

            // Recalculate the normals (this is important for lighting)
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
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