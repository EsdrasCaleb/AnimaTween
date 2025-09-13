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
        _testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _testMaterial = new Material((Shader.Find("Hidden/AnimaTweenTestShader")));
        var renderer = _testObject.GetComponent<MeshRenderer>();
        renderer.material = _testMaterial;
    }

    // Limpeza: Destrói o objeto de teste após cada teste.
    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(_testObject);
        Object.DestroyImmediate(_testMaterial);
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
    [UnityTest]
    public IEnumerator ATween_Material_ColorProperty_ChangesTowardsTarget()
    {
        // Arrange
        string propertyName = "_Color";
        Color startValue = Color.black;
        Color targetValue = Color.blue;
        float duration = 0.4f;
        _testMaterial.SetColor(propertyName, startValue);
        
        // Act
        _testMaterial.ATween(propertyName, targetValue, duration);

        // Observe
        yield return new WaitForSeconds(duration / 2f);
        Color midValue = _testMaterial.GetColor(propertyName);

        Assert.AreNotEqual(startValue, midValue, "A cor deve ter mudado a meio do tween.");
        // A cor a meio do caminho deve ser uma mistura, portanto, nem branca nem azul pura.
        Assert.Greater(midValue.b, startValue.b, 
            $"A componente azul da cor deve estar a aumentar.{startValue} {midValue}");
        
        yield return new WaitForSeconds(duration / 2f + 0.1f) ;

        // Assert
        Assert.AreEqual(targetValue, _testMaterial.GetColor(propertyName), "A cor final deve ser igual à cor alvo.");
    }

    [UnityTest]
    public IEnumerator ATween_Material_FloatProperty_ChangesTowardsTarget()
    {
        // Arrange
        string propertyName = "_Float";
        float startValue = 0f;
        float targetValue = 10f;
        float duration = 0.4f;
        _testMaterial.SetFloat(propertyName, startValue);

        // Act
        _testMaterial.ATween(propertyName, targetValue, duration);

        // Observe
        yield return new WaitForSeconds(duration / 2f);
        float midValue = _testMaterial.GetFloat(propertyName);
        
        Assert.AreNotEqual(startValue, midValue, "O float deve ter mudado a meio do tween.");
        Assert.IsTrue(midValue > startValue && midValue < targetValue, "O valor a meio do caminho deve estar entre o início e o fim.");

        yield return new WaitForSeconds(duration / 2f);

        // Assert
        Assert.AreEqual(targetValue, _testMaterial.GetFloat(propertyName), "O float final deve ser igual ao valor alvo.");
    }

    [UnityTest]
    public IEnumerator ATween_Material_IntProperty_ChangesTowardsTarget()
    {
        // Arrange
        string propertyName = "_Int";
        int startValue = 0;
        int targetValue = 100;
        float duration = 0.4f;
        _testMaterial.SetInt(propertyName, startValue);

        // Act
        _testMaterial.ATween(propertyName, targetValue, duration);

        // Observe
        yield return new WaitForSeconds(duration / 2f);
        int midValue = _testMaterial.GetInt(propertyName);

        Assert.AreNotEqual(startValue, midValue, "O int deve ter mudado a meio do tween.");
        Assert.IsTrue(midValue > startValue && midValue < targetValue, "O valor a meio do caminho deve estar entre o início e o fim.");

        yield return new WaitForSeconds(duration / 2f);

        // Assert
        Assert.AreEqual(targetValue, _testMaterial.GetInt(propertyName), "O int final deve ser igual ao valor alvo.");
    }
    
    [UnityTest]
    public IEnumerator ATween_Material_VectorProperty_ChangesTowardsTarget()
    {
        // Arrange
        string propertyName = "_Vector";
        Vector4 startValue = new Vector4(0, 0, 0, 0);
        Vector4 targetValue = new Vector4(1, 2, 3, 4);
        float duration = 0.4f;
        _testMaterial.SetVector(propertyName, startValue);
        float initialDistance = Vector4.Distance(startValue, targetValue);

        // Act
        _testMaterial.ATween(propertyName, targetValue, duration);

        // Observe
        yield return new WaitForSeconds(duration / 2f);
        Vector4 midValue = _testMaterial.GetVector(propertyName);
        float midDistance = Vector4.Distance(midValue, targetValue);

        Assert.AreNotEqual(startValue, midValue, "O vetor deve ter mudado a meio do tween.");
        Assert.Less(midDistance, initialDistance, "O vetor deve estar mais perto do alvo a meio do tween.");
        
        yield return new WaitForSeconds(duration / 2f);

        // Assert
        Assert.AreEqual(targetValue, _testMaterial.GetVector(propertyName), "O vetor final deve ser igual ao valor alvo.");
    }
    
}

