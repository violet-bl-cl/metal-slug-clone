using UnityEngine;

public class PlayerTopAction : MonoBehaviour
{
    public PlayerFSM Action = PlayerFSM.TopIdle;
    [SerializeField]
    private Animator _playerAnimator;
    public SpriteRenderer spriteRenderer;
    void Start()
    {
        _playerAnimator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    void Update()
    {
        switch (Action)
        {
            //Top Conditions
            case PlayerFSM.TopIdle:
                {
                    Vector2 topPosition = transform.localPosition;
                    topPosition.y = 0.6f;
                    transform.localPosition = new Vector2(topPosition.x, topPosition.y);
                    _playerAnimator.SetBool("IsWalk", false);
                    _playerAnimator.SetBool("IsJump", false);
                    _playerAnimator.SetBool("IsDown", false);
                    _playerAnimator.SetBool("IsUp", false);
                    break;
                }
            case PlayerFSM.TopAimUp:
                {
                    Vector2 topPosition = transform.localPosition;
                    topPosition.y = 0.6f;
                    transform.localPosition = new Vector2(topPosition.x, topPosition.y);
                    _playerAnimator.SetBool("IsUp", true);
                    _playerAnimator.SetBool("IsDown", false);
                    _playerAnimator.SetBool("IsWalk", false);
                    _playerAnimator.SetBool("IsJump", false);
                    break;
                }
            case PlayerFSM.TopAimDown:
                {
                    Vector2 topPosition = transform.localPosition;
                    topPosition.y = 0.35f;
                    transform.localPosition = new Vector2(topPosition.x, topPosition.y);
                    _playerAnimator.SetBool("IsDown", true);
                    _playerAnimator.SetBool("IsWalk", false);
                    _playerAnimator.SetBool("IsJump", false);
                    _playerAnimator.SetBool("IsUp", false);
                    break;
                }
            case PlayerFSM.TopMoveRight:
                {
                    Vector2 topPosition = transform.localPosition;
                    topPosition.y = 0.6f;
                    transform.localPosition = new Vector2(topPosition.x, topPosition.y);
                    _playerAnimator.SetBool("IsWalk", true);
                    _playerAnimator.SetBool("IsJump", false);
                    _playerAnimator.SetBool("IsDown", false);
                    _playerAnimator.SetBool("IsUp", false);
                    break;
                }
            case PlayerFSM.TopJumpRight:
                {
                    Vector2 topPosition = transform.localPosition;
                    topPosition.y = 0.6f;
                    transform.localPosition = new Vector2(topPosition.x, topPosition.y);
                    _playerAnimator.SetBool("IsJump", true);
                    _playerAnimator.SetBool("IsWalk", false);
                    _playerAnimator.SetBool("IsDown", false);
                    _playerAnimator.SetBool("IsUp", false);
                    break;
                }
            case PlayerFSM.TopShootRight:
                {
                    _playerAnimator.SetTrigger("ShootRight");
                    break;
                }
            case PlayerFSM.TopShootUp:
                {
                    Vector2 topPosition = transform.localPosition;
                    topPosition.y = 0.6f;
                    transform.localPosition = new Vector2(topPosition.x, topPosition.y);
                    _playerAnimator.SetTrigger("ShootUp");
                    break;
                }
            case PlayerFSM.TopShootDown:
                {
                    Vector2 topPosition = transform.localPosition;
                    topPosition.y = 0.6f;
                    transform.localPosition = new Vector2(topPosition.x, topPosition.y);
                    _playerAnimator.SetTrigger("ShootDown");
                    break;
                }
            default:
                {

                    break;
                }
        }
    }
}