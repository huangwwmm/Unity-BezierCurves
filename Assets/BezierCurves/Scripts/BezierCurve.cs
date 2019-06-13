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
		[Tooltip("闭合曲线")]
		public bool CloseCurve;
		/// <summary>
		/// The value of mid-points calculated for each pair of bezier points
		/// The larger the value, the smoother the curve
		/// </summary>
		[Tooltip("数值越大精度越高，曲线越圆滑")]
		public float Resolution = 30;
		/// <summary> 
		/// 	- Array of point objects that make up this curve
		///		- Populated through editor
		/// </summary>
		public BezierPoint[] Points = new BezierPoint[0];

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

		/// <summary>
		/// <see cref="m_LastLength"/>
		/// </summary>
		public float GetLength()
		{
			if (m_IsDirty)
			{
				m_IsDirty = false;

				m_LastLength = 0;
				for (int iPoint = 0; iPoint < Points.Length - 1; iPoint++)
				{
					m_LastLength += ApproximateLength(Points[iPoint], Points[iPoint + 1]);
				}

				if (CloseCurve)
				{ 
					m_LastLength += ApproximateLength(Points[Points.Length - 1], Points[0]);
				}
			}

			return m_LastLength;
		}

		/// <summary>
		/// Gets the point at 't' percent along this curve
		/// </summary>
		/// <param name="t">Value between 0 and 1 representing the percent along the curve (0 = 0%, 1 = 100%)</param>
		public Vector3 EvaluateInBezier(float t)
		{
			if (t <= 0) return Points[0].GetWorldPosition();
			else if (t >= 1) return Points[Points.Length - 1].GetWorldPosition();

			float totalPercent = 0;
			float curvePercent = 0;

			BezierPoint p1 = null;
			BezierPoint p2 = null;

			for (int i = 0; i < Points.Length - 1; i++)
			{
				curvePercent = ApproximateLength(Points[i], Points[i + 1]) / GetLength();
				if (totalPercent + curvePercent > t)
				{
					p1 = Points[i];
					p2 = Points[i + 1];
					break;
				}

				else totalPercent += curvePercent;
			}

			if (CloseCurve && p1 == null)
			{
				p1 = Points[Points.Length - 1];
				p2 = Points[0];
			}

			t -= totalPercent;

			return EvaluateInPointToPoint(p1, p2, t / curvePercent);
		}

		/// <summary>
		/// Gets the point 't' percent along a curve
		/// Automatically calculates for the number of relevant points
		/// </summary>
		/// <param name="t">Value between 0 and 1 representing the percent along the curve (0 = 0%, 1 = 100%)</param>
		public Vector3 EvaluateInPointToPoint(BezierPoint p1, BezierPoint p2, float t)
		{
			return p1.GetHandle2LocalPosition() != Vector3.zero
				? p2.GetHandle1LocalPosition() != Vector3.zero
					? EvaluateInCubicCurve(p1.GetWorldPosition(), p1.GetHandle2WorldPosition(), p2.GetHandle1WorldPosition(), p2.GetWorldPosition(), t)
					: EvaluateInQuadraticCurve(p1.GetWorldPosition(), p1.GetHandle2WorldPosition(), p2.GetWorldPosition(), t)
				: p2.GetHandle1LocalPosition() != Vector3.zero
					 ? EvaluateInQuadraticCurve(p1.GetWorldPosition(), p2.GetHandle1WorldPosition(), p2.GetWorldPosition(), t)
					 : Vector3.Lerp(p1.GetWorldPosition(), p2.GetWorldPosition(), t);
		}

		/// <summary>
		/// Gets the point 't' percent along a third-order curve
		/// </summary>
		/// <param name="t">Value between 0 and 1 representing the percent along the curve (0 = 0%, 1 = 100%)</param>
		public Vector3 EvaluateInCubicCurve(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float t)
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
		public Vector3 EvaluateInQuadraticCurve(Vector3 p1, Vector3 p2, Vector3 p3, float t)
		{
			t = Mathf.Clamp01(t);

			Vector3 part1 = Mathf.Pow(1 - t, 2) * p1;
			Vector3 part2 = 2 * (1 - t) * t * p2;
			Vector3 part3 = Mathf.Pow(t, 2) * p3;

			return part1 + part2 + part3;
		}

		/// <summary>
		/// Approximate length of p1 to p2
		/// </summary>
		public float ApproximateLength(BezierPoint p1, BezierPoint p2)
		{
			float total = 0;
			Vector3 lastPosition = p1.GetWorldPosition();
			Vector3 currentPosition;

			for (int iPoint = 0; iPoint < Resolution + 1; iPoint++)
			{
				currentPosition = EvaluateInPointToPoint(p1, p2, iPoint / Resolution);
				total += (currentPosition - lastPosition).magnitude;
				lastPosition = currentPosition;
			}

			return total;
		}

		protected void Awake()
		{
			m_IsDirty = true;
		}

		protected void Update()
		{
			for (int iPoint = 0; iPoint < Points.Length; iPoint++)
			{
				m_IsDirty |= Points[iPoint].DoUpdate();
			}
		}

#if UNITY_EDITOR
		protected void OnDrawGizmos()
		{
			if (Points.Length > 1)
			{
				Gizmos.color = CurveGizmosColor;
				for (int iPoint = 0; iPoint < Points.Length - 1; iPoint++)
				{
					OnDrawGizmos_PointToPoint(Points[iPoint], Points[iPoint + 1]);
				}

				if (CloseCurve)
				{
					OnDrawGizmos_PointToPoint(Points[Points.Length - 1], Points[0]);
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
			Vector3 lastPoint = pointFrom.GetWorldPosition();
			Vector3 currentPoint = Vector3.zero;
			for (int iSegment = 1; iSegment < Resolution + 1; iSegment++)
			{
				currentPoint = EvaluateInPointToPoint(pointFrom, pointTo, iSegment / Resolution);
				Gizmos.DrawLine(lastPoint, currentPoint);
				lastPoint = currentPoint;
			}
		}
#endif
	}
}