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

**Functions:**

```csharp
Target.AnimaTween(string propertyName, object toValue, float duration,
           Easing easing = Easing.Linear, Playback playback = Playback.Forward, Action onComplete = null)
````

**Parameters:**

| Parameter      | Description                                                                                                |
|----------------| ---------------------------------------------------------------------------------------------------------- |
| `Target`       | The object containing the field or property to animate.                                                    |
| `propertyName` | The name of the field or property (string).                                                                |
| `toValue`      | The target value of the animation.                                                                         |
| `duration`     | Duration of the animation in seconds.                                                                      |
| `easing`       | Animation easing function (see [Easing Reference](https://defold.com/manuals/property-animation/#easing)). |
| `playback`     | Playback mode (Forward, Reverse, PingPong, etc.).                                                          |
| `onComplete`   | Optional callback executed when the animation finishes.                                                    |

**Easing values:**

```
AnimaTween.InBack, AnimaTween.InBounce, AnimaTween.InCirc, AnimaTween.InCubic, AnimaTween.InElastic, AnimaTween.InExpo,
AnimaTween.InOutBack, AnimaTween.InOutBounce, AnimaTween.InOutCirc, AnimaTween.InOutCubic, AnimaTween.InOutElastic, AnimaTween.InOutExpo,
AnimaTween.InOutQuad, AnimaTween.InOutQuart, AnimaTween.InOutQuint, AnimaTween.InOutSine,
AnimaTween.InQuad, AnimaTween.InQuart, AnimaTween.InQuint, AnimaTween.InSine,
AnimaTween.Linear,
AnimaTween.OutBack, AnimaTween.OutBounce, AnimaTween.OutCirc, AnimaTween.OutCubic, AnimaTween.OutElastic, AnimaTween.OutExpo,
AnimaTween.OutInBack, AnimaTween.OutInBounce, AnimaTween.OutInCirc, AnimaTween.OutInCubic, AnimaTween.OutInElastic, AnimaTween.OutInExpo,
AnimaTween.OutInQuad, AnimaTween.OutInQuart, AnimaTween.OutInQuint, AnimaTween.OutInSine,
AnimaTween.OutQuad, AnimaTween.OutQuart, AnimaTween.OutQuint, AnimaTween.OutSine
```

See visual representation of each easing curve here:
➡️ **[Defold Easing Reference](https://defold.com/manuals/property-animation/#easing)**

**Example:**

```csharp
playerData.AnimaTween("score", 1000, 3.0f, 
                      easing: Easing.InOutCubic, 
                      playback: Playback.Forward, 
                      onComplete: UpdateText);
```


---

## 📝 License

This project is licensed under the **MIT License** – free to use for any purpose, including commercial, with attribution.
See the [LICENSE](LICENSE) file for details.
