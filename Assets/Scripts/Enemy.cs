using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    public float runningSpeed = 1.5f;

    public int enemyDamage = 10;

    Rigidbody2D rigidBody;
    Collider2D bodyCollider;

    public bool facingRight = false;

    private bool isDying = false;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
    }

    private void FixedUpdate()
    {
        if (isDying)
        {
            return;
        }

        float currentRunningSpeed = runningSpeed;

        if (facingRight)
        {
            //Mirando hacia la derecha
            currentRunningSpeed = runningSpeed;
            this.transform.eulerAngles = new Vector3(0, 180, 0);
        }
        else
        {
            //Mirando hacia la izquierda
            currentRunningSpeed = -runningSpeed;
            this.transform.eulerAngles = Vector3.zero;
        }

        if (GameManager.sharedInstance.currentGameState == GameState.inGame)
        {
            rigidBody.linearVelocity = new Vector2(currentRunningSpeed,
                                             rigidBody.linearVelocity.y);
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDying)
        {
            return;
        }

        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();

            //Si el jugador cae desde arriba, elimina al enemigo;
            //si lo toca de lado, recibe daño
            Rigidbody2D playerBody = collision.attachedRigidbody;
            bool stomp = playerBody != null &&
                         playerBody.linearVelocity.y < -0.5f &&
                         collision.bounds.min.y >= bodyCollider.bounds.center.y;

            if (stomp)
            {
                player.BounceUp();
                Die();
            }
            else
            {
                player.CollectHealth(-enemyDamage);
            }
            return;
        }

        //Ignorar coleccionables y zonas invisibles del nivel:
        //el enemigo solo debe girar al chocar con muros o suelo
        if (collision.CompareTag("Empanada") ||
            collision.GetComponent<Collectable>() != null ||
            collision.GetComponent<ExitZone>() != null ||
            collision.GetComponent<KillZone>() != null)
        {
            return;
        }

        facingRight = !facingRight;
    }

    private void Die()
    {
        isDying = true;
        rigidBody.simulated = false;
        bodyCollider.enabled = false;
        GetComponent<SpriteRenderer>().flipY = true;

        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }

        Destroy(this.gameObject, 0.6f);
    }
}
