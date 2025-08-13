using UnityEngine;
using AnimaTween;
using UnityEngine.UI;


namespace AnimaTween
{
    public class ExampleAnimaTween : MonoBehaviour
    {
        [Header("UI Elements")] [SerializeField]
        private Image image;

        [SerializeField] private Text textArea1;
        [SerializeField] private Text textArea2;

        [Header("Scene Objects")] [SerializeField]
        private Transform cube;

        [SerializeField] private Rigidbody fisicalCube;
        [SerializeField] private MeshRenderer meshCube;
        [SerializeField] private AudioSource music;
        [SerializeField] private Camera mainCamera;

        // Auxiliary variables
        private Color originalCubeColor;

        void Start()
        {
            // --- INITIAL SETUP ---
            // Ensure initial states are correct before starting the sequence.
            image.gameObject.SetActive(false); // Image starts inactive.
            textArea1.text = "";
            textArea2.text = "";
            music.Pause(); // Music is ready but paused.
            originalCubeColor = meshCube.material.color; // Store the cube's original color.

            // --- START OF THE ANIMATION SEQUENCE ---

            // 1. Write "This is AnimaTween" to textArea1 and fade it in.
            textArea1.text = "This is AnimaTween";
            textArea1.ATween("color", textArea1.color, 2f, onComplete: onCompleteText,
                fromValue: new Color(textArea1.color.r, textArea1.color.g, textArea2.color.b, 0f));
            // 2. When text appears, start the cube rotating on the Y-axis indefinitely.
            cube.ATween("eulerAngles", cube.eulerAngles + new Vector3(0, 360, 0), 5f, Easing.Linear,
                playback: Playback.LoopForward);
        }

        void onCompleteText()
        {
            // 3. Fade in textArea2 with "Tweens control data,"
            textArea2.text = "Tweens control data";
            textArea2.ATween("color", textArea2.color, 2f,
                fromValue: new Color(textArea2.color.r, textArea2.color.g, textArea2.color.b, 0f));

            // 4. Fade in the red square image.
            image.gameObject.SetActive(true);
            image.ATween("color", textArea2.color, 2f,
                fromValue: new Color(image.color.r, image.color.g, image.color.b, 0f));
            ;

            // 5. After a short delay, start fading out the first text area.
            this.ATimeout(3.0f, () =>
            {
                textArea1.AFade(2.5f, onComplete: () => textArea1.text = "");

                // 6. At the same time, complete the text in the second text area.
                float textTweenDuration = 3.0f;
                textArea2.ATween("text", "Tweens control data and data controls your game...", textTweenDuration,
                    Easing.Linear);

                // 7. Halfway through the text tween, start the music and the color change chain.
                this.ATimeout(textTweenDuration / 2f, () =>
                {
                    // 7A. Start music with a fade-in.
                    music.volume = 0f;
                    music.Play();
                    music.ATween("volume", 0.8f, 2.0f);

                    // 7B. Start the color change chain on the image.
                    StartImageColorChain();
                });
            });
        }

        void StartImageColorChain()
        {
            // Chain: Red -> White
            image.ATween("color", Color.white, 1.5f, Easing.InOutSine, onComplete: () =>
            {
                // 8. When the image turns from white to green, start the cube's color chain.
                StartCubeColorChain();

                // Chain: White -> Green
                image.ATween("color", Color.green, 1.5f, Easing.InOutSine, onComplete: () =>
                {
                    // Chain: Green -> Blue
                    image.ATween("color", Color.blue, 1.5f, Easing.InOutSine, onComplete: AfterCollorChain);
                });
            });
        }

        void StartCubeColorChain()
        {
            meshCube.material.ATween("color", Color.white, 1.0f, onComplete: () =>
            {
                meshCube.material.ATween("color", Color.green, 1.0f, onComplete: () =>
                {
                    meshCube.material.ATween("color", Color.blue, 1.0f, onComplete: () =>
                    {
                        meshCube.material.ATween("color", Color.yellow, 1.5f, onComplete: () =>
                        {
                            // Return to original color at the end.
                            meshCube.material.ATween("color", originalCubeColor, 1.0f);
                        });
                    });
                });
            });
        }

        void AfterCollorChain()
        {

            image.ATween("color", Color.yellow, 1.5f, Easing.InOutSine, onComplete: () =>
            {
                image.AFade(1.5f);
                textArea2.AFade(1.5f);
                // 10. Write "Tweens can also control audio" to the first text area.
                textArea1.AFade(0.1f, toAlpha: 1f); // Make it visible instantly before writing
                textArea1.ATween("text", "Tweens can also control audio", 2.5f, Easing.Linear);
                // 11. After part of text is written, change the music pitch.
                this.ATimeout(1.5f, MusicalChange);
            });
        }

        void MusicalChange()
        {
            music.ATween("pitch", 1.2f, 3.0f,
                Easing.OutInElastic, fromValue: 0.8, onComplete: () =>
                {
                    // 12. Return pitch to normal and lower the volume slightly.
                    music.ATween("pitch", 1.0f, 0.5f);
                    music.ATween("volume", 0.6f, 1.0f, onComplete: () =>
                    {
                        // 13. Erase textArea1.
                        textArea1.ATween("text", "", 2f, Easing.Linear);

                        // 14. Simultaneously, write "Tweens can also control physics" to textArea2.
                        textArea2.text = "";
                        textArea2.AFade(0.1f, toAlpha: 1f); // Make it visible instantly
                        textArea2.ATween("text", "Tweens can also control physics", 2.5f, Easing.Linear,
                            onComplete: () =>
                            {
                                // 15 & 16. Stop rotation, enable physics, and start physics-based effects.
                                StartPhysicsSequence();
                            });
                    });
                });
        }

        void StartPhysicsSequence()
        {
            // 15. Stop the kinematic rotation.
            cube.AComplete("eulerAngles");
            fisicalCube.isKinematic = false;

            // Use a repeating timer to apply torque.
            this.ATimeout(0.1f, () => fisicalCube.AddTorque(Random.onUnitSphere * 5f, ForceMode.Impulse), repeat: true);

            // 16. Contort the cube's scale while it rotates.
            cube.ATween("localScale", new Vector3(0.5f, 1.5f, 0.8f), 2f, Easing.InOutSine,
                () =>
                {
                    // Reset cube state
                    fisicalCube.isKinematic = true;
                    cube.ATween("localScale", Vector3.one, 0.5f);
                    cube.ATween("rotation", Quaternion.identity, 0.5f);
                    var finalPath = new Vector3[]
                    {
                        new Vector3(-2, 2, -4),
                        new Vector3(2, 2, -4),
                        new Vector3(2, -2, -4),
                        new Vector3(-2, -2, -4),
                        cube.position // Retorna à posição inicial no final
                    };

                    // Inicia a animação do caminho
                    cube.ATween("position", finalPath, 4.0f, Easing.InOutSine);
                    // Ao mesmo tempo, inicia uma rotação em loop infinito
                    cube.ATween("eulerAngles", cube.eulerAngles + new Vector3(0, 360, 0), 5f, Easing.Linear,
                        playback: Playback.LoopForward);
                });
            
            
            textArea1.ATween("text", "Control Tweens...", 2.0f, Easing.Linear);
            // 17. After a few seconds, stop the physics and write the final message.
            this.ATimeout(3.0f, () =>
            {
                textArea2.AFade(1f);
                textArea1.ATween("text", "Control Tweens... Control the GAME!", 2.0f, Easing.Linear);
                // Stop physics timers and scale animation.
                this.ACompleteTimer();



                // 18 & 19. Final sequence: fade out music and zoom camera.
                music.ATween("volume", 0f, 4.0f, onComplete: () => music.Stop());
                mainCamera.ATween("fieldOfView", 1f, 4.0f, Easing.InCubic);
            });
        }
    }
}