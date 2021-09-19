using EFT.Trainer.Configuration;
using EFT.Trainer.Extensions;
using UnityEngine;

#nullable enable

namespace EFT.Trainer.Features
{
	public class Aimbot : HoldMonoBehaviour
	{
		public override KeyCode Key { get; set; } = KeyCode.Mouse1; //Only execute Aimbot while right mouse button is held

		[ConfigurationProperty]
		public float MaximumDistance { get; set; } = 300f;
		public float RotationRate { get; set; } = 8f; //Value to smooth the rotation of Aimbot
		public float MaximumAimField { get; set; } = 16f; //Value to limit Aimbot's Field of View (0-360, angle)
		
		public enum TargetBones {
			Head,
			Neck,
			Spine3,
			LeftShoulder,
			RightShoulder,
			Ribcage,
			Spine2,
			RightUpperarm,
			LeftUpperarm,
			Stomach,
			Spine1,
			RightForearm,
			LeftForearm,
			Spine1,
			Pelvis,
			RightPalm,
			LeftPalm,
			RightThigh2,
			LeftThigh2,
			RightCalf,
			LeftCalf,
			RightFoot,
			LeftFoot
		}
		
		/**
		 * (BillyBishop) TODO:
		 *
		 *	   * Implement the property 'MaximumAimField' to limit the Aimbot's Field of View.
		 *
		 *     * Add and implement 'GetBestBone(player)' for dynamically targeting bones.
		 *	     - It should return the first visible bone position of a given player, iterating in this order:  head>body>limbs
		 *		 - Make use of the 'TargetBones' for full list of potential bones
		 *		 - Returns empty vec3 if no bones were visible
		 **/

		protected override void UpdateWhenHold()
		{
			var state = GameState.Current;
			if (state == null)
				return;

			var camera = state.Camera;
			if (camera == null)
				return;

			var localPlayer = state.LocalPlayer;
			if (localPlayer == null)
				return;

			var nearestTarget = Vector3.zero;
			var nearestTargetDistance = float.MaxValue;

			foreach (var player in state.Hostiles)
			{
				if (player == null)
					continue;

				var destination = GetHeadPosition(player);
				if (destination == Vector3.zero)
					continue;

				var screenPosition = camera.WorldPointToScreenPoint(destination);
				if (!camera.IsScreenPointVisible(screenPosition))
					continue;

				var distance = Vector3.Distance(camera.transform.position, player.Transform.position);
				if (distance > MaximumDistance)
					continue;

				if (distance >= nearestTargetDistance)
					continue;

				var template = localPlayer.Weapon?.CurrentAmmoTemplate;
				if (template == null)
					continue;

				nearestTargetDistance = distance;
				var travelTime = distance / template.InitialSpeed;
				destination.x += (player.Velocity.x * travelTime);
				destination.y += (player.Velocity.y * travelTime);

				nearestTarget = destination;
			}

			if (nearestTarget != Vector3.zero)
				AimAtPosition(localPlayer, nearestTarget);
		}

		private static void AimAtPosition(Player player, Vector3 position)
		{
			var delta = player.Fireport.position - player.Fireport.up * 1f;
			var eulerAngles = Quaternion.LookRotation((position - delta).normalized).eulerAngles;
			
			if (eulerAngles.x > 180f)
				eulerAngles.x -= 360f;

			player.MovementContext.Rotation = new Vector2(eulerAngles.y / RotationRate, eulerAngles.x / RotationRate);
		}
	
		public static Vector3 GetHeadPosition(Player player)
		{
			var bones = player.PlayerBones;
			if (bones == null)
				return Vector3.zero;

			var head = bones.Head;
			return head?.position ?? Vector3.zero;
		}
	}
}
