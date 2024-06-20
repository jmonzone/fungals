using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CookingStation : JobStation
{
    [Header("Gameplay References")]
    [SerializeField] private IngredientManager ingredientManager;
    [SerializeField] private BladeController bladeController;

    [Header("UI References")]
    [SerializeField] private Image background;

    [Header("Position References")]
    [SerializeField] private Transform fungalPositionAnchor;
    [SerializeField] private Transform fungalLookTarget;

    public override void SetFungal(FungalController fungal)
    {
        if (fungal)
        {
            fungal.MoveToPosition(fungalPositionAnchor.position, fungalLookTarget);
        }
    }

    protected override void OnJobStarted()
    {
    }

    protected override void OnCameraPrepared()
    {
        bladeController.enabled = true;

        StartCoroutine(background.LerpAlpha(0.75f, () =>
        {
            ingredientManager.enabled = true;
        }));
    }

    protected override void OnJobEnded()
    {
    }

    protected override void OnBackButtonClicked()
    {
        StopAllCoroutines();
        bladeController.enabled = false;

        StartCoroutine(background.LerpAlpha(0, () =>
        {
            EndAction();
            ingredientManager.enabled = false;
        }));
    }


}
