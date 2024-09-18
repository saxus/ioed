using IoEditor.Models.Model;

namespace IoEditor.Models.Comparison
{
    internal static class LDrawPartExtensions
    {
        public static bool EqualsWithCoordinates(this LDrawPart a, LDrawPart b)
        {
            if (a == null || b == null)
                return false;

            // Check if the model names are equal
            if (!string.Equals(a.Model?.Name, b.Model?.Name, StringComparison.OrdinalIgnoreCase))
                return false;

            // Check if the positions are equal
            if (a.Position != b.Position)
                return false;

            // Check if the rotation matrices are equal
            if (!a.Rotation.Equals(b.Rotation))
                return false;

            return true;
        }
    }
}
