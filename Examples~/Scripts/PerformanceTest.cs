using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using AnimaTween;

public class AnimaTweenPerformanceTest : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private int _objectCount = 100;
    [SerializeField] private float _tweenDuration = 2f;
    [SerializeField] private float _measurementTime = 3f;
    [SerializeField] private float _performanceThreshold = 0.90f; // 90%

    private List<Transform> _testObjects = new List<Transform>();
    private bool _isTestRunning = false;

    void Start()
    {
        RunPerformanceTestAsync();
    }

    private async void RunPerformanceTestAsync()
    {
        _isTestRunning = true;
        
        // --- 1. Setup ---
        Debug.Log("--- Starting AnimaTween Performance Test (Awaitable Version) ---");
        Debug.Log($"Creating {_objectCount} objects...");
        CreateTestObjects();
        
        // Give a frame for objects to be fully initialized
        await Awaitable.EndOfFrameAsync();

        // --- 2. Baseline FPS Measurement ---
        Debug.Log($"Measuring baseline FPS for {_measurementTime} seconds...");
        float baselineFps = await MeasureAverageFpsAsync(_measurementTime,_measurementTime);
        Debug.Log($"<b>Baseline Average FPS: {baselineFps:F2}</b>");

        await Awaitable.WaitForSecondsAsync(1f); // Short pause

        // --- 3. Activate Tweens and Measure FPS under load ---
        Debug.Log($"Starting {_objectCount} tweens and measuring FPS for {_measurementTime} seconds...");
        StartAllTweens();
        float tweenFps = await MeasureAverageFpsAsync(_measurementTime,0);
        Debug.Log($"<b>Tweening Average FPS: {tweenFps:F2}</b>");

        // --- 4. Analysis & Report ---
        Debug.Log("--- Test Finished: Analysis ---");
        float performanceRatio = tweenFps / baselineFps;
        string resultMessage = $"Performance is at <b>{(performanceRatio * 100f):F2}%</b> of the baseline.";
        
        if (performanceRatio >= _performanceThreshold)
        {
            Debug.Log($"<color=green><b>PASS:</b> {resultMessage}</color>");
        }
        else
        {
            Debug.Log($"<color=red><b>FAIL:</b> {resultMessage} (Dropped below the {(int)(_performanceThreshold * 100f)}% threshold)</color>");
        }

        // --- 5. Cleanup ---
        DestroyTestObjects();
        _isTestRunning = false;
    }

    private void CreateTestObjects()
    {
        for (int i = 0; i < _objectCount; i++)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.position = new Vector3(
                Random.Range(-20f, 20f),
                Random.Range(-10f, 10f),
                Random.Range(0f, 20f)
            );
            _testObjects.Add(go.transform);
        }
    }

    private void DestroyTestObjects()
    {
        foreach (var obj in _testObjects)
        {
            if(obj != null)
            {
                obj.ACancel();
                Destroy(obj.gameObject);
            }
        }
        _testObjects.Clear();
    }

    private void StartAllTweens()
    {
        foreach (var obj in _testObjects)
        {
            Vector3 targetPosition = obj.position + new Vector3(
                Random.Range(-5f, 5f),
                Random.Range(-5f, 5f),
                0
            );
            obj.ATween("position", targetPosition, _tweenDuration, 
                Easing.InOutSine, playback:Playback.PingPong);
        }
    }

    private async Awaitable<float> MeasureAverageFpsAsync(float duration,float prep=0)
    {
        var fpsReadings = new List<float>();
        float timer = 0f;
        while (timer < prep)
        {
            timer += Time.unscaledDeltaTime;
            // Await the end of the frame, which is the Awaitable equivalent of yield return null.
            await Awaitable.EndOfFrameAsync();
        }
        timer = 0f;
        while (timer < duration)
        {
            fpsReadings.Add(1.0f / Time.unscaledDeltaTime);
            timer += Time.unscaledDeltaTime;
            // Await the end of the frame, which is the Awaitable equivalent of yield return null.
            await Awaitable.EndOfFrameAsync();
        }

        return fpsReadings.Average();
    }
}