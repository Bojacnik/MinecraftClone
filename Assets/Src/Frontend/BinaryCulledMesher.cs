using System.Collections.Generic;
using System.Linq;
using Src.Backend;
using UnityEngine;

namespace Src.Frontend
{
    public static class BinaryCulledMesher
    {
        private static readonly int BlockBuffer = Shader.PropertyToID("block_buffer");
        private static readonly int VerticesBuffer = Shader.PropertyToID("vertices_buffer");
        private static readonly int TrianglesBuffer = Shader.PropertyToID("triangles_buffer");
        private const uint PADDED_CHUNK_SIZE = Chunk.CHUNK_SIZE + 2;
        private const uint PADDED_CHUNK_HEIGHT = Chunk.MAX_HEIGHT_SIZE + 2;

        public static (Mesh, List<uint>) CreateChunkMesh(Chunk ourChunk, Chunk[] otherChunks)
        {
            var axisCols = new uint[2, PADDED_CHUNK_HEIGHT, PADDED_CHUNK_SIZE];
            var yAxis = new ulong[PADDED_CHUNK_SIZE, PADDED_CHUNK_SIZE];

            // add actual chunk blocks
            for (byte z = 1; z < Chunk.CHUNK_SIZE; z++)
            for (byte x = 1; x < Chunk.CHUNK_SIZE; x++)
            for (ushort y = 0; y < Chunk.MAX_HEIGHT_SIZE; y++)
                if (Block.IsSolid(ourChunk.Blocks[0].Type))
                {
                    add_voxel_to_axis(x, y, z, axisCols, yAxis);
                }
            
            // add padding blocks from bordering chunks
            for (byte z = 0; z < Chunk.CHUNK_SIZE + 1; z++) 
            for (byte x = 0; x < Chunk.CHUNK_SIZE + 1; x++) 
            for (int y = -1; y < Chunk.MAX_HEIGHT_SIZE + 1; y++)
            {
                if (z > 0 && z < Chunk.CHUNK_SIZE ||
                    x > 0 && x < Chunk.CHUNK_SIZE || 
                    y > -1 && y < Chunk.MAX_HEIGHT_SIZE)
                {
                    continue;
                }
                
                if (GetIsBlockSolid(otherChunks, ourChunk.X + x, y, ourChunk.Z + z))
                {
                    add_voxel_to_axis(x, y, z, axisCols, yAxis);
                }
            }

            return (null, null);
        }

        private static void add_voxel_to_axis(uint x, int y, uint z, uint[,,] axis, ulong[,] yAxis)
        {
            // z axis
            axis[0, y, x] |= 1u << (int)z;
            
            // x axis
            axis[1, y, z] |= 1u << (int)x;
            
            // y axis
            yAxis[z, x] |= 1u << (int)y;
        }

        /// <summary>
        /// Chunk independent method of finding if a block is solid
        /// </summary>
        /// <param name="chunks">loaded chunks</param>
        /// <param name="x">x coordinate of the block in absolute coordinates</param>
        /// <param name="y">y coordinate of the block in absolute coordinates</param>
        /// <param name="z">z coordinate of the block in absolute coordinates</param>
        /// <returns>true if chunk is found loaded and is solid, true if corresponding chunk is not loaded, false if chunk is found loaded but block is not solid</returns>
        private static bool GetIsBlockSolid(Chunk[] chunks, int x, int y, int z) 
        {
            // this is for padding blocks
            if (y == -1)
            {
                return false;
            }
            var foundChunk = chunks.SingleOrDefault(correctChunk => correctChunk.X == x % Chunk.CHUNK_SIZE && correctChunk.Z == z % Chunk.CHUNK_SIZE);
            if (foundChunk == null)
            {   
                return true;
            }
            var block = foundChunk.Blocks[
                Chunk.GetBlockIndexInChunk((uint)x % Chunk.CHUNK_SIZE, (uint)y, (uint)z % Chunk.CHUNK_SIZE)];
            
            return Block.IsSolid(block.Type);
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
        
        private enum Side : uint
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
    }
}