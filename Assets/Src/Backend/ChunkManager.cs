using System;
using System.Numerics;

namespace Src.Backend
{
    // TODO: Implement
    public class ChunkManager
    {
        private Vector3 _playerPosition;
        public readonly Chunk[] LoadedChunks;
        private int _loadedChunksSize;
        
        public ChunkManager(uint a)
        {
            _playerPosition = Vector3.Zero;
            if ((a & 1) == 0)
            {
                throw new ArgumentException("ChunkManager loaded chunk size must be an odd number");
            }
            _loadedChunksSize = Convert.ToInt32(a);
            LoadedChunks = new Chunk[Convert.ToUInt32(a * a)];
        }   

        public void LoadChunks()
        {
            var playerChunkX = Convert.ToInt32(_playerPosition.X / Chunk.CHUNK_SIZE);
            var playerChunkZ = Convert.ToInt32(_playerPosition.Z / Chunk.CHUNK_SIZE);

            var index = 0;
            for (var z = playerChunkZ - _loadedChunksSize / 2; z < playerChunkZ + _loadedChunksSize / 2 + 1; z++)
            {
                for (var x = playerChunkX - _loadedChunksSize / 2; x < playerChunkX + _loadedChunksSize / 2 + 1; x++)
                {
                    LoadedChunks[index] = ChunkGenerator.GenerateChunk(x, z);
                    index++;
                }
            }
        }
        
        public void Update(Vector3 playerPosition)
        {
            _playerPosition = playerPosition;
        }

        
    }
}