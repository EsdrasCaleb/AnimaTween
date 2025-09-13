using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using AnimaTween;

public class AnimaTweenControlAndUtilityTests
{
    private GameObject _testObject;

    [SetUp]
    public void Setup()
    {
        _testObject = new GameObject("TestObject");
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(_testObject);
    }

    [UnityTest]
    public IEnumerator ATimeout_Callback_IsInvokedAfterDelay()
    {
        bool isCallbackCalled = false;
        float delay = 0.2f;

        this.ATimeout(delay, () => { isCallbackCalled = true; });

        yield return new WaitForSeconds(delay / 2f);
        Assert.IsFalse(isCallbackCalled, $"Callback should not be called halfway. Expected: false, Actual: {isCallbackCalled}.");

        yield return new WaitForSeconds(delay / 2f + 0.1f);
        Assert.IsTrue(isCallbackCalled, $"Callback should be called after the full delay. Expected: true, Actual: {isCallbackCalled}.");
    }

    [UnityTest]
    public IEnumerator AShake_TransformPosition_MovesObjectAndReturnsToStart()
    {
        Vector3 startPosition = new Vector3(10, 20, 30);
        float duration = 0.2f;
        float intensity = 2f;
        _testObject.transform.position = startPosition;

        _testObject.transform.AShake("position", duration, intensity);

        yield return new WaitForSeconds(duration / 2f);
        Assert.AreNotEqual(startPosition, _testObject.transform.position, $"Position should have changed during the shake. Start: {startPosition}, Mid: {_testObject.transform.position}.");

        yield return new WaitForSeconds(duration / 2f + 0.1f);
        Assert.IsTrue(Vector3.Distance(startPosition, _testObject.transform.position) < 0.001f, $"Position should return to start after shake. Expected: {startPosition}, Actual: {_testObject.transform.position}.");
    }

    [UnityTest]
    public IEnumerator APunch_TransformScale_OvershootsAndReturnsToStart()
    {
        Vector3 startScale = Vector3.one;
        float duration = 1f;
        float intensity = 3f;
        _testObject.transform.localScale = startScale;

        _testObject.transform.APunch("localScale", Vector3.one*intensity, duration);
        
        yield return null;
        Vector3 scaleAfterFirstFrame = _testObject.transform.localScale;
        float distanceAfterFirstFrame = Vector3.Distance(scaleAfterFirstFrame, startScale);

        Assert.AreNotEqual(startScale, scaleAfterFirstFrame, "A escala local deve mudar após o primeiro frame.");
        Assert.Greater(distanceAfterFirstFrame, 0, "A escala deve ter mudado na direção do valor alvo.");
        yield return new WaitForSeconds(duration);
        
        Assert.IsTrue(Vector3.Distance(startScale, _testObject.transform.localScale) < 0.001f, $"Scale should return to start after punch. Expected: {startScale}, Actual: {_testObject.transform.localScale}.");
    }

    [UnityTest]
    public IEnumerator AComplete_JumpsToFinalValueAndInvokesCallback()
    {
        bool isCallbackCalled = false;
        Vector3 targetPosition = new Vector3(100, 100, 100);
        
        _testObject.transform.ATween("position", targetPosition, 5f, onComplete: () => { isCallbackCalled = true; });
        _testObject.transform.AComplete("position");

        yield return null;

        Assert.IsTrue(Vector3.Distance(targetPosition, _testObject.transform.position) < 0.001f, $"Position should jump to target on AComplete. Expected: {targetPosition}, Actual: {_testObject.transform.position}.");
        Assert.IsTrue(isCallbackCalled, $"onComplete callback should be invoked by AComplete. Expected: true, Actual: {isCallbackCalled}.");
    }
    
    [UnityTest]
    public IEnumerator AStop_StopsAtIntermediateValueAndDoesNotInvokeCallback()
    {
        bool isCallbackCalled = false;
        Vector3 startPosition = Vector3.zero;
        Vector3 targetPosition = new Vector3(10, 0, 0);
        float duration = 0.4f;
        
        _testObject.transform.ATween("position", targetPosition, duration, onComplete: () => { isCallbackCalled = true; });

        yield return new WaitForSeconds(duration / 2f);
        _testObject.transform.AStop("position");
        Vector3 positionAfterStop = _testObject.transform.position;
        
        yield return new WaitForSeconds(duration);
        
        Assert.IsTrue(Vector3.Distance(positionAfterStop, _testObject.transform.position) < 0.001f, $"Position should remain stopped. Expected: {positionAfterStop}, Actual: {_testObject.transform.position}.");
        Assert.IsFalse(isCallbackCalled, $"onComplete callback should not be invoked by AStop. Expected: false, Actual: {isCallbackCalled}.");
    }

    [UnityTest]
    public IEnumerator ACancel_ReturnsToStartValueAndDoesNotInvokeCallback()
    {
        bool isCallbackCalled = false;
        Vector3 startPosition = new Vector3(5, 5, 5);
        Vector3 targetPosition = new Vector3(10, 10, 10);
        float duration = 0.4f;
        _testObject.transform.position = startPosition;

        _testObject.transform.ATween("position", targetPosition, duration, onComplete: () => { isCallbackCalled = true; });

        yield return new WaitForSeconds(duration / 2f);
        _testObject.transform.ACancel("position");

        yield return null;

        Assert.IsTrue(Vector3.Distance(startPosition, _testObject.transform.position) < 0.001f, $"Position should return to start on ACancel. Expected: {startPosition}, Actual: {_testObject.transform.position}.");
        Assert.IsFalse(isCallbackCalled, $"onComplete callback should not be invoked by ACancel. Expected: false, Actual: {isCallbackCalled}.");
    }

    [UnityTest]
    public IEnumerator ACompleteTimer_StopsTimerAndInvokesCallback()
    {
        bool isCallbackCalled = false;
        
        var timer = this.ATimeout(5f, () => { isCallbackCalled = true; });
        this.ACompleteTimer(timer);

        yield return null;
        
        Assert.IsTrue(isCallbackCalled, $"onComplete callback should be invoked by ACompleteTimer. Expected: true, Actual: {isCallbackCalled}.");
    }
}