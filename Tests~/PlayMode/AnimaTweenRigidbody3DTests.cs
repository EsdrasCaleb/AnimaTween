using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using AnimaTween;

public class AnimaTweenRigidbody3DTests
{
    private GameObject _testObject;
    private Rigidbody _testRigidbody;

    [SetUp]
    public void Setup()
    {
        _testObject = new GameObject("TestObject");
        // Adiciona um colisor, que é necessário para que a física funcione corretamente.
        _testObject.AddComponent<BoxCollider>();
        _testRigidbody = _testObject.AddComponent<Rigidbody>();
        // Garante que o objeto não durma durante o teste para que possamos ler a velocidade.
        _testRigidbody.sleepThreshold = 0.0f;
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(_testObject);
    }

    /// <summary>
    /// Testa a animação da posição de um Rigidbody cinemático.
    /// </summary>
    [UnityTest]
    public IEnumerator ATween_Rigidbody_Position_MovesTowardsTarget_WhenKinematic()
    {
        // Arrange
        _testRigidbody.isKinematic = true; // Essencial para animar a posição diretamente.
        Vector3 startPosition = Vector3.zero;
        Vector3 targetPosition = new Vector3(5, 10, 0);
        float duration = 0.3f;
        _testRigidbody.position = startPosition;

        // Act
        _testRigidbody.ATween("position", targetPosition, duration);

        // Observe
        yield return null; // Espera um frame.
        float initialDistance = Vector3.Distance(startPosition, targetPosition);
        float distanceAfterFrame = Vector3.Distance(_testRigidbody.position, targetPosition);

        Assert.AreNotEqual(startPosition, _testRigidbody.position, "A posição deve mudar após o primeiro frame.");
        Assert.Less(distanceAfterFrame, initialDistance, "O Rigidbody deve ter se movido para mais perto do alvo.");

        yield return new WaitForSeconds(duration);

        // Assert
        Assert.IsTrue(Vector3.Distance(targetPosition, _testRigidbody.position) < 0.001f, "A posição final deve ser muito próxima da posição alvo.");
    }

    /// <summary>
    /// Testa a animação da velocidade, considerando o efeito da gravidade.
    /// </summary>
    [UnityTest]
    public IEnumerator ATween_Rigidbody_Velocity_ChangesTowardsTarget_WithGravity()
    {
        // Arrange
        _testRigidbody.isKinematic = false;
        _testRigidbody.useGravity = true;
        Vector3 startVelocity = Vector3.zero;
        Vector3 targetVelocity = new Vector3(10, 0, 0); // Alvo é uma velocidade puramente horizontal.
        float duration = 0.5f;
        _testRigidbody.position = Vector3.up*50f;
        _testRigidbody.linearVelocity = startVelocity;

        // Act
        _testRigidbody.ATween("velocity", targetVelocity, duration);

        // Observe
        yield return new WaitForSeconds(duration/2); // Espera um pouco para a física agir.

        // Verificações durante o tween:
        // 1. A velocidade horizontal (X) deve estar a aumentar na direção certa.
        Assert.Greater(_testRigidbody.linearVelocity.x, startVelocity.x, "A velocidade horizontal deve aumentar na direção do alvo.");
        // 2. A velocidade vertical (Y) deve ser negativa por causa da gravidade.
        Assert.Less(_testRigidbody.linearVelocity.y, 0, "A gravidade deve puxar o objeto para baixo, resultando em velocidade vertical negativa.");

        yield return new WaitForSeconds(duration);

        // Assert (Final)
        // No fim, a velocidade X deve estar perto do alvo, e a Y deve ser o resultado da gravidade ao longo do tempo.
        Assert.IsTrue(Mathf.Abs(targetVelocity.x - _testRigidbody.linearVelocity.x) < 0.01f, "A componente X da velocidade final deve ser muito próxima do alvo.");
        Assert.Less(_testRigidbody.linearVelocity.y, 0, "A componente Y da velocidade final deve ser negativa devido à gravidade.");
    }

    /// <summary>
    /// Testa a animação da velocidade angular.
    /// </summary>
    [UnityTest]
    public IEnumerator ATween_Rigidbody_AngularVelocity_ChangesTowardsTarget()
    {
        // Arrange
        _testRigidbody.isKinematic = false;
        _testRigidbody.useGravity = false; // Desliga a gravidade para isolar a rotação.
        Vector3 startAngularVelocity = Vector3.zero;
        Vector3 targetAngularVelocity = new Vector3(0, 5, 0); // Girar em torno do eixo Y.
        float duration = 0.3f;
        _testRigidbody.angularVelocity = startAngularVelocity;

        // Act
        _testRigidbody.ATween("angularVelocity", targetAngularVelocity, duration);

        // Observe
        yield return new WaitForSeconds(duration/2);
        Assert.Greater(_testRigidbody.angularVelocity.y, 0, "A velocidade angular em Y deve aumentar após metade da duração.");
        
        yield return new WaitForSeconds(duration);
        
        // Assert
        Assert.IsTrue(Vector3.Distance(targetAngularVelocity, _testRigidbody.angularVelocity) < 0.01f, "A velocidade angular final deve ser muito próxima do alvo.");
    }
}
