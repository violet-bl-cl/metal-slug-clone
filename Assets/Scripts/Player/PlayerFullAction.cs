using UnityEngine;

public class PlayerFullAction : MonoBehaviour
{
    public PlayerFSM Action = PlayerFSM.FullCrouch;
    [SerializeField]
    private Animator _playerAnimator;

    private float _fadeTime = 0.2f;
    public SpriteRenderer spriteRenderer;
    void Start()
    {
        _playerAnimator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    void FixedUpdate()
    {
        switch (Action)
        {
            //Full Conditions
            case PlayerFSM.FullCrouch:
                {
                    _playerAnimator.SetBool("IsCrouch", true);
                    _playerAnimator.SetBool("IsWalk", false);
                    break;
                }
            case PlayerFSM.FullCrouchStart:
                {

                    break;
                }
            case PlayerFSM.FullCrouchMove:
                {
                    _playerAnimator.SetBool("IsCrouch", true);
                    _playerAnimator.SetBool("IsWalk", true);
                    break;
                }
            case PlayerFSM.FullCrouchShoot:
                {
                    _playerAnimator.SetTrigger("ShootRight");
                    break;
                }
            case PlayerFSM.FullSpawn:
                {
                    break;
                }
            default:
                {

                    break;
                }
        }
    }
}