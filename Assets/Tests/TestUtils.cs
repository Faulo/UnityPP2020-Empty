using UnityEngine;

namespace Tests
{
    public class TestUtils
    {
        public static bool Approximately(Vector3 one, Vector3 two)
        {
            return Mathf.Approximately(one.x, two.x)
                && Mathf.Approximately(one.y, two.y)
                && Mathf.Approximately(one.z, two.z);
        }
    }
}