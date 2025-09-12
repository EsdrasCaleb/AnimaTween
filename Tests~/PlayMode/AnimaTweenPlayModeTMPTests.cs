using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using TMPro;
using AnimaTween;

public class AnimaTweenPlayModeTMPTests
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

    [UnityTest]
    public IEnumerator ATween_TMP_FromEmptyToText_CompletesWithValue()
    {
        // Arrange
        var tmp = CreateTMPText("Text1");
        tmp.text = "";
        string targetText = "Hello World";
        float duration = 0.5f;

        // Act
        tmp.ATween("text", targetText, duration);
        yield return new WaitForSeconds(duration + 0.1f);

        // Assert
        Assert.AreEqual(targetText, tmp.text, "O texto final deve ser igual ao texto alvo.");
    }

    [UnityTest]
    public IEnumerator ATween_TMP_FromStringToStringWithPrefix_CompletesWithValue()
    {
        // Arrange
        var tmp = CreateTMPText("Text2");
        tmp.text = "AnimaTween";
        string targetText = "AnimaWorks"; // Prefixo comum: "Anima"
        float duration = 0.5f;

        // Act
        tmp.ATween("text", targetText, duration);
        yield return new WaitForSeconds(duration + 0.1f);

        // Assert
        Assert.AreEqual(targetText, tmp.text, "O texto deve transitar corretamente de uma string para outra com prefixo.");
    }

    [UnityTest]
    public IEnumerator ATween_TMP_FromNumberToNumber_CompletesWithValue()
    {
        // Arrange
        var tmp = CreateTMPText("Text3");
        tmp.text = "0";
        string targetText = "100";
        float duration = 0.5f;

        // Act
        tmp.ATween("text", targetText, duration);
        yield return new WaitForSeconds(duration + 0.1f);

        // Assert
        Assert.AreEqual(targetText, tmp.text, "A animação numérica deve terminar no valor final exato.");
    }

    [UnityTest]
    public IEnumerator ATween_TMP_TwoConcurrentTweens_DoNotInterfere()
    {
        // Arrange
        var tmp1 = CreateTMPText("ConcurrentText1");
        var tmp2 = CreateTMPText("ConcurrentText2");
        tmp1.text = "Start A";
        tmp2.text = "Start B";
        string targetA = "End A";
        string targetB = "End B";
        float duration = 0.5f;

        // Act
        tmp1.ATween("text", targetA, duration);
        tmp2.ATween("text", targetB, duration);
        yield return new WaitForSeconds(duration + 0.1f);

        // Assert
        Assert.AreEqual(targetA, tmp1.text, "O primeiro tween não deve ser afetado pelo segundo.");
        Assert.AreEqual(targetB, tmp2.text, "O segundo tween não deve ser afetado pelo primeiro.");
    }
    
     /// <summary>
    /// Este teste melhorado verifica, a cada frame, se duas animações de texto
    /// concorrentes permanecem independentes durante todo o processo.
    /// </summary>
    [UnityTest]
    public IEnumerator ATween_ConcurrentStringTweens_RemainIndependentThroughout()
    {
        // Arrange
        var tmp1 = CreateTMPText("TextA");
        var tmp2 = CreateTMPText("TextB");

        string startA = "A";
        string startB = "B";
        // Usamos textos longos para garantir que a animação dure vários frames.
        string targetA = "AAAAAAAAAA";
        string targetB = "BBBBBBBBBB";
        float duration = 0.5f;

        tmp1.text = startA;
        tmp2.text = startB;

        // Act: Inicia ambos os tweens simultaneamente.
        tmp1.ATween("text", targetA, duration);
        tmp2.ATween("text", targetB, duration);

        float elapsedTime = 0f;

        // Observe & Assert: Loop durante a animação.
        while (elapsedTime < duration)
        {
            // Verifica os valores a cada frame.
            // Esta é a verificação crucial: o texto de A não deve conter "B" e vice-versa.
            Assert.IsFalse(tmp1.text.Contains("B"), "O Tween A não deve conter caracteres do Tween B.");
            Assert.IsFalse(tmp2.text.Contains("A"), "O Tween B não deve conter caracteres do Tween A.");

            elapsedTime += Time.deltaTime;
            yield return null; // Espera pelo próximo frame.
        }
        
        // Assert Final: Garante que ambos chegaram ao destino correto.
        Assert.AreEqual(targetA, tmp1.text, "O Tween A deve terminar no seu valor alvo.");
        Assert.AreEqual(targetB, tmp2.text, "O Tween B deve terminar no seu valor alvo.");
    }
     
    [UnityTest]
    public IEnumerator ATween_ConcurrentNumericStringTweens_RemainIndependentThroughout()
    {
        // Arrange
        var tmp1 = CreateTMPText("NumericTextA");
        var tmp2 = CreateTMPText("NumericTextB");

        int startValue = 0;
        int targetA = -20;
        int targetB = 20;
        float duration = 0.5f;

        tmp1.text = startValue.ToString();
        tmp2.text = startValue.ToString();

        // Act: Inicia os tweens numéricos.
        tmp1.ATween("text", targetA.ToString(), duration);
        tmp2.ATween("text", targetB.ToString(), duration);

        float elapsedTime = 0f;
        float previousValueA = startValue;
        float previousValueB = startValue;

        // Observe & Assert: Loop durante a animação.
        while (elapsedTime < duration)
        {
            // Tenta converter o texto de volta para float para verificação.
            bool parseASuccess = float.TryParse(tmp1.text, out float currentValueA);
            bool parseBSuccess = float.TryParse(tmp2.text, out float currentValueB);

            // Garante que o texto seja sempre um número válido.
            Assert.IsTrue(parseASuccess, "O texto A deve ser sempre um número válido.");
            Assert.IsTrue(parseBSuccess, "O texto B deve ser sempre um número válido.");
            
            // Verificação crucial: O tween A só deve diminuir, e o B só deve aumentar.
            Assert.LessOrEqual(currentValueA, previousValueA, "O valor do Tween A deve sempre diminuir ou permanecer o mesmo.");
            Assert.GreaterOrEqual(currentValueB, previousValueB, "O valor do Tween B deve sempre aumentar ou permanecer o mesmo.");

            // Atualiza os valores para a verificação do próximo frame.
            previousValueA = currentValueA;
            previousValueB = currentValueB;
            
            elapsedTime += Time.deltaTime;
            yield return null; // Espera pelo próximo frame.
        }

        // Assert Final: Garante que ambos chegaram ao destino numérico correto.
        Assert.AreEqual(targetA.ToString(), tmp1.text, "O Tween A deve terminar no seu valor alvo.");
        Assert.AreEqual(targetB.ToString(), tmp2.text, "O Tween B deve terminar no seu valor alvo.");
    }
    
     [UnityTest]
    public IEnumerator ATween_TMP_Color_CompletesWithValue()
    {
        // Arrange
        var tmp = CreateTMPText("ColorText");
        tmp.color = Color.white;
        Color targetColor = Color.blue;
        float duration = 0.2f;

        // Act
        tmp.ATween("color", targetColor, duration);
        yield return null; // Dá à corrotina uma frame para começar
        yield return new WaitForSeconds(duration + 0.1f);

        // Assert
        Assert.AreEqual(targetColor, tmp.color, "A cor final deve ser igual à cor alvo.");
    }

    [UnityTest]
    public IEnumerator ATween_TMP_FontSize_CompletesWithValue()
    {
        // Arrange
        var tmp = CreateTMPText("FontSizeText");
        tmp.fontSize = 12;
        float targetSize = 24f;
        float duration = 0.2f;

        // Act
        tmp.ATween("fontSize", targetSize, duration);
        yield return null;
        yield return new WaitForSeconds(duration + 0.1f);

        // Assert
        Assert.AreEqual(targetSize, tmp.fontSize, "O tamanho da fonte final deve ser igual ao tamanho alvo.");
    }

    [UnityTest]
    public IEnumerator ATween_TMP_CharacterSpacing_CompletesWithValue()
    {
        // Arrange
        var tmp = CreateTMPText("CharSpacingText");
        tmp.characterSpacing = 0;
        float targetSpacing = 10f;
        float duration = 0.2f;

        // Act
        tmp.ATween("characterSpacing", targetSpacing, duration);
        yield return null;
        yield return new WaitForSeconds(duration + 0.1f);

        // Assert
        Assert.AreEqual(targetSpacing, tmp.characterSpacing, "O espaçamento de caracteres final deve ser igual ao valor alvo.");
    }

    [UnityTest]
    public IEnumerator ATween_TMP_WordSpacing_CompletesWithValue()
    {
        // Arrange
        var tmp = CreateTMPText("WordSpacingText");
        tmp.wordSpacing = 0;
        float targetSpacing = 15f;
        float duration = 0.2f;

        // Act
        tmp.ATween("wordSpacing", targetSpacing, duration);
        yield return null;
        yield return new WaitForSeconds(duration + 0.1f);

        // Assert
        Assert.AreEqual(targetSpacing, tmp.wordSpacing, "O espaçamento de palavras final deve ser igual ao valor alvo.");
    }

    [UnityTest]
    public IEnumerator ATween_TMP_LineSpacing_CompletesWithValue()
    {
        // Arrange
        var tmp = CreateTMPText("LineSpacingText");
        tmp.text = "Linha 1\nLinha 2";
        tmp.lineSpacing = 0;
        float targetSpacing = 5f;
        float duration = 0.2f;

        // Act
        tmp.ATween("lineSpacing", targetSpacing, duration);
        yield return null;
        yield return new WaitForSeconds(duration + 0.1f);

        // Assert
        Assert.AreEqual(targetSpacing, tmp.lineSpacing, "O espaçamento de linha final deve ser igual ao valor alvo.");
    }

    [UnityTest]
    public IEnumerator ATween_TMP_Margin_CompletesWithValue()
    {
        // Arrange
        var tmp = CreateTMPText("MarginText");
        tmp.margin = Vector4.zero;
        var targetMargin = new Vector4(10, 5, 10, 5);
        float duration = 0.2f;

        // Act
        tmp.ATween("margin", targetMargin, duration);
        yield return null;
        yield return new WaitForSeconds(duration + 0.1f);

        // Assert
        Assert.AreEqual(targetMargin, tmp.margin, "A margem final deve ser igual à margem alvo.");
    }

    [UnityTest]
    public IEnumerator ATween_TMP_MaxVisibleCharacters_CompletesWithValue()
    {
        // Arrange
        var tmp = CreateTMPText("VisibleCharsText");
        tmp.text = "Hello World"; // 11 caracteres
        tmp.maxVisibleCharacters = 0;
        int targetVisible = 11;
        float duration = 0.2f;

        // Act
        tmp.ATween("maxVisibleCharacters", targetVisible, duration);
        yield return null;
        yield return new WaitForSeconds(duration + 0.1f);

        // Assert
        Assert.AreEqual(targetVisible, tmp.maxVisibleCharacters, "O número de caracteres visíveis deve ser igual ao total de caracteres no final.");
    }
}
