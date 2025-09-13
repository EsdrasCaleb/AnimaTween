using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using AnimaTween;

public class AnimaTweenTrailRendererTests
{
    private TrailRenderer _testTrailRenderer;

    [SetUp]
    public void Setup()
    {
        var go = new GameObject("TestTrailRenderer");
        _testTrailRenderer = go.AddComponent<TrailRenderer>();
        _testTrailRenderer.time = 0.1f;
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(_testTrailRenderer.gameObject);
    }

    [UnityTest]
    public IEnumerator ATween_TrailRenderer_ColorGradient_ChangesOverTimeAndCompletes()
    {
        Gradient startValue = new Gradient();
        startValue.SetKeys(
            new[] { new GradientColorKey(Color.red, 0f), new GradientColorKey(Color.red, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );

        Gradient targetValue = new Gradient();
        targetValue.SetKeys(
            new[] { new GradientColorKey(Color.yellow, 0f), new GradientColorKey(Color.magenta, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );
        
        float duration = 0.2f;
        _testTrailRenderer.colorGradient = startValue;

        _testTrailRenderer.ATween("colorGradient", targetValue, duration);

        yield return new WaitForSeconds(duration / 2f);
        
        Color midColor = _testTrailRenderer.colorGradient.Evaluate(0.5f);
        Color startMidColor = startValue.Evaluate(0.5f);
        Assert.AreNotEqual(startMidColor, midColor, $"Gradient color should have changed halfway. Expected not to be {startMidColor}, but was {midColor}.");

        yield return new WaitForSeconds(duration / 2f);

        Color finalStartColor = _testTrailRenderer.colorGradient.Evaluate(0f);
        Color finalEndColor = _testTrailRenderer.colorGradient.Evaluate(1f);
        Assert.AreEqual(targetValue.Evaluate(0f), finalStartColor, $"Final gradient start color should match target. Expected: {targetValue.Evaluate(0f)}, Actual: {finalStartColor}.");
        Assert.AreEqual(targetValue.Evaluate(1f), finalEndColor, $"Final gradient end color should match target. Expected: {targetValue.Evaluate(1f)}, Actual: {finalEndColor}.");
    }
}