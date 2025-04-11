using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatUI : MonoBehaviour
{
    [SerializeField]
    private string key;
    [SerializeField]
    private float value;

    [SerializeField]
    private TextMeshProUGUI titleText;
    [SerializeField]
    private TextMeshProUGUI valueText;

    public void Init(string key, Stats stats)
    {
        this.key = key;
        titleText.text = key;
        valueText.text = stats.GetStat(key).ToString();

        stats.OnStatChanged.AddListener(OnStatChanged);
    }

    private void OnStatChanged(string changedStatKey, float oldValue, float newValue)
    {
        if (changedStatKey != key) return;

        valueText.text = newValue.ToString();
    }
}
