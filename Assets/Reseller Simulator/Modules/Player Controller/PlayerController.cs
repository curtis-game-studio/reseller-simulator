using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public bool canMove { get; private set; } = true;
    public bool isSprinting => canSprint && sprintToggled;
    public bool shouldJump => Input.GetKeyDown(jumpKey) && characterController.isGrounded && !isCrouching;
    public bool shouldCrouch => Input.GetKeyDown(crouchKey) && !duringCrouchAnimation && characterController.isGrounded;
    private float GetCurrentOffset => isCrouching ? baseStepSpeed * crouchStepMultiplier : isSprinting ? baseStepSpeed * sprintStepMultiplier : baseStepSpeed;

    [Header("Functional Options")]
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool canCrouch = true;
    [SerializeField] private bool canUseHeadbob = true;
    [SerializeField] private bool canSlideOnSlopes = true;
    [SerializeField] private bool canZoom = true;
    [SerializeField] private bool canInteract = true;
    [SerializeField] private bool useFootsteps = true;

    [Header("Controls")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode crouchKey = KeyCode.C;
    [SerializeField] private KeyCode zoomKey = KeyCode.Mouse1;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float slopeSpeed = 1.5f;
    [SerializeField] private float gravity = 30.0f;
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float standingHeight = 2.0f;
    [SerializeField] private float timeToCrouch = 0.25f;
    [SerializeField] private Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 standingCenter = new Vector3(0, 0, 0);
    private bool sprintToggled = false;
    private bool isCrouching;
    private bool duringCrouchAnimation;

    [Header("Camera Settings")]
    [SerializeField, Range(1, 10)] private float lookSpeedX = 2.0f;
    [SerializeField, Range(1, 10)] private float lookSpeedY = 2.0f;
    [SerializeField, Range(1, 180)] private float upperLookLimit = 80.0f;
    [SerializeField, Range(1, 180)] private float lowerLookLimit = 80.0f;
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = 0.1f;
    [SerializeField] private float crouchBobSpeed = 8.0f;
    [SerializeField] private float crouchBobAmount = 0.025f;
    [SerializeField] private float timeToZoom = 0.3f;
    [SerializeField] private float zoomFOV = 30;
    [SerializeField] private float sprintFOV = 70f;
    [SerializeField] private float fovChangeSpeed = 5f;

    private float defaultYPos = 0;
    private float defaultFOV;
    private Coroutine zoomRoutine;
    private float timer;

    [Header("Footstep Parameters")]
    [SerializeField] private float baseStepSpeed = 0.5f;
    [SerializeField] private float crouchStepMultiplier = 1.5f;
    [SerializeField] private float sprintStepMultiplier = 0.6f;

    public List<TagFootstepSound> tagFootstepSounds;
    public List<TerrainLayerFootstep> terrainFootstepSounds;

    [System.Serializable]
    public class TagFootstepSound
    {
        public string tag;
        public SoundSO footstepSound;
    }

    [System.Serializable]
    public class TerrainLayerFootstep
    {
        public string layerName;
        public SoundSO footstepSound;
    }

    private float footstepTimer;

    private Vector3 hitPointNormal;
    private bool isSliding
    {
        get
        {
            if (characterController.isGrounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, 2f))
            {
                hitPointNormal = slopeHit.normal;
                return Vector3.Angle(hitPointNormal, Vector3.up) > characterController.slopeLimit;
            }
            else
            {
                return false;
            }
        }
    }

    [Header("Interaction Settings")]
    [SerializeField] private Vector3 interactionRayPoint = default;
    [SerializeField] private float interactionDistance = default;
    [SerializeField] private LayerMask interactionLayer = default;
    private Interactable currentInteractable;

    private Camera playerCamera;
    private CharacterController characterController;
    private Vector3 moveDirection;
    private Vector2 currentInput;
    private float rotationX = 0;
    private bool wasGrounded;
    private float verticalVelocity;



    void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        defaultYPos = playerCamera.transform.localPosition.y;
        defaultFOV = playerCamera.fieldOfView;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (canMove)
        {
            HandleMovementInput();
            HandleMouseLook();

            if (canJump) HandleJump();
            if (canCrouch) HandleCrouch();
            if (canUseHeadbob) HandleHeadbob();
            if (canZoom) HandleCameraZoom();
            if (useFootsteps) HandleFootsteps();

            if (canInteract)
            {
                HandleInteractionCheck();
                HandleInteractionInput();
            }

            // Update vertical velocity while in the air
            if (!characterController.isGrounded)
            {
                verticalVelocity = moveDirection.y;
            }

            // Detect landing
            if (!wasGrounded && characterController.isGrounded)
            {
                // Optional: only play if falling fast enough
                if (Mathf.Abs(verticalVelocity) > 2f) 
                {
                    PlayLandingSound();
                }
            }

            wasGrounded = characterController.isGrounded;

            ApplyFinalMovements();
        }
    }

    private void HandleMovementInput()
    {
        if (Input.GetKeyDown(sprintKey)) sprintToggled = !sprintToggled;
        if (Input.GetAxis("Vertical") == 0 && Input.GetAxis("Horizontal") == 0 || duringCrouchAnimation) sprintToggled = false;

        float speed = isCrouching ? crouchSpeed : isSprinting ? sprintSpeed : walkSpeed;
        currentInput = new Vector2(Input.GetAxis("Vertical") * speed, Input.GetAxis("Horizontal") * speed);

        float moveDirectionY = moveDirection.y;
        moveDirection = transform.TransformDirection(Vector3.forward) * currentInput.x + transform.TransformDirection(Vector3.right) * currentInput.y;
        moveDirection.y = moveDirectionY;
    }

    private void HandleJump()
    {
        if (shouldJump) moveDirection.y = jumpForce;
    }

    private void HandleCrouch()
    {
        if (shouldCrouch) StartCoroutine(CrouchAndStand());
    }

    private void HandleFootsteps()
    {
        if (!characterController.isGrounded || currentInput == Vector2.zero) return;

        footstepTimer -= Time.deltaTime;
        if (footstepTimer > 0) return;

        SoundSO soundToPlay = null;

        if (Physics.Raycast(characterController.transform.position, Vector3.down, out RaycastHit hit, 3))
        {
            Terrain terrain = hit.collider.GetComponent<Terrain>();
            if (terrain != null) soundToPlay = GetSoundForTerrainLayer(hit.point);
            else soundToPlay = GetSoundForTag(hit.collider.tag);
        }

        if (soundToPlay != null)
        {
            AudioManager.Instance.PlayOneShot(soundToPlay.soundName, transform.position);
        }

        footstepTimer = GetCurrentOffset;
    }

    private SoundSO GetSoundForTag(string tag)
    {
        foreach (var entry in tagFootstepSounds) if (entry.tag == tag && entry.footstepSound != null) return entry.footstepSound;
        foreach (var entry in tagFootstepSounds) if (entry.tag == "Default" && entry.footstepSound != null) return entry.footstepSound;
        return null;
    }

    private SoundSO GetSoundForTerrainLayer(Vector3 worldPos)
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null) return GetSoundForTag("Default");

        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.InverseTransformPoint(worldPos);
        int mapX = (int)((terrainPos.x / terrainData.size.x) * terrainData.alphamapWidth);
        int mapZ = (int)((terrainPos.z / terrainData.size.z) * terrainData.alphamapHeight);

        float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

        int maxIndex = 0;
        float maxMix = 0f;
        for (int i = 0; i < splatmapData.GetLength(2); i++)
        {
            if (splatmapData[0, 0, i] > maxMix) { maxMix = splatmapData[0, 0, i]; maxIndex = i; }
        }

        if (terrainData.terrainLayers.Length <= maxIndex) return GetSoundForTag("Default");

        string layerName = terrainData.terrainLayers[maxIndex].name;
        foreach (var entry in terrainFootstepSounds) if (entry.layerName == layerName && entry.footstepSound != null) return entry.footstepSound;

        return GetSoundForTag("Default");
    }

    private void PlayLandingSound()
    {
        SoundSO soundToPlay = null;

        if (Physics.Raycast(characterController.transform.position, Vector3.down, out RaycastHit hit, 3))
        {
            Terrain terrain = hit.collider.GetComponent<Terrain>();
            if (terrain != null) soundToPlay = GetSoundForTerrainLayer(hit.point);
            else soundToPlay = GetSoundForTag(hit.collider.tag);
        }

        if (soundToPlay != null) AudioManager.Instance.PlayOneShot(soundToPlay.soundName, transform.position);
    }

    private void HandleMouseLook()
    {
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedX, 0);

        if (zoomRoutine != null || isCrouching) return;

        float targetFOV = isSprinting ? sprintFOV : defaultFOV;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * fovChangeSpeed);
    }

    private void HandleHeadbob()
    {
        if (!characterController.isGrounded) return;
        if (Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.z) > 0.1f)
        {
            timer += Time.deltaTime * (isCrouching ? crouchBobSpeed : isSprinting ? sprintBobSpeed : walkBobSpeed);
            playerCamera.transform.localPosition = new Vector3(
                playerCamera.transform.localPosition.x,
                defaultYPos + Mathf.Sin(timer) * (isCrouching ? crouchBobAmount : isSprinting ? sprintBobAmount : walkBobAmount),
                playerCamera.transform.localPosition.z);
        }
    }

    private void HandleCameraZoom()
    {
        if (Input.GetKeyDown(zoomKey)) { if (zoomRoutine != null) StopCoroutine(zoomRoutine); zoomRoutine = StartCoroutine(ToggleZoom(true)); }
        if (Input.GetKeyUp(zoomKey)) { if (zoomRoutine != null) StopCoroutine(zoomRoutine); zoomRoutine = StartCoroutine(ToggleZoom(false)); }
    }

    private void HandleInteractionCheck()
    {
        if (Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactionDistance))
        {
            if (hit.collider.gameObject.layer == 6 && (currentInteractable == null || hit.collider.GetInstanceID() != currentInteractable.GetInstanceID()))
            {
                hit.collider.TryGetComponent(out currentInteractable);
                currentInteractable?.OnFocus();
            }
        }
        else if (currentInteractable)
        {
            currentInteractable.OnLoseFocus();
            currentInteractable = null;
        }
    }

    private void HandleInteractionInput()
    {
        if (Input.GetKeyDown(interactKey) && currentInteractable != null &&
            Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactionDistance, interactionLayer))
        {
            currentInteractable.OnInteract();
        }
    }

    private void ApplyFinalMovements()
    {
        if (!characterController.isGrounded) moveDirection.y -= gravity * Time.deltaTime;
        if (canSlideOnSlopes && isSliding) moveDirection += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slopeSpeed;

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private IEnumerator CrouchAndStand()
    {
        if (isCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up, 1f)) yield break;

        duringCrouchAnimation = true;
        float timeElapsed = 0;
        float targetHeight = isCrouching ? standingHeight : crouchHeight;
        float currentHeight = characterController.height;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = characterController.center;

        while (timeElapsed < timeToCrouch)
        {
            characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch);
            characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / timeToCrouch);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        characterController.height = targetHeight;
        characterController.center = targetCenter;
        isCrouching = !isCrouching;
        duringCrouchAnimation = false;
    }

    private IEnumerator ToggleZoom(bool isEnter)
    {
        float targetFOV = isEnter ? zoomFOV : defaultFOV;
        float startingFOV = playerCamera.fieldOfView;
        float timeElapsed = 0;

        while (timeElapsed < timeToZoom)
        {
            playerCamera.fieldOfView = Mathf.Lerp(startingFOV, targetFOV, timeElapsed / timeToZoom);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        playerCamera.fieldOfView = targetFOV;
        zoomRoutine = null;
    }
}