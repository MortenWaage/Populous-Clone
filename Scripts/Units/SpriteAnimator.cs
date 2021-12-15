using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpriteAnimator : MonoBehaviour
{
    private MeshRenderer _rend;
    [SerializeField] private Texture[] _unitSprites;
    [SerializeField] private float _animationSpeed = 5;
    [SerializeField] private float _animationFrequency = 1;
    [SerializeField] private int _timesToLoopBeforePause = 3;

    private bool _isPlaying;
    private float _animationCurrentTime;
    private int _numberOfFrames;
    private int _currentIndex;
    private int _currentLoopAmount;

    private float _remainingWait;

    void Start()
    {
        _rend = GetComponent<MeshRenderer>();
        _numberOfFrames = _unitSprites.Length;
    }
    
    void Update()
    {
        switch (_isPlaying)
        {
            case true:  Play(); break;
            case false: Wait(); break;
        }

        void Play()
        {
            _animationCurrentTime = (_animationCurrentTime + _animationSpeed * Time.deltaTime) % _numberOfFrames;
            if (_currentIndex == (int) _animationCurrentTime) return;

            _currentIndex = (_currentIndex + 1) % _numberOfFrames;

            if (_currentIndex == 0) _currentLoopAmount++;
            if (_currentLoopAmount == _timesToLoopBeforePause)
            {
                _isPlaying = false;
                _currentLoopAmount = 0;
            }
            _rend.material.SetTexture("_MainTex", _unitSprites[_currentIndex]);
        }

        void Wait()
        {
            _remainingWait -= 1 * Time.deltaTime;

            if (_remainingWait > 0) return;

            _isPlaying = true;
            var randomPause = Random.Range(0, 0.2f);
            _remainingWait = _animationFrequency + randomPause;
        }
    }
}