using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicSelect : MonoBehaviour
{
    public BaseAttack usedMagic;

    public void SelectMagic()
    {
        GameObject.Find("BattleManager").GetComponent<BattleStateMachine>().Input3(usedMagic);
    }
}
