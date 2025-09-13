using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using AnimaTween;

public class AnimaTweenLightTests
{
    private Light _testLight;

    [SetUp]
    public void Setup()
    {
        var go = new GameObject("TestLight");
        _testLight = go.AddComponent<Light>();
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(_testLight.gameObject);
    }

    [UnityTest]
    public IEnumerator ATween_Light_Color_ChangesOverTimeAndCompletes()
    {
        Color startValue = Color.black;
        Color targetValue = Color.red;
        float duration = 0.2f;
        _testLight.color = startValue;

        _testLight.ATween("color", targetValue, duration);

        yield return new WaitForSeconds(duration / 2f);
        Color midValue = _testLight.color;
        Assert.AreNotEqual(startValue, midValue, $"Color should have changed halfway. Expected not to be {startValue}, but was {midValue}.");
        Assert.Greater(midValue.r, startValue.r, $"Red component should be increasing. Mid value {midValue.r} was not greater than start value {startValue.r}.");

        yield return new WaitForSeconds(duration / 2f +0.1f);

        Assert.AreEqual(targetValue, _testLight.color, $"Final color should be the target value. Expected: {targetValue}, Actual: {_testLight.color}.");
    }
    
    [UnityTest]
    public IEnumerator ATween_Light_Intensity_ChangesOverTimeAndCompletes()
    {
        float startValue = 0f;
        float targetValue = 5f;
        float duration = 0.2f;
        _testLight.intensity = startValue;

        _testLight.ATween("intensity", targetValue, duration);

        yield return new WaitForSeconds(duration / 2f);
        float midValue = _testLight.intensity;
        Assert.AreNotEqual(startValue, midValue, $"Intensity should have changed halfway. Expected not to be {startValue}, but was {midValue}.");
        Assert.Greater(midValue, startValue, $"Intensity should be increasing. Mid value {midValue} was not greater than start value {startValue}.");

        yield return new WaitForSeconds(duration / 2f+0.1f);
        
        Assert.AreEqual(targetValue, _testLight.intensity, $"Final intensity should be the target value. Expected: {targetValue}, Actual: {_testLight.intensity}.");
    }

    [UnityTest]
    public IEnumerator ATween_Light_Range_ChangesOverTimeAndCompletes()
    {
        float startValue = 10f;
        float targetValue = 50f;
        float duration = 0.2f;
        _testLight.range = startValue;

        _testLight.ATween("range", targetValue, duration);

        yield return new WaitForSeconds(duration / 2f);
        float midValue = _testLight.range;
        Assert.AreNotEqual(startValue, midValue, $"Range should have changed halfway. Expected not to be {startValue}, but was {midValue}.");
        Assert.Greater(midValue, startValue, $"Range should be increasing. Mid value {midValue} was not greater than start value {startValue}.");

        yield return new WaitForSeconds(duration / 2f+0.1f);

        Assert.AreEqual(targetValue, _testLight.range, $"Final range should be the target value. Expected: {targetValue}, Actual: {_testLight.range}.");
    }
}