using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] BattleHUD hud;

    [SerializeField] bool isPlayerUnit;

    public Beast Beast { get; set; }

    public BattleHUD HUD { get { return hud; } }

    public bool IsPlayerUnit { get { return isPlayerUnit; } }

    private Image image;

    private Vector3 ogPos;
    private Vector3 ogScale;

    private Color ogColor;

    private void Awake()
    {
        image = GetComponent<Image>();

        ogPos = image.transform.localPosition;
        ogScale = image.transform.localScale;

        ogColor = image.color;
    }

    public void SetUnit(Beast beast)
    {
        Beast = beast;

        image.sprite = Beast.BeastBase.BattleSprite;
        var size = GetComponent<RectTransform>();
        size.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, image.sprite.rect.height);
        size.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, image.sprite.rect.width);

        HUD.gameObject.SetActive(true);
        HUD.SetHUD(beast);

        PlayEnterAnimation();
    }

    public void ClearUnit()
    {
        HUD.gameObject.SetActive(false);
    }

    public void PlayEnterAnimation()
    {
        image.color = ogColor;
        image.transform.localScale = ogScale;

        if (IsPlayerUnit) { image.transform.localPosition = new Vector3(-1500f, ogPos.y); }
        else { image.transform.localPosition = new Vector3(1500f, ogPos.y); }

        image.transform.DOLocalMoveX(ogPos.x, 1f);
    }

    public void PlayRetreatAnimation()
    {
        image.transform.DOLocalMoveX(ogPos.x - 1000f, 1f);
    }

    public void PlayAttackAnimation()
    {
        var sequence = DOTween.Sequence();

        if (IsPlayerUnit) { sequence.Append(image.transform.DOLocalMoveX(ogPos.x + 150f, 0.25f)); }
        else { sequence.Append(image.transform.DOLocalMoveX(ogPos.x - 150f, 0.25f)); }

        sequence.Append(image.transform.DOLocalMoveX(ogPos.x, 0.5f));
    }

    public void PlayHitAnimation()
    {
        var sequence = DOTween.Sequence();

        sequence.Append(image.DOColor(Color.red, 0.125f));
        if (IsPlayerUnit) { sequence.Join(image.transform.DOLocalMoveX(ogPos.x - 100f, 0.25f)); }
        else { sequence.Join(image.transform.DOLocalMoveX(ogPos.x + 100f, 0.25f)); }

        sequence.Append(image.transform.DOLocalMoveX(ogPos.x, 0.5f));
        sequence.Join(image.DOColor(ogColor, 0.125f));
    }

    public void PlayDodgeAnimation()
    {
        var sequence = DOTween.Sequence();

        if (IsPlayerUnit) { sequence.Join(image.transform.DOLocalMoveX(ogPos.x - 100f, 0.25f)); }
        else { sequence.Join(image.transform.DOLocalMoveX(ogPos.x + 100f, 0.25f)); }

        sequence.Append(image.transform.DOLocalMoveX(ogPos.x, 0.5f));
    }

    public void PlayFaintAnimation()
    {
        var sequence = DOTween.Sequence();

        sequence.Append(image.transform.DOLocalMoveY(ogPos.y - 150f, 0.5f));
        sequence.Join(image.DOFade(0f, 0.5f));
    }

    public IEnumerator PlayCaptureAnimation()
    {
        var sequence = DOTween.Sequence();

        sequence.Append(image.DOFade(0f, 0.5f));
        sequence.Join(image.transform.DOLocalMoveX(ogPos.x - 280f, 0.5f));
        sequence.Join(image.transform.DOScale(new Vector3(0f, 0f, 1f), 0.5f));
        yield return sequence.WaitForCompletion();
    }

    public IEnumerator PlayBreakOutAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOFade(1f, 0.5f));
        sequence.Join(image.transform.DOLocalMoveX(ogPos.x, 0.5f));
        sequence.Join(image.transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f));
        yield return sequence.WaitForCompletion();
    }
}
