using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using AnimaTween;

public class AnimaTweenRectTransformTests
{
    private RectTransform _testRectTransform;
    private GameObject _canvasObject;
    private GameObject _testObject;

    [SetUp]
    public void Setup()
    {
        // Um Canvas é necessário para que os RectTransforms se comportem corretamente.
        _canvasObject = new GameObject("TestCanvas", typeof(Canvas));
        _testObject = new GameObject("TestObject");
        _testObject.transform.SetParent(_canvasObject.transform);
        _testRectTransform = _testObject.AddComponent<RectTransform>();
    }

    [TearDown]
    public void Teardown()
    {
        // Destrói na ordem inversa da criação para evitar avisos.
        Object.DestroyImmediate(_testObject);
        Object.DestroyImmediate(_canvasObject);
    }

    /// <summary>
    /// Testa se a 'anchoredPosition' de um RectTransform se move na direção correta.
    /// </summary>
    [UnityTest]
    public IEnumerator ATween_RectTransform_AnchoredPosition_MovesTowardsTarget()
    {
        // Arrange
        Vector2 startPosition = new Vector2(0, 0);
        Vector2 targetPosition = new Vector2(100, -50);
        float duration = 0.3f;
        _testRectTransform.anchoredPosition = startPosition;
        float initialDistance = Vector2.Distance(startPosition, targetPosition);

        // Act
        _testRectTransform.ATween("anchoredPosition", targetPosition, duration);

        // Observe
        yield return null;
        Vector2 positionAfterFirstFrame = _testRectTransform.anchoredPosition;
        float distanceAfterFirstFrame = Vector2.Distance(positionAfterFirstFrame, targetPosition);
        
        Assert.AreNotEqual(startPosition, positionAfterFirstFrame, "A posição ancorada deve mudar após o primeiro frame.");
        Assert.Less(distanceAfterFirstFrame, initialDistance, "O objeto deve ter se movido para mais perto do alvo.");

        yield return new WaitForSeconds(duration);
        
        // Assert
        Assert.IsTrue(Vector2.Distance(targetPosition, _testRectTransform.anchoredPosition) < 0.001f, "A posição ancorada final deve ser muito próxima da posição alvo.");
    }

    /// <summary>
    /// Testa se o 'sizeDelta' de um RectTransform muda na direção correta.
    /// </summary>
    [UnityTest]
    public IEnumerator ATween_RectTransform_SizeDelta_ChangesTowardsTarget()
    {
        // Arrange
        Vector2 startSize = new Vector2(100, 100);
        Vector2 targetSize = new Vector2(200, 50);
        float duration = 0.3f;
        _testRectTransform.sizeDelta = startSize;
        float initialDistance = Vector2.Distance(startSize, targetSize);

        // Act
        _testRectTransform.ATween("sizeDelta", targetSize, duration);

        // Observe
        yield return null; 
        Vector2 sizeAfterFirstFrame = _testRectTransform.sizeDelta;
        float distanceAfterFirstFrame = Vector2.Distance(sizeAfterFirstFrame, targetSize);

        Assert.AreNotEqual(startSize, sizeAfterFirstFrame, "O sizeDelta deve mudar após o primeiro frame.");
        Assert.Less(distanceAfterFirstFrame, initialDistance, "O sizeDelta deve ter mudado na direção do valor alvo.");
        
        yield return new WaitForSeconds(duration);
        
        // Assert
        Assert.IsTrue(Vector2.Distance(targetSize, _testRectTransform.sizeDelta) < 0.001f, "O sizeDelta final deve ser muito próximo do valor alvo.");
    }

    /// <summary>
    /// Testa se o 'localScale' de um RectTransform muda na direção correta.
    /// </summary>
    [UnityTest]
    public IEnumerator ATween_RectTransform_LocalScale_ChangesTowardsTarget()
    {
        // Arrange
        Vector3 startScale = Vector3.one;
        Vector3 targetScale = new Vector3(2f, 0.5f, 1f);
        float duration = 0.3f;
        _testRectTransform.localScale = startScale;
        float initialDistance = Vector3.Distance(startScale, targetScale);

        // Act
        _testRectTransform.ATween("localScale", targetScale, duration);

        // Observe
        yield return null;
        Vector3 scaleAfterFirstFrame = _testRectTransform.localScale;
        float distanceAfterFirstFrame = Vector3.Distance(scaleAfterFirstFrame, targetScale);

        Assert.AreNotEqual(startScale, scaleAfterFirstFrame, "A escala local deve mudar após o primeiro frame.");
        Assert.Less(distanceAfterFirstFrame, initialDistance, "A escala deve ter mudado na direção do valor alvo.");

        yield return new WaitForSeconds(duration);

        // Assert
        Assert.IsTrue(Vector3.Distance(targetScale, _testRectTransform.localScale) < 0.001f, "A escala local final deve ser muito próxima do valor alvo.");
    }

    /// <summary>
    /// Testa se o 'localEulerAngles' de um RectTransform muda na direção correta.
    /// </summary>
    [UnityTest]
    public IEnumerator ATween_RectTransform_LocalEulerAngles_ChangesTowardsTarget()
    {
        // Arrange
        Vector3 startRotation = Vector3.zero;
        Vector3 targetRotation = new Vector3(0, 0, 90);
        float duration = 0.3f;
        _testRectTransform.localEulerAngles = startRotation;
        float initialDistance = Vector3.Distance(startRotation, targetRotation);

        // Act
        _testRectTransform.ATween("localEulerAngles", targetRotation, duration);

        // Observe
        yield return null;
        Vector3 rotationAfterFirstFrame = _testRectTransform.localEulerAngles;
        // Normaliza a distância para rotações para evitar problemas com ângulos equivalentes (ex: 0 vs 360)
        float distanceAfterFirstFrame = Mathf.DeltaAngle(rotationAfterFirstFrame.z, targetRotation.z);
        
        Assert.AreNotEqual(startRotation, rotationAfterFirstFrame, "A rotação local deve mudar após o primeiro frame.");
        Assert.Less(Mathf.Abs(distanceAfterFirstFrame), Mathf.Abs(initialDistance), "A rotação deve ter mudado na direção do valor alvo.");
        
        yield return new WaitForSeconds(duration);

        // Assert
        Assert.IsTrue(Vector3.Distance(targetRotation, _testRectTransform.localEulerAngles) < 0.001f, "A rotação local final deve ser muito próxima do valor alvo.");
    }
}