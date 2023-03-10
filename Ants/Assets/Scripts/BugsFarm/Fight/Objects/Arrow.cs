using BugsFarm.Views.Hack;
using UnityEngine;
using Random = UnityEngine.Random;


public class Arrow : MonoBehaviour
{
    [SerializeField] SpriteRenderer _sr;

    Vector2 HitPos => (_targetUnit?.MB_Unit.HitPos ?? _targetTrEq ?? Vector2.zero) + _miss;
    Vector2 Delta_line => HitPos - _pos_line;
    Vector2 Dir_line => Delta_line.normalized;
    float Dist_line => Delta_line.magnitude;
    float TimeRest => Dist_line / _speed_line;


    Unit _targetUnit;
    Vector2? _targetTrEq;
    bool _isTargetReached;
    Vector2 _miss;

    Vector2 _pos_line;
    float _speed_line;

    float _timeFull;
    float _gravity;
    float _vy0;
    float _adj;

    Timer _timerFade = new Timer(TimerType.Scaled);

    float _damage;

    public void Init(Vector2 posStart, Unit target, float damage)
    {
        _targetUnit = target;
        _damage = damage;
        Init(posStart, 1, new Vector2(.25f, .125f));
    }


    public void Init(Vector2 posStart, Vector2 target)
    {
        _targetTrEq = target;
        Init(posStart, .5f, new Vector2(.125f, .25f));
    }


    void Init(Vector2 posStart, float h_mul, Vector2 miss)
    {
        _miss = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * miss;

        _pos_line = posStart;

        _gravity = HackRefsView.Instance.BattleSettings.arrowsGravity;

        float a = 4;
        float b = (-4) * (Delta_line.y * _gravity * Dist_line +
                          Mathf.Pow(HackRefsView.Instance.BattleSettings.arrowsSpeed, 2));
        float c = Mathf.Pow(_gravity * Dist_line, 2);
        float y = Solve(a, b, c);

        _speed_line = Mathf.Sqrt(y);
        _timeFull = Dist_line / _speed_line;
        _vy0 = _gravity * _timeFull / (-2);


        Vector2 dir = Dir(_vy0);
        float length = Tools.Size(_sr).x;
        _adj = length * dir.y;


        SetPosAndRotation(0, 1, dir);
    }


    float Solve(float a, float b, float c)
    {
        float D = b * b - 4 * a * c;
        float rD = D < 0 ? 0 : Mathf.Sqrt(D);

        float x1 = (-b - rD) / (2 * a);
        float x2 = (-b + rD) / (2 * a);

        return
            x1 <= 0 ? x2 :
            x2 <= 0 ? x1 :
            x1 > x2 ? x1 :
            x2
            ;
    }


    void Update()
    {
        if (_targetUnit == null && _targetTrEq == null)
            return;

        if (_isTargetReached)
        {
            _sr.color = new Color(1, 1, 1, 1 - _timerFade.Progress);

            SetPosToTarget();

            if (_timerFade.IsReady)
            {
                Arrows.Destroy(this);
                return;
            }
        }
        else
        {
            float dist = _speed_line * Time.deltaTime;

            if (dist > Delta_line.magnitude)
            {
                _isTargetReached = true;
                _timerFade.Set(.75f);
                transform.rotation *= Quaternion.AngleAxis(Random.Range(-30, 30), Vector3.forward);
                SetPosToTarget();

                _targetUnit?.TakeDamage(_damage); // Only for Fight, not for Training
            }
            else
            {
                _pos_line += Dir_line * dist; // (!) BEFORE other calculations

                float timePassed = _timeFull - TimeRest;
                float t = timePassed;
                float h = _vy0 * t + _gravity * t * t / 2;
                float vy = _vy0 + _gravity * t;

                Vector2 dir = Dir(vy);

                SetPosAndRotation(h, TimeRest / _timeFull, dir);
            }
        }
    }


    Vector2 Dir(float vy) => (_speed_line * Dir_line + Vector2.up * vy).normalized;


    void SetPosAndRotation(float h, float _adj_01, Vector2 dir)
    {
        transform.position = _pos_line + Vector2.up * (h + _adj * _adj_01);
        transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector2.Perpendicular(dir));
    }


    void SetPosToTarget()
    {
        transform.position = HitPos;
    }


    public void ClearForReturnToPool()
    {
        _isTargetReached = false;
        _targetUnit = null;
        _targetTrEq = null;

        transform.rotation = Quaternion.identity;
        _sr.color = Color.white;
    }
}