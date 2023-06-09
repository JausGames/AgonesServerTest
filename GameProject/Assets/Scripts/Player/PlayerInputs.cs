
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

namespace Inputs
{
    public class PlayerInputs : NetworkBehaviour
    {
        [SerializeField] PlayerController motor = null;
        [SerializeField] CombatController shooter = null;

        [SerializeField] float stickLookSensibility = 10f;
        [SerializeField] float mouseLookSensibility = 1f;

        public void Start()
        {
            Debug.Log("Network Informations : IsOwner " + IsOwner);
            if (!IsOwner) return;

            OnlineInputManager.Controls.PlayerAction.Move.performed += ctx => OnMove(ctx.ReadValue<Vector2>());
            OnlineInputManager.Controls.PlayerAction.Move.canceled += _ => OnMove(Vector2.zero);



            OnlineInputManager.Controls.PlayerAction.Shoot.performed += _ => OnShoot(true);
            OnlineInputManager.Controls.PlayerAction.Shoot.canceled += _ => OnShoot(false);

            OnlineInputManager.Controls.PlayerAction.Look.performed += ctx => OnLook(ctx.ReadValue<Vector2>());
            OnlineInputManager.Controls.PlayerAction.Look.canceled += _ => OnLook(Vector2.zero);

            //OnlineInputManager.Controls.PlayerAction.LookStick.performed += ctx => OnLook((ctx.ReadValue<Vector2>().x * Vector2.right - ctx.ReadValue<Vector2>().y * Vector2.up) * stickLookSensibility);
            //OnlineInputManager.Controls.PlayerAction.LookStick.canceled += _ => OnLook(Vector2.zero);

            //OnlineInputManager.Controls.PlayerAction.SwitchItem.performed += _ => OnSwitchItem();
        }

        private void OnSwitchItem()
        {
            if (shooter == null || !IsOwner) return;
            shooter.SwitchItem();
        }


        private void OnLook(Vector2 vector2)
        {
            if (shooter == null || !IsOwner) return;
            vector2 = vector2.x * Screen.width * Vector2.right + vector2.x * Screen.height * Vector2.up;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            worldPosition.z = 0f;
            Debug.DrawLine(transform.position, worldPosition);
            shooter.Look(worldPosition);
        }

        public void OnShoot(bool context)
        {
            Debug.Log("Network Informations : IsLocalPlayer " + IsLocalPlayer);
            if (shooter == null || !IsOwner) return;

            //if (shooter.IsOnMenu && context) shooter.BuyItem();
            //else 
            shooter.Attack(context);
        }
        public void OnMove(Vector2 context)
        {
            Debug.Log(gameObject.ToString() + ", Network Informations : IsLocalPlayer " + IsLocalPlayer);
            //Debug.Log("Network Informations : IsLocalPlayer " + IsLocalPlayer);
            if (motor == null || !IsOwner) return;
            motor.Move = context;
        }
    }
}
