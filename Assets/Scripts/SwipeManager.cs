using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Universal Touch and Mouse Swipe Detection for 3D Runner Game.
/// Supports both New Input System and Legacy Input Manager.
/// Detects taps, swipe left, swipe right, swipe up, and swipe down.
/// </summary>
public class SwipeManager : MonoBehaviour
{
    public static bool tap, swipeLeft, swipeRight, swipeUp, swipeDown;

    [Header("Swipe Threshold")]
    [Tooltip("Minimum pixel distance for a drag to count as a swipe")]
    [SerializeField] private float swipeThreshold = 50f;

    private bool isDragging = false;
    private Vector2 startTouch, swipeDelta;

    public static SwipeManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        tap = swipeLeft = swipeRight = swipeUp = swipeDown = false;

        ReadInputs();

        // Calculate drag delta
        swipeDelta = Vector2.zero;
        if (isDragging)
        {
            Vector2 currentPos = GetCurrentPointerPosition();
            swipeDelta = currentPos - startTouch;
        }

        // Did we cross the threshold?
        if (swipeDelta.magnitude > swipeThreshold)
        {
            float x = swipeDelta.x;
            float y = swipeDelta.y;

            if (Mathf.Abs(x) > Mathf.Abs(y))
            {
                // Horizontal
                if (x < 0) swipeLeft = true;
                else swipeRight = true;
            }
            else
            {
                // Vertical
                if (y < 0) swipeDown = true;
                else swipeUp = true;
            }

            ResetSwipe();
        }
    }

    private void ReadInputs()
    {
#if ENABLE_INPUT_SYSTEM
        // New Input System Touch
        bool touchProcessed = false;
        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;
            if (touch.press.wasPressedThisFrame)
            {
                tap = true;
                isDragging = true;
                startTouch = touch.position.ReadValue();
                touchProcessed = true;
            }
            else if (touch.press.wasReleasedThisFrame)
            {
                isDragging = false;
                ResetSwipe();
                touchProcessed = true;
            }
            else if (touch.press.isPressed)
            {
                touchProcessed = true;
            }
        }

        // New Input System Mouse (only when not interacting via active touchscreen touch)
        if (!touchProcessed && Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                tap = true;
                isDragging = true;
                startTouch = Mouse.current.position.ReadValue();
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                isDragging = false;
                ResetSwipe();
            }
        }

        if (touchProcessed || Mouse.current != null)
        {
            return;
        }
#endif

        // Fallback to Legacy Input
        try
        {
            if (Input.touches.Length > 0)
            {
                if (Input.touches[0].phase == UnityEngine.TouchPhase.Began)
                {
                    tap = true;
                    isDragging = true;
                    startTouch = Input.touches[0].position;
                }
                else if (Input.touches[0].phase == UnityEngine.TouchPhase.Ended || Input.touches[0].phase == UnityEngine.TouchPhase.Canceled)
                {
                    isDragging = false;
                    ResetSwipe();
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                tap = true;
                isDragging = true;
                startTouch = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                ResetSwipe();
            }
        }
        catch (System.Exception)
        {
            // Silently ignore if legacy input throws
        }
    }

    private Vector2 GetCurrentPointerPosition()
    {
#if ENABLE_INPUT_SYSTEM
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            return Touchscreen.current.primaryTouch.position.ReadValue();
        }
        if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }
#endif
        try
        {
            if (Input.touches.Length > 0) return Input.touches[0].position;
            return (Vector2)Input.mousePosition;
        }
        catch (System.Exception)
        {
            return startTouch;
        }
    }

    private void ResetSwipe()
    {
        startTouch = swipeDelta = Vector2.zero;
        isDragging = false;
    }
}
