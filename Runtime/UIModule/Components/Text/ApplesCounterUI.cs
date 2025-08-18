using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using UnityEngine.Localization;

public class ApplesCounterUI : MonoBehaviour
{
    [SerializeField] private TMP_Text label;

    // В инспекторе выберите Table=UI, Entry=apples_count
    [SerializeField] private LocalizedString applesCount;

    private async void OnEnable()
    {
        applesCount.StringChanged += OnStringChanged;
        SetCount(0); // начальное обновление
        await Task.Delay(1000);
        SetCount(1);
        await Task.Delay(1000);
        SetCount(2);
        await Task.Delay(1000);
        SetCount(3);
        await Task.Delay(1000);
        SetCount(9);
    }

    private void OnDisable()
    {
        applesCount.StringChanged -= OnStringChanged;
    }

    public void SetCount(int count)
    {
        // Передаём именованный аргумент "count"
        applesCount.Arguments = new object[] { new { count } };
        applesCount.RefreshString(); // триггерим пересчёт и вызов StringChanged
    }

    private void OnStringChanged(string value)
    {
        label.text = value;
    }
}