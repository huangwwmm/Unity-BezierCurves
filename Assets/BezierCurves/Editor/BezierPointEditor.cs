using UnityEditor;

namespace BezierCurve
{
	[CustomEditor(typeof(BezierPoint))
	, CanEditMultipleObjects]
	public class BezierPointEditor : Editor
	{
		private BezierPoint m_BezierPoint;

		public override void OnInspectorGUI()
		{
			BezierCurveEditor.OnInspectorGUI_BezierPoint(m_BezierPoint);
		}

		protected void OnEnable()
		{
			m_BezierPoint = (BezierPoint)target;
		}

		protected void OnSceneGUI()
		{
			BezierCurveEditor.OnSceneGUI_BezierPoint(m_BezierPoint);
		}
	}
}