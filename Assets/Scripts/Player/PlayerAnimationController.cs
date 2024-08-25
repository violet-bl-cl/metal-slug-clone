using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour {
    //Observer pattern this is streamer object
    public delegate void AnimationController();
    public event  AnimationController onAnimationChange;
    private PlayerController _playerController;

    void Start(){
        _playerController = GetComponent<PlayerController>(); 
    }
    void Update(){
        //add all animation logics here. make it clean.
        // if the key is pressed and the ground is attached and not crouching.
     //  if (isAnyDirectionKeyPressed && isGround && !isCrouch)
     //  {
     //      if (isLookUp && !isShoot)
     //      {
     //          _playerTopAction.Action = PlayerFSM.TopAimUp;
     //      }
     //      else if (isLookUp && isShoot)
     //      {
     //        //  Shoot(Direction.Up);
     //          _playerTopAction.Action = PlayerFSM.TopShootUp;
     //      }
     //      else if (!isLookUp && isShoot)
     //      {
     //         // Shoot(_direction);
     //          _playerTopAction.Action = PlayerFSM.TopShootRight;
     //      }
     //      else
     //      {
     //          _playerTopAction.Action = PlayerFSM.TopMoveRight;
     //      }
     //      _playerBotAction.Action = PlayerFSM.BotMoveLeft;
     //  }
     //  // if the key is pressed and the ground is not attached. 
     //  else if (isAnyDirectionKeyPressed && !isGround)
     //  {
     //      if (isLookUp && !isShoot && !isCrouch)
     //      {
     //          _playerTopAction.Action = PlayerFSM.TopAimUp;
     //      }
     //      else if (isLookUp && isShoot && !isCrouch)
     //      {
     //          _playerTopAction.Action = PlayerFSM.TopShootUp;
     //      }
     //      else if (!isLookUp && isShoot && !isCrouch)
     //      {
     //          _playerTopAction.Action = PlayerFSM.TopShootRight;
     //      }
     //      else if (!isLookUp && isCrouch && isShoot)
     //      {
     //          _playerTopAction.Action = PlayerFSM.TopShootDown;
     //      }
     //      else
     //      {
     //          _playerTopAction.Action = PlayerFSM.TopJumpRight;
     //      }
     //      _playerBotAction.Action = PlayerFSM.BotJumpLeft;
     //  }
     //  //
     //  else if (isAnyDirectionKeyNotPressed && !isGround)
     //  {
     //      if (isCrouch && !isLookUp && !isShoot)
     //      {
     //          _playerTopAction.Action = PlayerFSM.TopAimDown;
     //      }
     //      else if (isLookUp && !isCrouch && !isShoot)
     //      {
     //          _playerTopAction.Action = PlayerFSM.TopAimUp;
     //      }
     //      else if (isLookUp && !isCrouch && isShoot)
     //      {
     //         // Shoot(Direction.Up);
     //          _playerTopAction.Action = PlayerFSM.TopShootUp;
     //      }
     //      else if (!isLookUp && isCrouch && isShoot)
     //      {
     //        //  Shoot(Direction.Down);
     //          _playerTopAction.Action = PlayerFSM.TopShootDown;
     //      }
     //      else
     //      {
     //          _playerTopAction.Action = PlayerFSM.TopIdle;
     //      }
     //      _playerBotAction.Action = PlayerFSM.BotJumpIdle;
     //  }
     //  else if (isAnyDirectionKeyNotPressed && !isGround)
     //  {

     //      _playerTopAction.Action = isCrouch ? PlayerFSM.TopAimDown : PlayerFSM.TopIdle;
     //      _playerBotAction.Action = PlayerFSM.BotJumpIdle;
     //  }
     //  else if (isAnyDirectionKeyPressed && isCrouch && isGround)
     //  {
     //      if (!isLookUp && isShoot)
     //      {
     //          if (_inputCoroutine == null) _inputCoroutine = StartCoroutine(nameof(DelayInput));
     //          //Shoot(_direction);
     //          _playerFullAction.Action = PlayerFSM.FullCrouchShoot;
     //      }
     //      else
     //      {
     //          _playerFullAction.Action = PlayerFSM.FullCrouchMove;
     //      }
     //  }
     //  else if (isCrouch && isGround)
     //  {
     //      //calltime occurs
     //      if (!isLookUp && isShoot)
     //      {
     //          if (_inputCoroutine == null) _inputCoroutine = StartCoroutine(nameof(DelayInput));
     //         // Shoot(_direction);
     //          _playerFullAction.Action = PlayerFSM.FullCrouchShoot;
     //      }
     //      else
     //      {
     //          _playerFullAction.Action = PlayerFSM.FullCrouch;
     //      }
     //  }
     //  else if (!isCrouch && !isGround)
     //  {
     //      _playerTopAction.Action = (!isLookUp && isShoot) ? PlayerFSM.TopShootRight : PlayerFSM.TopIdle;
     //      _playerBotAction.Action = PlayerFSM.BotJumpIdle;
     //  }
     //  else
     //  {
     //      if (isLookUp && !isShoot)
     //      {
     //          _playerTopAction.Action = PlayerFSM.TopAimUp;
     //      }
     //      else if (isLookUp && isShoot)
     //      {
     //          Shoot(Direction.Up);
     //          _playerTopAction.Action = PlayerFSM.TopShootUp;
     //      }
     //      else if (!isLookUp && isShoot)
     //      {
     //          Shoot(_direction);
     //          _playerTopAction.Action = PlayerFSM.TopShootRight;
     //      }
     //      else
     //      {
     //          _playerTopAction.Action = PlayerFSM.TopIdle;

     //      }
     //      _playerBotAction.Action = PlayerFSM.BotIdle;
     //  }


        onAnimationChange?.Invoke();
    }
}