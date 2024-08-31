using System;

namespace Src.Backend 
{
    public enum BlockType : uint
    {
        Air,
        Bedrock,
        Stone,
        Dirt,
        Grass,
    }
    
    public class Block
    {
        public readonly uint X, Y, Z;
        public readonly BlockType Type;
        
        public Block(uint x, uint y, uint z, BlockType type)
        {
            X = x;
            Y = y;
            Z = z;
            Type = type;
        }
        
        public static bool IsSolid(BlockType type)
        {
            return type switch
            {
                BlockType.Air => false,
                _ => true
            };
        }

        public byte[] Serialize()
        {
            return Utility.Concatenate(
                Utility.Concatenate(
                    BitConverter.GetBytes(X),
                    BitConverter.GetBytes(Y)),
                Utility.Concatenate(
                    BitConverter.GetBytes(Z),
                    BitConverter.GetBytes((uint)Type))
            );
        }
    }
}