using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GabrielBissonnette.SAD
{
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Sequence Manager")]
        [Tooltip("Choose a type of intro.")] public Intro introState;
        public enum Intro { OneLiner_FadingMenu, FadingMenu, MenuOnly };

        [Header("Buttons")]
        [Space(10)] [Tooltip("Enable to change the texts manually.")] public bool manualModeButtons;
        [Tooltip("Choose a type of intro.")] public Buttons buttonsAppearance;
        public enum Buttons { Rounded, Rounded_Outlined, Rounded_AlwaysFilled, Squared, Squared_Outlined, Squared_AlwaysFilled };

        [Header("Colors")]
        [Space(10)] [Tooltip("Enable to change the colors manually.")] public bool manualModeColor;
        public Color32 mainColor;
        public float alpha_godrays = 0.13f;
        public float alpha_particleSlowNormal = 0.5f;
        public float alpha_particleHuge = 0.05f;

        [Header("Intro Sequence")]
        [Space(10)] [Tooltip("Enable to change the text manually.")] public bool manualModeIntroText;
        [SerializeField] string introTextContent = "It is never too late to be who you might have been.";

        [Header("Scene")]
        [Space(10)] [SerializeField] string sceneToLoad;
        [SerializeField] float delayBeforeLoading = 3f;

        [Header("Home Panel")]
        [Space(10)] [Tooltip("Enable to change the texts manually.")] public bool manualModeTexts;
        [SerializeField] string play = "Play";
        [SerializeField] string settings = "Options";
        [SerializeField] string quit = "Quit";

        [Header("Audio")]
        [SerializeField] bool customSoundtrack;
        [SerializeField] AudioClip customSoundtrackAudio;
        [SerializeField] float defaultVolume = 0.7f;
        [SerializeField] AudioClip sound_click;
        [SerializeField] AudioClip sound_hover;
        [SerializeField] AudioClip sound_loadScene;

        // Refs
        [Header("---- References")]
        [Space(50)] public Animator main_animator;
        [SerializeField] Image background_sprite;
        [Space(10)] [SerializeField] TextMeshProUGUI introText;
        public CanvasGroup homePanel;
        [SerializeField] TextMeshProUGUI homePanel_text_play;
        [SerializeField] TextMeshProUGUI homePanel_text_options;
        [SerializeField] TextMeshProUGUI homePanel_text_quit;

        [Space(10)] [SerializeField] Sprite buttonRounded;
        [SerializeField] Sprite buttonRoundedOutlined;
        [SerializeField] Sprite buttonSquared;
        [SerializeField] Sprite buttonSquaredOutlined;

        [Space(10)] [SerializeField] ParticleSystem[] particles;
        [SerializeField] Image[] godrays_sprite;
        [SerializeField] Image[] buttons;
        [SerializeField] Animator[] buttonsAnimators;
        [SerializeField] RuntimeAnimatorController buttonsAnimator_darkText;
        [SerializeField] RuntimeAnimatorController buttonsAnimator_lightText;
        [SerializeField] RuntimeAnimatorController buttonsAnimator_alwaysFilled;

        [Space(10)] [SerializeField] AudioSource audioSource;
        [SerializeField] AudioSource audioSourceSountrack;
        [SerializeField] AudioClip demo_soundtrack;
        [SerializeField] AudioClip demo_soundtrack_shorter;
        [SerializeField] Slider volumeSlider;

        private void Awake()
        {
            IntroSequence();
        }

        void Start()
        {
            if (!manualModeTexts)
                UpdateTexts();

            if (!manualModeButtons)
                UpdateButtons();

            SetStartVolume();
        }

        #region Levels
        public void LoadLevel()
        {
            // Fade Animation
            main_animator.enabled = true;
            main_animator.SetTrigger("LoadScene");

            StartCoroutine(WaitToLoadLevel());
        }

        IEnumerator WaitToLoadLevel()
        {
            yield return new WaitForSeconds(delayBeforeLoading);

            // Scene Load
            SceneManager.LoadScene(sceneToLoad);
        }

        public void Quit()
        {
            Application.Quit();
        }
        #endregion

        #region Audio

        public void SetVolume(float _volume)
        {
            // Adjust volume
            AudioListener.volume = _volume;

            // Save volume
            PlayerPrefs.SetFloat("Volume", _volume);
        }

        void SetStartVolume()
        {
            if (!PlayerPrefs.HasKey("Volume"))
            {
                PlayerPrefs.SetFloat("Volume", defaultVolume);
                LoadVolume();
            }
            else
            {
                LoadVolume();
            }
        }

        public void LoadVolume()
        {
            volumeSlider.value = PlayerPrefs.GetFloat("Volume");
        }

        public void UIClick()
        {
            audioSource.PlayOneShot(sound_click);
        }

        public void UIHover()
        {
            audioSource.PlayOneShot(sound_hover);
        }

        public void UISpecial()
        {
            audioSource.PlayOneShot(sound_loadScene);
        }

        #endregion

        void IntroSequence()
        {
            switch (introState)
            {
                case Intro.OneLiner_FadingMenu:

                    if (!manualModeTexts && introText != null)
                    {
                        introText.text = introTextContent;
                    }

                    // Animator
                    main_animator.SetTrigger("OneLiner_FadingMenu");

                    // Soundtracks
                    if(customSoundtrack)
                    {
                        audioSourceSountrack.clip = customSoundtrackAudio;
                        audioSourceSountrack.Play();
                    }
                    else
                    {
                        audioSourceSountrack.clip = demo_soundtrack;
                        audioSourceSountrack.Play();
                    }

                    break;
                case Intro.FadingMenu:

                    // Animator
                    main_animator.SetTrigger("FadingMenu");

                    // Soundtracks
                    if (customSoundtrack)
                    {
                        audioSourceSountrack.clip = customSoundtrackAudio;
                        audioSourceSountrack.Play();
                    }
                    else
                    {
                        audioSourceSountrack.clip = demo_soundtrack_shorter;
                        audioSourceSountrack.Play();
                    }

                    break;
                case Intro.MenuOnly:

                    // Animator
                    main_animator.SetTrigger("MenuOnly");

                    // Soundtracks
                    if (customSoundtrack)
                    {
                        audioSourceSountrack.clip = customSoundtrackAudio;
                        audioSourceSountrack.Play();
                    }
                    else
                    {
                        audioSourceSountrack.clip = demo_soundtrack_shorter;
                        audioSourceSountrack.Play();
                    }

                    break;
                default:

                    // Animator
                    main_animator.SetTrigger("OneLiner_FadingMenu");

                    // Soundtracks
                    if (customSoundtrack)
                    {
                        audioSourceSountrack.clip = customSoundtrackAudio;
                        audioSourceSountrack.Play();
                    }
                    else
                    {
                        audioSourceSountrack.clip = demo_soundtrack;
                        audioSourceSountrack.Play();
                    }
                    break;
            }
        }

        void UpdateButtons()
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttonsAppearance == Buttons.Rounded)
                {
                    buttons[i].sprite = buttonRounded;
                    buttonsAnimators[i].runtimeAnimatorController = buttonsAnimator_darkText;
                }
                else if (buttonsAppearance == Buttons.Squared)
                {
                    buttons[i].sprite = buttonSquared;
                    buttonsAnimators[i].runtimeAnimatorController = buttonsAnimator_darkText;
                }
                else if (buttonsAppearance == Buttons.Squared_Outlined)
                {
                    buttons[i].sprite = buttonSquaredOutlined;
                    buttonsAnimators[i].runtimeAnimatorController = buttonsAnimator_lightText;
                }
                else if (buttonsAppearance == Buttons.Rounded_Outlined)
                {
                    buttons[i].sprite = buttonRoundedOutlined;
                    buttonsAnimators[i].runtimeAnimatorController = buttonsAnimator_lightText;
                }
                else if (buttonsAppearance == Buttons.Rounded_AlwaysFilled)
                {
                    buttons[i].sprite = buttonRounded;
                    buttonsAnimators[i].runtimeAnimatorController = buttonsAnimator_alwaysFilled;
                }
                else if (buttonsAppearance == Buttons.Squared_AlwaysFilled)
                {
                    buttons[i].sprite = buttonSquared;
                    buttonsAnimators[i].runtimeAnimatorController = buttonsAnimator_alwaysFilled;
                }
            }
        }

        void UpdateTexts()
        {
            if (homePanel_text_play != null)
                homePanel_text_play.text = play;

            if (homePanel_text_options != null)
                homePanel_text_options.text = settings;

            if (homePanel_text_quit != null)
                homePanel_text_quit.text = quit;
        }

        // Updates every time the inspector script is refreshed
        public void UIEditorUpdate()
        {
            #region Colors
            if (!manualModeColor)
            {
                // Apply the color to the background
                background_sprite.color = mainColor;

                // Get the main color with a lower alpha value
                Color newColor_godrays = mainColor;
                newColor_godrays.a = alpha_godrays;

                // Godrays
                if (godrays_sprite.Length > 0)
                {
                    // Apply the new color to the godrays
                    for (int i = 0; i < godrays_sprite.Length; i++)
                    {
                        godrays_sprite[i].color = newColor_godrays;
                    }
                }

                // Get the main color with a lower alpha value for Slow and Normal Particles
                Color newColor_slowNormal = mainColor;
                newColor_slowNormal.a = alpha_particleSlowNormal;

                // Get the main color with a lower alpha value for Huge Particles
                Color newColor_huge = mainColor;
                newColor_huge.a = alpha_particleHuge;

                // Apply to Particles
                if (particles.Length > 0)
                {
                    // Apply the new color to the godrays
                    for (int i = 0; i < particles.Length; i++)
                    {
                        if(i != 2)
                        {
                            var main1 = particles[i].main;
                            main1.startColor = new ParticleSystem.MinMaxGradient(newColor_slowNormal);
                        }
                        else
                        {
                            var main1 = particles[i].main;
                            main1.startColor = new ParticleSystem.MinMaxGradient(newColor_huge);
                        }
                    }
                }
            }
            #endregion

            if (!manualModeTexts)
                UpdateTexts();

            if (!manualModeButtons)
                UpdateButtons();

            #region Particles
            if (particles.Length > 0)
            {
                // Prewarm the particles
                for (int i = 0; i < particles.Length; i++)
                {
                    if(introState == Intro.MenuOnly)
                    {
                        var main1 = particles[i].main;
                        main1.prewarm = true;
                    }
                    else
                    {
                        if (i == 0)
                        {
                            var main1 = particles[i].main;
                            main1.prewarm = true;
                        }
                        else if (i > 0)
                        {
                            var main1 = particles[i].main;
                            main1.prewarm = false;
                        }
                    }
                }
            }
            #endregion
        }

        public void _fadingAnimationIsDone()
        {
            main_animator.enabled = false;
            homePanel.blocksRaycasts = true;
        }
    }
}
