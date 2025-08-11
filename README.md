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

---
Of course. Here is the updated documentation, now including the new `ATimeout` and `ACompleteTimers` functions, along with a new section for usage examples.

-----

## 📚 Documentation

### Functions

#### Main Animation Function

The most versatile function, capable of animating any public property or field of an object.

```csharp
Target.ATween(string propertyName, object toValue, float duration, 
           Easing easing = Easing.Linear, Action onComplete = null, 
           Playback playback = Playback.Forward, object fromValue = null)
```

#### Shortcut Function

A pre-configured function for a common task.

```csharp
// Animates the transparency (alpha) of common visual components.
Component.AFade(float duration, Easing easing = Easing.Linear, 
             Action onComplete = null, float toAlpha = 0)
```

#### Timer Functions ⏰

Execute code after a delay, with an option to repeat.

```csharp
// Executes a callback after a delay. Can also be used to create a repeating interval.
Target.ATimeout(float time, Action callback, bool repeat = false)
```

#### Control Functions

Allow you to stop animations and timers in progress.

```csharp
// Stops ALL active animations and timers on an object.
Target.AComplete()

// Stops a specific animation on an object, identified by its property name.
Target.AComplete(string propertyName)

// Stops only the active timers on an object, leaving animations untouched.
Target.ACompleteTimers()
```

-----

### Parameters

#### For `ATween`

| Parameter | Description |
| :--- | :--- |
| `Target` | The object containing the field or property to animate. |
| `propertyName` | The name of the field or property (as a string). |
| `toValue` | The target value of the animation. Can be a single value or a collection for a path. |
| `duration` | The duration of the animation in seconds. |
| `easing` | The animation's acceleration curve. |
| `onComplete` | An optional callback that executes when the animation finishes (not called on loops). |
| `playback` | The animation's playback mode (see the **Playback Modes** section). |
| `fromValue` | **(Optional)** Forces the animation to start from this value instead of the property's current value. |

#### For `AFade`

| Parameter | Description |
| :--- | :--- |
| `Target` | The visual component to fade (`Image`, `SpriteRenderer`, `CanvasGroup`). |
| `duration` | The duration of the fade in seconds. |
| `easing` | The animation's acceleration curve. |
| `onComplete` | An optional callback that executes when the fade is finished. |
| `toAlpha` | The target alpha (transparency) value. `0` is fully transparent, `1` is fully opaque. **Defaults to `0` (fade-out).** |

-----

### Playback Modes

The `playback` parameter defines how `ATween` behaves over time.

| Mode | Behavior | Description |
| :--- | :--- | :--- |
| `Forward` | 🏃 A → B | **(Default)** Animates from the start value to the end value and stops. |
| `Backward` | ◀️ B → A | Animates from the end value to the start value and stops. |
| `PingPong` | 🏓 A → B → A | Animates from start to end, then immediately animates back to the start, then stops. |
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
➡️ **[Visual Easing Reference](https://defold.com/manuals/property-animation/#easing/)** (an excellent visual resource for understanding each curve)

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
