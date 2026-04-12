using UnityEngine;
using Mirror;
using Unity.Mathematics;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movimento")]
    public float MoveSpd;
    public float WalkSpd = 5f;
    public float RunSpd = 12f;
    public float JumpForce = 20f;

    [Header("GroundCheck")]
    public LayerMask GroundL;
    public float GroundDis = 0.2f;
    public Transform GroundCheck;
    public bool IsGround;

    public float MoveX;
    public float MoveY;
    Rigidbody rig;

    void Awake()
    {
        rig = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer) return;
        if (InventoryUI.isOpen)
        {
            rig.linearVelocity = new Vector3(0, rig.linearVelocity.y, 0);
            return;
        }

        MoveX = Input.GetAxis("Horizontal");
        MoveY = Input.GetAxis("Vertical");

        MoveSpd = Input.GetKey(KeyCode.LeftShift) ? RunSpd : WalkSpd;
        IsGround = Physics.CheckSphere(GroundCheck.position, GroundDis, GroundL);

        Move();
        Jump();
    }

    void Move()
    {
        Vector3 direction = new Vector3(MoveX, 0, MoveY).normalized;
        Vector3 velocity = transform.TransformDirection(direction) * MoveSpd;
        rig.linearVelocity = new Vector3(velocity.x, rig.linearVelocity.y, velocity.z);
    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump") && IsGround)
        {
            rig.linearVelocity = new Vector3(rig.linearVelocity.x, 0f, rig.linearVelocity.z);
            rig.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
        }
    }
}
