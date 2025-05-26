using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Entity : MonoBehaviour
{
    //-- Vida

    [Tooltip("La vida máxima de la entidad.")]
    public int MaxHealth {get; protected set;} = 3;

    [Tooltip("La vida actual de la entidad.")]
    public int Health = 3;

    // El "estado" de muerte, altera como reacciona la cámara.
    // 0 = vivo, 1 = muerte estándar, 2 = cámara estática.
    public int DeathState {get; private set;}

    // La gema que puede opcionalmente estar unida a este objeto.
    private Gem _gem;


    //-- Métodos Núcleo --//

    virtual protected void Start()
    {
        Health = MaxHealth;
        _gem = GetComponentInChildren<Gem>();
        if (_gem != null)
            _gem.transform.localPosition = Vector3.up * -10000;
    }


    private void OnDestroy()
    {
        if (_gem != null)
        {
            _gem.transform.localPosition = Vector3.zero;
            _gem.transform.parent = null;
            _gem.transform.localScale = Vector3.one;
            _gem.UpdateModel();
        }
    }


    //-- Métodos Públicos --//

    public void Damage(int dmg)
    {
        Health -= dmg;
        if (Health <= 0)
            Kill(1);
    }

    public virtual void Knockback(Vector3 vector, bool instantChange = true, bool stun = true)
    {
        // Vacio, para ser heredado por hijos.
    }


    //-- Métodos Protejidos/Privados --//

    /// <summary>
    /// Llama la animación de muerte y se prepara para borrar la entidad.
    /// </summary>
    /// <param name="deathCause">La "Causa" de la muerte, que puede ser recibir daño o caerse.</param>
    virtual protected void Kill(int deathCause = 1)
    {
        DeathState = deathCause;
        StartCoroutine(DeathAnimation());
    }


    /// <summary>
    /// Corrutina que se activa al llamar Kill(), es una animación derivable que puede modificar la entidad antes de borrarla.
    /// </summary>
    virtual protected IEnumerator DeathAnimation()
    {
        yield return null;       
        Destroy(gameObject);
    }
}
