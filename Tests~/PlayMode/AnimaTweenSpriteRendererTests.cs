using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using AnimaTween;

public class AnimaTweenSpriteRendererTests
{
    private GameObject _testObject;
    private SpriteRenderer _testSpriteRenderer;

    [SetUp]
    public void Setup()
    {
        _testObject = new GameObject("TestSprite");
        _testSpriteRenderer = _testObject.AddComponent<SpriteRenderer>();
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(_testObject);
    }

    [UnityTest]
    public IEnumerator ATween_SpriteRenderer_Color_ChangesOverTimeAndCompletes()
    {
        Color startValue = Color.black;
        Color targetValue = Color.blue;
        float duration = 0.2f;
        _testSpriteRenderer.color = startValue;

        _testSpriteRenderer.ATween("color", targetValue, duration);

        yield return new WaitForSeconds(duration / 2f);
        Color midValue = _testSpriteRenderer.color;
        Assert.AreNotEqual(startValue, midValue, $"Color should have changed halfway. Expected not to be {startValue}, but was {midValue}.");
        Assert.Greater(midValue.b, startValue.b, $"Blue component should be increasing. Mid value {midValue.b} was not greater than start value {startValue.b}.");

        yield return new WaitForSeconds(duration / 2f+0.1f);

        Assert.AreEqual(targetValue, _testSpriteRenderer.color, $"Final color should be the target value. Expected: {targetValue}, Actual: {_testSpriteRenderer.color}.");
    }
}