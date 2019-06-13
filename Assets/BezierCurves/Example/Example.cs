using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BezierCurve
{
	public class Example : MonoBehaviour
	{
		public BezierCurve BezierCurve;
		public int Count;

		private void Awake()
		{
			float t = 1.0f / Count;
			for (int iSphere = 0; iSphere < Count; iSphere++)
			{
				Vector3 position = BezierCurve.EvaluateInBezier(t * iSphere);
				GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				gameObject.name = iSphere.ToString();
				gameObject.transform.SetParent(transform, false);

				gameObject.transform.position = position;
			}
		}

		private void Update()
		{
			float t = 1.0f / Count;
			for (int iSphere = 0; iSphere < Count; iSphere++)
			{
				transform.GetChild(iSphere).position = BezierCurve.EvaluateInBezier(t * iSphere);
			}
		}
	}
}