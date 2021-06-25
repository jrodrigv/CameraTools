using System;
using System.Collections.Generic;
using UnityEngine;

namespace CameraTools
{
	public class CameraPath
	{
		public string pathName;
		int keyCount = 0;
		public int keyframeCount
		{
			get
			{
				return keyCount;
			}
			private set
			{
				keyCount = value;
			}
		}
		public List<Vector3> points;
		public List<Quaternion> rotations;
		public List<float> times;
		public List<float> zooms;

		public float lerpRate = 0.3f;
		public float timeScale = 1;

		Vector3Animation pointCurve;
		RotationAnimation rotationCurve;
		AnimationCurve zoomCurve;

		public CameraPath()
		{
			pathName = "New Path";
			points = new List<Vector3>();
			rotations = new List<Quaternion>();
			times = new List<float>();
			zooms = new List<float>();
		}

		public static CameraPath Load(ConfigNode node)
		{
			CameraPath newPath = new CameraPath();

			newPath.pathName = node.GetValue("pathName");
			newPath.points = ParseVectorList(node.GetValue("points"));
			newPath.rotations = ParseQuaternionList(node.GetValue("rotations"));
			newPath.times = ParseFloatList(node.GetValue("times"));
			newPath.zooms = ParseFloatList(node.GetValue("zooms"));
			newPath.lerpRate = float.Parse(node.GetValue("lerpRate"));
			newPath.timeScale = float.Parse(node.GetValue("timeScale"));
			newPath.Refresh();

			return newPath;
		}

		public void Save(ConfigNode node)
		{
			Debug.Log("[CameraTools]: Saving path: " + pathName);
			ConfigNode pathNode = node.AddNode("CAMERAPATH");
			pathNode.AddValue("pathName", pathName);
			pathNode.AddValue("points", WriteVectorList(points));
			pathNode.AddValue("rotations", WriteQuaternionList(rotations));
			pathNode.AddValue("times", WriteFloatList(times));
			pathNode.AddValue("zooms", WriteFloatList(zooms));
			pathNode.AddValue("lerpRate", lerpRate);
			pathNode.AddValue("timeScale", timeScale);
		}

		public static string WriteVectorList(List<Vector3> list)
		{
			string output = string.Empty;
			foreach (var val in list)
			{
				output += ConfigNode.WriteVector(val) + ";";
			}
			return output;
		}

		public static string WriteQuaternionList(List<Quaternion> list)
		{
			string output = string.Empty;
			foreach (var val in list)
			{
				output += ConfigNode.WriteQuaternion(val) + ";";
			}
			return output;
		}

		public static string WriteFloatList(List<float> list)
		{
			string output = string.Empty;
			foreach (var val in list)
			{
				output += val.ToString() + ";";
			}
			return output;
		}

		public static List<Vector3> ParseVectorList(string arrayString)
		{
			string[] vectorStrings = arrayString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			List<Vector3> vList = new List<Vector3>();
			for (int i = 0; i < vectorStrings.Length; i++)
			{
				try
				{
					vList.Add(ConfigNode.ParseVector3(vectorStrings[i]));
				}
				catch (Exception e)
				{
					Debug.LogError("[CameraTools]: Failed to parse vector: --" + vectorStrings[i] + "--, reason: " + e.Message);
				}
			}

			return vList;
		}

		public static List<Quaternion> ParseQuaternionList(string arrayString)
		{
			string[] qStrings = arrayString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			List<Quaternion> qList = new List<Quaternion>();
			for (int i = 0; i < qStrings.Length; i++)
			{
				qList.Add(ConfigNode.ParseQuaternion(qStrings[i]));
			}

			return qList;
		}

		public static List<float> ParseFloatList(string arrayString)
		{
			string[] fStrings = arrayString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			List<float> fList = new List<float>();
			for (int i = 0; i < fStrings.Length; i++)
			{
				fList.Add(float.Parse(fStrings[i]));
			}

			return fList;
		}

		public void AddTransform(Transform cameraTransform, float zoom, float time)
		{
			points.Add(cameraTransform.localPosition);
			rotations.Add(cameraTransform.localRotation);
			zooms.Add(zoom);
			times.Add(time);
			keyframeCount = times.Count;
			Sort();
			UpdateCurves();
		}

		public void SetTransform(int index, Transform cameraTransform, float zoom, float time)
		{
			points[index] = cameraTransform.localPosition;
			rotations[index] = cameraTransform.localRotation;
			zooms[index] = zoom;
			times[index] = time;
			Sort();
			UpdateCurves();
		}

		public void Refresh()
		{
			keyframeCount = times.Count;
			Sort();
			UpdateCurves();
		}

		public void RemoveKeyframe(int index)
		{
			points.RemoveAt(index);
			rotations.RemoveAt(index);
			zooms.RemoveAt(index);
			times.RemoveAt(index);
			keyframeCount = times.Count;
			UpdateCurves();
		}

		public void Sort()
		{
			List<CameraKeyframe> keyframes = new List<CameraKeyframe>();
			for (int i = 0; i < points.Count; i++)
			{
				keyframes.Add(new CameraKeyframe(points[i], rotations[i], zooms[i], times[i]));
			}
			keyframes.Sort(new CameraKeyframeComparer());

			for (int i = 0; i < keyframes.Count; i++)
			{
				points[i] = keyframes[i].position;
				rotations[i] = keyframes[i].rotation;
				zooms[i] = keyframes[i].zoom;
				times[i] = keyframes[i].time;
			}
		}

		public CameraKeyframe GetKeyframe(int index)
		{
			int i = index;
			return new CameraKeyframe(points[i], rotations[i], zooms[i], times[i]);
		}

		public void UpdateCurves()
		{
			pointCurve = new Vector3Animation(points.ToArray(), times.ToArray());
			rotationCurve = new RotationAnimation(rotations.ToArray(), times.ToArray());
			zoomCurve = new AnimationCurve();
			for (int i = 0; i < zooms.Count; i++)
			{
				zoomCurve.AddKey(new Keyframe(times[i], zooms[i]));
			}
		}

		public CameraTransformation Evaulate(float time)
		{
			CameraTransformation tf = new CameraTransformation();
			tf.position = pointCurve.Evaluate(time);
			tf.rotation = rotationCurve.Evaluate(time);
			tf.zoom = zoomCurve.Evaluate(time);

			return tf;
		}





	}
}

