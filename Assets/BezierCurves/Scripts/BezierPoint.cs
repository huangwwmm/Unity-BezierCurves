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
		private Vector3 m_Handle1LocalPosition;
		/// <summary>
		/// Local position of the second handle
		/// </summary>
		[SerializeField]
		private Vector3 m_Handle2LocalPosition;

		/// <summary>
		/// <see cref="BezierCurve.m_IsDirty"/>
		/// set to false in <see cref="DoUpdate"/>
		/// </summary>
		private bool m_IsDirty;
		/// <summary>
		/// Used to determine if this point has moved since the last frame
		/// </summary>
		private Vector3 m_LastPosition;

		public Vector3 GetWorldPosition()
		{
			return transform.position;
		}

		public void SetWorldPosition(Vector3 position)
		{
			if (transform.position == position)
			{
				return;
			}

			m_IsDirty = true;
			transform.position = position;
		}

		public Vector3 GetLocalPosition()
		{
			return transform.localPosition;
		}

		public void SetLocalPosition(Vector3 position)
		{
			if (transform.localPosition == position)
			{
				return;
			}

			m_IsDirty = true;
			transform.localPosition = position;
		}

		public Vector3 GetHandle1LocalPosition()
		{
			return m_Handle1LocalPosition;
		}

		public void SetHandle1LocalPosition(Vector3 position)
		{
			if (m_Handle1LocalPosition == position)
			{
				return;
			}

			m_IsDirty = true;
			m_Handle1LocalPosition = position;

			if (MyHandleStyle == HandleStyle.None)
			{
				MyHandleStyle = HandleStyle.Broken;
			}
			else if (MyHandleStyle == HandleStyle.Connected)
			{
				m_Handle2LocalPosition = -m_Handle1LocalPosition;
			}
		}

		public Vector3 GetHandle1WorldPosition()
		{
			return transform.TransformPoint(GetHandle1LocalPosition());
		}

		public void SetHandle1WorldPosition(Vector3 position)
		{
			SetHandle1LocalPosition(transform.InverseTransformPoint(position));
		}

		public Vector3 GetHandle2LocalPosition()
		{
			return m_Handle2LocalPosition;
		}

		public void SetHandle2LocalPosition(Vector3 position)
		{
			if (m_Handle2LocalPosition == position)
			{
				return;
			}

			m_IsDirty = true;
			m_Handle2LocalPosition = position;

			if (MyHandleStyle == HandleStyle.None)
			{
				MyHandleStyle = HandleStyle.Broken;
			}
			else if (MyHandleStyle == HandleStyle.Connected)
			{
				m_Handle1LocalPosition = -m_Handle2LocalPosition;
			}
		}

		public Vector3 GetHandle2WorldPosition()
		{
			return transform.TransformPoint(GetHandle2LocalPosition());
		}

		public void GetHandle2WorldPosition(Vector3 position)
		{
			SetHandle2LocalPosition(transform.InverseTransformPoint(position));
		}

		public void SetHandle2WorldPosition(Vector3 position)
		{
			SetHandle2LocalPosition(transform.InverseTransformPoint(position));
		}

		public bool DoUpdate()
		{
			if (GetWorldPosition() != m_LastPosition)
			{
				m_IsDirty = true;
				m_LastPosition = GetWorldPosition();
			}

			if (m_IsDirty)
			{
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