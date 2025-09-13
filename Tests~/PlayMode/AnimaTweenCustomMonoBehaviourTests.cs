using System;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using AnimaTween;
using Object = UnityEngine.Object;


public class AnimaTweenCustomMonoBehaviourTests
{
    public class AnimaTweenTestComponent : MonoBehaviour
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
    private AnimaTweenTestComponent _testComponent;

    [SetUp]
    public void Setup()
    {
        var go = new GameObject("TestComponentObject");
        _testComponent = go.AddComponent<AnimaTweenTestComponent>();
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(_testComponent.gameObject);
    }

    [UnityTest]
    public IEnumerator ATween_Custom_Float_ChangesOverTimeAndCompletes()
    {
        float startValue = 0f;
        float targetValue = 100f;
        float duration = 1f;
        _testComponent.myFloat = startValue;

        _testComponent.ATween("myFloat", targetValue, duration);

        yield return new WaitForSeconds(duration / 2f);
        float midValue = _testComponent.myFloat;
        Assert.IsTrue(midValue > startValue && midValue < targetValue, $"Value should be between start and end halfway. Expected between {startValue} to {targetValue}, but was {midValue}.");

        yield return new WaitForSeconds(duration / 2f+0.1f);
        Assert.AreEqual(targetValue, _testComponent.myFloat, $"Final value should be target. Expected: {targetValue}, Actual: {_testComponent.myFloat}.");
    }

    [UnityTest]
    public IEnumerator ATween_Custom_Double_ChangesOverTimeAndCompletes()
    {
        double startValue = 0.0;
        double targetValue = 200.0;
        float duration = 1f;
        _testComponent.myDouble = startValue;

        _testComponent.ATween("myDouble", targetValue, duration);

        yield return new WaitForSeconds(duration / 2f);
        double midValue = _testComponent.myDouble;
        Assert.IsTrue(midValue > startValue && midValue < targetValue, $"Value should be between start and end halfway. Expected between {startValue}-{targetValue}, but was {midValue}.");

        yield return new WaitForSeconds(duration / 2f+0.1f);
        Assert.AreEqual(targetValue, _testComponent.myDouble, $"Final value should be target. Expected: {targetValue}, Actual: {_testComponent.myDouble}.");
    }

    [UnityTest]
    public IEnumerator ATween_Custom_Int_ChangesOverTimeAndCompletes()
    {
        int startValue = -50;
        int targetValue = 50;
        float duration = 1f;
        _testComponent.myInt = startValue;

        _testComponent.ATween("myInt", targetValue, duration);

        yield return new WaitForSeconds(duration / 2f);
        int midValue = _testComponent.myInt;
        Assert.IsTrue(midValue > startValue && midValue < targetValue, $"Value should be between start and end halfway. Expected between {startValue}-{targetValue}, but was {midValue}.");
        
        yield return new WaitForSeconds(duration / 2f +1f);
        Assert.AreEqual(targetValue, _testComponent.myInt, $"Final value should be target. Expected: {targetValue}, Actual: {_testComponent.myInt}.");
    }
    
    [UnityTest]
    public IEnumerator ATween_Custom_Vector3_ChangesTowardsTarget()
    {
        Vector3 startValue = Vector3.zero;
        Vector3 targetValue = new Vector3(10, 20, 30);
        float duration = 1f;
        _testComponent.myVector3 = startValue;
        float initialDistance = Vector3.Distance(startValue, targetValue);

        _testComponent.ATween("myVector3", targetValue, duration);
        
        yield return new WaitForSeconds(duration / 2f);
        float midDistance = Vector3.Distance(_testComponent.myVector3, targetValue);
        Assert.Less(midDistance, initialDistance, $"Object should be closer to target halfway. Mid distance {midDistance} was not less than initial {initialDistance}.");

        yield return new WaitForSeconds(duration / 2f+0.1f);
        Assert.IsTrue(Vector3.Distance(targetValue, _testComponent.myVector3) < 0.001f, $"Final value should be target. Expected: {targetValue}, Actual: {_testComponent.myVector3}.");
    }
    
    [UnityTest]
    public IEnumerator ATween_Custom_Color_ChangesTowardsTarget()
    {
        Color startValue = Color.black;
        Color targetValue = Color.white;
        float duration = 1f;
        _testComponent.myColor = startValue;

        _testComponent.ATween("myColor", targetValue, duration);

        yield return new WaitForSeconds(duration / 2f);
        Color midValue = _testComponent.myColor;
        Assert.Greater(midValue.r, startValue.r, $"Color channel should be increasing. Mid value {midValue.r} was not greater than start {startValue.r}.");

        yield return new WaitForSeconds(duration / 2f+0.1f);
        Assert.AreEqual(targetValue, _testComponent.myColor, $"Final value should be target. Expected: {targetValue}, Actual: {_testComponent.myColor}.");
    }

    [UnityTest]
    public IEnumerator ATween_Custom_Quaternion_ChangesTowardsTarget()
    {
        Quaternion startValue = Quaternion.identity;
        Quaternion targetValue = Quaternion.Euler(90, 45, 0);
        float duration = 1f;
        _testComponent.myQuaternion = startValue;
        float initialAngle = Quaternion.Angle(startValue, targetValue);
        
        _testComponent.ATween("myQuaternion", targetValue, duration);

        yield return new WaitForSeconds(duration / 2f);
        float midAngle = Quaternion.Angle(_testComponent.myQuaternion, targetValue);
        Assert.Less(midAngle, initialAngle, $"Rotation should be closer to target halfway. Mid angle {midAngle} was not less than initial {initialAngle}.");
        
        yield return new WaitForSeconds(duration / 2f+0.1f);
        Assert.IsTrue(Quaternion.Angle(targetValue, _testComponent.myQuaternion) < 0.001f, $"Final value should be target. Expected: {targetValue.eulerAngles}, Actual: {_testComponent.myQuaternion.eulerAngles}.");
    }

    [UnityTest]
    public IEnumerator ATween_Custom_Rect_ChangesTowardsTarget()
    {
        Rect startValue = new Rect(0, 0, 50, 50);
        Rect targetValue = new Rect(100, 100, 100, 100);
        float duration = 1f;
        _testComponent.myRect = startValue;
        float initialDistance = Vector2.Distance(startValue.center, targetValue.center);
        
        _testComponent.ATween("myRect", targetValue, duration);

        yield return new WaitForSeconds(duration / 2f);
        float midDistance = Vector2.Distance(_testComponent.myRect.center, targetValue.center);
        Assert.Less(midDistance, initialDistance, $"Rect should be closer to target halfway. Mid distance {midDistance} was not less than initial {initialDistance}.");

        yield return new WaitForSeconds(duration / 2f+0.1f);
        Assert.AreEqual(targetValue, _testComponent.myRect, $"Final value should be target. Expected: {targetValue}, Actual: {_testComponent.myRect}.");
    }

    [UnityTest]
    public IEnumerator ATween_Custom_Bounds_ChangesTowardsTarget()
    {
        Bounds startValue = new Bounds(Vector3.zero, Vector3.one);
        Bounds targetValue = new Bounds(new Vector3(10,10,10), new Vector3(5,5,5));
        float duration = 0.2f;
        _testComponent.myBounds = startValue;
        float initialDistance = Vector3.Distance(startValue.center, targetValue.center);

        _testComponent.ATween("myBounds", targetValue, duration);

        yield return new WaitForSeconds(duration / 2f);
        float midDistance = Vector3.Distance(_testComponent.myBounds.center, targetValue.center);
        Assert.Less(midDistance, initialDistance, $"Bounds should be closer to target halfway. Mid distance {midDistance} was not less than initial {initialDistance}.");
        
        yield return new WaitForSeconds(duration / 2f+0.1f);
        Assert.AreEqual(targetValue, _testComponent.myBounds, $"Final value should be target. Expected: {targetValue}, Actual: {_testComponent.myBounds}.");
    }
    
    [UnityTest]
    public IEnumerator ATween_Path_MonoBehaviour_Float_FollowsWaypointsAndCompletes()
    {
        float startValue = 0f;
        var path = new[] { 10f, -5f, 5f };
        float duration = 1.2f;

        _testComponent.myFloat = startValue;
        float startTime = Time.time;
        bool condition1Met = false;
        bool condition2Met = false;
        
        _testComponent.ATween("myFloat", path, duration);

        while (Time.time - startTime < duration+0.1f)
        {
            if (Mathf.Abs(_testComponent.myFloat -path[0])<1)
            {
                condition1Met = true;
            }
            else if (Mathf.Abs(_testComponent.myFloat -path[1])<1)
            {
                condition2Met = true;
            }
            yield return null; // Wait one frame and check again.
        }
        
        Assert.IsTrue(condition1Met, 
            $"Did not pass to the first value");
        Assert.IsTrue(condition2Met, 
            $"Did not pass to the second value");
        
        Assert.AreEqual(path[2], _testComponent.myFloat,2, 
            $"Should be at final waypoint. Expected: {path[2]}, Actual: {_testComponent.myFloat}.");
    }

    [UnityTest]
    public IEnumerator ATween_Path_MonoBehaviour_Int_FollowsWaypointsAndCompletes()
    {
        int startValue = 0;
        var path = new[] { 10, 5, 20 };
        float duration = 0.6f;
        _testComponent.myInt = startValue;
        float startTime = Time.time;
        bool condition1Met = false;
        bool condition2Met = false;
        
        _testComponent.ATween("myInt", path, duration);

        while (Time.time - startTime < duration+0.1f)
        {
            if (Math.Abs(_testComponent.myInt -path[0])<1)
            {
                condition1Met = true;
            }
            else if (Math.Abs(_testComponent.myInt -path[1])<1)
            {
                condition2Met = true;
            }
            yield return null; // Wait one frame and check again.
        }
        
        Assert.IsTrue(condition1Met, 
            $"Did not pass to the first value");
        Assert.IsTrue(condition2Met, 
            $"Did not pass to the second value");
        
        Assert.AreEqual(path[2], _testComponent.myInt, 
            $"Should be at final waypoint. Expected: {path[2]}, Actual: {_testComponent.myInt}.");
    }

    [UnityTest]
    public IEnumerator ATween_Path_MonoBehaviour_Color_FollowsWaypointsAndCompletes()
    {
        Color startValue = Color.white;
        var path = new[] { Color.red, Color.green, Color.blue };
        float duration = 0.6f;
        float segmentDuration = duration / path.Length;
        _testComponent.myColor = startValue;
        float startTime = Time.time;
        bool condition1Met = false;
        bool condition2Met = false;

        _testComponent.ATween("myColor", path, duration);

        while (Time.time - startTime < duration+0.1f)
        {
            if ((path[0] - _testComponent.myColor).maxColorComponent < 0.1f)
            {
                condition1Met = true;
            }
            else if ((path[1] - _testComponent.myColor).maxColorComponent < 0.1f)
            {
                condition2Met = true;
            }
            yield return null; // Wait one frame and check again.
        }
        
        Assert.IsTrue(condition1Met, 
            $"Did not pass to the first value");
        Assert.IsTrue(condition2Met, 
            $"Did not pass to the second value");
        
        Assert.IsTrue((path[2] - _testComponent.myColor).maxColorComponent < 0.1f,
            $"Should be at final color waypoint. Expected: {path[2]}, Actual: {_testComponent.myColor}.");
    }

    [UnityTest]
    public IEnumerator ATween_Path_MonoBehaviour_Vector2_FollowsWaypointsAndCompletes()
    {
        Vector2 startValue = Vector2.zero;
        var path = new[] { new Vector2(10, 10), new Vector2(-10, 10), new Vector2(1, 1) };
        float duration = 0.6f;
        float segmentDuration = duration / path.Length;
        _testComponent.myVector2 = startValue;

        float startTime = Time.time;
        bool condition1Met = false;
        bool condition2Met = false;
        
        _testComponent.ATween("myVector2", path, duration);

        while (Time.time - startTime < duration+0.1f)
        {
            if (Vector2.Distance(path[0], _testComponent.myVector2) < 1f)
            {
                condition1Met = true;
            }
            else if (Vector2.Distance(path[1], _testComponent.myVector2) < 1f)
            {
                condition2Met = true;
            }
            yield return null; // Wait one frame and check again.
        }
        
        Assert.IsTrue(condition1Met, 
            $"Did not pass to the first value");
        Assert.IsTrue(condition2Met, 
            $"Did not pass to the second value");
        
        Assert.IsTrue(Vector2.Distance(path[2], _testComponent.myVector2) < 0.1f, 
            $"Should be at final waypoint. Expected: {path[2]}, Actual: {_testComponent.myVector2}.");
    }
    
    [UnityTest]
    public IEnumerator ATween_Path_MonoBehaviour_String_FollowsWaypointsAndCompletes()
    {
        // Arrange
        string startValue = "start";
        var path = new[] { "apple", "orange", "banana" };
        float duration = 0.9f;
        _testComponent.myString = startValue;
        float startTime = Time.time;
        bool passedPoint1 = false;
        bool passedPoint2 = false;

        // Act
        _testComponent.ATween("myString", path, duration);

        // Observe
        while (Time.time - startTime < duration + 0.2f) // Give a little extra time for the final frame
        {
            // Check if the string has been exactly equal to the waypoints at any point.
            if (!passedPoint1 && path[0].CompareTo(_testComponent.myString) < 1)
            {
                passedPoint1 = true;
            }
            else if (!passedPoint2 && path[1].CompareTo(_testComponent.myString) < 1)
            {
                passedPoint2 = true;
            }
            yield return null;
        }
        
        // Assert
        Assert.IsTrue(passedPoint1,
            $"Did not pass through the first waypoint ('{path[0]}'). Last value: {_testComponent.myString}");
        Assert.IsTrue(passedPoint2, 
            $"Did not pass through the second waypoint ('{path[1]}'). Last value: {_testComponent.myString}");
        
        int finalComparison = string.Compare(_testComponent.myString, path[2]);
        Assert.AreEqual(0, finalComparison, 
            $"Should be at final waypoint. Expected: {path[2]}, Actual: {_testComponent.myString}.");
    }
    
    
}