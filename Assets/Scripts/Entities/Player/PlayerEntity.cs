using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEntity : Entity
{
    //-- Constantes

    public const float steepAngle = 37f;

    public const float feetHeight = 0.3f;


    //-- Movimiento

    [Tooltip("La aceleración del personaje.")]
    [SerializeField] private float _acceleration = 85.0f;

    [Tooltip("La deaceleración del personaje.")]
    [SerializeField] private float _deacceleration = 75.0f;

    [Tooltip("La velocidad máxima de movimiento del personaje.")]
    [SerializeField] private float _maxSpeed = 20.0f;

    [Tooltip("La fuerza aplicada al saltar.")]
    [SerializeField] private float _jumpPower = 10.0f;


    //-- Variables Privadas

    // Si el jugador está en el suelo.
    private bool _isGrounded;

    // Si es verdadero, se considera que el jugador está en suelo muy enpinado. (Impidiendo movimiento)
    private bool _tooSteep;


    //-- Variables de Animación

    // -1 si solo las patas de atras tocan el suelo, +1 si son las de delante, 0 si ambas (O ninguna).
    private int _groundedIndex = 0;


    //-- Inputs

    // El movimiento izquierda/derecha (X) y delante/atras (Y)
    private Vector2 _movementInput = new Vector2();

    // El input de salto.
    // Es un float que se pone a 0.3 cuando se pulsa, bajando a 0 hasta que se pueda saltar. (Es decir, un buffer de salto)
    private float _jumpInput;

    // La habilidad de correr.
    private bool _dashInput;

    // La habilidad de planear.
    private bool _glideInput;


    //-- Componentes

    // El RigidBody para movimiento.
    private Rigidbody _rb;

    // Transform que actúa como punto de referencia para colocar la cámara.
    public Transform cameraAnchor;



    //-- Métodos Núcleo --//

    // Start is called before the first frame update
    private void Start()
    {
        PlayerCamera.entity = this;
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        GroundDetection();
        InputDetection();
    }

    private void FixedUpdate()
    {
        Movement();
    }


    //-- Métodos de Update --//

    /// <summary>
    /// Detección del suelo.
    /// </summary>
    private void GroundDetection()
    {
        // Se reinicia las variables.
        _isGrounded = false;
        _tooSteep = true;

        // Se crea el rayo que se va a reutilizar.
        Ray ray = new Ray(transform.position + Vector3.up, Vector3.down);
        RaycastHit hit;

        // Se empiezan los checks de suelo, empezando en el origen del jugador.
        if (Physics.Raycast(ray, out hit, 1f + feetHeight))
        {
            _isGrounded = true;

            // Se comprueba el ángulo del suelo para permitir movimiento.
            if (SteepnessCheck(hit.normal))
                _tooSteep = false;
        }

        // Ahora se hace el check en los pies de delante.
        ray.origin = ray.origin + transform.forward;
        if (Physics.Raycast(ray, out hit, 1f + feetHeight))
        {
            _isGrounded = true;
            _groundedIndex += 1; // Los pies de delante tocan el suelo.

            // Se comprueba el ángulo del suelo para permitir movimiento.
            if (SteepnessCheck(hit.normal))
                _tooSteep = false;
        }
        

        // Finalmente, un check en los pies de atrás.
        ray.origin = ray.origin - transform.forward * 2;
        if (Physics.Raycast(ray, out hit, 1f + feetHeight))
        {
            _isGrounded = true;
            _groundedIndex -= 1; // Los pies de atrás tocan el suelo.

            // Se comprueba el ángulo del suelo para permitir movimiento.
            if (SteepnessCheck(hit.normal))
                _tooSteep = false;
        }
    }


    /// <summary>
    /// La detección de todos los inputs aplicables al jugador.
    /// </summary>
    private void InputDetection()
    {   
        // Se reduce el buffer the salto.
        _jumpInput -= Time.deltaTime;

        // Se aplica el input de movimiento.
        _movementInput.x = Input.GetAxis("Horizontal");
        _movementInput.y = Input.GetAxis("Vertical");

/*
        // Cuando se pulsa el botón de planear, se invierte el estado.
        if (!isGrounded && Input.GetButtonDown("Jump"))
            glideInput = !glideInput;
            
        // Se refresca el salto si se pulsa.
        else*/ if (Input.GetButtonDown("Jump"))
            _jumpInput = 0.3f;

        // Se corre si se mantiene pulsado el botón.
        _dashInput = Input.GetButton("Dash");
    }


    //-- Métodos de FixedUpdate --//

    /// <summary>
    /// La lógica raíz de movimiento.
    /// </summary>
    private void Movement()
    {
        // Se salta si se está en el suelo y pulsando saltar.
        if (_isGrounded && _jumpInput > 0.0f)
        {
            _jumpInput = -1f;
            Jump();
        }


        if (!_tooSteep || !_isGrounded) // Solo se puede mover si se está o en el aire o el suelo es poco enpinado.
        {
            // Si no se mueve, decelerar a 0.
            if (_movementInput.sqrMagnitude < 0.2f)
            {
                Vector3 v = Vector3.MoveTowards(_rb.velocity, new Vector3(), _deacceleration * Time.fixedDeltaTime);
                _rb.velocity = new Vector3(v.x, _rb.velocity.y, v.z); // Se cancela el componente Y del MoveTowards.
            }
            else // Si se está moviendo, hacer todo el cálculo de movimiento.
            {
                // El adelante y derecha respecto a la cámara.
                Vector3 forward = new Vector3(cameraAnchor.forward.x, 0, cameraAnchor.forward.z).normalized;
                Vector3 right = new Vector3(cameraAnchor.right.x, 0, cameraAnchor.right.z).normalized;

                // Se calcula la dirección de movimiento y la velocidad que se va a aplicar.
                Vector3 direction = (forward * _movementInput.y + right * _movementInput.x).normalized; // Se consigue la dirección de movimiento.
                Vector3 v = Vector3.MoveTowards(_rb.velocity, direction * _maxSpeed, _acceleration * Mathf.Min(_movementInput.magnitude, 1.0f) * Time.fixedDeltaTime);
                v.y = 0.0f; // Se quita el componente Y.

                // Se hace un calculo para que no se pueda mover contra paredes (para no caer) y no poder superar rampas muy enpinadas.
                bool canMove = true;
                Ray checkRay = new Ray(transform.position + Vector3.up * 0.1f, v);
                if (Physics.Raycast(checkRay, out RaycastHit hitInfo, 2.4f))
                {
                    print(Vector3.Angle(v, hitInfo.normal));
                    canMove = Vector3.Angle(v, hitInfo.normal) < 90f + steepAngle;
                }

                // Finalmente, se aplica la velocidad.
                if (canMove)
                    _rb.velocity = new Vector3(v.x, _rb.velocity.y, v.z); // Se mantiene la velocidad vertical.
            }
        }
    }


    /// <summary>
    /// Empuja el personaje cara arriba.
    /// </summary>
    private void Jump()
    {
        _rb.velocity = new Vector3(_rb.velocity.x, _jumpPower, _rb.velocity.z);
    }


    //-- Métodos Privados --//

    /// <summary>
    /// Comprueba el ángulo del suelo, comparandolo la constante SteepAngle.
    /// </summary>
    /// <param name="normal">La normal del suelo.</param>
    /// <returns>Devuelve si el ángulo está dentro de los ángulos permitidos.</returns>
    private bool SteepnessCheck(Vector3 normal)
    {
        return Vector3.Angle(Vector3.down, normal) > 180f - steepAngle;
    }
}
