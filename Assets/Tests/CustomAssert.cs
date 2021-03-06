﻿using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Utils;

public static class CustomAssert {
    public static void AreEqual(Color expected, Color actual, string message) {
        Assert.That(actual, Is.EqualTo(expected).Using(ColorEqualityComparer.Instance), message);
    }
    public static void AreNotEqual(Color expected, Color actual, string message) {
        Assert.That(actual, Is.Not.EqualTo(expected).Using(ColorEqualityComparer.Instance), message);
    }
    public static void AreEqual(Vector2 expected, Vector2 actual, string message) {
        Assert.That(actual, Is.EqualTo(expected).Using(Vector2EqualityComparer.Instance), message);
    }
    public static void AreEqual(Vector3 expected, Vector3 actual, string message) {
        Assert.That(actual, Is.EqualTo(expected).Using(Vector3EqualityComparer.Instance), message);
    }
    public static void AreEqual(Quaternion expected, Quaternion actual, string message) {
        Assert.That(actual, Is.EqualTo(expected).Using(QuaternionEqualityComparer.Instance), message);
    }
    public static void InBounds(float actual, float expectedMinimum, float expectedMaximum, string message) {
        Assert.GreaterOrEqual(actual, expectedMinimum, message);
        Assert.LessOrEqual(actual, expectedMaximum, message);
    }
}
