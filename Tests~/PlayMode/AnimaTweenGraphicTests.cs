using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using AnimaTween;

public class AnimaTweenGraphicTests
{
    private GameObject _testCanvas;
    private Image _testImage;

    [SetUp]
    public void Setup()
    {
        _testCanvas = new GameObject("TestCanvas", typeof(Canvas));
        var imageGo = new GameObject("TestImage");
        imageGo.transform.SetParent(_testCanvas.transform);
        _testImage = imageGo.AddComponent<Image>();
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(_testCanvas);
    }

    [UnityTest]
    public IEnumerator ATween_Graphic_Color_ChangesOverTimeAndCompletes()
    {
        Color startValue = Color.black;
        Color targetValue = Color.red;
        float duration = 0.2f;
        _testImage.color = startValue;

        _testImage.ATween("color", targetValue, duration);

        yield return new WaitForSeconds(duration / 2f);
        Color midValue = _testImage.color;
        Assert.AreNotEqual(startValue, midValue, $"Color should have changed halfway. Expected not to be {startValue}, but was {midValue}.");
        Assert.Greater(midValue.r, startValue.r, $"Red component should be increasing. Mid value {midValue.r} was not greater than start value {startValue.r}.");

        yield return new WaitForSeconds(duration / 2f +0.1f);
        
        Assert.AreEqual(targetValue, _testImage.color, $"Final color should be the target value. Expected: {targetValue}, Actual: {_testImage.color}.");
    }
}