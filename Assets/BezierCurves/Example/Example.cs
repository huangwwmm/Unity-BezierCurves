using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BezierCurve
{
	public class Example : MonoBehaviour
	{
		public BezierCurve BezierCurve;
		public int Count;

		Transform[] ts;

		private void Awake()
		{
			float t = 1.0f / Count;
			ts = new Transform[Count];
			for (int iSphere = 0; iSphere < Count; iSphere++)
			{
				Vector3 position = BezierCurve.EvaluateInBezier_LocalSpace(t * iSphere);
				GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				go.name = iSphere.ToString();
				go.transform.SetParent(transform, false);

				go.transform.localPosition = position;
				ts[iSphere] = go.transform;
			}
		}

		private void Update()
		{
			float t = 1.0f / Count;
			for (int iSphere = 0; iSphere < Count; iSphere++)
			{
				ts[iSphere].localPosition = BezierCurve.EvaluateInBezier_LocalSpace(t * iSphere);
			}
		}
	}
}