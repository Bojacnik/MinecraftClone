using System;

namespace Src.Backend 
{
    public enum BlockType
    {
        Air,
        Stone,
        Grass,
        Dirt,
        Bedrock
    }
    
    public class Block
    {
        public readonly int X, Y, Z;
        public readonly BlockType Type;
        
        public Block(int x, int y, int z, BlockType type)
        {
            X = x;
            Y = y;
            Z = z;
            Type = type;
        }

        public byte[] Serialize()
        {
            return Utility.Concatenate(
                Utility.Concatenate(
                    BitConverter.GetBytes(X),
                    BitConverter.GetBytes(Y)),
                Utility.Concatenate(
                    BitConverter.GetBytes(Z),
                    BitConverter.GetBytes((int)Type))
            );
        }
    }
}