using System.IO;

namespace Src.Backend
{
    public class ChunkLoader
    {
        private const string FILE_EXTENSION = ".chunk";
        private readonly string _folder;
        
        public ChunkLoader(string folder)
        {
            _folder = folder;
        }

        public void Initialize()
        {
            if (Directory.Exists(_folder)) return;
            throw new DirectoryNotFoundException($"Directory {_folder} does not exist");
        }
        
        public void SaveChunk(Chunk chunk)
        {
            var chunkName = $"{_folder}/gamechunk-{chunk.GetHash()}{FILE_EXTENSION}";
            
            if (ChunkExists(chunkName + FILE_EXTENSION))
            {
                var tmp = Path.GetTempFileName();
                using (var writer = new FileStream(chunkName + tmp + FILE_EXTENSION, FileMode.Create, FileAccess.Write))
                {
                    var chunkData = chunk.Serialize();
                    writer.Write(chunkData, 0, chunkData.Length);
                }    
                File.Replace(chunkName + tmp + FILE_EXTENSION, chunkName + FILE_EXTENSION, null);
            }
            else
            {
                using var writer = new FileStream(chunkName + FILE_EXTENSION, FileMode.Create, FileAccess.Write);
                var chunkData = chunk.Serialize();
                writer.Write(chunkData, 0, chunkData.Length);
            }
        }
        
        public Chunk LoadChunk(int x, int y)
        {
            var chunkName = $"{_folder}/gamechunk-{Chunk.GetHash(x, y)}{FILE_EXTENSION}";
            if (!ChunkExists(chunkName))
            {
                //TODO: if not found then it most likely hasnt been generated yet
                // add logic to generate that chunk
                throw new FileNotFoundException($"Chunk {chunkName} not found");
            }
            
            using var reader = new FileStream(chunkName, FileMode.Open, FileAccess.Read);
            var chunkData = new byte[Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE * Chunk.MAX_HEIGHT];
            var read = reader.Read(chunkData, 0, chunkData.Length);
            if (read != chunkData.Length)
            {
                throw new InvalidDataException($"Chunk {chunkName} is corrupted");
            }
            return new Chunk(x, y);
        }
        
        private static bool ChunkExists(string chunkName)
        {
            return File.Exists(chunkName);
        }
    }
}