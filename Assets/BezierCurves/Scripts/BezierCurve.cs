using UnityEngine;
using System;
using System.Collections.Generic;

namespace BezierCurve
{
	[ExecuteInEditMode
	, Serializable]
	public class BezierCurve : MonoBehaviour
	{
		/// <summary>
		/// Used to determine if the curve should be drawn as "closed" in the editor
		/// Used to determine if the curve's length should include the curve between the first and the last points in "points" array
		/// </summary>
		[Tooltip("闭合曲线")
			, SerializeField]
		private bool m_CloseCurve;
		/// <summary>
		/// The value of mid-points calculated for each pair of bezier points
		/// The larger the value, the smoother the curve
		/// </summary>
		[Tooltip("数值越大精度越高，曲线越圆滑")
			, SerializeField]
		private float m_Resolution = 30;
		/// <summary> 
		/// 	- Array of point objects that make up this curve
		///		- Populated through editor
		/// </summary>
		public List<BezierPoint> Points = new List<BezierPoint>();

#if UNITY_EDITOR
		[Header("Debug")]
		public bool EnableGizmos = true;
		/// <summary>
		/// Color this curve will be drawn with in the editor
		/// </summary>
		public Color CurveGizmosColor = Color.white;
#endif

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="BezierCurve"/> is dirty.
		/// </summary>
		private bool m_IsDirty = true;
		/// <summary>
		/// The approximate length of the curve
		/// Recalculates if the curve is <see cref="m_IsDirty"/>
		/// </summary>
		private float m_LastLength;

		public bool IsCloseCurve()
		{
			return m_CloseCurve;
		}

		public void SetCloseCurve(bool close)
		{
			if (m_CloseCurve != close)
			{
				m_IsDirty = true;
				m_CloseCurve = close;
			}
		}

		public float GetResolution()
		{
			return m_Resolution;
		}

		public void SetResolution(float resolution)
		{
			if (m_Resolution != resolution)
			{
				m_IsDirty = true;
				m_Resolution = resolution;
			}
		}

		/// <summary>
		/// <see cref="m_LastLength"/>
		/// </summary>
		public float GetLength()
		{
			if (m_IsDirty)
			{
				m_IsDirty = false;

				m_LastLength = 0;
				for (int iPoint = 0; iPoint < Points.Count - 1; iPoint++)
				{
					m_LastLength += ApproximateLength_LocalSpace(Points[iPoint], Points[iPoint + 1]);
				}

				if (m_CloseCurve)
				{
					m_LastLength += ApproximateLength_LocalSpace(Points[Points.Count - 1], Points[0]);
				}
			}

			return m_LastLength;
		}

		public BezierPoint AddPoint_LocalSpace(BezierPoint.HandleStyle handleStyle
			, Vector3 pointLocalPosition
			, Vector3 handle1LocalPosition
			, Vector3 handle2LocalPosition)
		{
			m_IsDirty = true;
			BezierPoint newPoint = new GameObject("Point " + Points.Count).AddComponent<BezierPoint>();
			newPoint.SetOwner(this);
			newPoint.transform.parent = transform;

			newPoint.MyHandleStyle = handleStyle;
			newPoint.SetPosition_LocalSpace(pointLocalPosition);
			newPoint.SetHandle1Position_LocalSpace(handle1LocalPosition);
			newPoint.SetHandle2Position_LocalSpace(handle2LocalPosition);
			Points.Add(newPoint);
			return newPoint;
		}

		public BezierPoint AddPoint_WorldSpace(BezierPoint.HandleStyle handleStyle
			, Vector3 pointWorldPosition
			, Vector3 handle1WorldPosition
			, Vector3 handle2WorldPosition)
		{
			m_IsDirty = true;
			BezierPoint newPoint = new GameObject("Point " + Points.Count).AddComponent<BezierPoint>();
			newPoint.SetOwner(this);
			newPoint.transform.parent = transform;

			newPoint.MyHandleStyle = handleStyle;
			newPoint.SetPosition_WorldSpace(pointWorldPosition);
			newPoint.SetHandle1Position_WorldSpace(handle1WorldPosition);
			newPoint.SetHandle2Position_WorldSpace(handle2WorldPosition);
			Points.Add(newPoint);
			return newPoint;
		}

		/// <summary>
		/// Gets the point at 't' percent along this curve
		/// </summary>
		/// <param name="t">Value between 0 and 1 representing the percent along the curve (0 = 0%, 1 = 100%)</param>
		public Vector3 EvaluateInBezier_WorldSpace(float t)
		{
			return transform.TransformPoint(EvaluateInBezier_LocalSpace(t));
		}

		/// <summary>
		/// Gets the point at 't' percent along this curve
		/// </summary>
		/// <param name="t">Value between 0 and 1 representing the percent along the curve (0 = 0%, 1 = 100%)</param>
		public Vector3 EvaluateInBezier_LocalSpace(float t)
		{
			if (t <= 0)
			{
				return Points[0].GetPosition_CurveLocalSpace();
			}
			else if (t >= 1)
			{
				return Points[Points.Count - 1].GetPosition_CurveLocalSpace();
			}

			float totalPercent = 0;
			float curvePercent = 0;

			BezierPoint p1 = null;
			BezierPoint p2 = null;

			for (int iPoint = 0; iPoint < Points.Count - 1; iPoint++)
			{
				curvePercent = ApproximateLength_LocalSpace(Points[iPoint], Points[iPoint + 1]) / GetLength();
				if (totalPercent + curvePercent > t)
				{
					p1 = Points[iPoint];
					p2 = Points[iPoint + 1];
					break;
				}

				else totalPercent += curvePercent;
			}

			if (m_CloseCurve && p1 == null)
			{
				p1 = Points[Points.Count - 1];
				p2 = Points[0];
			}

			t -= totalPercent;

			return EvaluateInPointToPoint_LocalSpace(p1, p2, t / curvePercent);
		}

		/// <summary>
		/// test evaluate
		/// </summary>
		/// <returns>elapsed milliseconds</returns>
		public long TestPerformance(int evaluateCount)
		{
			System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
			stopwatch.Start();
			Vector3 point = Vector3.zero;
			for (int iEvaluate = 0; iEvaluate < evaluateCount; iEvaluate++)
			{
				 point += EvaluateInBezier_LocalSpace(0.99f);
			}
			stopwatch.Stop();
			return stopwatch.ElapsedMilliseconds;
		}

		protected void OnEnable()
		{
			m_IsDirty = true;
			for (int iPoint = 0; iPoint < Points.Count; iPoint++)
			{
				Points[iPoint].SetOwner(this);
			}
		}

		protected void Update()
		{
			for (int iPoint = 0; iPoint < Points.Count; iPoint++)
			{
				m_IsDirty |= Points[iPoint].DoUpdate();
			}
		}

#if UNITY_EDITOR
		protected void OnDrawGizmos()
		{
			if (EnableGizmos
				&& Points.Count > 1)
			{
				for (int iPoint = 0; iPoint < Points.Count - 1; iPoint++)
				{
					OnDrawGizmos_PointToPoint(Points[iPoint], Points[iPoint + 1]);
				}

				if (m_CloseCurve)
				{
					OnDrawGizmos_PointToPoint(Points[Points.Count - 1], Points[0]);
				}
			}
		}
#endif

#if UNITY_EDITOR
		/// <summary>
		/// Draws the curve in the Editor
		/// </summary>
		private void OnDrawGizmos_PointToPoint(BezierPoint pointFrom, BezierPoint pointTo)
		{
			Gizmos.color = CurveGizmosColor;
				Vector3 lastPoint = pointFrom.GetPosition_LocalSpace();
				Vector3 currentPoint = Vector3.zero;
				for (int iSegment = 1; iSegment < m_Resolution + 1; iSegment++)
				{
					currentPoint = EvaluateInPointToPoint_LocalSpace(pointFrom, pointTo, iSegment / m_Resolution);
					Gizmos.DrawLine(transform.TransformPoint(lastPoint)
						, transform.TransformPoint(currentPoint));
					lastPoint = currentPoint;
				}
		}
#endif

		/// <summary>
		/// Gets the point 't' percent along a curve
		/// Automatically calculates for the number of relevant points
		/// </summary>
		/// <param name="t">Value between 0 and 1 representing the percent along the curve (0 = 0%, 1 = 100%)</param>
		private Vector3 EvaluateInPointToPoint_LocalSpace(BezierPoint p1, BezierPoint p2, float t)
		{
			return p1.GetHandle2Position_LocalSpace() != Vector3.zero
				? p2.GetHandle1Position_LocalSpace() != Vector3.zero
					? EvaluateInCubicCurve(p1.GetPosition_CurveLocalSpace(), p1.GetHandle2Position_CurveLocalSpace(), p2.GetHandle1Position_CurveLocalSpace(), p2.GetPosition_CurveLocalSpace(), t)
					: EvaluateInQuadraticCurve(p1.GetPosition_CurveLocalSpace(), p1.GetHandle2Position_CurveLocalSpace(), p2.GetPosition_CurveLocalSpace(), t)
				: p2.GetHandle1Position_LocalSpace() != Vector3.zero
					 ? EvaluateInQuadraticCurve(p1.GetPosition_CurveLocalSpace(), p2.GetHandle1Position_CurveLocalSpace(), p2.GetPosition_CurveLocalSpace(), t)
					 : Vector3.Lerp(p1.GetPosition_CurveLocalSpace(), p2.GetPosition_CurveLocalSpace(), t);
		}

		/// <summary>
		/// Approximate length of p1 to p2
		/// </summary>
		private float ApproximateLength_LocalSpace(BezierPoint p1, BezierPoint p2)
		{
			float total = 0;
			Vector3 lastPosition = p1.GetPosition_CurveLocalSpace();
			Vector3 currentPosition;

			for (int iPoint = 0; iPoint < m_Resolution + 1; iPoint++)
			{
				currentPosition = EvaluateInPointToPoint_LocalSpace(p1, p2, iPoint / m_Resolution);
				total += (currentPosition - lastPosition).magnitude;
				lastPosition = currentPosition;
			}

			return total;
		}

		/// <summary>
		/// Gets the point 't' percent along a third-order curve
		/// </summary>
		/// <param name="t">Value between 0 and 1 representing the percent along the curve (0 = 0%, 1 = 100%)</param>
		private Vector3 EvaluateInCubicCurve(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float t)
		{
			t = Mathf.Clamp01(t);

			Vector3 part1 = Mathf.Pow(1 - t, 3) * p1;
			Vector3 part2 = 3 * Mathf.Pow(1 - t, 2) * t * p2;
			Vector3 part3 = 3 * (1 - t) * Mathf.Pow(t, 2) * p3;
			Vector3 part4 = Mathf.Pow(t, 3) * p4;

			return part1 + part2 + part3 + part4;
		}

		/// <summary>
		/// Gets the point 't' percent along a second-order curve
		/// </summary>
		/// <param name="t">Value between 0 and 1 representing the percent along the curve (0 = 0%, 1 = 100%)</param>
		private Vector3 EvaluateInQuadraticCurve(Vector3 p1, Vector3 p2, Vector3 p3, float t)
		{
			t = Mathf.Clamp01(t);

			Vector3 part1 = Mathf.Pow(1 - t, 2) * p1;
			Vector3 part2 = 2 * (1 - t) * t * p2;
			Vector3 part3 = Mathf.Pow(t, 2) * p3;

			return part1 + part2 + part3;
		}


#if UNITY_EDITOR
		[ContextMenu("Test Performance")]
		internal void _TestPerformance()
		{
			const int EVALUATE_COUNT = 10000;
			UnityEditor.EditorUtility.DisplayDialog("BezierCurve"
				, string.Format("Evaluate {0} times for {1} milliseconds", EVALUATE_COUNT, TestPerformance(EVALUATE_COUNT))
				, "Ok");
		}
#endif
	}
}