using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PauseScreen : MonoBehaviour
{
    static public bool Active;

    [SerializeField] private GameObject _pauseScreen;


    void Start()
    {
        Active = true;
        OnActiveChanged();
    }


    // Update is called once per frame
    void Update()
    {

        if (Input.GetButtonDown("Pause"))
            ToggleActive(); // Se invierte el estado activo del menú.
    }


    private void ToggleActive()
    {
        Active = !Active;
        OnActiveChanged();
    }


    /// <summary>
    /// Actualiza el estado de la interfaz cuando se activa el menú de pausa.
    /// </summary>
    private void OnActiveChanged()
    {
        if (Active)
            Cursor.lockState = CursorLockMode.Confined;
        else
            Cursor.lockState = CursorLockMode.Locked;
        _pauseScreen.SetActive(Active);
    }
}
