using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using AnimaTween;

public class AnimaTweenMaterialTests
{
    private GameObject _testObject;
    private Material _testMaterial;

    // Configuração: Cria um cubo com um material único para cada teste.
    [SetUp]
    public void Setup()
    {
        // CORREÇÃO: Usa PrimitiveType.Cube em vez de MeshType.Cube
        _testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        // Criamos uma instância do material para que as alterações não afetem outros objetos.
        _testMaterial = _testObject.GetComponent<MeshRenderer>().material;
    }

    // Limpeza: Destrói o objeto de teste após cada teste.
    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(_testObject);
    }

    // --- TESTES DE PLAY MODE ---

    [UnityTest]
    public IEnumerator ATween_MaterialFloatProperty_CompletesWithValue()
    {
        // Arrange
        string propertyName = "_Metallic"; // Propriedade padrão do shader URP/Lit
        float startValue = 0f;
        float targetValue = 1f;
        float duration = 0.5f;
        _testMaterial.SetFloat(propertyName, startValue);

        // Act
        _testMaterial.ATween(propertyName, targetValue, duration);
        yield return null; // Espera um frame para a corrotina começar
        yield return new WaitForSeconds(duration + 0.1f);

        // Assert
        Assert.AreEqual(targetValue, _testMaterial.GetFloat(propertyName), "A propriedade float do material deve ser igual ao valor alvo.");
    }

    [UnityTest]
    public IEnumerator ATween_MaterialColorProperty_CompletesWithValue()
    {
        // Arrange
        string propertyName = "_BaseColor"; // URP/Lit. Para o shader padrão, use "_Color".
        Color startValue = Color.white;
        Color targetValue = Color.blue;
        float duration = 0.5f;
        _testMaterial.SetColor(propertyName, startValue);

        // Act
        _testMaterial.ATween(propertyName, targetValue, duration);
        yield return null;
        yield return new WaitForSeconds(duration + 0.1f);

        // Assert
        Assert.AreEqual(targetValue, _testMaterial.GetColor(propertyName), "A propriedade de cor do material deve ser igual ao valor alvo.");
    }
    
    // --- TESTE DE EDITOR ---

    [Test]
    public void AComplete_MaterialColorTween_JumpsToFinalValueImmediately()
    {
        // Arrange
        string propertyName = "_BaseColor";
        Color startValue = Color.white;
        Color targetValue = Color.red;
        float duration = 5f; // Duração longa para provar que não esperamos.
        _testMaterial.SetColor(propertyName, startValue);

        // Act
        _testMaterial.ATween(propertyName, targetValue, duration);
        _testMaterial.AComplete(propertyName); // Completa imediatamente

        // Assert
        Assert.AreEqual(targetValue, _testMaterial.GetColor(propertyName), "AComplete deve levar a cor do material ao valor final instantaneamente.");
    }
}

