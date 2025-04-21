using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinLoseManager : MonoBehaviour
{
    public static WinLoseManager Instance { private set; get; }

    private bool isFinished = false;

    [SerializeField]
    private GameObject winEffect;
    [SerializeField]
    private GameObject lsoeEffect;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        Reset();
    }

    public void Reset()
    {
        isFinished = false;
        winEffect.SetActive(false);
        lsoeEffect.SetActive(false);
    }

    public void Win()
    {
        if (isFinished) return;
        isFinished = true;
        winEffect.SetActive(true);
    }

    public void Lose()
    {
        if (isFinished) return;
        isFinished = true;
        lsoeEffect.SetActive(true);
    }
}
