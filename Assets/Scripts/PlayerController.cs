using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Set in inspector")]
    [SerializeField] float speed;

    [Header("Debug")]
    // Actual pos/rot
    [SerializeField] Vector2 pos;
    [SerializeField] float rot;

    [SerializeField] Vector2 lookTarget;
    [SerializeField] Vector2 input;
    [SerializeField] Vector2 cursorWorldPos;

    // State from server
    public PlayerState playerState;
    CharacterController CC;

    float playerSpeed = 2.0f;

    private void Start()
    {
        CC = gameObject.GetComponent<CharacterController>();
    }

    void Update()
    {
        input = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    void FixedUpdate()
    {
        UpdateCursorWorldPos();
        DoMove();
    }

    void DoMove()
    {
        Vector2 pos2d = new Vector2(transform.position.x, transform.position.z);

        Vector3 move = speed * new Vector3(input.x, 0, input.y);
        CC.Move(move * Time.fixedDeltaTime * playerSpeed);

        // Rotate the player to face the direction of the cursor
        lookTarget = cursorWorldPos - pos2d;

        // Rotate the player to face the direction of the cursor
        if (lookTarget.magnitude > 0.1f)
        {
            rot = Mathf.Atan2(lookTarget.x, lookTarget.y) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, rot, 0);
        }

        NetworkController.instance.ReportMove(GameState.instance.WorldToNormalizedPos(
            new Vector3(transform.position.x, 0, transform.position.z)
        ), rot);

    }

    void UpdateCursorWorldPos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f))
        {
            cursorWorldPos = new Vector2(hit.point.x, hit.point.z);
        }
    }

    public void SetPlayerState(PlayerState playerState)
    {
        this.playerState = playerState;
    }
}