//
// Controlador con Rigidbody cuando los datos de animación de Mecanim no se mueven en el origen
// Muestra
// 2014/03/13 N.Kobyasahi
//
using UnityEngine;
using System.Collections;

// 必要なコンポーネントの列記
[RequireComponent(typeof (Animator))]
[RequireComponent(typeof (CapsuleCollider))]
[RequireComponent(typeof (Rigidbody))]

public class UnityChanControlScriptWithRgidBody : MonoBehaviour
{

	public float animSpeed = 1.5f;              // Ajuste de velocidad de reproducción de animación
    public float lookSmoother = 3.0f;			// a smoothing setting for camera motion
	public bool useCurves = true;               // Utilice Mecanim para ajustar el ajuste de la curva.
                                                // Las curvas no se usan a menos que este interruptor esté encendido
    public float useCurvesHeight = 0.5f;        // Altura efectiva de la corrección de la curva (aumentar cuando es fácil pasar por el suelo)

    // Parámetro para el controlador de caracteres a continuación
    // Velocidad de avance
    public float forwardSpeed = 7.0f;
    // Velocidad de retractacion
    public float backwardSpeed = 2.0f;
    // Velocidad de giro
    public float rotateSpeed = 2.0f;
    // Poder de salto
    public float jumpPower = 3.0f;
    // Referencia del controlador de caracteres (colisionador de cápsulas)
    private CapsuleCollider col;
	private Rigidbody rb;
    // Cantidad de movimiento del controlador de caracteres(colisionador de cápsulas)

    private Vector3 velocity;
    // Una variable que puede contener el valor inicial de Heiht, el centro del colector establecido por CapsuleCollider
    private float orgColHight;
	private Vector3 orgVectColCenter;
	
	private Animator anim;                          // Referencia al animador adjunto al personaje.
    private AnimatorStateInfo currentBaseState;         // Una referencia al estado actual del animador utilizado en la capa base

    private GameObject cameraObject;    // Referencia a la cámara principal.

    // Referencia del animador a cada estado.
    static int idleState = Animator.StringToHash("Base Layer.Idle");
	static int locoState = Animator.StringToHash("Base Layer.Locomotion");
	static int jumpState = Animator.StringToHash("Base Layer.Jump");
	static int restState = Animator.StringToHash("Base Layer.Rest");

    private bool isGrounded;

    void OnCollisionStay(Collision coll)
    {
        isGrounded = true;
    }
    void OnCollisionExit(Collision coll)
    {
        if (isGrounded)
        {
            isGrounded = false;
        }
    }


    // Inicialización
    void Start ()
	{
        // Recuperar el componente animador
        anim = GetComponent<Animator>();
        // Recuperar el componente CapsuleCollider (colisión de cápsula)
        col = GetComponent<CapsuleCollider>();
		rb = GetComponent<Rigidbody>();
        // Conseguir la camara principal
        cameraObject = GameObject.FindWithTag("MainCamera");
        // Guardar el valor inicial del componente Height, Center of CapsuleCollider
                orgColHight = col.height;
		orgVectColCenter = col.center;
}


    // En lo sucesivo, el procesamiento principal. Dado que está relacionado con el cuerpo rígido, el procesamiento se realiza dentro de FixedUpdate.
    void FixedUpdate ()
	{
		float h = Input.GetAxis("Horizontal");              // Defina el eje horizontal del dispositivo de entrada como h
        float v = Input.GetAxis("Vertical");                // Eje vertical del dispositivo de entrada definido por v
        anim.SetFloat("Speed", v);                          // Pase v al parámetro "Velocidad" establecido en el lado del animadorw 


        //anim.SetFloat("Direction", h);                      // Pase h al parámetro "Dirección" establecido en el lado del animador
        anim.speed = animSpeed;                             // Establezca animSpeed ​​a la velocidad de reproducción de animación de Animator
        currentBaseState = anim.GetCurrentAnimatorStateInfo(0); // Establezca el estado actual de la capa base (0) como referencia a la variable de estado
        rb.useGravity = true;// Cortaremos la gravedad durante el salto, por lo que seremos afectados por la gravedad de lo contrario



        // En adelante, el proceso de movimiento del personaje.
        velocity = new Vector3(h*0.8f ,0, v);        // Obtenga la cantidad de movimiento en la dirección del eje Z desde la entrada de teclas superior e inferior
                                                // Convertir la dirección del personaje en el espacio local.
        velocity = transform.TransformDirection(velocity);
        // El siguiente umbral de v se ajusta junto con la transición de Mecanim
        /*if (v > 0.1|| ) {
			velocity *= forwardSpeed;       // Multiplicar moviendo velocidad
        } else if (v < -0.1) {
			velocity *= backwardSpeed;  // Multiplicar moviendo velocidad
        }*/
        velocity *= forwardSpeed;

        if (Input.GetButtonDown("Jump"))
        {  // Después de introducir la tecla de espacio


            if (isGrounded == true)
            {
                //if (currentBaseState.nameHash == locoState){ //アニメーションのステートがLocomotionの最中のみジャンプできる
                // Puedes saltar si la transición de estado no está en progreso
                //if (!anim.IsInTransition(0))

                rb.AddForce(Vector3.up * jumpPower, ForceMode.VelocityChange);
                anim.SetBool("Jump", true);     // Enviar bandera para cambiar para saltar al animador
                //}
            }
			//}
		}


        // Mueve el personaje con la tecla de entrada arriba y abajo.
        transform.localPosition += velocity * Time.fixedDeltaTime;


        // Gire el carácter en el eje Y con las teclas de entrada izquierda y derecha
        //transform.Rotate(0, h * rotateSpeed, 0)


        transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Camera.main.transform.up );

        // Abajo, procesando en cada estado de animador.
        // En locomoción
        // Cuando la capa base actual es locoState.
        if (currentBaseState.nameHash == locoState){
            // Cuando esté ajustando el colisionador con una curva, restablézcala por si acaso
            if (useCurves){
				resetCollider();
			}
		}
        // Procesamiento durante el SALTO
        // Cuando la capa base actual es jumpState
        else if (currentBaseState.nameHash == jumpState)
		{
            //cameraObject.SendMessage("setCameraPositionJumpView");	// Cambia a cámara mientras saltas.
            // Cuando el estado no está en transición
            //if(!anim.IsInTransition(0))
            //{

            // A continuación, el procesamiento en el caso de realizar el ajuste de la curva.
            if (useCurves){
                // Debajo de la curva JUMP 00 adjunta a la animación JumpHeight y GravityControl
                // JumpHeight: Altura de salto en SALTO 00 (0 a 1)
                // Control de Gravedad: 1 ⇒ Salto (gravedad no válida), 0 ⇒ Gravedad efectiva
                float jumpHeight = anim.GetFloat("JumpHeight");
					float gravityControl = anim.GetFloat("GravityControl"); 
					if(gravityControl > 0)
						rb.useGravity = false;  //Cortar la influencia de la gravedad durante el salto.

                // Drop Ray emitido desde el centro del personaje.
                Ray ray = new Ray(transform.position + Vector3.up, -Vector3.up);
					RaycastHit hitInfo = new RaycastHit();
                // Ajusta la altura y el centro del colector con la curva asociada a la animación JUMP 00 solo cuando la altura está sobre uso. Curvas Altura
                if (Physics.Raycast(ray, out hitInfo))
					{
						if (hitInfo.distance > useCurvesHeight)
						{
							col.height = orgColHight - jumpHeight;          // Altura de colisionador ajustada
                        float adjCenterY = orgVectColCenter.y + jumpHeight;
							col.center = new Vector3(0, adjCenterY, 0); // Centro de colisionador ajustado
                    }
						else{
                        // Cuando sea inferior al valor de umbral, devuélvalo al valor inicial (por si acaso)		
                        resetCollider();
						}
					}
				}
            // Restablecer el valor de Jump bool (No hacer bucle)
            anim.SetBool("Jump", false);
			//}
		}
        // Procesando en IDLE
        // Cuando la capa base actual es idleState
        else if (currentBaseState.nameHash == idleState)
		{
            // Cuando esté ajustando el colisionador con una curva, restablézcala por si acaso
            if (useCurves){
				resetCollider();
			}
            // Luego de ingresar la tecla de espacio estará en estado de reposo.
            //	if (Input.GetButtonDown("Jump")) {
            //		anim.SetBool("Jump", true);
            //	}
        }
        // Procesamiento durante REST
        // Cuando la capa base actual es restState
        else if (currentBaseState.nameHash == restState)
		{
            //cameraObject.SendMessage("setCameraPositionFrontView");		// Cambia la cámara al frente
            // Si el estado no está en transición, restablezca el valor Bool Rest (asegúrese de no hacer un bucle)
            if (!anim.IsInTransition(0))
			{
				anim.SetBool("Rest", false);
			}
		}
	}

    /*void OnGUI()
	{
		GUI.Box(new Rect(Screen.width -260, 10 ,250 ,150), "Interaction");
		GUI.Label(new Rect(Screen.width -245,30,250,30),"Up/Down Arrow : Go Forwald/Go Back");
		GUI.Label(new Rect(Screen.width -245,50,250,30),"Left/Right Arrow : Turn Left/Turn Right");
		GUI.Label(new Rect(Screen.width -245,70,250,30),"Hit Space key while Running : Jump");
		GUI.Label(new Rect(Screen.width -245,90,250,30),"Hit Spase key while Stopping : Rest");
		GUI.Label(new Rect(Screen.width -245,110,250,30),"Left Control : Front Camera");
		GUI.Label(new Rect(Screen.width -245,130,250,30),"Alt : LookAt Camera");
	}*/


    // Función de restablecimiento del tamaño del colisionador del personaje
    void resetCollider()
	{
        // Devolver valor inicial de altura, centro de componente
        col.height = orgColHight;
		col.center = orgVectColCenter;
	}
}
