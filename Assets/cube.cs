using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class cube : MonoBehaviour {
    public float velocidadMovimiento;
    public float jumpSpeed;
    public float runSpeed;
    public Text scoreText;
    public int puntaje;
    // Use this for initialization

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

    void Start () {
        velocidadMovimiento = 3f;
        jumpSpeed = 5f;
        runSpeed = 1f;
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            runSpeed = 2f;
        }
        else
        {
            runSpeed = 1f;
        }
        if ((int)(transform.position.y) > puntaje)
        {
            puntaje =(int)(transform.position.y);
            scoreText.text = "Score " + puntaje;
         }

        transform.Translate(Input.GetAxis("Horizontal") * velocidadMovimiento * Time.deltaTime *runSpeed, 0f, Input.GetAxis("Vertical") * velocidadMovimiento * Time.deltaTime *runSpeed);
        if (Input.GetKeyDown(KeyCode.Space)){
            if (isGrounded == true){
                GetComponent<Rigidbody>().velocity = Vector3.up * jumpSpeed;
            }
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.rotation = Quaternion.identity;
        }

    }
}
