﻿using System;
using System.Security.Cryptography;

namespace Src.Backend
{
    public class Chunk
    {
        public const uint CHUNK_SIZE = 16;
        public const uint MAX_HEIGHT = 61;
        public const uint MAX_HEIGHT_SIZE = MAX_HEIGHT + 1;
        public const uint OCEAN_LEVEL = 54;

        private static readonly MD5 MD5 = MD5.Create();
        
        public readonly int X, Z;
        public readonly Block[] Blocks;
        
        public Chunk(int x, int z)
        {
            X = x;
            Z = z;
            Blocks = new Block[MAX_HEIGHT_SIZE * CHUNK_SIZE * CHUNK_SIZE];
        }
        
        public byte[] GetHash()
        {
            var hashx = MD5.ComputeHash(BitConverter.GetBytes(X));
            var hashy = MD5.ComputeHash(BitConverter.GetBytes(Z));
            return Utility.Concatenate(hashx, hashy);
        }
        
        public static byte[] GetHash(int x, int y)
        {
            var hashx = MD5.ComputeHash(BitConverter.GetBytes(x));
            var hashy = MD5.ComputeHash(BitConverter.GetBytes(y));
            return Utility.Concatenate(hashx, hashy);
        }

        public void SetAllAir()
        {
            for (uint z = 0; z < CHUNK_SIZE; z++)
            {
                for (uint x = 0; x < CHUNK_SIZE; x++)
                {
                    for (uint y = 0; y < MAX_HEIGHT; y++)
                    {
                        SetBlock(x, y, z, BlockType.Air);
                    }
                }
            }
        }
        
        public void SetBlock(uint x, uint y, uint z, BlockType type)
        {
            Blocks[GetBlockIndexInChunk(x, y, z)] = new Block(x, y, z, type);
        }

        public static long GetBlockIndexInChunk(uint x, uint y, uint z)
        {
            return y + x * MAX_HEIGHT_SIZE + z * MAX_HEIGHT_SIZE * CHUNK_SIZE;
        }
        
        public static (uint x, uint z) ConvertWorldToChunk(uint x, uint z)
        {
            return (x / CHUNK_SIZE, z / CHUNK_SIZE);
        }
        
        public static (long x, long z) ConvertLocalToWorld(uint xBlock, uint zBlock, uint xChunk, uint zChunk)
        {
            return (xBlock + xChunk * CHUNK_SIZE, zBlock + zChunk * CHUNK_SIZE);
        }
        
        public byte[] Serialize()
        {
            // TODO: Implement better error handling
            if (Blocks.Length <= 0)
                throw new Exception("No blocks to serialize");

            var result = Blocks[0].Serialize();
            for (var i = 1; i < Blocks.Length; i++)
            {
                result = Utility.Concatenate(result, Blocks[i].Serialize());
            }

            return result;
        }

        public static Chunk Deserialize(byte[] data)
        {
            var chunk = new Chunk(0, 0);
            
            for (var i = 0; i < data.Length; i += 16)
            {
                var x = BitConverter.ToUInt32(data, i);
                var y = BitConverter.ToUInt32(data, i + 4);
                var z = BitConverter.ToUInt32(data, i + 8);
                var type = (BlockType) BitConverter.ToInt32(data, i + 12);
                chunk.Blocks[i] = new Block(x, y, z, type);
            }

            return chunk;
        }

        public override string ToString()
        {
            return $"Chunk {X},{Z}";
        }
    }
}