using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MonsterController : CreatureController
{
    Coroutine _coPatrol;
    Coroutine _coSkill;
    Coroutine _coSearch;
    Vector3Int _destCellPos;
    [SerializeField]
    GameObject _target;
    float _searchRange = 5.0f;
    float _skillRange = 1.0f;

    [SerializeField]//활잡이
    bool _rangedSkill = false;
    public override CreatureState State
    {
        get { return _state; }
        set
        {
            if (_state == value)
                return;

            base.State = value;

            if (_coPatrol != null)
            {
                StopCoroutine(_coPatrol);
                _coPatrol = null;
            }

            if (_coSearch != null)
            {
                StopCoroutine(_coSearch);
                _coSearch = null;
            }
        }
    }

    protected override void Init()
    {
        base.Init();

        State = CreatureState.Idle;
        Dir = MoveDir.None;
        _rangedSkill = (UnityEngine.Random.Range(0, 2) == 0 ? true : false);

        if (_rangedSkill)
            _skillRange = 10.0f;
        else
            _skillRange = 1.0f;
    }

    protected override void UpdateIdle()
    {
        base.UpdateIdle();

        if (_coPatrol == null)
        {
            _coPatrol = StartCoroutine("CoPatrol");
        }
        if (_coSearch == null)
        {
            _coSearch = StartCoroutine("CoSearch");
        }
    }

    public override void MoveToNextPos()
    {
        Vector3Int destPos = _destCellPos;
        if(_target != null)
        {
            destPos = _target.GetComponent<CreatureController>().CellPos;

            Vector3Int dir = destPos - CellPos;
            if(dir.magnitude <= _skillRange && (dir.x == 0 || dir.y ==0))
            {
                Dir = GetDirFromVec(dir);
                State = CreatureState.Skill;

                if (_rangedSkill)
                {
                    _coSkill = StartCoroutine("CorStartShootArrow");
                }
                else
                {
                    _coSkill = StartCoroutine("CoStartPunch");
                }
       
                return;
            }
        }

        List<Vector3Int> path = Managers.Map.FindPath(CellPos, destPos, ignoreDestCollision:true);
        if(path.Count < 2 || (_target == null && path.Count > 10))//너무 멀리 갔거나 못찾았거나.
        {
            _target = null;
            State = CreatureState.Idle;
            return;
        }

        Vector3Int nextPos = path[1];
        Vector3Int moveCellDir = nextPos - CellPos;
        Dir = GetDirFromVec(moveCellDir);


        if (Managers.Map.CanGo(nextPos) && Managers.Obj.Find(nextPos) == null)
        {
            CellPos = nextPos;
        }
        else
        {
            State = CreatureState.Idle;
        }
    }

    public override void OnDamaged()
    {
        GameObject effect = Managers.Resource.Instantiate("Effect/DieEffect");
        effect.transform.position = transform.position;
        effect.GetComponent<Animator>().Play("START");
        GameObject.Destroy(effect, 0.5f);

        Managers.Obj.Remove(gameObject);
        Managers.Resource.Destroy(gameObject);
    }

    IEnumerator CoPatrol()
    {
        int waitSeconds = UnityEngine.Random.Range(1, 4);
        yield return new WaitForSeconds(waitSeconds);

        for (int i = 0; i < 10; i++)
        {
            int xRange = UnityEngine.Random.Range(-5, 6);
            int yRange = UnityEngine.Random.Range(-5, 6);
            Vector3Int randPos = CellPos + new Vector3Int(xRange, yRange, 0);

            if (Managers.Map.CanGo(randPos) && Managers.Obj.Find(randPos) == null)
            {
                _destCellPos = randPos;
                State = CreatureState.Moving;
                yield break;
            }
        }

        State = CreatureState.Idle;
    }
    IEnumerator CoSearch()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);

            if (_target != null) continue;

         _target = Managers.Obj.Find((go) =>
            {
                PlayerController pc = go.GetComponent<PlayerController>();
                if (pc == null) return false;

                Vector3Int dir = (pc.CellPos - CellPos);
                if (dir.magnitude > _searchRange)
                    return false;

                return true;
            });
        }
    }
    IEnumerator CoStartPunch()
    {
        //피격판정
        GameObject go = Managers.Obj.Find(GetFrontCellPos());
        if (go != null)
        {
            CreatureController cc = go.GetComponent<CreatureController>();
            if (cc != null)
                cc.OnDamaged();
        }

        yield return new WaitForSeconds(0.5f);
        State = CreatureState.Moving;
        _coSkill = null;
    }

    IEnumerator CorStartShootArrow()
    {

        GameObject go = Managers.Resource.Instantiate("Creature/Arrow");
        ArrowController ac = go.gameObject.GetComponent<ArrowController>();
        ac.Dir = _lastDir;
        ac.CellPos = CellPos;
        //_rangeSkill = true;
        yield return new WaitForSeconds(0.3f);
        State = CreatureState.Moving;
        _coSkill = null;
       // _rangeSkill = false;
    }

}
