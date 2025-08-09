# AnimaTween

**AnimaTween** – The animation tween you see in any game engine.
A simple and flexible way to animate fields and properties of your objects directly in Unity.

---

## 📦 Installation

1. Open Unity and go to **Window → Package Manager**.
2. Click the **+** button → **Add package from git URL...**
3. Paste the repository URL:

   ```
   https://github.com/SEU_USUARIO/AnimaTween.git
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
        playerData.AnimaTween("score", 1000, 3.0f, onComplete: UpdateText);
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

---
## 📚 Documentation

### Functions

#### Main Function

The most versatile function, capable of animating any public property or field of an object.

```csharp
Target.AnimaTween(string propertyName, object toValue, float duration,
           Easing easing = Easing.Linear, Playback playback = Playback.Forward, Action onComplete = null)
```

#### Shortcut Function

Pre-configured functions for common tasks.

```csharp
// Animates the transparency (alpha) of common visual components.
Component.AnimaFade(float toAlpha, float duration, Easing easing = Easing.Linear, Action onComplete = null)
```

#### Control Functions

Allow you to stop animations in progress.

```csharp
// Stops all active animations on an object.
Target.Complete()

// Stops a specific animation on an object, identified by its property name.
Target.Complete(string propertyName)
```

-----

### Parameters

| Parameter | Description |
| :--- | :--- |
| `Target` | The object containing the field or property to animate. |
| `propertyName` | The name of the field or property (as a string). |
| `toValue` | The target value of the animation. Can be a single value or a collection (e.g., `List<Vector3>`) to create a path. |
| `duration` | The duration of the animation in seconds. |
| `easing` | The animation's acceleration curve (see the list below). |
| `playback` | The animation's playback mode (see the **Playback Modes** section). |
| `onComplete` | An optional callback function that executes when the animation finishes (not called on loops). |

-----

### Playback Modes

The `playback` parameter defines how the animation behaves over time.

| Mode | Behavior | Description |
| :--- | :--- | :--- |
| `Forward` | 🏃 A → B | **(Default)** Animates from the start value to the end value and stops. |
| `Backward` | ◀️ B → A | Animates from the end value to the start value and stops. |
| `PingPong` | 🏓 A → B → A | Animates from start to end, and then immediately animates back to the start, then stops. |
| `LoopForward` | 🔁 A → B, A → B... | Repeats the animation from start to end indefinitely. |
| `LoopBackward` | 🔁 B → A, B → A... | Repeats the animation from end to start indefinitely. |
| `LoopPingPong` | 🔄 A → B → A, A → B → A... | Repeats the "forward and back" animation indefinitely. Ideal for pulsing effects. |

**Note:** The `onComplete` callback is **not** called for *Loop* modes, as they, by definition, never finish.

-----

### Easing Values

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

See a visual representation of each easing curve here:
➡️ **[Visual Easing Reference](https://easings.net/)** (an excellent visual resource for understanding each curve)

-----

### Usage Examples

**1. Move a `Transform` to a new position:**

```csharp
public Transform playerTransform;

void Start() {
    Vector3 targetPosition = new Vector3(10, 0, 0);
    playerTransform.AnimaTween("position", targetPosition, 2.0f, Easing.OutBack);
}
```

**2. Make a warning icon pulse using `LoopPingPong`:**

```csharp
public Transform warningIcon;

void Start() {
    // The icon will continuously scale up and down.
    warningIcon.AnimaTween("localScale", Vector3.one * 1.2f, 0.7f, Easing.InOutSine, Playback.LoopPingPong);
}
```

**3. Fade out a UI Image and then disable it:**

```csharp
public Image myImage;

void HideImage() {
    myImage.AnimaFade(0f, 1.5f, Easing.OutQuad, () => {
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
    this.AnimaTween(nameof(currentScore), newScore, 1.0f, Easing.OutCubic, onComplete: UpdateScoreText);
}

void UpdateScoreText() {
    scoreText.text = "Score: " + currentScore;
}
```

**5. Stop a specific animation:**

```csharp
// At some point, the player enters a cutscene and needs to stop moving.
playerTransform.Complete("position");
```

## 📝 License

This project is licensed under the **MIT License** – free to use for any purpose, including commercial, with attribution.
See the [LICENSE](LICENSE) file for details.
