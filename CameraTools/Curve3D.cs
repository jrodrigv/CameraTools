using UnityEngine;

namespace CameraTools
{
	public class Curve3D 
	{
		private Vector3[] points;
		private float[] times;
		private AnimationCurve[] curves;

		bool curveReady = false;

		// Use this for initialization
		public Curve3D() 
		{
			curves = new AnimationCurve[]{new AnimationCurve(), new AnimationCurve(), new AnimationCurve()};
		}

		public Curve3D(Vector3[] newPoints, float[] newTimes)
		{
			curves = new AnimationCurve[]{new AnimationCurve(), new AnimationCurve(), new AnimationCurve()};
			SetPoints(newPoints, newTimes);
		}

		public void SetPoints(Vector3[] newPoints, float[] newTimes)
		{
			if(newPoints.Length != newTimes.Length)
			{
				Debug.LogError("[CameraTools]: Curve3D: points array must be same length as times array");
				return;
			}
			points = new Vector3[newPoints.Length];
			times = new float[newPoints.Length];
			for(int i = 0; i < points.Length; i++)
			{
				points[i] = newPoints[i];
				times[i] = newTimes[i];
			}

			UpdateCurve();
		}

		public void SetPoint(int index, Vector3 newPoint, float newTime)
		{
			if(index < points.Length)
			{
				SetAnimKey(index, newPoint, newTime);
			}
			else
			{
				Debug.LogError("[CameraTools]: Tried to set new point in a Curve3D beyond the existing array.  Not yet implemented.");
			}
		}


		private void UpdateCurve()
		{
			curveReady = false;
			//clear existing keys
			for(int i = 0; i < 3; i++)
			{
				curves[i] = new AnimationCurve();
			}

			if(points.Length == 0) return;

			for(int i = 0; i < points.Length; i++)
			{
				SetAnimKey(i, points[i], times[i]);
			}

			curveReady = true;
		}

		void SetAnimKey(int index, Vector3 point, float time)
		{
			if(index >= curves[0].keys.Length)
			{
				curves[0].AddKey(time, point.x);
				curves[1].AddKey(time, point.y);
				curves[2].AddKey(time, point.z);
			}
			else
			{
				curves[0].MoveKey(index, new Keyframe(time, point.x));
				curves[1].MoveKey(index, new Keyframe(time, point.y));
				curves[2].MoveKey(index, new Keyframe(time, point.z));
			}
		}

		public Vector3 GetPoint(float time)
		{
			if(!curveReady)
			{
				Debug.LogWarning("[CameraTools]: Curve was accessed but it was not properly initialized.");
				return Vector3.zero;
			}

			float x = curves[0].Evaluate(time);
			float y = curves[1].Evaluate(time);
			float z = curves[2].Evaluate(time);
			return new Vector3(x,y,z);
		}

		public Vector3 GetTangent(float time)
		{
			if(!curveReady)
			{
				Debug.LogWarning("[CameraTools]: Curve was accessed but it was not properly initialized.");
				return Vector3.one;
			}

			if(time < 1)
			{
				return (GetPoint(Mathf.Min(time+0.01f, 1))-GetPoint(time)).normalized;
			}
			else
			{
				return (GetPoint(1)-GetPoint(0.99f)).normalized;
			}
		}


	}
}

