using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{

    //TODO make dash remove y velocity

    static Player s_player;

    [SerializeField] Transform _camera;

    [SerializeField] float
        _explosionDelay = 2.5f,
        _walkSpeed = 10f,
        _groundWalkMultiplier = 2f,
        _jumpSpeed = 10f,
        _dashSpeed = 20f,
        _dashDelay = 1f,
        _fallSpeed = 15f,
        _fallDuration = 1.3f,
        _jumpDuration = .15f,
        _jumpInputDuration = .2f,
        _jumpDelay = .5f,
        _dashDuration = 1f,
        _secondJumpDuration = 1f,
        _secondJumpInputDuration = 1f,
        _buffer = .3f,
        _coyotte = .3f,
        _cameraSpeed = 20f;

    [SerializeField] AnimationCurve
        _fallCurve,
        _walkCurve,
        _dashCurve;

    [SerializeField] AudioSource
        _beepSource,
        _jumpSource,
        _secondJumpSource,
        _deathSource,
        _explosionSource,
        _itemSource;

    [SerializeField] Animator _animator;

    LayerMask 
        _deathLayer,
        _peanutLayer,
        _winLayer;

    public static PlayerInput s_playerInput;
    Rigidbody2D _rb;

    Vector2 _startPosition = Vector2.zero;
    Vector2 _camStart = Vector2.zero;

    Vector2 _velocity = Vector2.zero;

    float _walk;
    float _walkT;
    
    float _jumpBuffer;
    float _coyotteTime;
    float _jumpT;
    float _jumpTime;
    float _curJumpDuration;
    bool _jumpHeld;
    bool _jumping;
    bool _secondJumping;
    bool _secondJumped;

    bool _dash;
    bool _dashing;
    float _dashT;
    bool _dashed;

    bool _grounded;
    float _fallT;
    ContactPoint2D[] _contacts;

    bool _unstable;
    float _unstableTime;

    SpriteRenderer _r;

    [SerializeField] Settings _settings;

    bool _dead;

    void Start() {
        s_player = this;

        s_playerInput = GetComponent<PlayerInput>();
        s_playerInput.ActivateInput();

        _rb = GetComponent<Rigidbody2D>();
        _startPosition = _rb.position;
        _camStart =_camera.position;

        _contacts = new ContactPoint2D[20];
        _deathLayer = LayerMask.NameToLayer("Death");
        _peanutLayer = LayerMask.NameToLayer("Peanut");
        _winLayer = LayerMask.NameToLayer("Win");

        _r = GetComponent<SpriteRenderer>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    public static void Restart() {
        s_player._rb.position = s_player._startPosition;
        s_player.transform.position = new Vector3(
            s_player._startPosition.x, s_player._startPosition.y, s_player.transform.position.z
            );
        s_player._camera.transform.position = new Vector3(
            s_player._camStart.x, s_player._camStart.y, s_player._camera.transform.position.z
        );

        s_player._rb.velocity = Vector2.zero;
        s_player._walk = 0;
        s_player._walkT = 0;
        s_player._jumpBuffer = 0;
        s_player._coyotteTime = 0;
        s_player._jumpT = 0;
        s_player._jumpTime = 0;
        s_player._jumping = false;
        s_player._secondJumping = false;
        s_player._secondJumped = false;
        s_player._dash = false;
        s_player._dashing = false;
        s_player._dashT = 0;
        s_player._dashed = false;
        s_player._unstable = false;
        s_player._dead = false;

        s_player._animator.SetBool("Dead", false);
        s_player._animator.SetBool("Walk", false);
        s_player._animator.SetBool("Jump", false);
        s_player._animator.SetBool("Fall", false);
    }

    void LateUpdate() {
        var _cam = _camera.position;
        _cam.x = Mathf.MoveTowards(_cam.x, transform.position.x, Time.deltaTime * _cameraSpeed);
        _cam.y = Mathf.MoveTowards(_cam.y, transform.position.y, Time.deltaTime * _cameraSpeed * .5f);
        _camera.position = _cam;
    }

    void FixedUpdate() {
        _velocity = Vector2.zero;
        
        if (!_dead) {
            var unstable = _unstable;

            Jump();
            Dash();
            Walk();
            Fall();

            _grounded = false;
            _unstableTime -= Time.deltaTime;

            if (_unstable) {
                if (!unstable) _unstableTime = _explosionDelay;
                if (_unstableTime <= 0) Explode();

                var color = new Color(
                    1f - Mathf.Clamp01(_unstableTime / _explosionDelay) * .5f,
                    1f,
                    Mathf.Clamp01(_unstableTime / _explosionDelay), 
                    1f
                );
                _r.color = color;
            }else{
                _r.color = Color.white;
            }
        }

        _rb.velocity = _velocity * Time.deltaTime;
    }

    
    void Jump() {
        if (_grounded) {
            _secondJumped = false;
            _coyotteTime = _coyotte;
        }

        _coyotteTime -= Time.deltaTime;
        _jumpTime -= Time.deltaTime;

        // First Jump
        if (_jumpBuffer > 0 && (_grounded || _coyotteTime > 0f)) {
            if (!_jumping) {
                _curJumpDuration = _jumpDuration;
                _unstable = true;
                _jumpSource.Play();
                _animator.SetBool("Jump", true);
            }

            _jumpBuffer = 0f;
            _jumping = true;
            _jumpTime = _jumpDelay;
        }else {
            _jumpBuffer -= Time.deltaTime;
        }

        if (_jumping) {
            if (_jumpHeld) {
                _curJumpDuration = Mathf.Clamp(
                    _curJumpDuration + Time.deltaTime, 0, _jumpDuration + _jumpInputDuration
                );
            }

            _jumpT = Mathf.Clamp01(_jumpT + Time.deltaTime / _curJumpDuration);

            var ended = _curJumpDuration + .0001f >= _jumpDuration + _jumpInputDuration;
            if(Mathf.Approximately(1f, _jumpT) && ((_jumpHeld && ended) || !_jumpHeld)) {
                _jumpT = 0;
                _jumping = false;
                _animator.SetBool("Jump", false);
            }

            _velocity.y += _jumpT * _jumpSpeed;
        }

        // Second Jump
        if (_jumpBuffer > 0 && !_grounded && _jumpTime <= 0f && !_secondJumped) {
            _secondJumping = true;
            _jumping = false;
            _jumpT = 0f;
            _secondJumped = true;
            _curJumpDuration = _secondJumpDuration;
            _unstable = true;
            _secondJumpSource.Play();
            _animator.SetBool("Jump", true);
        }

        if (_secondJumping) {
            if (_jumpHeld) {
                _curJumpDuration = Mathf.Clamp(
                    _curJumpDuration + Time.deltaTime, 0, _secondJumpDuration + _secondJumpInputDuration
                );
            }

            _jumpT = Mathf.Clamp01(_jumpT + Time.deltaTime / _curJumpDuration);

            var ended = _curJumpDuration + .0001f >= _secondJumpDuration + _secondJumpInputDuration;
            if(Mathf.Approximately(1f, _jumpT) && ((_jumpHeld && ended) || !_jumpHeld)) {
                _jumpT = 0;
                _secondJumping = false;
                _animator.SetBool("Jump", false);
            }

            _velocity.y += _jumpT * _jumpSpeed;
        }
    }
    
    void Dash() {
        if (_grounded) _dashed = false;

        if (_dash && !_dashed) {
            if (!_dashing) {
                _jumping = false;
                _secondJumping = false;
                _jumpT = 0;
                _unstable = true;
                _animator.SetTrigger("Dash");
            }
            _dashing = true;
        };
        
        _dash = false;
        if (!_dashing) return;

        _fallT = 0f;
        _dashed = true;

        _dashT = Mathf.Clamp01(_dashT + Time.deltaTime / _dashDuration);

        if(Mathf.Approximately(1f, _dashT)) {
            _dashT = 0;
            _dashing = false;
        }

        var dashMotion = _dashCurve.Evaluate(_dashT) * _dashSpeed * transform.localScale.x;
        _velocity.x += dashMotion;
    }
    
    void Walk() {
        _animator.SetBool("Walk", _walk != 0f);

        var speed = _walkSpeed;
        if (_grounded) speed *= _groundWalkMultiplier;
        _velocity.x += _walk * speed;
    }

    void Fall() {
        if (_jumping || _secondJumping) {
            _fallT = 0f;
        }else {
            _fallT = Mathf.Clamp01(_fallT + Time.deltaTime / _fallDuration);
        }

        _animator.SetBool("Fall", !_grounded && !_jumping && !_secondJumping && !_dashing);

        _velocity.y -= _fallSpeed * _fallCurve.Evaluate(_fallT);
    }

    IEnumerator Die() {
        //TODO Anim
        _dead = true;
        _deathSource.Play();
        _animator.SetBool("Dead", true);
        _animator.SetBool("Fall", false);
        _animator.SetBool("Walk", false);

        yield return new WaitForSeconds(1f);
        Level.Restart();
    }

    void Explode() {
        //TODO Anim
        _explosionSource.Play();
        Level.Restart();
    }

    void Win() {
        //TODO anim
        Level.Next();
    }

    void OnMove(InputValue value) {
        _walk = value.Get<float>();

        var s = transform.localScale;
        if (_walk < 0) transform.localScale = new Vector3(-1f, s.y, s.z);
        else if (_walk > 0) transform.localScale = new Vector3(1f, s.y, s.z);
    }

    void OnJump(InputValue value) {
        var _jump = value.Get<float>() == 1f;

        if (_jump) {
            if (_grounded || _coyotteTime > 0 || (!_grounded && _jumpTime <= 0f && !_secondJumped)) {
                _jumpHeld = true;
            }
            _jumpBuffer = _buffer;
        }else {
            _jumpHeld = false;
        }
    }

    void OnDash() {
        _dash = true;
    }

    void OnPause() {
        _settings.TogglePause();
    }

    void OnSkip() {
#if UNITY_EDITOR
        Level.Next();
#endif
    }

    void OnCollisionStay2D(Collision2D collision) {
        if ((LayerMask)collision.gameObject.layer == _deathLayer && !_dead) StartCoroutine("Die");

        var count = collision.GetContacts(_contacts);

        for (int i=0; i<count; i++) {
            if (Mathf.Approximately(1f, _contacts[i].normal.y)) {
                _grounded = true;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if ((LayerMask)collider.gameObject.layer == _peanutLayer) {
            Level.Disable(collider.gameObject);
            _unstable = false;
            _itemSource.Play();
        }else if ((LayerMask)collider.gameObject.layer == _winLayer) Win();
    }

}