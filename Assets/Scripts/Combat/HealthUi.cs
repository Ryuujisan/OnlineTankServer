using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUi : MonoBehaviour
{
    [SerializeField]
    private Health health;

    [SerializeField]
    private GameObject healthBarParent;

    [SerializeField]
    private Image healthBarImage;

    private void Awake()
    {
        health.ClientOnHealthUpdate += HandleHealthUpdated;
        healthBarParent.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        health.ClientOnHealthUpdate -= HandleHealthUpdated;
    }

    private void OnMouseEnter()
    {
        healthBarParent.gameObject.SetActive(true);
    }

    private void OnMouseExit()
    {
        healthBarParent.gameObject.SetActive(false);
    }

    private void HandleHealthUpdated(int currentHealth, int maxHealth)
    {
        healthBarImage.fillAmount = (float) currentHealth / maxHealth;
    }
}