using IoEditor.Models.Model;
using IoEditor.Models.Studio;

using System.Numerics;

namespace IoEditor.Models.Comparison
{
    internal class IndexedStepItemComparer : IComparer<IndexedStepItem>
    {
        private const double Tolerance = 0.01;

        public int Compare(IndexedStepItem x, IndexedStepItem y)
        {
            if (x == null || y == null)
            {
                return x == null ? (y == null ? 0 : -1) : 1;
            }

            // Compare LDrawPartName ignoring case
            int nameComparison = string.Compare(x.LDrawPartName, y.LDrawPartName, StringComparison.OrdinalIgnoreCase);
            if (nameComparison != 0)
            {
                return nameComparison;
            }

            // Compare Position
            int positionComparison = ComparePositions(x.LDrawPart.Position, y.LDrawPart.Position);
            if (positionComparison != 0)
            {
                return positionComparison;
            }

            // Compare Rotation
            return CompareRotations(x.LDrawPart.Rotation, y.LDrawPart.Rotation);
        }

        private int ComparePositions(Vector3 pos1, Vector3 pos2)
        {
            int xComparison = CompareWithTolerance(pos1.X, pos2.X);
            if (xComparison != 0) return xComparison;

            int yComparison = CompareWithTolerance(pos1.Y, pos2.Y);
            if (yComparison != 0) return yComparison;

            return CompareWithTolerance(pos1.Z, pos2.Z);
        }

        private int CompareRotations(Matrix3x3 rot1, Matrix3x3 rot2)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int comparison = CompareWithTolerance(rot1[i, j], rot2[i, j]);
                    if (comparison != 0)
                    {
                        return comparison;
                    }
                }
            }
            return 0;
        }

        private int CompareWithTolerance(float value1, float value2)
        {
            if (Math.Abs(value1 - value2) < Tolerance)
            {
                return 0;
            }
            return value1.CompareTo(value2);
        }
    }
}
