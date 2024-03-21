using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHpUI : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI text;

    void Update()
    {
        if (BattleState.Instance == null)
            return;
        var max = BattleState.Instance.playerMaxHp;
        var cur = BattleState.Instance.playerCurHp;

        slider.value = (float)cur / max;
        text.text = cur.ToString();
    }
}
