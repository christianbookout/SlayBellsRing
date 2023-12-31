using System;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 3f;
    public float rotationSpeed = 2f;
    public float runSpeed = 5f;
    [Header("Stamina")]
    public float maxStamina = 100f;
    public float stamina = 100f;
    public float staminaDepletionRate = 10f;
    public float staminaRegenRate = 5f; 
    public float runCooldownSeconds = 2f;
    [Header("Headbob/Breathing")]
    public float headbobWalkMagnitude = 0.05f;
    public float headbobRunMagnitude = 0.1f;
    public float headbobWalkSpeed = 1f;
    public float headbobRunSpeed = 2f;
    public float breathingAmplitude = 0.1f;
    public float breatheSpeed = 0.5f;
    public Camera playerCamera;
    public AudioSource breathing;

    public bool IsRunning { get; private set; } = false;
    public bool IsWalking { get; private set; } = false;
    
    private CharacterController characterController;
    private PlayerFootsteps playerFootsteps;
    private Vector3 originalCameraPosition;
    private float headbobTimer = 0.0f;
    private float breatheTimer = 0.0f;
    private bool lastFrameWasMoving = false;
    private float targetBobPos = 0f;
    private float runCooldownTimer = 0f;
    private bool onStaminaCooldown = false;

    public void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        playerFootsteps = GetComponent<PlayerFootsteps>();
        originalCameraPosition = playerCamera.transform.localPosition;
        targetBobPos = originalCameraPosition.y;
        GameManager.Instance.StartGame();
    }

    public void Update()
    {
        if (GameManager.Instance.gameState != GameManager.GameState.Playing) return;
        HandleMovementInput();
        HandleStaminaCooldown();
        AddGravity();
        BreathSound();
    }    

    public void BreathSound() {
        // Play the breath sound louder/faster when stamina is depleted.
        breathing.volume = 1 - (stamina / maxStamina);
        breathing.pitch = 2 - (stamina / maxStamina);
        
        if (stamina > 0 && !breathing.isPlaying) {
            breathing.Play();
        }
        else if (stamina <= 0 && breathing.isPlaying) {
            breathing.Stop();
        }
    }
    
    private void HandleStaminaCooldown()
    {
        if (onStaminaCooldown)
        {
            runCooldownTimer -= Time.deltaTime;
            if (runCooldownTimer <= 0)
            {
                onStaminaCooldown = false;
            }
        }
    }

    private void AddGravity() {
        var gravity = Physics.gravity;
        characterController.Move(gravity * Time.deltaTime);
    }

    private float GetSpeed(bool attemptingToRun) {
        float curSpeed = speed;
        if (attemptingToRun && !onStaminaCooldown)
        {
            if (stamina > 0)
            {
                IsRunning = true;
                playerFootsteps.isRunning = true;
                curSpeed = runSpeed;
                stamina -= staminaDepletionRate * Time.deltaTime;
            }
            else
            {
                IsRunning = false;
                playerFootsteps.isRunning = false;
                onStaminaCooldown = true;
                runCooldownTimer = runCooldownSeconds;
            }
        }
        else
        {
            stamina += staminaRegenRate * Time.deltaTime;
            IsRunning = false;
            playerFootsteps.isRunning = false;
        }
        stamina = Mathf.Clamp(stamina, 0, maxStamina);
        return curSpeed;
    }
    private float verticalRotation = 0f;
    private void HandleMovementInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        Vector3 moveDirection = transform.TransformDirection(direction);

        var attemptingToRun = Input.GetKey(KeyCode.LeftShift);

        var curSpeed = GetSpeed(attemptingToRun);
        characterController.Move(moveDirection * curSpeed * Time.deltaTime);

        float mouseX = Input.GetAxisRaw("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxisRaw("Mouse Y") * rotationSpeed;

        verticalRotation += mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        transform.Rotate(0f, mouseX, 0f);
        playerCamera.transform.localEulerAngles = new Vector3(-verticalRotation, 0f, 0f);

        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, out _, 1.5f);
        bool isMoving = isGrounded && characterController.velocity.magnitude > 0;
        playerFootsteps.SetIsMoving(isMoving);

        if (isMoving) {
            if (!lastFrameWasMoving) headbobTimer = 0f;
            headbobTimer += Time.deltaTime * (IsRunning ? headbobRunSpeed : headbobWalkSpeed);
            Headbob(IsRunning ? headbobRunMagnitude : headbobWalkMagnitude);
        } else {
            if (lastFrameWasMoving) breatheTimer = 0f;
            breatheTimer += Time.deltaTime * breatheSpeed * (10 - (10 * (stamina / maxStamina)));
            BreatheHeadBob();
        }
        // Lerp between current camera y and target bob position
        playerCamera.transform.localPosition = new Vector3(
            playerCamera.transform.localPosition.x,
            Mathf.Lerp(playerCamera.transform.localPosition.y, targetBobPos, Time.deltaTime * 10f),
            playerCamera.transform.localPosition.z
        );
        lastFrameWasMoving = isMoving;
        IsWalking = isMoving && !IsRunning;
    }

    private void BreatheHeadBob()
    {
        float breathingEffect = Mathf.Sin(breatheTimer) * breathingAmplitude;
        targetBobPos = originalCameraPosition.y + breathingEffect;
    }

    private void Headbob(float magnitude)
    {
        float bobbingEffect = Mathf.Sin(headbobTimer) * magnitude;
        targetBobPos = originalCameraPosition.y + bobbingEffect;
    }
}
