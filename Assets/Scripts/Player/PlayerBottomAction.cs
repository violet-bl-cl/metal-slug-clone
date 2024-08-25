using Unity.VisualScripting;
using UnityEngine;

public class PlayerBottomAction : MonoBehaviour
{
    public PlayerFSM Action = PlayerFSM.BotIdle;
    [SerializeField]
    private Animator m_playerAnimator;
    private string m_currentAction = "";
    public SpriteRenderer BotRenderer;
    void Start()
    {
        m_playerAnimator = GetComponent<Animator>();
        BotRenderer = GetComponent<SpriteRenderer>();
    }
    private void OnEnable() {
        
    }

    private void OnDsiable(){

    }
    void UpdatePlayerAnimation()
    {
        switch (Action)
        {
            case PlayerFSM.BotJumpIdle:
                {
                    CheckAnimation("MarcoBotFalling");
                    break;
                }
            case PlayerFSM.BotJumpLeft:
                {
                    CheckAnimation("MarcoBotJump");
                    break;
                }
            case PlayerFSM.BotMoveLeft:
                {
                    CheckAnimation("MarcoBotWalk");
                    break;
                }
            case PlayerFSM.BotIdle:
                {
                    CheckAnimation("MarcoBotIdle");
                    break;
                }
        }
    }


    public void CheckAnimation(string currentAction = "", float transitionTime = 0.2f)
    {
        if (m_currentAction != currentAction)
        {
            m_currentAction = currentAction;
            m_playerAnimator.CrossFade(currentAction, transitionTime);
        }
    }
}