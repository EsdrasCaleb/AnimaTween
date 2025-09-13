using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using AnimaTween;

public class AnimaTweenCameraTests
{
    private Camera _testCamera;

    [SetUp]
    public void Setup()
    {
        var go = new GameObject("TestCamera");
        _testCamera = go.AddComponent<Camera>();
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(_testCamera.gameObject);
    }

    [UnityTest]
    public IEnumerator ATween_Camera_FieldOfView_ChangesOverTimeAndCompletes()
    {
        float startValue = 60f;
        float targetValue = 90f;
        float duration = 0.2f;
        _testCamera.fieldOfView = startValue;

        _testCamera.ATween("fieldOfView", targetValue, duration);

        yield return new WaitForSeconds(duration / 2f);
        float midValue = _testCamera.fieldOfView;
        Assert.AreNotEqual(startValue, midValue, $"Field of view should have changed halfway. Expected not to be {startValue}, but was {midValue}.");
        Assert.Greater(midValue, startValue, $"Field of view should be increasing. Mid value {midValue} was not greater than start value {startValue}.");

        yield return new WaitForSeconds(duration / 2f + 0.1f);

        Assert.Less(Mathf.Abs(targetValue- _testCamera.fieldOfView),0.01, $"Final field of view should be the target value. Expected: {targetValue}, Actual: {_testCamera.fieldOfView}.");
    }

    [UnityTest]
    public IEnumerator ATween_Camera_OrthographicSize_ChangesOverTimeAndCompletes()
    {
        _testCamera.orthographic = true;
        float startValue = 5f;
        float targetValue = 1f;
        float duration = 0.2f;
        _testCamera.orthographicSize = startValue;

        _testCamera.ATween("orthographicSize", targetValue, duration);

        yield return new WaitForSeconds(duration / 2f);
        float midValue = _testCamera.orthographicSize;
        Assert.AreNotEqual(startValue, midValue, $"Orthographic size should have changed halfway. Expected not to be {startValue}, but was {midValue}.");
        Assert.Less(midValue, startValue, $"Orthographic size should be decreasing. Mid value {midValue} was not less than start value {startValue}.");
        
        yield return new WaitForSeconds(duration / 2f +0.1f);

        Assert.AreEqual(targetValue, _testCamera.orthographicSize, $"Final orthographic size should be the target value. Expected: {targetValue}, Actual: {_testCamera.orthographicSize}.");
    }
    
    [UnityTest]
    public IEnumerator ATween_Camera_BackgroundColor_ChangesOverTimeAndCompletes()
    {
        Color startValue = Color.black;
        Color targetValue = Color.yellow;
        float duration = 0.2f;
        _testCamera.backgroundColor = startValue;

        _testCamera.ATween("backgroundColor", targetValue, duration);

        yield return new WaitForSeconds(duration / 2f);
        Color midValue = _testCamera.backgroundColor;
        Assert.AreNotEqual(startValue, midValue, $"Background color should have changed halfway. Expected not to be {startValue}, but was {midValue}.");
        Assert.Greater(midValue.g, startValue.g, $"Green component should be increasing. Mid value {midValue.g} was not greater than start value {startValue.g}.");

        yield return new WaitForSeconds(duration / 2f+0.1f);
        
        Assert.AreEqual(targetValue, _testCamera.backgroundColor, $"Final background color should be the target value. Expected: {targetValue}, Actual: {_testCamera.backgroundColor}.");
    }
}
