# AnimaTween

**AnimaTween** – The animation tween you see in any game engine.
A simple and flexible way to animate fields and properties of your objects directly in Unity.

Say goodbye to complex setups and hello to smooth, code-driven animation. The API design is heavily inspired by the beloved libraries of the Defold engine.

[![AnimaTween Showcase](Examples~/Images/animatween.gif)](https://youtu.be/nTCLchP_ZNk)

---

## 📦 Installation

1. Open Unity and go to **Window → Package Manager**.
2. Click the **+** button → **Add package from git URL...**
3. Paste the repository URL:

   ```
   https://github.com/EsdrasCaleb/AnimaTween
   ```
4. Click **Add** to install.

---

## 🚀 Usage

Once installed, simply import the namespace and call `AnimaTween` on any supported object.

**Example:**

```csharp
using UnityEngine;
using AnimaTween; // Important!

public class GameManager : MonoBehaviour
{
    public PlayerData playerData;
    public TMPro.TMP_Text scoreText;

    void Start()
    {
        // Animates the 'score' variable from 'playerData' to 1000 over 3 seconds.
        // When finished, calls the UpdateText function.
        playerData.ATween("score", 1000, 3.0f, onComplete: UpdateText);
    }

    void Update()
    {
        // The text will be updated dynamically during the animation
        UpdateText();
    }

    void UpdateText()
    {
        scoreText.text = "Score: " + playerData.score;
    }
}
```

---

## 📖 How it works

* `AnimaTween` takes the name of a **field** or **property** as a string and animates its value over time.
* Supports **float**, **int**, **Vector2**, **Vector3**, and other numerical types.
* Executes an optional callback (`onComplete`) when the animation finishes.

-----

## 📚 Documentation

### Functions

#### Main Animation Function

A função mais versátil, capaz de animar qualquer propriedade ou campo público de um objeto.

```csharp
Target.ATween(string propertyName, object toValue, float duration, 
           Easing easing = Easing.Linear, Action onComplete = null, 
           Playback playback = Playback.Forward, object fromValue = null)
```

#### Shortcut Function

Uma função pré-configurada para uma tarefa comum.

```csharp
// Anima a transparência (alpha) de componentes visuais comuns.
Component.AFade(float duration, Easing easing = Easing.Linear, 
             Action onComplete = null, float toAlpha = 0)
```

#### Timer Functions ⏰

Execute código após um atraso, com a opção de repetir.

```csharp
// Executa um callback após um atraso. Pode também ser usado para criar um intervalo repetido.
Target.ATimeout(float time, Action callback, bool repeat = false)
```

-----

### Parameters

#### `ATween`

Esta é a função principal do AnimaTween. Aqui estão os detalhes de seus parâmetros:

* **`target`**: O objeto que contém o campo ou propriedade a ser animada.

* **`propertyName`**: O nome do campo ou propriedade (como uma string).

* **`toValue`**: O valor de destino da animação. Este é um parâmetro versátil que aceita diferentes tipos de entrada.

  \<details\>
  \<summary\>\<strong\>▶️ Tipos de Valores Suportados\</strong\>\</summary\>

  Você pode animar qualquer campo ou propriedade pública dos seguintes tipos:

   - **`float`**: Para valores numéricos únicos (ex: `alpha` de um CanvasGroup, velocidade).
   - **`int`**: Para valores inteiros. A animação ocorrerá com arredondamento.
   - **`Vector2`**: Para posições 2D, escalas, etc.
   - **`Vector3`**: Para posições 3D, escalas, ângulos de Euler, etc.
   - **`Color`**: Para animar cores de materiais, imagens, sprites, etc.
   - **`Quaternion`**: Para rotações. Usa `Quaternion.Slerp` para uma interpolação suave.
   - **`string`**: Cria um efeito de máquina de escrever (typewriter).

  \</details\>

  \<details\>
  \<summary\>\<strong\>▶️ Animação de Caminho (Waypoints)\</strong\>\</summary\>

  Em vez de um único valor, você pode fornecer uma coleção (`List<T>` ou `T[]`) de qualquer tipo suportado para criar uma animação de caminho. A `duration` será distribuída igualmente entre os segmentos do caminho.

  **Exemplo:** Fazendo um objeto se mover através de três pontos.

  ```csharp
  var path = new Vector3[]
  {
      new Vector3(5, 0, 0),
      new Vector3(5, 5, 0),
      new Vector3(0, 5, 0)
  };

  // Levará 3 segundos para completar o caminho inteiro (1s por segmento).
  transform.ATween("position", path, 3f); 
  ```

  \</details\>

* **`duration`**: A duração da animação em segundos.

* **`easing`**: A curva de aceleração da animação (veja a seção **Easing Values**).

* **`onComplete`**: Um callback opcional que é executado quando a animação termina (não é chamado em loops).

* **`playback`**: O modo de reprodução da animação (veja a seção **Playback Modes**).

* **`fromValue`** (Opcional): Força a animação a começar deste valor em vez do valor atual da propriedade.

-----

### Control Functions 🎮

Controle o ciclo de vida de suas animações e timers depois que eles já foram iniciados.

| Função | Descrição | Estado Final | Executa Callback? |
|:---|:---|:---|:---|
| **`AComplete`** | **Completa** o tween, saltando para seu estado final. | Fim ou Início | **Sim** (padrão) |
| **`AStop`** | **Para** o tween imediatamente, congelando-o no lugar. | Valor Atual | **Não** |
| **`ACancel`** | **Cancela** o tween, revertendo para seu estado inicial. | Valor Inicial | **Não** |
| **`ACompleteTimer`** | **Completa** o timer, acionando seu callback. | - | **Sim** (padrão) |

\<details\>
\<summary\>\<strong\>▶️ Detalhes das Funções de Controle\</strong\>\</summary\>

#### **`AComplete`**

Completa um ou todos os tweens em um alvo, saltando para um estado final especificado e acionando seus callbacks.

```csharp
Target.AComplete(string propertyName = null, bool withCallback = true, EndState endState = EndState.End)
```

* `propertyName`: O tween específico a ser completado. Se `null`, completa **todos** os tweens no alvo.
* `withCallback`: Se `true` (padrão), o callback `onComplete` do tween será executado.
* `endState`: Determina para onde a propriedade salta. Use `EndState.End` (padrão) para saltar para o `toValue` ou `EndState.Start` para saltar para o `fromValue`.

#### **`AStop`**

Para um ou todos os tweens em um alvo, deixando-os em seu estado atual.

```csharp
Target.AStop(string propertyName = null)
```

* `propertyName`: O tween específico a ser parado. Se `null`, para **todos** os tweens no alvo.

#### **`ACancel`**

Cancela um ou todos os tweens em um alvo, revertendo-os para seu estado inicial.

```csharp
Target.ACancel(string propertyName = null)
```

* `propertyName`: O tween específico a ser cancelado. Se `null`, cancela **todos** os tweens no alvo.

#### **`ACompleteTimer`**

Completa um timer específico ou todos os timers em um alvo.

```csharp
Target.ACompleteTimer(int timerId = -1, bool withFinalCallback = true)
```

* `timerId`: O ID do timer a ser completado (retornado por `ATimeout`). Se `-1` (padrão), completa **todos** os timers no alvo.
* `withFinalCallback`: Se `true` (padrão), o callback do timer será executado.

\</details\>

-----

### Playback Modes

O parâmetro `playback` define como o `ATween` se comporta ao longo do tempo.

| Modo | Comportamento | Descrição |
|:---|:---|:---|
| `Forward` | 🏃 A → B | **(Padrão)** Anima do valor inicial para o final e para. |
| `Backward` | ◀️ B → A | Anima do valor final para o inicial e para. |
| `PingPong` | 🏓 A → B → A | Anima do início ao fim, depois volta para o início e para. |
| `LoopForward` | 🔁 A → B, A → B... | Repete a animação do início ao fim indefinidamente. |
| `LoopBackward` | 🔁 B → A, B → A... | Repete a animação do fim ao início indefinidamente. |
| `LoopPingPong` | 🔄 A → B → A, A → B → A... | Repete a animação "ida e volta" indefinidamente. Ideal para efeitos de pulsação. |

**Nota:** O callback `onComplete` **não** é chamado para os modos *Loop*, pois eles, por definição, nunca terminam.

-----

### Easing Values

Uma lista de todas as curvas de easing disponíveis.

\<details\>
\<summary\>\<strong\>▶️ Lista de Easing Values\</strong\>\</summary\>

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

\</details\>

Veja uma representação visual de cada curva de easing aqui:
➡️ **[Visual Easing Reference](https://easings.net/)** (um excelente recurso visual para entender cada curva)
-----

### 💻 Usage Examples


**1. Move a `Transform` to a new position:**

```csharp
public Transform playerTransform;

void Start() {
    Vector3 targetPosition = new Vector3(10, 0, 0);
    playerTransform.ATween("position", targetPosition, 2.0f, Easing.OutBack);
}
```

**2. Make a warning icon pulse using `LoopPingPong`:**

```csharp
public Transform warningIcon;

void Start() {
    // The icon will continuously scale up and down.
    warningIcon.ATween("localScale", Vector3.one * 1.2f, 0.7f, Easing.InOutSine, Playback.LoopPingPong);
}
```

**3. Fade out a UI Image and then disable it:**

```csharp
public Image myImage;

void HideImage() {
    myImage.AFade(Easing.OutQuad, () => {
        // This code will be executed when the fade completes.
        myImage.gameObject.SetActive(false);
    });
}
```

**4. Animate a score value and update the text upon completion:**

```csharp
public Text scoreText;
public int currentScore = 0;

void AddPoints(int pointsToAdd) {
    int newScore = currentScore + pointsToAdd;
    // Animate the "invisible" 'currentScore' value
    this.ATween(nameof(currentScore), newScore, 1.0f, Easing.OutCubic, onComplete: UpdateScoreText);
}

void UpdateScoreText() {
    scoreText.text = "Score: " + currentScore;
}
```

**5. Stop a specific animation:**

```csharp
// At some point, the player enters a cutscene and needs to stop moving.
playerTransform.AComplete("position");
```

**6. Simple Timeout**
Run a piece of code once after 2.5 seconds.

```csharp
void Start()
{
    // 'this' refers to the current MonoBehaviour instance.
    this.ATimeout(2.5f, () => 
    {
        Debug.Log("Timeout finished!");
    });
}
```

**7. Repeating Interval**
Spawn a prefab every second, indefinitely.

```csharp
public GameObject prefabToSpawn;

void Start()
{
    // Create an interval that fires every 1.0 second.
    this.ATimeout(1.0f, () =>
    {
        Instantiate(prefabToSpawn, Vector3.zero, Quaternion.identity);
        Debug.Log("Prefab spawned!");
    }, repeat: true);
}
```

**8. Stopping All Timers on an Object**
This example starts a repeating interval and then schedules a separate timeout to stop **all** timers on that object after 5 seconds.

```csharp
void Start()
{
    // Start the repeating interval.
    this.ATimeout(() =>
    {
        Instantiate(prefabToSpawn, Vector3.zero, Quaternion.identity);
    }, repeat: true);

    // Schedule a one-shot timeout to stop all timers on this object later.
    this.ATimeout(5.0f, () =>
    {
        Debug.Log("Stopping all repeating timers now.");
        this.ACompleteTimers(); // This stops every timer on the object.
    });
}
```

**9. Stopping a Specific Timer**
This example shows how to capture a timer's ID and stop only that specific timer, leaving other timers on the same object running.

```csharp
// Class variable to store the ID of the timer we want to control.
private int _blinkingTimerId;

void Start()
{
    // Start a repeating interval and STORE its returned ID.
    _blinkingTimerId = this.ATimeout(0.5f, () =>
    {
        Debug.Log("Blink effect is running...");
        // Code to make a sprite blink...
    }, repeat: true);

    // Start a second, different timer that will not be affected.
    this.ATimeout(1.0f, () => Debug.Log("Health regen tick..."), repeat: true);

    // After 4 seconds, stop ONLY the blinking timer using its stored ID.
    this.ATimeout(4.0f, () =>
    {
        Debug.Log($"Stopping only the blink effect (ID: {_blinkingTimerId}).");
        this.ACompleteTimer(_blinkingTimerId); // The other timer will keep running.
    });
}
```

## 📝 License

This project is licensed under the **MIT License** – free to use for any purpose, including commercial, with attribution.
See the [LICENSE](LICENSE) file for details.
