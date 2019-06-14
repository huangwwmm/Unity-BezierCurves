using UnityEngine;
using System;

namespace BezierCurve
{
	[Serializable]
	public class BezierPoint : MonoBehaviour
	{
		/// <summary>
		/// Value describing the relationship between this point's handles
		/// </summary>
		public HandleStyle MyHandleStyle;

		/// <summary>
		/// Local position of the first handle
		/// </summary>
		[SerializeField]
		private Vector3 m_Handle1Position_LocalSpace;
		/// <summary>
		/// Local position of the second handle
		/// </summary>
		[SerializeField]
		private Vector3 m_Handle2Position_LocalSpace;

		[SerializeField]
		private Vector3 m_Handle1Position_CurveLocalSpace;
		[SerializeField]
		private Vector3 m_Handle2Position_CurveLocalSpace;

		private BezierCurve m_Owner;
		private Vector3 m_Position_CurveLocalSpace;

		/// <summary>
		/// <see cref="BezierCurve.m_IsDirty"/>
		/// set to false in <see cref="DoUpdate"/>
		/// </summary>
		private bool m_IsDirty;
		/// <summary>
		/// Used to determine if this point has moved since the last frame
		/// </summary>
		private Vector3 m_LastPosition;

		#region Point
		public void SetDirty()
		{
			m_IsDirty = true;
		}

		public BezierCurve GetOwner()
		{
			return m_Owner;
		}

		public void SetOwner(BezierCurve owner)
		{
			m_Owner = owner;
		}

		public Vector3 GetPosition_WorldSpace()
		{
			return transform.position;
		}

		public void SetPosition_WorldSpace(Vector3 position)
		{
			if (transform.position == position)
			{
				return;
			}

			m_IsDirty = true;
			transform.position = position;
		}

		public Vector3 GetPosition_LocalSpace()
		{
			return transform.localPosition;
		}

		public void SetPosition_LocalSpace(Vector3 position)
		{
			if (transform.localPosition == position)
			{
				return;
			}

			m_IsDirty = true;
			transform.localPosition = position;
		}

		public Vector3 GetPosition_CurveLocalSpace()
		{
			return m_Position_CurveLocalSpace;
		}
		#endregion End Point

		#region Handle1
		public Vector3 GetHandle1Position_LocalSpace()
		{
			return m_Handle1Position_LocalSpace;
		}

		public void SetHandle1Position_LocalSpace(Vector3 position)
		{
			if (m_Handle1Position_LocalSpace == position)
			{
				return;
			}

			m_IsDirty = true;
			m_Handle1Position_LocalSpace = position;

			if (MyHandleStyle == HandleStyle.Connected)
			{
				m_Handle2Position_LocalSpace = -m_Handle1Position_LocalSpace;
			}

			m_Handle1Position_CurveLocalSpace = m_Owner.transform.InverseTransformPoint(transform.TransformPoint(m_Handle1Position_LocalSpace));
		}

		public Vector3 GetHandle1Position_WorldSpace()
		{
			return transform.TransformPoint(GetHandle1Position_LocalSpace());
		}

		public void SetHandle1Position_WorldSpace(Vector3 position)
		{
			SetHandle1Position_LocalSpace(transform.InverseTransformPoint(position));
		}

		public Vector3 GetHandle1Position_CurveLocalSpace()
		{
			return m_Handle1Position_CurveLocalSpace;
		}
		#endregion End Handle1

		#region Handle2
		public Vector3 GetHandle2Position_LocalSpace()
		{
			return m_Handle2Position_LocalSpace;
		}

		public void SetHandle2Position_LocalSpace(Vector3 position)
		{
			if (m_Handle2Position_LocalSpace == position)
			{
				return;
			}

			m_IsDirty = true;
			m_Handle2Position_LocalSpace = position;

			if (MyHandleStyle == HandleStyle.Connected)
			{
				m_Handle1Position_LocalSpace = -m_Handle2Position_LocalSpace;
			}

			m_Handle2Position_CurveLocalSpace = m_Owner.transform.InverseTransformPoint(transform.TransformPoint(m_Handle2Position_LocalSpace));
		}

		public Vector3 GetHandle2Position_WorldSpace()
		{
			return transform.TransformPoint(GetHandle2Position_LocalSpace());
		}

		public void SetHandle2Position_WorldSpace(Vector3 position)
		{
			SetHandle2Position_LocalSpace(transform.InverseTransformPoint(position));
		}

		public Vector3 GetHandle2Position_CurveLocalSpace()
		{
			return m_Handle2Position_CurveLocalSpace;
		}
		#endregion End Handle2

		public bool DoUpdate()
		{
			if (GetPosition_WorldSpace() != m_LastPosition)
			{
				m_IsDirty = true;
				m_LastPosition = GetPosition_WorldSpace();
			}

			if (m_IsDirty)
			{
				m_Position_CurveLocalSpace = m_Owner.transform.InverseTransformPoint(transform.position);
				m_Handle1Position_CurveLocalSpace = m_Owner.transform.InverseTransformPoint(transform.TransformPoint(m_Handle1Position_LocalSpace));
				m_Handle2Position_CurveLocalSpace = m_Owner.transform.InverseTransformPoint(transform.TransformPoint(m_Handle2Position_LocalSpace));
				m_IsDirty = false;
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Enumeration describing the relationship between a point's handles
		/// </summary>
		public enum HandleStyle
		{
			/// <summary>
			/// The point's handles are mirrored across the point
			/// </summary>
			Connected,
			/// <summary>
			///  Each handle moves independently of the other
			/// </summary>
			Broken,
			/// <summary>
			/// This point has no handles (both handles are located ON the point)
			/// </summary>
			None,
		}
	}
}