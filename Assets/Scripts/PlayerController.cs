using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    //Variables del movimiento del personaje
    public float jumpForce = 6f;
    public float runningSpeed = 2f;

    Rigidbody2D rigidBody;
    Animator animator;
    AudioSource audioSource;
    BoxCollider2D feetCollider;
    SpriteRenderer spriteRenderer;
    Vector3 startPosition;
    Vector3 baseScale;

    const string STATE_ALIVE = "itsAlive";
    const string STATE_ON_THE_GROUND = "itsOnTheGround";

    private int healthPoints, manaPoints;
    private bool isDead = false;

    public const int INITIAL_HEALTH = 200, INITIAL_MANA = 15,
        MAX_HEALTH = 200, MAX_MANA = 50,
        MIN_HEALTH = 10, MIN_MANA = 0;

    public const int SUPERJUMP_COST = 5;
    public const float SUPERJUMP_FORCE = 1.5f;

    //Margen de tolerancia del salto: permite saltar justo después de salir
    //de una plataforma (coyote time) o si se presionó el botón un instante
    //antes de aterrizar (jump buffer)
    const float COYOTE_TIME = 0.12f;
    const float JUMP_BUFFER_TIME = 0.12f;
    const float GROUND_CHECK_DISTANCE = 0.1f;

    //Dificultad progresiva: la velocidad sube con la distancia recorrida
    const float SPEED_GAIN_PER_UNIT = 0.01f;
    const float MAX_EXTRA_SPEED = 2.5f;

    //Efecto de daño estilo Mario: el jugador se encoge según la vida
    //restante, parpadea y queda invulnerable un instante
    const float HURT_INVULNERABLE_TIME = 1.2f;
    const float MIN_SIZE_FACTOR = 0.6f;

    private float lastGroundedTime = float.NegativeInfinity;
    private float lastJumpPressedTime = float.NegativeInfinity;
    private bool superjumpQueued = false;
    private float invulnerableUntil = 0f;

    public LayerMask groundMask;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        feetCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Use this for initialization
    void Start () {

        startPosition = this.transform.position;
        baseScale = this.transform.localScale;
        healthPoints = INITIAL_HEALTH;
        manaPoints = INITIAL_MANA;
	}

    public void StartGame(){
        isDead = false;
        animator.SetBool(STATE_ALIVE, true);
        animator.SetBool(STATE_ON_THE_GROUND, true);

        healthPoints = INITIAL_HEALTH;
        manaPoints = INITIAL_MANA;

        invulnerableUntil = 0f;
        this.transform.localScale = baseScale;
        spriteRenderer.color = Color.white;

        Invoke("RestartPosition", 0.2f);
    }

    void RestartPosition(){
        this.transform.position = startPosition;
        this.rigidBody.linearVelocity = Vector2.zero;

        GameObject mainCamera = GameObject.Find("Main Camera");
        mainCamera.GetComponent<CameraFollow>().ResetCameraPosition();
    }

	// Update is called once per frame
	void Update () {
        if (Input.GetButtonDown("Jump"))
        {
            lastJumpPressedTime = Time.time;
            superjumpQueued = false;
        }

        if (Input.GetButtonDown("SuperJump"))
        {
            lastJumpPressedTime = Time.time;
            superjumpQueued = true;
        }

        bool onTheGround = IsTouchingTheGround();
        if (onTheGround)
        {
            lastGroundedTime = Time.time;
        }
        animator.SetBool(STATE_ON_THE_GROUND, onTheGround);

        if (GameManager.sharedInstance.currentGameState == GameState.inGame &&
            Time.time - lastJumpPressedTime <= JUMP_BUFFER_TIME &&
            Time.time - lastGroundedTime <= COYOTE_TIME)
        {
            //Consumir el salto para que no se repita
            lastJumpPressedTime = float.NegativeInfinity;
            lastGroundedTime = float.NegativeInfinity;
            Jump(superjumpQueued);
        }
	}

    void FixedUpdate()
    {
        if (GameManager.sharedInstance.currentGameState == GameState.inGame)
        {
            float currentSpeed = GetCurrentRunningSpeed();
            if (rigidBody.linearVelocity.x < currentSpeed)
            {
                rigidBody.linearVelocity = new Vector2(currentSpeed, //x
                                                 rigidBody.linearVelocity.y //y
                                                );
            }
        }else{//Si no estamos dentro de la partida
            rigidBody.linearVelocity = new Vector2(0, rigidBody.linearVelocity.y);
        }
    }

    //La velocidad aumenta con la distancia recorrida (hasta un tope)
    //para que la partida se complique progresivamente
    public float GetCurrentRunningSpeed()
    {
        float extraSpeed = Mathf.Clamp(GetTravelledDistance() * SPEED_GAIN_PER_UNIT,
                                       0f, MAX_EXTRA_SPEED);
        return runningSpeed + extraSpeed;
    }

    void Jump(bool superjump)
    {
        float jumpForceFactor = jumpForce;
        if (superjump && manaPoints >= SUPERJUMP_COST){
            manaPoints -= SUPERJUMP_COST;
            jumpForceFactor *= SUPERJUMP_FORCE;
        }

        audioSource.Play();

        //Anular la velocidad vertical antes de saltar para que
        //todos los saltos tengan la misma altura
        rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, 0);
        rigidBody.AddForce(Vector2.up * jumpForceFactor, ForceMode2D.Impulse);
    }

    //Rebote al eliminar un enemigo cayendo sobre él
    public void BounceUp()
    {
        rigidBody.linearVelocity = new Vector2(rigidBody.linearVelocity.x, 0);
        rigidBody.AddForce(Vector2.up * jumpForce * 0.8f, ForceMode2D.Impulse);
    }

    //Nos indica si el personaje está o no tocando el suelo:
    //proyecta la caja de los pies ligeramente hacia abajo
    bool IsTouchingTheGround(){
        Bounds feet = feetCollider.bounds;
        return Physics2D.BoxCast(feet.center,
                                 new Vector2(feet.size.x * 0.9f, feet.size.y),
                                 0f,
                                 Vector2.down,
                                 GROUND_CHECK_DISTANCE,
                                 groundMask);
    }


    public void Die(){
        //Evita que la muerte se procese más de una vez
        //(por ejemplo KillZone y vida en 0 al mismo tiempo)
        if (isDead)
        {
            return;
        }
        isDead = true;

        float travelledDistance = GetTravelledDistance();
        float previousMaxDistance = PlayerPrefs.GetFloat("maxscore",0f);
        if (travelledDistance > previousMaxDistance)
        {
            PlayerPrefs.SetFloat("maxscore", travelledDistance);
        }

        this.animator.SetBool(STATE_ALIVE, false);
        GameManager.sharedInstance.GameOver();
    }

    public void CollectHealth(int points) {
        if (isDead)
        {
            return;
        }

        //Durante la invulnerabilidad el daño se ignora (como Mario al encogerse)
        if (points < 0 && Time.time < invulnerableUntil)
        {
            return;
        }

        this.healthPoints = Mathf.Clamp(this.healthPoints + points, 0, MAX_HEALTH);

        if (this.healthPoints <= 0)
        {
            Die();
            return;
        }

        UpdateSizeByHealth();

        if (points < 0)
        {
            invulnerableUntil = Time.time + HURT_INVULNERABLE_TIME;
            StartCoroutine(HurtBlink());
        }
    }

    //El tamaño del jugador refleja su vida restante
    void UpdateSizeByHealth()
    {
        float healthFraction = Mathf.Clamp01((float)healthPoints / INITIAL_HEALTH);
        float sizeFactor = Mathf.Lerp(MIN_SIZE_FACTOR, 1f, healthFraction);
        this.transform.localScale = baseScale * sizeFactor;
    }

    //Parpadeo en rojo mientras dura la invulnerabilidad
    IEnumerator HurtBlink()
    {
        while (Time.time < invulnerableUntil && !isDead)
        {
            spriteRenderer.color = new Color(1f, 0.4f, 0.4f, 0.5f);
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }
        spriteRenderer.color = Color.white;
    }

    public void CollectMana(int points) {
        this.manaPoints = Mathf.Clamp(this.manaPoints + points, MIN_MANA, MAX_MANA);
    }


    public int GetHealth()
    {
        return healthPoints;
    }

    public int GetMana()
    {
        return manaPoints;
    }

    public float GetTravelledDistance()
    {
        return this.transform.position.x - startPosition.x;
    }
}
