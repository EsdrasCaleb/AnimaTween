using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using AnimaTween;

public class AnimaTweenRigidbody2DTests
{
    private GameObject _testObject;
    private Rigidbody2D _testRigidbody2D;

    [SetUp]
    public void Setup()
    {
        _testObject = new GameObject("TestObject2D");
        // Adiciona um colisor 2D, que é necessário para a física 2D.
        _testObject.AddComponent<BoxCollider2D>();
        _testRigidbody2D = _testObject.AddComponent<Rigidbody2D>();
        // Garante que o objeto não durma durante o teste para que possamos ler a velocidade.
        _testRigidbody2D.sleepMode = RigidbodySleepMode2D.NeverSleep;
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(_testObject);
    }

    /// <summary>
    /// Testa a animação da posição de um Rigidbody2D cinemático.
    /// </summary>
    [UnityTest]
    public IEnumerator ATween_Rigidbody2D_Position_MovesTowardsTarget()
    {
        // Arrange
        _testRigidbody2D.bodyType = RigidbodyType2D.Static;
        Vector2 startPosition = Vector2.zero;
        Vector2 targetPosition = new Vector2(10, -20);
        float duration = 0.4f;
        _testRigidbody2D.position = startPosition;
        float initialDistance = Vector2.Distance(startPosition, targetPosition);

        // Act
        _testRigidbody2D.ATween("position", targetPosition, duration);

        // Observe
        yield return new WaitForSeconds(duration / 2f); // Espera metade da duração
        float midDistance = Vector2.Distance(_testRigidbody2D.position, targetPosition);
        
        Assert.AreNotEqual(startPosition, _testRigidbody2D.position, "A posição deve ter mudado a meio do tween.");
        Assert.Less(midDistance, initialDistance, "O Rigidbody2D deve estar mais perto do alvo a meio do tween.");

        yield return new WaitForSeconds(duration / 2f+0.5f); // Espera o resto da duração

        // Assert
        Assert.IsTrue(Vector2.Distance(targetPosition, _testRigidbody2D.position) < 1f, 
            $"A posição final{_testRigidbody2D.position} deve ser muito próxima da posição alvo{targetPosition}.");
    }

    /// <summary>
    /// Testa a animação da rotação (um float) de um Rigidbody2D.
    /// </summary>
    [UnityTest]
    public IEnumerator ATween_Rigidbody2D_Rotation_ChangesTowardsTarget()
    {
        // Arrange
        _testRigidbody2D.bodyType = RigidbodyType2D.Static;
        float startRotation = 0f;
        float targetRotation = 180f;
        float duration = 0.4f;
        _testRigidbody2D.rotation = startRotation;

        // Act
        _testRigidbody2D.ATween("rotation", targetRotation, duration);

        // Observe
        yield return new WaitForSeconds(duration / 2f);
        float midRotation = _testRigidbody2D.rotation;
        
        // A diferença angular a meio do caminho deve ser menor que a inicial.
        float initialAngleDiff = Mathf.Abs(Mathf.DeltaAngle(startRotation, targetRotation));
        float midAngleDiff = Mathf.Abs(Mathf.DeltaAngle(midRotation, targetRotation));

        Assert.AreNotEqual(startRotation, midRotation, "A rotação deve ter mudado a meio do tween.");
        Assert.Less(midAngleDiff, initialAngleDiff, "A rotação deve estar mais perto do alvo a meio do tween.");

        yield return new WaitForSeconds(duration / 2f);

        // Assert
        Assert.IsTrue(Mathf.Abs(Mathf.DeltaAngle(_testRigidbody2D.rotation, targetRotation)) < 0.01f, "A rotação final deve ser muito próxima do alvo.");
    }

    /// <summary>
    /// Testa a animação da velocidade de um Rigidbody2D dinâmico.
    /// </summary>
    [UnityTest]
    public IEnumerator ATween_Rigidbody2D_Velocity_ChangesTowardsTarget()
    {
        // Arrange
        _testRigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        _testRigidbody2D.gravityScale = 0; // Desliga a gravidade para isolar o teste de velocidade.
        Vector2 startVelocity = Vector2.zero;
        Vector2 targetVelocity = new Vector2(15, 10);
        float duration = 0.4f;
        _testRigidbody2D.linearVelocity = startVelocity;
        float initialDistance = Vector2.Distance(startVelocity, targetVelocity);

        // Act
        _testRigidbody2D.ATween("velocity", targetVelocity, duration);

        // Observe
        yield return new WaitForSeconds(duration / 2f);
        float midDistance = Vector2.Distance(_testRigidbody2D.linearVelocity, targetVelocity);

        Assert.AreNotEqual(startVelocity, _testRigidbody2D.linearVelocity, "A velocidade deve ter mudado a meio do tween.");
        Assert.Less(midDistance, initialDistance, "A velocidade deve estar mais perto do alvo a meio do tween.");

        yield return new WaitForSeconds(duration / 2f +0.1f);

        // Assert
        Assert.IsTrue(Vector2.Distance(targetVelocity, _testRigidbody2D.linearVelocity) < 0.01f, 
            $"A velocidade final{_testRigidbody2D.linearVelocity} deve ser muito próxima do alvo{targetVelocity}.");
    }

    /// <summary>
    /// Testa a animação da velocidade angular de um Rigidbody2D dinâmico.
    /// </summary>
    [UnityTest]
    public IEnumerator ATween_Rigidbody2D_AngularVelocity_ChangesTowardsTarget()
    {
        // Arrange
        _testRigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        float startVelocity = 0f;
        float targetVelocity = 360f; // 360 graus por segundo.
        float duration = 0.4f;
        _testRigidbody2D.angularVelocity = startVelocity;

        // Act
        _testRigidbody2D.ATween("angularVelocity", targetVelocity, duration);

        // Observe
        yield return new WaitForSeconds(duration / 2f);
        float midVelocity = _testRigidbody2D.angularVelocity;
        
        float initialDiff = Mathf.Abs(targetVelocity - startVelocity);
        float midDiff = Mathf.Abs(targetVelocity - midVelocity);
        
        Assert.AreNotEqual(startVelocity, midVelocity, "A velocidade angular deve ter mudado a meio do tween.");
        Assert.Less(midDiff, initialDiff, "A velocidade angular deve estar mais perto do alvo a meio do tween.");

        yield return new WaitForSeconds(duration/2f +0.01f);
        
        // Assert
        Assert.IsTrue(Mathf.Abs(_testRigidbody2D.angularVelocity - targetVelocity) < 1f, 
            $"A velocidade angular final{_testRigidbody2D.angularVelocity} " +
            $"deve ser muito próxima do alvo{targetVelocity}. ${_testRigidbody2D.angularVelocity - targetVelocity} ");
    }
}
