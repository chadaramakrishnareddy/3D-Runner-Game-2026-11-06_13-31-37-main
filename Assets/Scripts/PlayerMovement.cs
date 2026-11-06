using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Subway Surfers style 3D runner controller.
/// Sprints forward at constant speed with smooth 3-lane horizontal steering.
/// Features lively procedural running animation (alternating leg strides,
/// opposing arm swings, torso bounce, forward lean, lane roll), jump mechanics,
/// slide mechanics, crash stumble pose, and dust particle effects.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [Header("Forward Movement")]
    [Tooltip("Constant forward sprinting speed")]
    [SerializeField] private float forwardSpeed = 15f;

    [Header("3-Lane Movement")]
    [Tooltip("Distance between lanes: Left = -3, Center = 0, Right = +3")]
    [SerializeField] private float laneDistance = 3.0f;
    [Tooltip("Speed of switching between lanes")]
    [SerializeField] private float laneChangeSpeed = 18f;
    [Tooltip("Maximum horizontal clamp")]
    [SerializeField] private float maxHorizontalLimit = 4.2f;

    [Header("Jump & Vertical Physics")]
    [SerializeField] private float jumpForce = 9.5f;
    [SerializeField] private float gravity = 28f;
    private float verticalVelocity = 0f;
    private bool isGrounded = true;

    [Header("Character Rig Components")]
    [SerializeField] private Transform characterModel;
    [SerializeField] private Transform torsoTransform;
    [SerializeField] private Transform headTransform;
    [SerializeField] private Transform leftArmTransform;
    [SerializeField] private Transform rightArmTransform;
    [SerializeField] private Transform leftLegTransform;
    [SerializeField] private Transform rightLegTransform;

    [Header("Subway Surfers Animation")]
    [SerializeField] private float runAnimationFrequency = 14f;
    [SerializeField] private float legSwingAngle = 42f;
    [SerializeField] private float armSwingAngle = 45f;
    [SerializeField] private float runBobHeight = 0.14f;
    [SerializeField] private float runForwardLean = 8f;
    [SerializeField] private float laneRollAngle = 18f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem runDustParticles;

    // Movement state
    private int currentLane = 0;
    private float targetX = 0f;
    private bool isDead = false;
    private bool isSliding = false;
    private Vector3 startPosition;
    private float runCadence = 0f;

    // Component references
    private CharacterController characterController;
    private Rigidbody rb;
    private Animator animator;

    // Animator parameter hashes
    private static readonly int HashIsDead = Animator.StringToHash("IsDead");
    private static readonly int HashSpeed = Animator.StringToHash("Speed");

    public float CurrentDistance => transform.position.z - startPosition.z;
    public bool IsDead => isDead;
    public float ForwardSpeed => forwardSpeed;
    public int CurrentLane => currentLane;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;

        AutoFindRigReferences();

        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // Ensure SwipeManager exists for mobile touch / mouse gestures
        if (SwipeManager.Instance == null && FindFirstObjectByType<SwipeManager>() == null)
        {
            GameObject swipeObj = new GameObject("SwipeManager");
            swipeObj.AddComponent<SwipeManager>();
        }
    }

    private void Start()
    {
        currentLane = 0;
        targetX = 0f;
        isDead = false;
        runCadence = 0f;
        verticalVelocity = -0.5f;

        if (runDustParticles != null && !runDustParticles.isPlaying)
        {
            runDustParticles.Play();
        }

        if (animator != null)
        {
            animator.applyRootMotion = false;
            animator.SetFloat(HashSpeed, 1f);
            animator.SetBool(HashIsDead, false);
        }
    }

    private void AutoFindRigReferences()
    {
        if (characterModel == null)
        {
            characterModel = transform.Find("CharacterModel");
        }

        if (characterModel != null)
        {
            if (torsoTransform == null) torsoTransform = characterModel.Find("Torso");
            if (headTransform == null) headTransform = characterModel.Find("Head");
            if (leftArmTransform == null) leftArmTransform = characterModel.Find("LeftArm");
            if (rightArmTransform == null) rightArmTransform = characterModel.Find("RightArm");
            if (leftLegTransform == null) leftLegTransform = characterModel.Find("LeftLeg");
            if (rightLegTransform == null) rightLegTransform = characterModel.Find("RightLeg");
        }

        if (runDustParticles == null)
        {
            runDustParticles = GetComponentInChildren<ParticleSystem>();
        }
    }

    private void Update()
    {
        if (isDead)
        {
            AnimateDeathFall();
            return;
        }

        HandleInput();
        MovePlayer();
        AnimateSubwaySurferRunning();
    }

    /// <summary>
    /// Dual Input System reader: supports both New Input System and Legacy Input Manager.
    /// Supports Left/Right lane shifting, Space/Up jump, and Down/Slide mechanics with mobile swipes.
    /// </summary>
    private void HandleInput()
    {
        bool leftPressed = false;
        bool rightPressed = false;
        bool jumpPressed = false;
        bool slidePressed = false;

        // Mobile touch & mouse swipe inputs
        if (SwipeManager.swipeLeft) leftPressed = true;
        if (SwipeManager.swipeRight) rightPressed = true;
        if (SwipeManager.swipeUp) jumpPressed = true;
        if (SwipeManager.swipeDown) slidePressed = true;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (!leftPressed) leftPressed = Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame;
            if (!rightPressed) rightPressed = Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame;
            if (!jumpPressed) jumpPressed = Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame;
            if (!slidePressed) slidePressed = Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame;
        }
#endif

        if (!leftPressed && !rightPressed && !jumpPressed && !slidePressed)
        {
            try
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                {
                    leftPressed = true;
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                {
                    rightPressed = true;
                }
                else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                {
                    jumpPressed = true;
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                {
                    slidePressed = true;
                }
            }
            catch (System.Exception)
            {
                // Silently ignored if activeInputHandler exclusively requires New Input System
            }
        }

        // Lane steering
        if (leftPressed && currentLane > -1)
        {
            currentLane--;
            targetX = currentLane * laneDistance;
        }
        else if (rightPressed && currentLane < 1)
        {
            currentLane++;
            targetX = currentLane * laneDistance;
        }

        // Jump trigger
        if (jumpPressed && isGrounded && !isSliding)
        {
            verticalVelocity = jumpForce;
            isGrounded = false;
            if (runDustParticles != null && runDustParticles.isPlaying)
            {
                runDustParticles.Stop();
            }
        }

        // Slide / Dive down trigger
        if (slidePressed)
        {
            if (!isGrounded)
            {
                // Fast-dive to the ground
                verticalVelocity = -jumpForce * 1.5f;
            }
            else if (!isSliding)
            {
                StartCoroutine(PerformSlide());
            }
        }
    }

    private System.Collections.IEnumerator PerformSlide()
    {
        isSliding = true;

        if (characterController != null)
        {
            characterController.height = 0.8f;
            characterController.center = new Vector3(0f, 0.4f, 0f);
        }

        if (characterModel != null)
        {
            characterModel.localRotation = Quaternion.Euler(35f, 0f, 0f);
            characterModel.localPosition = new Vector3(0f, -0.3f, 0f);
        }

        yield return new WaitForSeconds(0.75f);

        if (characterController != null)
        {
            characterController.height = 2.0f;
            characterController.center = new Vector3(0f, 1.0f, 0f);
        }

        if (characterModel != null)
        {
            characterModel.localRotation = Quaternion.Euler(runForwardLean, 0f, 0f);
            characterModel.localPosition = Vector3.zero;
        }

        isSliding = false;
    }

    /// <summary>
    /// Moves player forward along Z, slides horizontally along X, and handles vertical jump/gravity.
    /// </summary>
    private void MovePlayer()
    {
        // Smooth horizontal steering
        float newX = Mathf.MoveTowards(transform.position.x, targetX, laneChangeSpeed * Time.deltaTime);
        newX = Mathf.Clamp(newX, -maxHorizontalLimit, maxHorizontalLimit);

        // Constant forward velocity
        float forwardDelta = forwardSpeed * Time.deltaTime;

        // Vertical physics (gravity and jump)
        if (characterController != null && characterController.enabled)
        {
            if (characterController.isGrounded)
            {
                if (verticalVelocity < 0f)
                {
                    verticalVelocity = -1.5f; // Stick to road
                    isGrounded = true;
                    if (runDustParticles != null && !runDustParticles.isPlaying)
                    {
                        runDustParticles.Play();
                    }
                }
            }
            else
            {
                verticalVelocity -= gravity * Time.deltaTime;
                isGrounded = false;
            }

            Vector3 motion = new Vector3(newX - transform.position.x, verticalVelocity * Time.deltaTime, forwardDelta);
            characterController.Move(motion);
        }
        else
        {
            float newZ = transform.position.z + forwardDelta;
            transform.position = new Vector3(newX, transform.position.y, newZ);
        }

        // Subway Surfers dynamic banking tilt when steering between lanes
        if (characterModel != null)
        {
            float deltaX = targetX - transform.position.x;
            float targetTilt = -Mathf.Clamp(deltaX, -1f, 1f) * laneRollAngle;
            Quaternion targetRot = Quaternion.Euler(runForwardLean, 0f, targetTilt);
            characterModel.localRotation = Quaternion.Slerp(characterModel.localRotation, targetRot, Time.deltaTime * 12f);
        }
    }

    /// <summary>
    /// Animates the 3D runner character in authentic Subway Surfers style:
    /// alternating leg strides, opposing arm swings, bouncing spine cadence,
    /// and airborne jump tucking.
    /// </summary>
    private void AnimateSubwaySurferRunning()
    {
        if (isSliding) return;

        if (isGrounded)
        {
            runCadence += Time.deltaTime * runAnimationFrequency;

            // Spine vertical bounce
            if (characterModel != null)
            {
                float bobOffset = Mathf.Abs(Mathf.Sin(runCadence)) * runBobHeight;
                Vector3 mPos = characterModel.localPosition;
                mPos.y = bobOffset;
                characterModel.localPosition = mPos;
            }

            // Alternating leg strides
            float legAngle = Mathf.Sin(runCadence) * legSwingAngle;
            if (leftLegTransform != null)
            {
                leftLegTransform.localRotation = Quaternion.Euler(legAngle, 0f, 0f);
            }
            if (rightLegTransform != null)
            {
                rightLegTransform.localRotation = Quaternion.Euler(-legAngle, 0f, 0f);
            }

            // Opposing arm swings
            float armAngle = Mathf.Sin(runCadence + Mathf.PI) * armSwingAngle;
            if (leftArmTransform != null)
            {
                leftArmTransform.localRotation = Quaternion.Euler(armAngle, 0f, 0f);
            }
            if (rightArmTransform != null)
            {
                rightArmTransform.localRotation = Quaternion.Euler(-armAngle, 0f, 0f);
            }

            // Gentle head bob
            if (headTransform != null)
            {
                headTransform.localRotation = Quaternion.Euler(Mathf.Sin(runCadence * 0.5f) * 4f, 0f, 0f);
            }
        }
        else
        {
            // Airborne Jump Pose: legs tucked up, arms reaching forward for balance
            if (leftLegTransform != null)
            {
                leftLegTransform.localRotation = Quaternion.Slerp(leftLegTransform.localRotation, Quaternion.Euler(35f, 0f, 0f), Time.deltaTime * 10f);
            }
            if (rightLegTransform != null)
            {
                rightLegTransform.localRotation = Quaternion.Slerp(rightLegTransform.localRotation, Quaternion.Euler(20f, 0f, 0f), Time.deltaTime * 10f);
            }
            if (leftArmTransform != null)
            {
                leftArmTransform.localRotation = Quaternion.Slerp(leftArmTransform.localRotation, Quaternion.Euler(-35f, 0f, 0f), Time.deltaTime * 10f);
            }
            if (rightArmTransform != null)
            {
                rightArmTransform.localRotation = Quaternion.Slerp(rightArmTransform.localRotation, Quaternion.Euler(-35f, 0f, 0f), Time.deltaTime * 10f);
            }
        }
    }

    /// <summary>
    /// Crash knockdown animation when hitting an obstacle.
    /// </summary>
    private void AnimateDeathFall()
    {
        if (characterModel != null)
        {
            Quaternion crashRot = Quaternion.Euler(-55f, 30f, -15f);
            characterModel.localRotation = Quaternion.Slerp(characterModel.localRotation, crashRot, Time.deltaTime * 8f);

            Vector3 targetPos = characterModel.localPosition;
            targetPos.y = Mathf.Lerp(targetPos.y, -0.3f, Time.deltaTime * 6f);
            characterModel.localPosition = targetPos;
        }

        if (leftArmTransform != null)
        {
            leftArmTransform.localRotation = Quaternion.Slerp(leftArmTransform.localRotation, Quaternion.Euler(-70f, 40f, 0f), Time.deltaTime * 8f);
        }
        if (rightArmTransform != null)
        {
            rightArmTransform.localRotation = Quaternion.Slerp(rightArmTransform.localRotation, Quaternion.Euler(-50f, -30f, 0f), Time.deltaTime * 8f);
        }
        if (leftLegTransform != null)
        {
            leftLegTransform.localRotation = Quaternion.Slerp(leftLegTransform.localRotation, Quaternion.Euler(45f, 15f, 0f), Time.deltaTime * 8f);
        }
        if (rightLegTransform != null)
        {
            rightLegTransform.localRotation = Quaternion.Slerp(rightLegTransform.localRotation, Quaternion.Euler(-20f, -10f, 0f), Time.deltaTime * 8f);
        }
    }

    /// <summary>
    /// Halts player upon colliding with an obstacle.
    /// </summary>
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        if (runDustParticles != null)
        {
            runDustParticles.Stop();
        }

        if (animator != null)
        {
            animator.SetBool(HashIsDead, true);
            animator.SetFloat(HashSpeed, 0f);
        }
    }

    /// <summary>
    /// Resets player position and state for restarting a run.
    /// </summary>
    public void ResetPlayer()
    {
        StopAllCoroutines();
        isSliding = false;
        if (characterController != null)
        {
            characterController.height = 2.0f;
            characterController.center = new Vector3(0f, 1.0f, 0f);
        }

        isDead = false;
        currentLane = 0;
        targetX = 0f;
        verticalVelocity = -0.5f;
        isGrounded = true;
        runCadence = 0f;
        transform.position = startPosition;

        if (characterModel != null)
        {
            characterModel.localPosition = Vector3.zero;
            characterModel.localRotation = Quaternion.Euler(runForwardLean, 0f, 0f);
        }

        if (animator != null)
        {
            animator.SetBool(HashIsDead, false);
            animator.SetFloat(HashSpeed, 1f);
        }

        if (runDustParticles != null)
        {
            runDustParticles.Play();
        }
    }
}
