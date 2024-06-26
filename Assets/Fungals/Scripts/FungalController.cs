﻿using UnityEngine;
using UnityEngine.Events;

public abstract class EntityController : MonoBehaviour
{
    [SerializeField] private bool hasInteraction = false;

    public bool HasInteraction => hasInteraction;
    public abstract Sprite ActionImage { get; }
    public abstract Color ActionColor { get; }
    public abstract string ActionText { get; }

    public abstract void UseAction();
}

public enum FungalState
{
    RANDOM,
    ESCORT,
    TARGET,
}

[RequireComponent(typeof(RandomMovement))]
public class FungalController : EntityController
{
    [Header("References")]
    [SerializeField] private Transform indicatorAnchor;
    [SerializeField] private RectTransform hungerIndicator;
    [SerializeField] private Camera spotlightCamera;

    public FungalModel Model { get; private set; }
    public GameObject Render { get; private set; }
    public bool IsFollowing { get; set; }

    public Camera SpotlightCamera => spotlightCamera;

    public override Sprite ActionImage => Model.Data.ActionImage;
    public override Color ActionColor => Model.Data.ActionColor;
    public override string ActionText => "Talk";

    private Camera mainCamera;
    private MoveController movement;
    private RandomMovement randomMovement;

    private float hungerTimer;
    private FungalState state;

    public event UnityAction OnTalkStart;

    private void Awake()
    {
        mainCamera = Camera.main;
        movement = GetComponent<MoveController>();
        randomMovement = GetComponent<RandomMovement>();
    }

    public void Initialize(FungalModel model, Collider bounds)
    {
        Debug.Log($"initializing fungal controller {model}");

        Model = model;

        if (model)
        {
            name = $"Fungal Controller - {model.name}";
            Render = Instantiate(model.Data.Prefab, transform);
            Render.transform.localScale = Vector3.one;

            var animator = Render.GetComponentInChildren<Animator>();
            animator.speed = 0.25f;
            randomMovement.SetAnimator(animator);
            randomMovement.SetBounds(bounds);

            movement.SetSpeed(1f + model.Speed * 0.1f);

            SetState(FungalState.RANDOM);
        }
    }

    public void MoveToPosition(Vector3 position, Transform lookTarget = null)
    {
        movement.SetPosition(position, () =>
        {
            if (lookTarget) movement.SetLookTarget(lookTarget);
        });
    }

    public void MoveToTarget(Transform target)
    {
        movement.SetTarget(target);
        SetState(FungalState.TARGET);
    }

    public void Stop()
    {
        IsFollowing = false;
        movement.Stop();
        SetState(FungalState.RANDOM);
    }

    public void Escort(Transform target)
    {
        IsFollowing = true;
        movement.SetTarget(target);
        SetState(FungalState.ESCORT);
    }

    public void Unescort()
    {
        IsFollowing = false;
        movement.Stop();
        SetState(FungalState.RANDOM);
    }

    private void Update()
    {
        UpdateHunger();
    }

    private void SetState(FungalState state)
    {
        this.state = state;
        randomMovement.enabled = state == FungalState.RANDOM;
    }

    public override void UseAction()
    {
        OnTalkStart?.Invoke();
    }

    private void UpdateHunger()
    {
        if (Model)
        {
            hungerTimer += Time.deltaTime;

            if (hungerTimer > 5)
            {
                Model.Hunger -= 5 / (1 + Model.Stamina * 0.1f);
                hungerTimer = 0;
            }

            if (Model.Hunger < 30)
            {
                hungerIndicator.gameObject.SetActive(true);
                var position = mainCamera.WorldToScreenPoint(indicatorAnchor.transform.position);
                hungerIndicator.position = position;
            }
            else
            {
                hungerIndicator.gameObject.SetActive(false);
            }
        }
    }
}
