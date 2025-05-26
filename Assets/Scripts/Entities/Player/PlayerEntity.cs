using System.Collections;
using UnityEngine;

public class PlayerEntity : Entity
{
    //-- Constantes

    public const float SteepAngle = 37f;

    public const float FeetHeight = 0.5f;


    //-- Movimiento

    [Tooltip("La aceleración del personaje.")]
    [SerializeField] private float _acceleration = 85.0f;

    [Tooltip("La deaceleración del personaje.")]
    [SerializeField] private float _deacceleration = 75.0f;

    [Tooltip("La velocidad máxima de movimiento del personaje.")]
    [SerializeField] private float _maxSpeed = 20.0f;

    [Tooltip("Multiplicador aplicado a la velocidad máxima y aceleración cuando se corre.")]
    [SerializeField] private float _dashMultiplier = 1.6f;

    [Tooltip("La fuerza aplicada al saltar.")]
    [SerializeField] private float _jumpPower = 10.0f;


    //-- Variables Privadas

    // Si el jugador está en el suelo.
    private bool _isGrounded;

    // Si es verdadero, se considera que el jugador está en suelo muy enpinado. (Impidiendo movimiento)
    private bool _tooSteep;

    // Si es mayor que 0, el jugador no puede hacer nada, y está en un estado de "Stun".
    private float _stun = -1f;


    //-- Variables de Animación

    [Tooltip("La velocidad a la cual el modelo mira a la dirección de movimiento.")]
    [SerializeField] private float _modelRotationSpeed = 8.0f;


    [Tooltip("El modelo 3D del personaje.")]
    [SerializeField] private Transform _model;

    [Tooltip("El Animator del personaje.")]
    [SerializeField] private Animator _animator;


    //-- Inputs

    // El movimiento izquierda/derecha (X) y delante/atras (Y)
    public Vector2 InputMovement { get; private set; } = new Vector2();

    // El input de salto.
    // Es un float que se pone a 0.3 cuando se pulsa, bajando a 0 hasta que se pueda saltar. (Es decir, un buffer de salto)
    public float InputJump {get; private set;}

    // La habilidad de correr.
    public bool InputDash {get; private set;}

    // La habilidad de planear.
    public bool InputGlide {get; private set;}

    // El input de ataque.
    public bool InputAttack {get; private set;}


    //-- Componentes

    [Tooltip("Transform que actúa como punto de referencia para colocar la cámara.")]
    public Transform CameraAnchor;

    [Tooltip("El RigidBody para movimiento.")]
    [HideInInspector] public Rigidbody Rigidbody {get; private set;}

    [Tooltip("El Hitbox de daño cuando el jugador carga.")]
    [SerializeField] private DamageBox _chargeHitbox;



    //-- Métodos Núcleo --//

    // Start is called before the first frame update
    override protected void Start()
    {
        base.Start();
        Player.Entity = this;
        Rigidbody = GetComponent<Rigidbody>();
        _chargeHitbox.gameObject.SetActive(false);
    }

    private void Update()
    {
        GroundDetection();
        InputDetection();
        AnimatorUpdate();
    }

    private void FixedUpdate()
    {
        if (!FallDeathCheck() && DeathState == 0)
            Movement();
    }

    //-- Métodos Override --//

    protected override IEnumerator DeathAnimation()
    {
        switch (DeathState)
        {
            case 1:
                // Animación iría aquí.
                yield return new WaitForSeconds(1.5f);
                break;
            case 2:
                yield return new WaitForSeconds(1.5f);
                break;
        }
        Destroy(gameObject);
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

        // -1 si solo las patas de atras tocan el suelo, +1 si son las de delante, 0 si ambas (O ninguna).
        int _groundedIndex = 0;

        // Se crea el rayo que se va a reutilizar.
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;

        // Se empiezan los checks de suelo, empezando en el origen del jugador.
        if (Physics.Raycast(ray, out hit, 1f + FeetHeight))
        {
            _isGrounded = true;

            // Se comprueba el ángulo del suelo para permitir movimiento.
            if (SteepnessCheck(hit.normal))
                _tooSteep = false;
        }

        // Ahora se hace el check en los pies de delante.
        ray.origin = ray.origin + _model.forward;
        if (Physics.Raycast(ray, out hit, 1f + FeetHeight))
        {
            _isGrounded = true;
            _groundedIndex -= 1; // Los pies de delante tocan el suelo.

            // Se comprueba el ángulo del suelo para permitir movimiento.
            if (SteepnessCheck(hit.normal))
                _tooSteep = false;
        }


        // Finalmente, un check en los pies de atrás.
        ray.origin = ray.origin - _model.forward * 2;
        if (Physics.Raycast(ray, out hit, 1f + FeetHeight))
        {
            _isGrounded = true;
            _groundedIndex += 1; // Los pies de atrás tocan el suelo.

            // Se comprueba el ángulo del suelo para permitir movimiento.
            if (SteepnessCheck(hit.normal))
                _tooSteep = false;
        }

        // Se aplica el groundedIndex para la animación.
        _animator.SetFloat("Edge", _groundedIndex);
    }


    /// <summary>
    /// La detección de todos los inputs aplicables al jugador.
    /// </summary>
    private void InputDetection()
    {   
        // Se reduce el buffer the salto.
        InputJump -= Time.deltaTime;

        if (PauseScreen.Active) // Si el menú de pausa está activo, se cancela los inputs del jugador.
        {
            InputMovement = Vector2.zero;
            InputJump = -1f;
            InputDash = false;
        }
        else
        {
            // Se aplica el input de movimiento.
            InputMovement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            // Se refresca el salto si se pulsa.
            if (Input.GetButtonDown("Jump"))
                InputJump = 0.3f;

            // Se corre si se mantiene pulsado el botón.
            InputDash = Input.GetButton("Dash");
        }
    }


    //-- Métodos de FixedUpdate --//

    /// <summary>
    /// La lógica raíz de movimiento.
    /// </summary>
    private void Movement()
    {
        _chargeHitbox.gameObject.SetActive(false);
        if (_stun > 0f) // Si _stun es mayor que 0, se activa el estado de stun.
        {
            if (_isGrounded) // Solo baja el temporizador si se está en el suelo, incluso si es enpinado.
                _stun -= Time.fixedDeltaTime;
        }
        else if (!_tooSteep || !_isGrounded) // Solo se puede mover si se está o en el aire o el suelo es poco enpinado.
        {
            // Primero se comprueba si se está pulsando el botón de correr.
            if (InputDash)
            {
                // Se activa el hitbox de daño.
                _chargeHitbox.gameObject.SetActive(true);

                // Se gira al personaje para que mire a la dirección de la cámara.
                Quaternion currentRotation = _model.rotation;
                _model.LookAt(_model.position + new Vector3(CameraAnchor.forward.x, 0, CameraAnchor.forward.z));
                _model.rotation = Quaternion.RotateTowards(currentRotation, _model.rotation, _modelRotationSpeed * 20.0f * Time.fixedDeltaTime);

                // Se calcula el movimiento, que siempre es máxima velocidad a donde mira el modelo.
                Vector3 v = Vector3.MoveTowards(Rigidbody.velocity, _model.forward * _maxSpeed * _dashMultiplier, _acceleration * _dashMultiplier * Time.fixedDeltaTime);
                v.y = 0.0f; // Se quita el componente Y.

                // Finalmente, se aplica la velocidad.
                if (!WallCheck(v))
                    Rigidbody.velocity = new Vector3(v.x, Rigidbody.velocity.y, v.z); // Se mantiene la velocidad vertical.
                else
                    Knockback(-v.normalized * 7f + Vector3.up);
            }
            // Si no se mueve, decelerar a 0.
            else if (InputMovement.sqrMagnitude < 0.2f)
            {
                Vector3 v = Vector3.MoveTowards(Rigidbody.velocity, new Vector3(), _deacceleration * Time.fixedDeltaTime);
                Rigidbody.velocity = new Vector3(v.x, Rigidbody.velocity.y, v.z); // Se cancela el componente Y del MoveTowards.
            }
            // Si se está moviendo sin correr, hacer todo el cálculo de movimiento.
            else
            {
                // El adelante y derecha respecto a la cámara. Y cancelado.
                Vector3 forward = new Vector3(CameraAnchor.forward.x, 0, CameraAnchor.forward.z).normalized;
                Vector3 right = new Vector3(CameraAnchor.right.x, 0, CameraAnchor.right.z).normalized;

                // Se calcula la dirección de movimiento.
                Vector3 direction = (forward * InputMovement.y + right * InputMovement.x).normalized; // Se consigue la dirección de movimiento.

                // Se gira al personaje para que mire a la dirección de movimiento.
                Quaternion currentRotation = _model.rotation;
                _model.LookAt(_model.position + direction);
                _model.rotation = Quaternion.Slerp(currentRotation, _model.rotation, _modelRotationSpeed * Time.fixedDeltaTime);

                // La velocidad que se va a aplicar.
                Vector3 v = Vector3.MoveTowards(Rigidbody.velocity, direction * _maxSpeed, _acceleration * Mathf.Min(InputMovement.magnitude, 1.0f) * Time.fixedDeltaTime);
                v.y = 0.0f; // Se quita el componente Y.

                // Finalmente, se aplica la velocidad.
                if (!WallCheck(v))
                    Rigidbody.velocity = new Vector3(v.x, Rigidbody.velocity.y, v.z); // Se mantiene la velocidad vertical.
            }


            // Se salta si se está en el suelo y pulsando saltar.
            if (_isGrounded && InputJump > 0.0f)
            {
                InputJump = -1f;
                Jump();
            }
        }
        else
        {
            // Se gira al personaje para que mire a la dirección de movimiento.
            Quaternion currentRotation = _model.rotation;
            _model.LookAt(_model.position + new Vector3(Rigidbody.velocity.x, 0, Rigidbody.velocity.z));
            _model.rotation = Quaternion.Slerp(currentRotation, _model.rotation, _modelRotationSpeed * Time.fixedDeltaTime);
        }
    }


    /// <summary>
    /// Método que actualiza el Animator del modelo 3D de este personaje cada frame.
    /// </summary>
    private void AnimatorUpdate()
    {
        _animator.SetBool("Falling", !_isGrounded || _tooSteep);
        _animator.SetBool("Dash", InputDash);
        _animator.SetFloat("Move", new Vector3(Rigidbody.velocity.x, 0, Rigidbody.velocity.z).magnitude / _maxSpeed);
    }


    /// <summary>
    /// Empuja el personaje cara arriba.
    /// </summary>
    private void Jump()
    {
        // Se transiciona a la animación de saltar.
        _animator.CrossFadeInFixedTime("Base.Jump", 0.12f);

        Rigidbody.velocity = new Vector3(Rigidbody.velocity.x, _jumpPower, Rigidbody.velocity.z);
    }


    /// <summary>
    /// Check auxiliar que comprueba si la entidad ha pasado de una cierta profundidad, forzando una muerte si es el caso.
    /// </summary>
    /// <returns>True si la entidad se va a morir por este check.</returns>
    private bool FallDeathCheck()
    {
        if (transform.position.y < -100)
        {
            Kill(2);
            return true;
        }
        return false;
    }


    //-- Métodos Públicos --//

    override public void Knockback(Vector3 vector, bool instantChange = true, bool stun = true)
    {
        if (instantChange)
            Rigidbody.velocity = vector;
        else
            Rigidbody.AddForce(vector, ForceMode.VelocityChange);
        
        if (stun)
            _stun = 1.0f;
    }

    public void SetLook(Vector3 direction)
    {
        // Se quita el eje Y.
        direction = new Vector3(direction.x, 0, direction.z).normalized;

        CameraAnchor.LookAt(CameraAnchor.position + direction);
        _model.LookAt(_model.position + direction);
    }


    //-- Métodos Privados --//

    /// <summary>
    /// Comprueba el ángulo del suelo, comparandolo la constante SteepAngle.
    /// </summary>
    /// <param name="normal">La normal del suelo.</param>
    /// <returns>Devuelve si el ángulo está dentro de los ángulos permitidos.</returns>
    private bool SteepnessCheck(Vector3 normal)
    {
        return Vector3.Angle(Vector3.down, normal) > 180f - SteepAngle;
    }


    /// <summary>
    /// Devuelve si hay un muro impassable si se mueve siguiendo un vector.
    /// </summary>
    /// <param name="velocityVector">El vector a seguir para detectar el muro</param>
    private bool WallCheck(Vector3 velocityVector)
    {
        Ray checkRay = new Ray(transform.position + Vector3.up * 0.15f, velocityVector);
        if (Physics.Raycast(checkRay, out RaycastHit hitInfo, 2.4f, 0b1))
        {
            return !(Vector3.Angle(velocityVector, hitInfo.normal) < 90f + SteepAngle);
        }
        return false;
    }
}
