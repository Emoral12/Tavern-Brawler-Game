using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BasePlayer
{
    public string name;

    public float baseHP;
    public float curHP;

    public float baseMP;
    public float curMP;

    public List<BaseAttack> attacks = new List<BaseAttack>();
}