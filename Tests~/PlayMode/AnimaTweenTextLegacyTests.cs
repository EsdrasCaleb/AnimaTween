using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI; // Required for legacy Text
using AnimaTween;

public class AnimaTweenTextLegacyTests
{
    private GameObject _testCanvas;
    private Text _testText;

    [SetUp]
    public void Setup()
    {
        // A Canvas is required for UI components.
        _testCanvas = new GameObject("TestCanvas", typeof(Canvas));
        
        // Creates the text object and the Text component.
        var textGo = new GameObject("TestLegacyText");
        textGo.transform.SetParent(_testCanvas.transform);
        _testText = textGo.AddComponent<Text>();
        
        // Legacy Text requires a Font to be visible.
        // We use Unity's default built-in "Arial" resource.
        _testText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _testText.fontSize = 14;
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(_testCanvas);
    }

    /// <summary>
    /// Tests if the 'text' property animation (typewriter effect) works correctly.
    /// </summary>
    [UnityTest]
    public IEnumerator ATween_TextLegacy_Text_ChangesOverTimeAndCompletes()
    {
        // Arrange
        string startValue = "";
        string targetValue = "Testing Legacy Text";
        float duration = 0.4f;
        _testText.text = startValue;

        // Act
        _testText.ATween("text", targetValue, duration);

        // Observe
        yield return new WaitForSeconds(duration / 2f);
        string midValue = _testText.text;
        
        Assert.IsNotEmpty(midValue, "Text should not be empty halfway through the tween.");
        Assert.AreNotEqual(targetValue, midValue, "Text should not be complete halfway through the tween.");
        
        yield return new WaitForSeconds(duration / 2f);
        
        // Assert
        Assert.AreEqual(targetValue, _testText.text, "The final text should be equal to the target text.");
    }

    /// <summary>
    /// Tests if the 'color' property animation works correctly.
    /// </summary>
    [UnityTest]
    public IEnumerator ATween_TextLegacy_Color_ChangesTowardsTarget()
    {
        // Arrange
        Color startValue = Color.black;
        Color targetValue = Color.green;
        float duration = 0.4f;
        _testText.color = startValue;

        // Act
        _testText.ATween("color", targetValue, duration);

        // Observe
        yield return new WaitForSeconds(duration / 2f);
        Color midValue = _testText.color;

        Assert.AreNotEqual(startValue, midValue, "Color should have changed halfway through the tween.");
        // The green component should be increasing.
        Assert.Greater(midValue.g, startValue.g, "The green component of the color should be increasing.");
        Assert.Less(midValue.g, targetValue.g, "The green component should be less than the final value halfway through the tween.");
        
        yield return new WaitForSeconds(duration / 2f +0.1f);
        
        // Assert
        Assert.AreEqual(targetValue, _testText.color, "The final color should be equal to the target color.");
    }
}