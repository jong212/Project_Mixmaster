using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DemoPlayer : MonoBehaviour
{
    public Button PlayButton;
    public Button NextButton;
    public Button PrevButton;
    public Text PlayButtonText;

    private static readonly int Vertical = Animator.StringToHash("Vertical");
    private static readonly int Horizontal = Animator.StringToHash("Horizontal");
    private static readonly float LoopDuration = 2f;

    private Coroutine _loopDelayRoutine;
    private Vector2 _playerRotation = Vector2.zero;
    private Animator _animator;
    private AnimationClip[] _animationClips;
    private int _clipIndex;
    private string _enabledParam = String.Empty;
    private bool _initialized;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayButton == null || NextButton == null || PrevButton == null || PlayButtonText == null)
        {
            Debug.LogError("[DemoPlayer.cs] Some UI elements references are missing. Check references in scene");
            return;
        }

        _animator = FindObjectOfType<Animator>();
        if (_animator == null)
        {
            Debug.LogError("[DemoPlayer.cs] Can't find 'Animator' component in scene." +
                           " Add character prefab with 'Animator' component attached to it in the scene");
            return;
        }
        
        LoadAnimations();
        UpdateAnimationName();

        PlayButton.onClick.AddListener(PlayClicked);
        NextButton.onClick.AddListener(NextClicked);
        PrevButton.onClick.AddListener(PrevClicked);

        _initialized = true;
    }

    private void OnEnable()
    {
        ResetRotation();
    }

    // Update is called once per frame
    void Update()
    {
        if(!_initialized)
            return;
        
        ResetRotation();
        
        if (Input.GetKey(KeyCode.W))
            _playerRotation.y = 1;
        if (Input.GetKey(KeyCode.A))
            _playerRotation.x = -1;
        if (Input.GetKey(KeyCode.S))
            _playerRotation.y = -1;
        if (Input.GetKey(KeyCode.D))
            _playerRotation.x = 1;

        if (Mathf.Approximately(_playerRotation.x, 0)
            && Mathf.Approximately(_playerRotation.y, 0))
        {
            _playerRotation.y = -1;
        }

        UpdateParams();
    }

    private void PlayClicked()
    {
        if(_clipIndex < 0 || _clipIndex > _animationClips.Length)
            return;

        ClearAnimatorParameter();
        
        var clip = _animationClips[_clipIndex];
        _animator.Play(clip.name);

        EnableParamIfExist(clip.name.Replace("Start", ""));
    }

    private void NextClicked()
    {
        _clipIndex++;
        if (_clipIndex >= _animationClips.Length)
            _clipIndex = 0;
        
        UpdateAnimationName();
    }

    private void PrevClicked()
    {
        _clipIndex--;
        if (_clipIndex < 0)
            _clipIndex = _animationClips.Length - 1;
        
        UpdateAnimationName();
    }

    private void UpdateAnimationName()
    {
        var clipName = _animationClips[_clipIndex].name;
        PlayButtonText.text = clipName;
    }

    private void EnableParamIfExist(string clipName)
    {
        var animatorParams = _animator.parameters;
        var parameter = animatorParams.FirstOrDefault(paramName => paramName.name.Contains(clipName));
        if (parameter != null && parameter.type == AnimatorControllerParameterType.Bool)
        {
            _animator.SetBool(parameter.name, true);
            _enabledParam = parameter.name;
            _loopDelayRoutine = StartCoroutine(DelayOffParameter(LoopDuration));
        }
    }

    private IEnumerator DelayOffParameter(float delay)
    {
        yield return new WaitForSeconds(delay);
        ClearAnimatorParameter();
    }

    private void ClearAnimatorParameter()
    {
        if (_loopDelayRoutine != null)
        {
            StopCoroutine(_loopDelayRoutine);
            _loopDelayRoutine = null;
        }
        
        if(string.IsNullOrWhiteSpace(_enabledParam))
            return;
        
        _animator.SetBool(_enabledParam, false);
        _enabledParam = String.Empty;
    }

    private void LoadAnimations()
    {
        var clips = _animator.runtimeAnimatorController.animationClips;

        _animationClips = clips.GroupBy(clip => clip.name)
            .Select(item => item.First())
            .Where(clip => !clip.name.Contains("Loop") && !clip.name.Contains("End")).ToArray();
    }

    private void ResetRotation()
    {
        _playerRotation.x = 0;
        _playerRotation.y = 0;
    }

    private void UpdateParams()
    {
        _animator.SetFloat(Horizontal, _playerRotation.x);
        _animator.SetFloat(Vertical, _playerRotation.y);
    }
}
