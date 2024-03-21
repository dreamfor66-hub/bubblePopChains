using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BasicAttackCountUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    void Update()
    {
        text.text = BattleState.Instance.basicAttackStack.ToString();
    }
}
