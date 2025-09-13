using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using AnimaTween;

public class AnimaTweenAudioSourceTests
{
    private AudioSource _testAudioSource;

    [SetUp]
    public void Setup()
    {
        var go = new GameObject("TestAudioSource");
        _testAudioSource = go.AddComponent<AudioSource>();
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(_testAudioSource.gameObject);
    }

    [UnityTest]
    public IEnumerator ATween_AudioSource_Volume_ChangesOverTimeAndCompletes()
    {
        float startValue = 1f;
        float targetValue = 0.1f;
        float duration = 0.2f;
        _testAudioSource.volume = startValue;

        _testAudioSource.ATween("volume", targetValue, duration);

        yield return new WaitForSeconds(duration / 2f);
        float midValue = _testAudioSource.volume;
        Assert.AreNotEqual(startValue, midValue, $"Volume should have changed halfway. Expected not to be {startValue}, but was {midValue}.");
        Assert.Less(midValue, startValue, $"Volume should be decreasing. Mid value {midValue} was not less than start value {startValue}.");

        yield return new WaitForSeconds(duration / 2f+0.1f);

        Assert.AreEqual(targetValue, _testAudioSource.volume, $"Final volume should be the target value. Expected: {targetValue}, Actual: {_testAudioSource.volume}.");
    }

    [UnityTest]
    public IEnumerator ATween_AudioSource_Pitch_ChangesOverTimeAndCompletes()
    {
        float startValue = 1f;
        float targetValue = 2.5f;
        float duration = 1f;
        _testAudioSource.pitch = startValue;

        _testAudioSource.ATween("pitch", targetValue, duration);

        yield return new WaitForSeconds(duration / 2f);
        float midValue = _testAudioSource.pitch;
        Assert.AreNotEqual(startValue, midValue, $"Pitch should have changed halfway. Expected not to be {startValue}, but was {midValue}.");
        Assert.Greater(midValue, startValue, $"Pitch should be increasing. Mid value {midValue} was not greater than start value {startValue}.");

        yield return new WaitForSeconds(duration / 2f+0.1f);

        Assert.AreEqual(targetValue, _testAudioSource.pitch, $"Final pitch should be the target value. Expected: {targetValue}, Actual: {_testAudioSource.pitch}.");
    }
}