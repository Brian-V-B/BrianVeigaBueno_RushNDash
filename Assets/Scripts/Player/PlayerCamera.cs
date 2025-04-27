using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    //-- Variables estáticas

    // La referencia a la cámara principal.
    public static PlayerCamera instance {get; private set;}

    // La referencia a la entidad que controla el jugador.
    public static PlayerEntity entity = null;


    //-- Velocidades

    // La velocidad de interpolación de la cámara.
    [SerializeField] private float _speed = 15.0f;

    // La velocidad de la rotación del Anchor de la entidad.
    [SerializeField] private float _sensitivity = 3.0f;


    //-- Otras Variables

    // Offset aplicado cuando el Raycast para colocar la cámara ocurre.
    [SerializeField] private float _springOffset = 0.2f;

    // La distancia de la cámara al jugador.
    [SerializeField] private float _cameraDistance = 4.0f;



    //-- Métodos Núcleo --//

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (entity != null && entity.cameraAnchor != null)
        {
            Cursor.lockState = CursorLockMode.Locked;

            // Se lee el input de cámara.
            Vector2 rotation = new Vector2();
            rotation.x = Input.GetAxis("CameraX");
            rotation.y = -Input.GetAxis("CameraY") * 1.22f;
            rotation *= _sensitivity;

            Transform anchor = entity.cameraAnchor; // Tener referencia rápida, aparte de moverlo al Stack.

            // Se rota el Anchor de referencia.
            anchor.Rotate(Vector3.up, rotation.x, Space.Self);
            anchor.Rotate(Vector3.right, rotation.y, Space.Self);
            anchor.LookAt(anchor.position + anchor.forward, Vector3.up); // Se cancela el "Twist" (rotación en eje Z).

            // Se coloca la cámara.
            if (Physics.Raycast(anchor.position, -anchor.forward, out RaycastHit hit, _cameraDistance))
                transform.position = hit.point + anchor.forward * _springOffset;
            else
                transform.position = anchor.position - anchor.forward * _cameraDistance;

            // Se rota la cámara
            transform.rotation = anchor.rotation;
        }
    }
}
