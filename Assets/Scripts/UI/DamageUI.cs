using TMPro;
using UnityEngine;
using UnityEngine.Pool;

public class DamageUI : MonoBehaviour
{
    public ObjectPool<DamageUI> Pool { get; set; }

    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float duration;
    [SerializeField] private Animator animator;

    private float time;
    public void Set(Vector3 position, int damage)
    {
        gameObject.SetActive(true);
        transform.position = position;
        text.text = $"-{damage}";
        text.color = Color.red;
        time = duration;
        animator.Play("Play", 0, 0f);
    }

    public void SetHeal(Vector3 position, int value)
    {
        gameObject.SetActive(true);
        transform.position = position;
        text.text = $"+{value}";
        text.color = Color.green;
        time = duration;
        animator.Play("Play", 0, 0f);
    }

    void Update()
    {
        time -= Time.deltaTime;
        if (time < 0f)
            Release();
    }

    void Release()
    {
        if (Pool != null)
        {
            gameObject.SetActive(false);
            Pool.Release(this);
        }
        else
            Destroy(this.gameObject);
    }
}
