using System;
using System.Security.Cryptography.X509Certificates;

namespace Src.Backend
{
    public interface IChunkGenerator
    {
        Chunk GenerateChunk(int x, int z);
    }
}