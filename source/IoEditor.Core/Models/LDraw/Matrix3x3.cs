namespace IoEditor.Models.Model
{
    public struct Matrix3x3
    {
        public float M11;
        public float M12;
        public float M13;
        public float M21;
        public float M22;
        public float M23;
        public float M31;
        public float M32;
        public float M33;

        public Matrix3x3(float m11, float m12, float m13, float m21, float m22, float m23, float m31, float m32, float m33)
        {
            M11 = m11;
            M12 = m12;
            M13 = m13;
            M21 = m21;
            M22 = m22;
            M23 = m23;
            M31 = m31;
            M32 = m32;
            M33 = m33;
        }

        public static Matrix3x3 operator *(Matrix3x3 a, Matrix3x3 b)
        {
            return new Matrix3x3(
                a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31,
                a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32,
                a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33,
                a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31,
                a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32,
                a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33,
                a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31,
                a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32,
                a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33
            );
        }

        public bool Equals(Matrix3x3 other)
        {
            return M11 == other.M11 &&
                   M12 == other.M12 &&
                   M13 == other.M13 &&
                   M21 == other.M21 &&
                   M22 == other.M22 &&
                   M23 == other.M23 &&
                   M31 == other.M31 &&
                   M32 == other.M32 &&
                   M33 == other.M33;
        }

        public float this[int row, int col]
        {
            get
            {
                return (row, col) switch
                {
                    (0, 0) => M11,
                    (0, 1) => M12,
                    (0, 2) => M13,
                    (1, 0) => M21,
                    (1, 1) => M22,
                    (1, 2) => M23,
                    (2, 0) => M31,
                    (2, 1) => M32,
                    (2, 2) => M33,
                    _ => throw new IndexOutOfRangeException("Invalid matrix index")
                };
            }
        }

        public override int GetHashCode()
        {
            var hash1 = HashCode.Combine(M11, M12, M13, M21, M22, M23);
            var hash2 = HashCode.Combine(M31, M32, M33);
            return HashCode.Combine(hash1, hash2);
        }

        public static bool operator ==(Matrix3x3 left, Matrix3x3 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Matrix3x3 left, Matrix3x3 right)
        {
            return !(left == right);
        }
    }
}