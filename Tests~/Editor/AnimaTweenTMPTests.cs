using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using TMPro; // Necessário para o TextMesh Pro
using AnimaTween;

public class AnimaTweenTMPTests
{
    private GameObject _testCanvas;
    
    // Configuração: Cria um Canvas para os nossos testes de UI
    [SetUp]
    public void Setup()
    {
        _testCanvas = new GameObject("TestCanvas", typeof(Canvas));
    }

    // Limpeza: Destrói o Canvas após cada teste
    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(_testCanvas);
    }

    // Função auxiliar para criar um objeto de texto TMP
    private TextMeshProUGUI CreateTMPText(string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(_testCanvas.transform);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 14;
        return tmp;
    }

    // --- TESTES DE EDITOR ---
    // Estes testes usam [Test] e rodam instantaneamente sem entrar em Play Mode.
    // São perfeitos para lógica que não depende do tempo.

    [Test]
    public void AComplete_TMP_TextTween_JumpsToFinalValueImmediately()
    {
        // Arrange
        var tmp = CreateTMPText("EditModeText");
        tmp.text = "Start";
        string targetText = "End";
        float duration = 5f; // Uma duração longa para provar que não esperamos por ela.

        // Act
        // Inicia a animação, mas a completa imediatamente a seguir.
        tmp.ATween("text", targetText, duration);
        tmp.AComplete("text");

        // Assert
        // Não precisamos de "yield return". A verificação é instantânea.
        Assert.AreEqual(targetText, tmp.text, "AComplete deve saltar para o valor final imediatamente.");
    }
}

