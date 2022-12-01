using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FacingDirection { Down, Up, Left, Right }

public class Character : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] FacingDirection defaultDir = FacingDirection.Down;

    private Animator charAnim;

    public FacingDirection DefaultDir { get { return defaultDir; } }

    public Animator Anim { get { return charAnim; } }
    public bool IsMoving { get; set; }

    private void Awake()
    {
        charAnim = GetComponent<Animator>();
    }

    private void Start()
    {
        SetFacingDirection(DefaultDir);
    }

    public void HandleUpdate()
    {
        charAnim.SetBool("isMoving", IsMoving);
    }

    public IEnumerator Move(Vector2 moveVec, Action onMoveOver = null)
    {
        charAnim.SetFloat("moveX", Mathf.Clamp(moveVec.x, -1f, 1f));
        charAnim.SetFloat("moveY", Mathf.Clamp(moveVec.y, -1f, 1f));

        Vector3 targetPos = transform.position;
        targetPos.x += moveVec.x;
        targetPos.y += moveVec.y;

        if (!IsWalkable(targetPos)) { yield break; }

        IsMoving = true;

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;

        IsMoving = false;

        onMoveOver?.Invoke();
    }

    private bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, Mathf.Epsilon, GameLayers.Instance.Collision
            | GameLayers.Instance.Interactable) != null) { return false; }

        return true;
    }

    public void LookToward(Vector3 targetPos)
    {
        var xDiff = Mathf.Floor(targetPos.x - transform.position.x);
        var yDiff = Mathf.Floor(targetPos.y - transform.position.y);

        if (xDiff == 0 || yDiff == 0)
        {
            charAnim.SetFloat("moveX", Mathf.Clamp(xDiff, -1f, 1f));
            charAnim.SetFloat("moveY", Mathf.Clamp(yDiff, -1f, 1f));
        }
    }

    public void SetFacingDirection(FacingDirection dir)
    {
        if (dir == FacingDirection.Left) { charAnim.SetFloat("moveX", 1); }
        else if (dir == FacingDirection.Right) { charAnim.SetFloat("moveX", -1); }
        else if (dir == FacingDirection.Up) { charAnim.SetFloat("moveY", 1); }
        else if (dir == FacingDirection.Down) { charAnim.SetFloat("moveY", -1); }
    }
}
