using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using AnimaTween;

public class AnimaTweenTransformTests
{
    private GameObject _testObject;
    private Transform _testTransform;

    [SetUp]
    public void Setup()
    {
        _testObject = new GameObject("TestObject");
        _testTransform = _testObject.transform;
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(_testObject);
    }

    /// <summary>
    /// Testa se a propriedade 'position' muda ao longo do tempo e termina no valor correto.
    /// </summary>
    [UnityTest]
    public IEnumerator ATween_Transform_Position_ChangesOverTimeAndCompletes()
    {
        // Arrange
        Vector3 startPosition = Vector3.zero;
        Vector3 targetPosition = new Vector3(10, 5, -2);
        float duration = 0.3f;
        _testTransform.position = startPosition;

        // Act
        _testTransform.ATween("position", targetPosition, duration);

        // Observe
        yield return null; // Espera um frame para o tween começar.
        Vector3 positionAfterFirstFrame = _testTransform.position;
        Assert.AreNotEqual(startPosition, positionAfterFirstFrame, "A posição deve mudar após o primeiro frame.");

        // Espera a duração do tween.
        yield return new WaitForSeconds(duration);
        
        // Assert
        // Usamos uma tolerância para a comparação de Vector3 devido a imprecisões de ponto flutuante.
        Assert.IsTrue(Vector3.Distance(targetPosition, _testTransform.position) < 0.001f, "A posição final deve ser muito próxima da posição alvo.");
    }

    /// <summary>
    /// Testa se a propriedade 'localScale' muda ao longo do tempo e termina no valor correto.
    /// </summary>
    [UnityTest]
    public IEnumerator ATween_Transform_LocalScale_ChangesOverTimeAndCompletes()
    {
        // Arrange
        Vector3 startScale = Vector3.one;
        Vector3 targetScale = new Vector3(2, 0.5f, 3);
        float duration = 0.3f;
        _testTransform.localScale = startScale;

        // Act
        _testTransform.ATween("localScale", targetScale, duration);

        // Observe
        yield return null;
        Assert.AreNotEqual(startScale, _testTransform.localScale, "A escala deve mudar após o primeiro frame.");

        yield return new WaitForSeconds(duration);

        // Assert
        Assert.IsTrue(Vector3.Distance(targetScale, _testTransform.localScale) < 0.001f, "A escala final deve ser muito próxima da escala alvo.");
    }
    
    /// <summary>
    /// Testa se a propriedade 'eulerAngles' muda ao longo do tempo e termina no valor correto.
    /// </summary>
    [UnityTest]
    public IEnumerator ATween_Transform_EulerAngles_ChangesOverTimeAndCompletes()
    {
        // Arrange
        Vector3 startRotation = Vector3.zero;
        Vector3 targetRotation = new Vector3(0, 90, 45);
        float duration = 0.3f;
        _testTransform.eulerAngles = startRotation;

        // Act
        _testTransform.ATween("eulerAngles", targetRotation, duration);

        // Observe
        yield return null;
        Assert.AreNotEqual(startRotation, _testTransform.eulerAngles, "A rotação deve mudar após o primeiro frame.");

        yield return new WaitForSeconds(duration);

        // Assert
        Assert.IsTrue(Vector3.Distance(targetRotation, _testTransform.eulerAngles) < 0.001f, "Os ângulos de Euler finais devem ser muito próximos dos ângulos alvo.");
    }
    
    /// <summary>
    /// Testa se a propriedade 'rotation' (Quaternion) muda ao longo do tempo e termina no valor correto.
    /// </summary>
    [UnityTest]
    public IEnumerator ATween_Transform_Rotation_ChangesOverTimeAndCompletes()
    {
        // Arrange
        Quaternion startRotation = Quaternion.identity;
        Quaternion targetRotation = Quaternion.Euler(45, 90, 0);
        float duration = 0.3f;
        _testTransform.rotation = startRotation;

        // Act
        _testTransform.ATween("rotation", targetRotation, duration);

        // Observe
        yield return null;
        Assert.AreNotEqual(startRotation, _testTransform.rotation, "A rotação (Quaternion) deve mudar após o primeiro frame.");

        yield return new WaitForSeconds(duration);

        // Assert
        // Usamos uma tolerância para a comparação de Quaternion.
        Assert.IsTrue(Quaternion.Angle(targetRotation, _testTransform.rotation) < 0.001f, "A rotação final deve ser muito próxima da rotação alvo.");
    }
}
