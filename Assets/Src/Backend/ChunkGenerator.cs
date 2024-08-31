using System;
using UnityEngine;

namespace Src.Backend
{
    public class ChunkGenerator : IChunkGenerator
    {
        public Chunk GenerateChunk(int x, int z)
        {
            /*
            for (var i = 1; i < perlin.Length; i++)
            {
                perlin[i].seed = 
            }
            */


            var result = Mathf.PerlinNoise(5, 3);
            var chunk = new Chunk(x, z);
            chunk.SetAllAir();
            for (uint _z = 0; _z < Chunk.CHUNK_SIZE; _z++)
            {
                for (uint _x = 0; _x < Chunk.CHUNK_SIZE; _x++)
                {
                    chunk.SetBlock(_x, 0, _z, BlockType.Bedrock);
                    var (worldX, worldZ) = Chunk.ConvertLocalToWorld(_x, _z, (uint)x, (uint)z);

                    var height = Convert.ToInt32(Mathf.PerlinNoise((worldX + 0.5f) * 0.5f, (worldZ + 0.5f) * 0.5f)) +
                                 Convert.ToInt32(2 * Mathf.PerlinNoise((worldX + 0.5f) * 0.5f, (worldZ + 0.5f) * 0.05f)) +
                                 Convert.ToInt32(4 * Mathf.PerlinNoise((worldX + 0.5f) * 0.05f, (worldZ + 0.5f) * 0.05f)) +
                                 Convert.ToInt32(8 * Mathf.PerlinNoise((worldX + 0.5f) * 0.05f, (worldZ + 0.5f) * 0.05f)) +
                                 Chunk.OCEAN_LEVEL;
                    for (var _y = 1;
                         _y < height;
                         _y++)
                    {
                        // all is temporary in chunk generation
                        BlockType type;
                        if (_y > height)
                        {
                            type = BlockType.Air;
                        }
                        else if (_y == height)
                        {
                            type = BlockType.Grass;
                        }
                        else if (_y > height - 3)
                        {
                            type = BlockType.Dirt;
                        }
                        else
                        {
                            type = BlockType.Stone;
                        }
                        
                        chunk.SetBlock(_x, (uint)_y, _z, type);
                    }
                }
            }

            return chunk;
        }
    }
}