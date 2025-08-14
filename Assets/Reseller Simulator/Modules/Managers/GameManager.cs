using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public TimeManager Time; // Your TimeManager instance
    public DayTransitionController dayTransitionController;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            float actualSpeed = Time.timeSpeed;

            Time.timeSpeed = actualSpeed * 2;
        }

        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            float actualSpeed = Time.timeSpeed;

            Time.timeSpeed = actualSpeed / 2;
        }
    }
}
