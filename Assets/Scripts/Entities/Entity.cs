using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    /// Variables ///

    /// VIDA
    [Tooltip("La vida máxima de la entidad.")] // Es protected para permitir calculos con ella en Clases derivadas.
    [SerializeField] protected int maxHealth = 3;

    [Tooltip("La vida actual de la entidad.")]
    public int Health = 3;



    /// Métodos Públicos ///

    public void Damage(int dmg)
    {
        Health -= dmg;
        if (Health <= 0)
            Kill();
    }


    /// Métodos Protejidos/Privados ///

    virtual protected void Kill()
    {
        Destroy(gameObject);
    }
}
