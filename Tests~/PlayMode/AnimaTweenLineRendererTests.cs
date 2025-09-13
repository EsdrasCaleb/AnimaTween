using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using AnimaTween;

public class AnimaTweenLineRendererTests
{
    private LineRenderer _testLineRenderer;

    [SetUp]
    public void Setup()
    {
        var go = new GameObject("TestLineRenderer");
        _testLineRenderer = go.AddComponent<LineRenderer>();
        _testLineRenderer.positionCount = 2;
        _testLineRenderer.SetPosition(0, Vector3.zero);
        _testLineRenderer.SetPosition(1, Vector3.right);
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(_testLineRenderer.gameObject);
    }

    [UnityTest]
    public IEnumerator ATween_LineRenderer_ColorGradient_ChangesOverTimeAndCompletes()
    {
        Gradient startValue = new Gradient();
        startValue.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.black, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );

        Gradient targetValue = new Gradient();
        targetValue.SetKeys(
            new[] { new GradientColorKey(Color.blue, 0f), new GradientColorKey(Color.green, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );
        
        float duration = 0.2f;
        _testLineRenderer.colorGradient = startValue;

        _testLineRenderer.ATween("colorGradient", targetValue, duration);

        yield return new WaitForSeconds(duration / 2f);
        
        Color midColor = _testLineRenderer.colorGradient.Evaluate(0.5f);
        Color startMidColor = startValue.Evaluate(0.5f);
        Assert.AreNotEqual(startMidColor, midColor, $"Gradient color should have changed halfway. Expected not to be {startMidColor}, but was {midColor}.");

        yield return new WaitForSeconds(duration / 2f+0.1f);

        Color finalStartColor = _testLineRenderer.colorGradient.Evaluate(0f);
        Color finalEndColor = _testLineRenderer.colorGradient.Evaluate(1f);
        Assert.AreEqual(targetValue.Evaluate(0f), finalStartColor, $"Final gradient start color should match target. Expected: {targetValue.Evaluate(0f)}, Actual: {finalStartColor}.");
        Assert.AreEqual(targetValue.Evaluate(1f), finalEndColor, $"Final gradient end color should match target. Expected: {targetValue.Evaluate(1f)}, Actual: {finalEndColor}.");
    }
}