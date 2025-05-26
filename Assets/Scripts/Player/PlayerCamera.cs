using System.Collections;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    //-- Variables estáticas

    // La referencia a la cámara principal.
    public static PlayerCamera Instance {get; private set;}


    //-- Velocidades

    // La velocidad de interpolación de la cámara.
    [SerializeField] private float _speed = 15.0f;

    // La velocidad de la rotación del Anchor de la entidad.
    [SerializeField] private float _sensitivity = 3.0f;


    //-- Cámara

    // Offset aplicado cuando el Raycast para colocar la cámara ocurre.
    [SerializeField] private float _springOffset = 0.2f;

    // La distancia de la cámara al jugador. X = Movimiento normal, Y = Dash
    [SerializeField] private Vector2 _cameraDistance = new Vector2(4.0f, 5.0f);

    // El FoV de la cámara si el jugador se mueve normal o cuando corre.
    [SerializeField] private Vector2 _cameraFOV = new Vector2(70f, 50f);

    // La velocidad a la que se interpola la camara entre modo caminar y correr.
    [SerializeField] private float _cameraSwitchSpeed = 3.0f;

    // La distancia actual de la cámara al jugador.
    private float _currentCameraDistance;


    //-- Variables Privadas

    // El prefab que se usa para crear al jugador.
    [SerializeField] private PlayerEntity playerEntityPrefab;

    private Camera _camera;



    //-- Métodos Núcleo --//

    // Start is called before the first frame update
    private void Start()
    {
        Instance = this;
        _camera = GetComponent<Camera>();
        _currentCameraDistance = _cameraDistance.x;
    }

    // Update is called once per frame
    private void Update()
    {
        if (Player.TryGetEntity(out PlayerEntity entity))
        {
            Transform anchor = entity.CameraAnchor; // Tener referencia rápida, aparte de moverlo al Stack.

            if (entity.DeathState == 2)
            {
                transform.LookAt(anchor.position);
                return;
            }

            // Se lee el input de cámara.
            Vector2 rotation = new Vector2();
            if (!PauseScreen.Active) // No se rota si el menú de pausa está activo.
            {
                rotation.x = Input.GetAxis("CameraX");
                rotation.y = -Input.GetAxis("CameraY") * 1.22f;
                rotation *= _sensitivity;
            }

            // Se rota el Anchor de referencia.
            anchor.Rotate(Vector3.up, rotation.x, Space.Self);
            anchor.Rotate(Vector3.right, rotation.y, Space.Self);
            float x = anchor.eulerAngles.x;
            if (x > 180) // Se fuerza que sea negativo cuando es mayor que 180 grados.
                x -= 360;
            anchor.eulerAngles = new Vector3(Mathf.Clamp(x, -42f, 70f), anchor.eulerAngles.y, 0);

            // Se altera el FOV y la distancia de la cámara según si se está corriendo o no.
            if (entity.InputDash)
            {
                _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, _cameraFOV.y, _cameraSwitchSpeed * Time.deltaTime);
                _currentCameraDistance = Mathf.Lerp(_currentCameraDistance, _cameraDistance.y, _cameraSwitchSpeed * Time.deltaTime);
            }
            else
            {
                _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, _cameraFOV.x, _cameraSwitchSpeed * Time.deltaTime);
                _currentCameraDistance = Mathf.Lerp(_currentCameraDistance, _cameraDistance.x, _cameraSwitchSpeed * Time.deltaTime);
            }

            // Se coloca la cámara.
            if (Physics.Raycast(anchor.position, -anchor.forward, out RaycastHit hit, _currentCameraDistance, 0b1000011))
                transform.position = hit.point + anchor.forward * _springOffset;
            else
                transform.position = anchor.position - anchor.forward * _currentCameraDistance;

            // Se rota la cámara
            transform.rotation = anchor.rotation;
        }
        else
        {
            Player.RespawnEntity(playerEntityPrefab);
        }
    }
}
