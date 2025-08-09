## ? Documenta��o

### Fun��es

#### Fun��o Principal

� a fun��o mais vers�til, capaz de animar qualquer propriedade ou campo p�blico de um objeto.

```csharp
Target.AnimaTween(string propertyName, object toValue, float duration,
           Easing easing = Easing.Linear, Playback playback = Playback.Forward, Action onComplete = null)
```

#### Fun��o de Atalho (Shortcut)

Fun��es pr�-configuradas para tarefas comuns.

```csharp
// Anima a transpar�ncia (alpha) de componentes visuais comuns.
Component.AnimaFade(float toAlpha, float duration, Easing easing = Easing.Linear, Action onComplete = null)
```

#### Fun��es de Controle

Permitem interromper anima��es em andamento.

```csharp
// Para todas as anima��es ativas em um objeto.
Target.Complete()

// Para uma anima��o espec�fica em um objeto, identificada pelo nome da propriedade.
Target.Complete(string propertyName)
```

-----

### Par�metros

| Par�metro | Descri��o |
| :--- | :--- |
| `Target` | O objeto que cont�m o campo ou a propriedade a ser animada. |
| `propertyName` | O nome do campo ou da propriedade (em formato de string). |
| `toValue` | O valor final da anima��o. Pode ser um valor �nico ou uma cole��o (ex: `List<Vector3>`) para criar um caminho. |
| `duration` | A dura��o da anima��o em segundos. |
| `easing` | A curva de acelera��o da anima��o (veja a lista abaixo). |
| `playback` | O modo de reprodu��o da anima��o (veja a se��o **Modos de Playback**). |
| `onComplete` | Uma fun��o opcional que ser� executada quando a anima��o terminar (n�o � chamada em loops). |

-----

### Modos de Playback

O par�metro `playback` define como a anima��o se comporta ao longo do tempo.

| Modo | Comportamento | Descri��o |
| :--- | :--- | :--- |
| `Forward` | ? A ? B | **(Padr�o)** Anima do valor inicial para o final e para. |
| `Backward` |  backwards\_button: B ? A | Anima do valor final para o inicial e para. |
| `PingPong` | ? A ? B ? A | Anima do inicial para o final, e imediatamente volta ao inicial, parando em seguida. |
| `LoopForward` | ? A ? B, A ? B... | Repete a anima��o do inicial para o final indefinidamente. |
| `LoopBackward` | ? B ? A, B ? A... | Repete a anima��o do final para o inicial indefinidamente. |
| `LoopPingPong` | ? A ? B ? A, A ? B ? A... | Repete a anima��o de "ida e volta" indefinidamente. Ideal para efeitos de pulsa��o. |

**Nota:** O callback `onComplete` **n�o** � chamado para os modos de *Loop*, pois eles, por defini��o, nunca terminam.

-----

### Valores de Easing

```
InBack, InBounce, InCirc, InCubic, InElastic, InExpo,
InOutBack, InOutBounce, InOutCirc, InOutCubic, InOutElastic, InOutExpo,
InOutQuad, InOutQuart, InOutQuint, InOutSine,
InQuad, InQuart, InQuint, InSine,
Linear,
OutBack, OutBounce, OutCirc, OutCubic, OutElastic, OutExpo,
OutInBack, OutInBounce, OutInCirc, OutInCubic, OutInElastic, OutInExpo,
OutInQuad, OutInQuart, OutInQuint, OutInSine,
OutQuad, OutQuart, OutQuint, OutSine
```

Veja a representa��o visual de cada curva de easing aqui:
?? **[Refer�ncia Visual de Easing](https://easings.net/)** (um excelente recurso visual para entender cada curva)

-----

### Exemplos de Uso

**1. Mover um `Transform` para uma nova posi��o:**

```csharp
public Transform playerTransform;

void Start() {
    Vector3 targetPosition = new Vector3(10, 0, 0);
    playerTransform.AnimaTween("position", targetPosition, 2.0f, Easing.OutBack);
}
```

**2. Fazer um �cone de alerta pulsar usando `LoopPingPong`:**

```csharp
public Transform warningIcon;

void Start() {
    // O �cone vai aumentar e diminuir de tamanho continuamente.
    warningIcon.AnimaTween("localScale", Vector3.one * 1.2f, 0.7f, Easing.InOutSine, Playback.LoopPingPong);
}
```

**3. Fazer o fade-out de uma imagem da UI e depois desativ�-la:**

```csharp
public Image myImage;

void HideImage() {
    myImage.AnimaFade(0f, 1.5f, Easing.OutQuad, () => {
        // Este c�digo ser� executado quando o fade terminar.
        myImage.gameObject.SetActive(false);
    });
}
```

**4. Animar um valor de pontua��o e atualizar o texto no final:**

```csharp
public Text scoreText;
public int currentScore = 0;

void AddPoints(int pointsToAdd) {
    int newScore = currentScore + pointsToAdd;
    // Anima o valor "invis�vel" 'currentScore'
    this.AnimaTween(nameof(currentScore), newScore, 1.0f, Easing.OutCubic, onComplete: UpdateScoreText);
}

void UpdateScoreText() {
    scoreText.text = "Score: " + currentScore;
}
```

**5. Parar uma anima��o espec�fica:**

```csharp
// Em algum momento, o jogador entra numa cutscene e precisa parar de se mover.
playerTransform.Complete("position");

// Parar todas as anima��es
playerTransform.Complete();
