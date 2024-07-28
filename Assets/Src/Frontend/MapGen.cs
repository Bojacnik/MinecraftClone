using System.Collections.Generic;
using UnityEngine;

using Src.Backend;

namespace Src.Frontend
{
    public class MapGen : MonoBehaviour
    {
        private void Awake()
        {
            var chunkManager = new ChunkManager(1);

            chunkManager.LoadChunks();

            var chunks = chunkManager.LoadedChunks;
            var list = new List<Chunk>(chunks);
            
            foreach (var chunk in list)
            {
                foreach (var block in chunk.Blocks)
                {
                    if (block.Type == BlockType.Stone) continue;
                    Display.DisplayBlock(block.X, block.Y, block.Z, block.Type, chunk.X, chunk.Z);
                }
            }
        }
    }
}
