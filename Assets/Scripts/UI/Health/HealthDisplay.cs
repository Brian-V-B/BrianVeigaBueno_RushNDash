using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthDisplay : MonoBehaviour
{
    // Cuantos segmentos tiene cada 'rueda' de vida.
    const int BarCount = 3;


    [Tooltip("La 'rueda' de vida que se va a duplicar.")]
    [SerializeField] private HealthWheel _healthWheelTemplate;

    // Todas las ruedas de vida actualmente instanciadas.
    private List<HealthWheel> _wheels = new List<HealthWheel>();



    // Update is called once per frame
    private void Update()
    {
        if (Player.TryGetEntity(out PlayerEntity e))
        {
            // Se calcula cuantas 'ruedas' de vida se quieren tener.
            int wheelCount = Mathf.CeilToInt(Mathf.Max(e.MaxHealth, e.Health) / (BarCount * 1f));
            for (int i = 0; i < wheelCount || i < _wheels.Count; i++)
            {
                if (i < wheelCount)
                {
                    // Se añade ruedas nuevas si no existen.
                    if (i >= _wheels.Count)
                        _wheels.Add(Instantiate(_healthWheelTemplate, transform));
                    
                    // Se calcula cuanta vida tiene el segmento de la rueda y se actualiza la rueda.
                    int hp = e.Health - i * BarCount;
                    _wheels[i].UpdateHealth(hp, i * BarCount + BarCount > e.MaxHealth);
                }
                else
                {
                    // Si la rueda sobra, se borra.
                    Destroy(_wheels[i].gameObject);
                }
            }
            // Si la Lista es demasiado grande, se borran las referencias (null) extra.
            if (wheelCount < _wheels.Count)
                _wheels.RemoveRange(wheelCount, _wheels.Count - 1);
        }   
    }
}
