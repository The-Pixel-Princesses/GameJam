using UnityEngine;
using UnityEngine.UI;

public class GoldHealthBarScript : MonoBehaviour
{
    public Slider slider;
    public float maxValue = 10f;
    public float drainRate = 5f;   // how much drains per second
    public float refillRate = 10f; // how fast it refills near torch

    private float currentValue;
    private bool nearTorch = false;

    void Start()
    {
        currentValue = maxValue;
        slider.maxValue = maxValue;
        slider.value = currentValue;
    }

    void Update()
    {
        if (nearTorch)
        {
            Debug.Log("Refilling");
            currentValue += refillRate * Time.deltaTime;
        }
        else
        {
            Debug.Log("Draining");
            currentValue -= drainRate * Time.deltaTime;
        }

        currentValue = Mathf.Clamp(currentValue, 0, maxValue);
        slider.value = currentValue;
    }

    public void SetNearTorch(bool isNear)
    {
        Debug.Log("State: " + isNear);
        nearTorch = isNear;
    }
}
