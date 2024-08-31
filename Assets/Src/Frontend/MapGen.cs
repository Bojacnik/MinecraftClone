using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Src.Backend;

namespace Src.Frontend
{

    public class MapGen : MonoBehaviour
    {
        private void Awake()
        {
            var chunkManager = new ChunkManager(1, new ChunkGenerator());

            chunkManager.LoadChunks();

            var chunks = chunkManager.LoadedChunks;
            
            Display.DisplayMesh(chunks.ToList());
        }
    }
}
