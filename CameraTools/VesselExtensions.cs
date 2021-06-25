using System;
using UnityEngine;

namespace CameraTools
{
	public static class VesselExtensions
	{
		public static bool InOrbit(this Vessel v)
		{
			try
			{
				if (v == null) return false;
				return !v.LandedOrSplashed &&
						   (v.situation == Vessel.Situations.ORBITING ||
							v.situation == Vessel.Situations.SUB_ORBITAL ||
							v.situation == Vessel.Situations.ESCAPING);
			}
			catch (Exception e)
			{
				Debug.LogWarning("[CameraTools.VesselExtensions]: Exception thrown in InOrbit: " + e.Message + "\n" + e.StackTrace);
				return false;
			}
		}

		public static Vector3d Velocity(this Vessel v)
		{
			try
			{
				if (v == null) return Vector3d.zero;
				if (!v.InOrbit())
				{
					return v.srf_velocity;
				}
				else
				{
					return v.obt_velocity;
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning("[CameraTools.VesselExtensions]: Exception thrown in Velocity: " + e.Message + "\n" + e.StackTrace);
				return new Vector3d(0, 0, 0);
			}
		}

		public static double Speed(this Vessel v)
		{
			try
			{
				if (v == null) return 0;
				if (!v.InOrbit())
				{
					return v.srfSpeed;
				}
				else
				{
					return v.obt_speed;
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning("[CameraTools.VesselExtensions]: Exception thrown in Speed: " + e.Message + "\n" + e.StackTrace);
				return 0;
			}
		}	}
}