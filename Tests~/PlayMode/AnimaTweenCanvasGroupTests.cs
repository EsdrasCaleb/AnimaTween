using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using AnimaTween;

public class AnimaTweenCanvasGroupTests
{
    private CanvasGroup _testCanvasGroup;

    [SetUp]
    public void Setup()
    {
        var go = new GameObject("TestCanvasGroup");
        _testCanvasGroup = go.AddComponent<CanvasGroup>();
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(_testCanvasGroup.gameObject);
    }

    [UnityTest]
    public IEnumerator ATween_CanvasGroup_Alpha_ChangesOverTimeAndCompletes()
    {
        float startValue = 1f;
        float targetValue = 0f;
        float duration = 0.2f;
        _testCanvasGroup.alpha = startValue;

        _testCanvasGroup.ATween("alpha", targetValue, duration);

        yield return new WaitForSeconds(duration / 2f);
        float midValue = _testCanvasGroup.alpha;
        Assert.AreNotEqual(startValue, midValue, $"Alpha should have changed halfway. Expected not to be {startValue}, but was {midValue}.");
        Assert.Less(midValue, startValue, $"Alpha should be decreasing. Mid value {midValue} was not less than start value {startValue}.");

        yield return new WaitForSeconds(duration / 2f);

        Assert.AreEqual(targetValue, _testCanvasGroup.alpha, $"Final alpha should be the target value. Expected: {targetValue}, Actual: {_testCanvasGroup.alpha}.");
    }
}