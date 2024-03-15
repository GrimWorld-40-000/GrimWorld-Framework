using System;
using RimWorld;

namespace GW4KArmor.Data
{
    public struct TextureID : IEquatable<TextureID>
    {
        public BodyTypeDef BodyType { get; set; }
        public int Index { get; set; }
        public byte Rotation { get; set; }

        public string MakeTexturePath(string formatString)
        {
            var text = BodyType == null ? null : "_" + BodyType.defName;
            return $"{formatString}{text}_mask{Index + 1}{GetRotationName(Rotation)}";
        }

        public bool Equals(TextureID other)
        {
            return Equals(BodyType, other.BodyType) && Index == other.Index &&
                   Rotation == other.Rotation;
        }

        public override bool Equals(object obj)
        {
            bool result;
            if (obj is TextureID)
            {
                var other = (TextureID)obj;
                result = Equals(other);
            }
            else
            {
                result = false;
            }
            return result;
        }

        public override int GetHashCode()
        {
            var num = BodyType != null ? BodyType.GetHashCode() : 0;
            num = (num * 397) ^ Index;
            return (num * 397) ^ Rotation.GetHashCode();
        }

        private static string GetRotationName(byte rotation)
        {
            string result;
            switch (rotation)
            {
                case 0:
                    result = "_north";
                    break;
                case 1:
                    result = "_east";
                    break;
                case 2:
                    result = "_south";
                    break;
                case 3:
                    result = "_east";
                    break;
                case 4:
                    result = "";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("rotation", rotation.ToString());
            }

            return result;
        }

    }
}