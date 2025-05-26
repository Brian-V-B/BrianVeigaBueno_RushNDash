using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthWheel : MonoBehaviour
{
    [SerializeField] private Image[] _healthChunks;


    public void UpdateHealth(int amount, bool overHealth = false)
    {
        for (int i = 0; i < _healthChunks.Length; i++)
        {
            if (i < amount)
            {
                if (overHealth)
                    _healthChunks[i].color = Color.cyan;
                else
                    _healthChunks[i].color = Color.green;
            }
            else
                _healthChunks[i].color = Color.gray;
        }
    }
}
