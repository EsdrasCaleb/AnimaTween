using System;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using AnimaTween;



public class AnimaTweenCSharpObjectTests
{
    public class AnimaTweenTestCSharpObject
    {
        public float myFloat;
        public double myDouble;
        public int myInt;
        public string myString;
        public Color myColor;
        public Vector2 myVector2;
        public Vector3 myVector3;
        public Vector4 myVector4;
        public Quaternion myQuaternion;
        public Rect myRect;
        public Bounds myBounds;
        public Gradient myGradient;
        public Vector2Int myVector2Int;
        public Vector3Int myVector3Int;
    }
    
    private AnimaTweenTestCSharpObject _testObject;

    [SetUp]
    public void Setup()
    {
        _testObject = new AnimaTweenTestCSharpObject();
    }

    [TearDown]
    public void Teardown()
    {
        _testObject = null;
    }

    [UnityTest]
    public IEnumerator ATween_CSharpObject_Float_ChangesOverTimeAndCompletes()
    {
        float startValue = 0f;
        float targetValue = 100f;
        float duration = 1f;
        _testObject.myFloat = startValue;

        _testObject.ATween("myFloat", targetValue, duration);

        yield return new WaitForSeconds(duration / 2f);
        float midValue = _testObject.myFloat;
        Assert.IsTrue(midValue > startValue && midValue < targetValue, $"Value should be between start and end halfway. Expected between {startValue} to {targetValue}, but was {midValue}.");

        yield return new WaitForSeconds(duration / 2f+0.1f);
        Assert.AreEqual(targetValue, _testObject.myFloat, $"Final value should be target. Expected: {targetValue}, Actual: {_testObject.myFloat}.");
    }

    [UnityTest]
    public IEnumerator ATween_CSharpObject_Vector3_ChangesTowardsTarget()
    {
        Vector3 startValue = Vector3.down * 10f;
        Vector3 targetValue = Vector3.up * 10f;
        float duration = 1f;
        _testObject.myVector3 = startValue;
        float initialDistance = Vector3.Distance(startValue, targetValue);

        _testObject.ATween("myVector3", targetValue, duration);
        
        yield return new WaitForSeconds(duration / 2f);
        float midDistance = Vector3.Distance(_testObject.myVector3, targetValue);
        Assert.Less(midDistance, initialDistance, $"Object should be closer to target halfway. Mid distance {midDistance} was not less than initial {initialDistance}.");

        yield return new WaitForSeconds(duration / 2f+0.1f);
        Assert.IsTrue(Vector3.Distance(targetValue, _testObject.myVector3) < 0.001f, $"Final value should be target. Expected: {targetValue}, Actual: {_testObject.myVector3}.");
    }
    
    [UnityTest]
    public IEnumerator ATween_CSharpObject_Color_ChangesTowardsTarget()
    {
        Color startValue = new Color(1f, 0f, 0f, 0f);
        Color targetValue = new Color(0f, 1f, 1f, 1f);
        float duration = 1f;
        _testObject.myColor = startValue;

        _testObject.ATween("myColor", targetValue, duration);

        yield return new WaitForSeconds(duration / 2f);
        Color midValue = _testObject.myColor;
        Assert.Less(midValue.r, startValue.r, $"Red channel should be decreasing. Mid value {midValue.r} was not less than start {startValue.r}.");
        Assert.Greater(midValue.g, startValue.g, $"Green channel should be increasing. Mid value {midValue.g} was not greater than start {startValue.g}.");

        yield return new WaitForSeconds(duration / 2f+0.1f);
        Assert.AreEqual(targetValue, _testObject.myColor, $"Final value should be target. Expected: {targetValue}, Actual: {_testObject.myColor}.");
    }
    
    [UnityTest]
    public IEnumerator ATween_Path_CSharpObject_Float_FollowsWaypointsAndCompletes()
    {
        float startValue = 0f;
        var path = new[] { 10f, -5f, 5f };
        float duration = 1.2f;

        _testObject.myFloat = startValue;
        float startTime = Time.time;
        bool condition1Met = false;
        bool condition2Met = false;
        
        _testObject.ATween("myFloat", path, duration);

        while (Time.time - startTime < duration+0.1f)
        {
            if (Mathf.Abs(_testObject.myFloat -path[0])<1)
            {
                condition1Met = true;
            }
            else if (Mathf.Abs(_testObject.myFloat -path[1])<1)
            {
                condition2Met = true;
            }
            yield return null; // Wait one frame and check again.
        }
        
        Assert.IsTrue(condition1Met, 
            $"Did not pass to the first value");
        Assert.IsTrue(condition2Met, 
            $"Did not pass to the second value");
        
        Assert.AreEqual(path[2], _testObject.myFloat,2, $"Should be at final waypoint. Expected: {path[2]}, Actual: {_testObject.myFloat}.");
    }

    [UnityTest]
    public IEnumerator ATween_Path_CSharpObject_Int_FollowsWaypointsAndCompletes()
    {
        int startValue = 0;
        var path = new[] { 10, 5, 20 };
        float duration = 0.6f;
        _testObject.myInt = startValue;
        float startTime = Time.time;
        bool condition1Met = false;
        bool condition2Met = false;
        
        _testObject.ATween("myInt", path, duration);

        while (Time.time - startTime < duration+0.1f)
        {
            if (Math.Abs(_testObject.myInt -path[0])<1)
            {
                condition1Met = true;
            }
            else if (Math.Abs(_testObject.myInt -path[1])<1)
            {
                condition2Met = true;
            }
            yield return null; // Wait one frame and check again.
        }
        
        Assert.IsTrue(condition1Met, 
            $"Did not pass to the first value");
        Assert.IsTrue(condition2Met, 
            $"Did not pass to the second value");
        
        Assert.AreEqual(path[2], _testObject.myInt, 
            $"Should be at final waypoint. Expected: {path[2]}, Actual: {_testObject.myInt}.");
    }

    [UnityTest]
    public IEnumerator ATween_Path_CSharpObject_Color_FollowsWaypointsAndCompletes()
    {
        Color startValue = Color.white;
        var path = new[] { Color.red, Color.green, Color.blue };
        float duration = 0.6f;
        float segmentDuration = duration / path.Length;
        _testObject.myColor = startValue;
        float startTime = Time.time;
        bool condition1Met = false;
        bool condition2Met = false;

        _testObject.ATween("myColor", path, duration);

        while (Time.time - startTime < duration+0.1f)
        {
            if ((path[0] - _testObject.myColor).maxColorComponent < 0.1f)
            {
                condition1Met = true;
            }
            else if ((path[1] - _testObject.myColor).maxColorComponent < 0.1f)
            {
                condition2Met = true;
            }
            yield return null; // Wait one frame and check again.
        }
        
        Assert.IsTrue(condition1Met, 
            $"Did not pass to the first value");
        Assert.IsTrue(condition2Met, 
            $"Did not pass to the second value");
        
        Assert.IsTrue((path[2] - _testObject.myColor).maxColorComponent < 0.1f,
            $"Should be at final color waypoint. Expected: {path[2]}, Actual: {_testObject.myColor}.");
    }

    [UnityTest]
    public IEnumerator ATween_Path_CSharpObject_Vector2_FollowsWaypointsAndCompletes()
    {
        Vector2 startValue = Vector2.zero;
        var path = new[] { new Vector2(1, 1), new Vector2(-1, 1), new Vector2(-1, -1) };
        float duration = 0.6f;
        float segmentDuration = duration / path.Length;
        _testObject.myVector2 = startValue;

        float startTime = Time.time;
        bool condition1Met = false;
        bool condition2Met = false;
        
        _testObject.ATween("myVector2", path, duration);

        while (Time.time - startTime < duration+0.1f)
        {
            if (Vector2.Distance(path[2], _testObject.myVector2) < 0.1f)
            {
                condition1Met = true;
            }
            else if (Vector2.Distance(path[2], _testObject.myVector2) < 0.1f)
            {
                condition2Met = true;
            }
            yield return null; // Wait one frame and check again.
        }
        
        Assert.IsTrue(condition1Met, 
            $"Did not pass to the first value");
        Assert.IsTrue(condition2Met, 
            $"Did not pass to the second value");
        
        Assert.IsTrue(Vector2.Distance(path[2], _testObject.myVector2) < 0.1f, $"Should be at final waypoint. Expected: {path[2]}, Actual: {_testObject.myVector2}.");
    }
    
    [UnityTest]
    public IEnumerator ATween_Path_CSharpObject_String_PassesThroughWaypoints()
    {
        // Arrange
        string startValue = "start";
        var path = new[] { "apple", "orange", "banana" };
        float duration = 0.9f;
        _testObject.myString = startValue;
        float startTime = Time.time;
        bool passedPoint1 = false;
        bool passedPoint2 = false;

        // Act
        _testObject.ATween("myString", path, duration);

        // Observe
        while (Time.time - startTime < duration + 0.2f) // Give a little extra time for the final frame
        {
            // Check if the string has been exactly equal to the waypoints at any point.
            if (!passedPoint1 && path[0].CompareTo(_testObject.myString) < 1)
            {
                passedPoint1 = true;
            }
            else if (!passedPoint2 && path[1].CompareTo(_testObject.myString) < 1)
            {
                passedPoint2 = true;
            }
            yield return null;
        }
        
        // Assert
        Assert.IsTrue(passedPoint1, $"Did not pass through the first waypoint ('{path[0]}'). Last value: {_testObject.myString}");
        Assert.IsTrue(passedPoint2, $"Did not pass through the second waypoint ('{path[1]}'). Last value: {_testObject.myString}");
        
        int finalComparison = string.Compare(_testObject.myString, path[2]);
        Assert.AreEqual(0, finalComparison, $"Should be at final waypoint. Expected: {path[2]}, Actual: {_testObject.myString}.");
    }
    
    [UnityTest]
    public IEnumerator ATween_Path_Vector3_PassesThroughWaypoints()
    {
        var path = new[] { new Vector3(10, 0, 0), new Vector3(10, 10, 0), new Vector3(0, 10, 0) };
        float duration = 0.6f;
        _testObject.myVector3 = Vector3.zero;
        float startTime = Time.time;
        bool passedPoint1 = false, passedPoint2 = false;

        _testObject.ATween("myVector3", path, duration);

        while (Time.time - startTime < duration + 0.1f)
        {
            if (!passedPoint1 && Vector3.Distance(_testObject.myVector3, path[0]) < 1.5f) { passedPoint1 = true; }
            else if (!passedPoint2 && Vector3.Distance(_testObject.myVector3, path[1]) < 1.5f) { passedPoint2 = true; }
            yield return null;
        }
        
        Assert.IsTrue(passedPoint1, $"Did not pass near the first waypoint. Last value: {_testObject.myVector3}");
        Assert.IsTrue(passedPoint2, $"Did not pass near the second waypoint. Last value: {_testObject.myVector3}");
        Assert.IsTrue(Vector3.Distance(_testObject.myVector3, path[2]) < 0.001f, $"Should be at final waypoint. Expected: {path[2]}, Actual: {_testObject.myVector3}.");
    }

    [UnityTest]
    public IEnumerator ATween_Path_Vector4_PassesThroughWaypoints()
    {
        var path = new[] { new Vector4(1, 2, 3, 4), new Vector4(-4, -3, -2, -1), Vector4.one };
        float duration = 0.6f;
        _testObject.myVector4 = Vector4.zero;
        float startTime = Time.time;
        bool passedPoint1 = false, passedPoint2 = false;

        _testObject.ATween("myVector4", path, duration);

        while (Time.time - startTime < duration + 0.1f)
        {
            if (!passedPoint1 && Vector4.Distance(_testObject.myVector4, path[0]) < 1.5f) { passedPoint1 = true; }
            else if (!passedPoint2 && Vector4.Distance(_testObject.myVector4, path[1]) < 1.5f) { passedPoint2 = true; }
            yield return null;
        }
        
        Assert.IsTrue(passedPoint1, $"Did not pass near the first waypoint. Last value: {_testObject.myVector4}");
        Assert.IsTrue(passedPoint2, $"Did not pass near the second waypoint. Last value: {_testObject.myVector4}");
        Assert.IsTrue(Vector4.Distance(_testObject.myVector4, path[2]) < 0.001f, $"Should be at final waypoint. Expected: {path[2]}, Actual: {_testObject.myVector4}.");
    }

    [UnityTest]
    public IEnumerator ATween_Path_Vector2Int_PassesThroughWaypoints()
    {
        var path = new[] { new Vector2Int(10, 20), new Vector2Int(-10, 20), new Vector2Int(0, 0) };
        float duration = 0.6f;
        _testObject.myVector2Int = Vector2Int.zero;
        float startTime = Time.time;
        bool passedPoint1 = false, passedPoint2 = false;

        _testObject.ATween("myVector2Int", path, duration);

        while (Time.time - startTime < duration + 0.1f)
        {
            if (!passedPoint1 && Vector2.Distance(_testObject.myVector2Int, path[0]) < 1.0f) { passedPoint1 = true; }
            else if (!passedPoint2 && Vector2.Distance(_testObject.myVector2Int, path[1]) < 1.0f) { passedPoint2 = true; }
            yield return null;
        }
        
        Assert.IsTrue(passedPoint1, $"Did not pass near the first waypoint. Last value: {_testObject.myVector2Int}");
        Assert.IsTrue(passedPoint2, $"Did not pass near the second waypoint. Last value: {_testObject.myVector2Int}");
        Assert.AreEqual(path[2], _testObject.myVector2Int, $"Should be at final waypoint. Expected: {path[2]}, Actual: {_testObject.myVector2Int}.");
    }
    
    [UnityTest]
    public IEnumerator ATween_Path_Vector3Int_PassesThroughWaypoints()
    {
        var path = new[] { new Vector3Int(1, 2, 3), new Vector3Int(-1, -2, 3), new Vector3Int(0, 0, 0) };
        float duration = 0.6f;
        _testObject.myVector3Int = Vector3Int.zero;
        float startTime = Time.time;
        bool passedPoint1 = false, passedPoint2 = false;

        _testObject.ATween("myVector3Int", path, duration);

        while (Time.time - startTime < duration + 0.1f)
        {
            if (!passedPoint1 && Vector3.Distance(_testObject.myVector3Int, path[0]) < 1.0f) { passedPoint1 = true; }
            else if (!passedPoint2 && Vector3.Distance(_testObject.myVector3Int, path[1]) < 1.0f) { passedPoint2 = true; }
            yield return null;
        }
        
        Assert.IsTrue(passedPoint1, $"Did not pass near the first waypoint. Last value: {_testObject.myVector3Int}");
        Assert.IsTrue(passedPoint2, $"Did not pass near the second waypoint. Last value: {_testObject.myVector3Int}");
        Assert.AreEqual(path[2], _testObject.myVector3Int, $"Should be at final waypoint. Expected: {path[2]}, Actual: {_testObject.myVector3Int}.");
    }

    [UnityTest]
    public IEnumerator ATween_Path_Bounds_PassesThroughWaypoints()
    {
        var path = new[] { new Bounds(Vector3.one * 5, Vector3.one), new Bounds(Vector3.one * -5, Vector3.one), new Bounds(Vector3.zero, Vector3.one * 2) };
        float duration = 0.6f;
        _testObject.myBounds = new Bounds(Vector3.zero, Vector3.one);
        float startTime = Time.time;
        bool passedPoint1 = false, passedPoint2 = false;

        _testObject.ATween("myBounds", path, duration);

        while (Time.time - startTime < duration + 0.1f)
        {
            if (!passedPoint1 && Vector3.Distance(_testObject.myBounds.center, path[0].center) < 1.5f) { passedPoint1 = true; }
            else if (!passedPoint2 && Vector3.Distance(_testObject.myBounds.center, path[1].center) < 1.5f) { passedPoint2 = true; }
            yield return null;
        }
        
        Assert.IsTrue(passedPoint1, $"Did not pass near the first waypoint. Last value: {_testObject.myBounds}");
        Assert.IsTrue(passedPoint2, $"Did not pass near the second waypoint. Last value: {_testObject.myBounds}");
        Assert.AreEqual(path[2].center, _testObject.myBounds.center, 
            $"Should be at final waypoint. Expected: {path[2].center}, Actual: {_testObject.myBounds.center}.");
        Assert.AreEqual(path[2].extents, _testObject.myBounds.extents, 
            $"Should be at final waypoint. Expected: {path[2].extents}, Actual: {_testObject.myBounds.extents}.");
    }

    [UnityTest]
    public IEnumerator ATween_Path_Rect_PassesThroughWaypoints()
    {
        var path = new[] { new Rect(10, 10, 10, 10), new Rect(-10, -10, 20, 20), new Rect(0, 0, 5, 5) };
        float duration = 0.6f;
        _testObject.myRect = new Rect(0, 0, 0, 0);
        float startTime = Time.time;
        bool passedPoint1 = false, passedPoint2 = false;

        _testObject.ATween("myRect", path, duration);

        while (Time.time - startTime < duration + 0.1f)
        {
            if (!passedPoint1 && Vector2.Distance(_testObject.myRect.center, path[0].center) < 1.0f) { passedPoint1 = true; }
            else if (!passedPoint2 && Vector2.Distance(_testObject.myRect.center, path[1].center) < 1.0f) { passedPoint2 = true; }
            yield return null;
        }
        
        Assert.IsTrue(passedPoint1, $"Did not pass near the first waypoint. Last value: {_testObject.myRect}");
        Assert.IsTrue(passedPoint2, $"Did not pass near the second waypoint. Last value: {_testObject.myRect}");
        Assert.AreEqual(path[2], _testObject.myRect, $"Should be at final waypoint. Expected: {path[2]}, Actual: {_testObject.myRect}.");
    }
    
    [UnityTest]
    public IEnumerator ATween_Path_Quaternion_PassesThroughWaypoints()
    {
        var path = new[] { Quaternion.Euler(90, 0, 0), Quaternion.Euler(90, 90, 0), Quaternion.Euler(0, 0, 0) };
        float duration = 0.6f;
        _testObject.myQuaternion = Quaternion.identity;
        float startTime = Time.time;
        bool passedPoint1 = false, passedPoint2 = false;
        
        _testObject.ATween("myQuaternion", path, duration);

        while (Time.time - startTime < duration + 0.1f)
        {
            if (!passedPoint1 && Quaternion.Angle(_testObject.myQuaternion, path[0]) < 5.0f) { passedPoint1 = true; }
            else if (!passedPoint2 && Quaternion.Angle(_testObject.myQuaternion, path[1]) < 5.0f) { passedPoint2 = true; }
            yield return null;
        }
        
        Assert.IsTrue(passedPoint1, $"Did not pass near the first waypoint. Last value: {_testObject.myQuaternion.eulerAngles}");
        Assert.IsTrue(passedPoint2, $"Did not pass near the second waypoint. Last value: {_testObject.myQuaternion.eulerAngles}");
        Assert.IsTrue(Quaternion.Angle(_testObject.myQuaternion, path[2]) < 0.01f, $"Should be at final waypoint. Expected: {path[2].eulerAngles}, Actual: {_testObject.myQuaternion.eulerAngles}.");
    }

    [UnityTest]
    public IEnumerator ATween_Path_Gradient_PassesThroughWaypoints()
    {
        Gradient startGradient = new Gradient();
        startGradient.SetKeys(new[] { new GradientColorKey(Color.black, 0f) }, new[] { new GradientAlphaKey(1f, 0f) });
        var path = new[]
        {
            new Gradient() { colorKeys = new[] { new GradientColorKey(Color.red, 0f) } },
            new Gradient() { colorKeys = new[] { new GradientColorKey(Color.green, 0f) } },
            new Gradient() { colorKeys = new[] { new GradientColorKey(Color.blue, 0f) } }
        };
        float duration = 0.6f;
        _testObject.myGradient = startGradient;
        float startTime = Time.time;
        bool passedPoint1 = false, passedPoint2 = false;

        _testObject.ATween("myGradient", path, duration);

        while (Time.time - startTime < duration + 0.1f)
        {
            Color currentColor = _testObject.myGradient.Evaluate(0.5f);
            if (!passedPoint1 && (path[0].Evaluate(0.5f) - currentColor).maxColorComponent < 0.1f) { passedPoint1 = true; }
            else if (!passedPoint2 && (path[1].Evaluate(0.5f) - currentColor).maxColorComponent < 0.1f) { passedPoint2 = true; }
            yield return null;
        }
        
        Assert.IsTrue(passedPoint1, $"Did not pass near the first waypoint. Last value: {_testObject.myGradient.Evaluate(0.5f)}");
        Assert.IsTrue(passedPoint2, $"Did not pass near the second waypoint. Last value: {_testObject.myGradient.Evaluate(0.5f)}");
        Color finalColor = _testObject.myGradient.Evaluate(0.5f);
        Color targetColor = path[2].Evaluate(0.5f);
        Assert.IsTrue((finalColor-targetColor).maxColorComponent < 0.01f, $"Should be at final waypoint. Expected: {targetColor}, Actual: {finalColor}.");
    }
}