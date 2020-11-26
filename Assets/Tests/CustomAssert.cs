using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Utils;

public static class CustomAssert
{
    public static void AreEqual(Color expected, Color actual, string message)
    {
        Assert.That(expected, Is.EqualTo(actual).Using(ColorEqualityComparer.Instance), message);
    }
    public static void AreEqual(Vector2 expected, Vector2 actual, string message)
    {
        Assert.That(expected, Is.EqualTo(actual).Using(Vector2EqualityComparer.Instance), message);
    }
    public static void AreEqual(Vector3 expected, Vector3 actual, string message)
    {
        Assert.That(expected, Is.EqualTo(actual).Using(Vector3EqualityComparer.Instance), message);
    }
}
