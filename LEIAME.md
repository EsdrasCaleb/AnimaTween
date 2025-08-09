## ? Documentação

### Funções

#### Função Principal

É a função mais versátil, capaz de animar qualquer propriedade ou campo público de um objeto.

```csharp
Target.AnimaTween(string propertyName, object toValue, float duration,
           Easing easing = Easing.Linear, Playback playback = Playback.Forward, Action onComplete = null)
```

#### Função de Atalho (Shortcut)

Funções pré-configuradas para tarefas comuns.

```csharp
// Anima a transparência (alpha) de componentes visuais comuns.
Component.AnimaFade(float toAlpha, float duration, Easing easing = Easing.Linear, Action onComplete = null)
```

#### Funções de Controle

Permitem interromper animações em andamento.

```csharp
// Para todas as animações ativas em um objeto.
Target.Complete()

// Para uma animação específica em um objeto, identificada pelo nome da propriedade.
Target.Complete(string propertyName)
```

-----

### Parâmetros

| Parâmetro | Descrição |
| :--- | :--- |
| `Target` | O objeto que contém o campo ou a propriedade a ser animada. |
| `propertyName` | O nome do campo ou da propriedade (em formato de string). |
| `toValue` | O valor final da animação. Pode ser um valor único ou uma coleção (ex: `List<Vector3>`) para criar um caminho. |
| `duration` | A duração da animação em segundos. |
| `easing` | A curva de aceleração da animação (veja a lista abaixo). |
| `playback` | O modo de reprodução da animação (veja a seção **Modos de Playback**). |
| `onComplete` | Uma função opcional que será executada quando a animação terminar (não é chamada em loops). |

-----

### Modos de Playback

O parâmetro `playback` define como a animação se comporta ao longo do tempo.

| Modo | Comportamento | Descrição |
| :--- | :--- | :--- |
| `Forward` | ? A ? B | **(Padrão)** Anima do valor inicial para o final e para. |
| `Backward` |  backwards\_button: B ? A | Anima do valor final para o inicial e para. |
| `PingPong` | ? A ? B ? A | Anima do inicial para o final, e imediatamente volta ao inicial, parando em seguida. |
| `LoopForward` | ? A ? B, A ? B... | Repete a animação do inicial para o final indefinidamente. |
| `LoopBackward` | ? B ? A, B ? A... | Repete a animação do final para o inicial indefinidamente. |
| `LoopPingPong` | ? A ? B ? A, A ? B ? A... | Repete a animação de "ida e volta" indefinidamente. Ideal para efeitos de pulsação. |

**Nota:** O callback `onComplete` **não** é chamado para os modos de *Loop*, pois eles, por definição, nunca terminam.

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

Veja a representação visual de cada curva de easing aqui:
?? **[Referência Visual de Easing](https://easings.net/)** (um excelente recurso visual para entender cada curva)

-----

### Exemplos de Uso

**1. Mover um `Transform` para uma nova posição:**

```csharp
public Transform playerTransform;

void Start() {
    Vector3 targetPosition = new Vector3(10, 0, 0);
    playerTransform.AnimaTween("position", targetPosition, 2.0f, Easing.OutBack);
}
```

**2. Fazer um ícone de alerta pulsar usando `LoopPingPong`:**

```csharp
public Transform warningIcon;

void Start() {
    // O ícone vai aumentar e diminuir de tamanho continuamente.
    warningIcon.AnimaTween("localScale", Vector3.one * 1.2f, 0.7f, Easing.InOutSine, Playback.LoopPingPong);
}
```

**3. Fazer o fade-out de uma imagem da UI e depois desativá-la:**

```csharp
public Image myImage;

void HideImage() {
    myImage.AnimaFade(0f, 1.5f, Easing.OutQuad, () => {
        // Este código será executado quando o fade terminar.
        myImage.gameObject.SetActive(false);
    });
}
```

**4. Animar um valor de pontuação e atualizar o texto no final:**

```csharp
public Text scoreText;
public int currentScore = 0;

void AddPoints(int pointsToAdd) {
    int newScore = currentScore + pointsToAdd;
    // Anima o valor "invisível" 'currentScore'
    this.AnimaTween(nameof(currentScore), newScore, 1.0f, Easing.OutCubic, onComplete: UpdateScoreText);
}

void UpdateScoreText() {
    scoreText.text = "Score: " + currentScore;
}
```

**5. Parar uma animação específica:**

```csharp
// Em algum momento, o jogador entra numa cutscene e precisa parar de se mover.
playerTransform.Complete("position");

// Parar todas as animações
playerTransform.Complete();
