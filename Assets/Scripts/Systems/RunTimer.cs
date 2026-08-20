using UnityEngine;

public class RunTimer : MonoBehaviour
{
    [SerializeField]
    private PlayerHealth playerHealth;

    public float ElapsedTime { get; private set; }
    public bool IsRunning { get; private set; } = true;

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.Died += StopTimer;
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.Died -= StopTimer;
        }
    }

    private void Update()
    {
        if (IsRunning)
        {
            ElapsedTime += Time.deltaTime;
        }
    }

    public void StopTimer()
    {
        IsRunning = false;
    }
}