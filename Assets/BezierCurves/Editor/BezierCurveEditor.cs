using UnityEngine;
using UnityEditor;

namespace BezierCurve
{
	[CustomEditor(typeof(BezierCurve))]
	public class BezierCurveEditor : Editor
	{
		private BezierCurve m_BezierCurve;
		private SerializedProperty m_PointsProperty;
		private SerializedProperty[] m_NeedDrawPropertys;

		public static void OnInspectorGUI_BezierPoint(BezierPoint bezierPoint)
		{
			Vector3 oldHandle1LocalPosition = bezierPoint.GetHandle1LocalPosition();
			Vector3 oldHandle2LocalPosition = bezierPoint.GetHandle2LocalPosition();

			bezierPoint.MyHandleStyle = (BezierPoint.HandleStyle)EditorGUILayout.EnumPopup("Handle Type", bezierPoint.MyHandleStyle);
			bezierPoint.SetHandle1LocalPosition(EditorGUILayout.Vector3Field("Handle 1 LocalPosition", oldHandle1LocalPosition));
			bezierPoint.SetHandle2LocalPosition(EditorGUILayout.Vector3Field("Handle 2 LocalPosition", oldHandle2LocalPosition));

			switch (bezierPoint.MyHandleStyle)
			{
				case BezierPoint.HandleStyle.Connected:
					if (oldHandle1LocalPosition != bezierPoint.GetHandle1LocalPosition())
					{
						bezierPoint.SetHandle1LocalPosition(oldHandle1LocalPosition);
						bezierPoint.SetHandle2LocalPosition(-oldHandle1LocalPosition);
					}
					else if (oldHandle2LocalPosition != bezierPoint.GetHandle2LocalPosition())
					{
						bezierPoint.SetHandle1LocalPosition(-oldHandle2LocalPosition);
						bezierPoint.SetHandle2LocalPosition(oldHandle2LocalPosition);
					}
					else if (bezierPoint.GetHandle1LocalPosition() != -bezierPoint.GetHandle2LocalPosition())
					{
						if (bezierPoint.GetHandle1LocalPosition() != Vector3.zero)
						{
							bezierPoint.SetHandle2LocalPosition(-bezierPoint.GetHandle1LocalPosition());
						}
						else if (bezierPoint.GetHandle2LocalPosition() != Vector3.zero)
						{
							bezierPoint.SetHandle1LocalPosition(-bezierPoint.GetHandle2LocalPosition());
						}
						else
						{
							bezierPoint.SetHandle1LocalPosition(new Vector3(0.1f, 0, 0));
							bezierPoint.SetHandle2LocalPosition(new Vector3(-0.1f, 0, 0));
						}
					}
					break;
				case BezierPoint.HandleStyle.Broken:
					if (bezierPoint.GetHandle1LocalPosition() == Vector3.zero
						&& bezierPoint.GetHandle2LocalPosition() == Vector3.zero)
					{
						bezierPoint.SetHandle1LocalPosition(new Vector3(0.1f, 0, 0));
						bezierPoint.SetHandle2LocalPosition(new Vector3(-0.1f, 0, 0));
					}
					break;
				case BezierPoint.HandleStyle.None:
					bezierPoint.SetHandle1LocalPosition(Vector3.zero);
					bezierPoint.SetHandle2LocalPosition(Vector3.zero);
					break;
				default:
					throw new System.Exception(string.Format("not support HandleStyle({0})", bezierPoint.MyHandleStyle));
			}

			if (GUI.changed)
			{
				EditorUtility.SetDirty(bezierPoint);
			}
		}

		public static void OnSceneGUI_BezierPoint(BezierPoint bezierPoint)
		{
			Handles.Label(bezierPoint.GetWorldPosition()
				+ new Vector3(0, HandleUtility.GetHandleSize(bezierPoint.GetWorldPosition()) * 0.4f, 0)
				, bezierPoint.gameObject.name);

			Handles.color = Color.green;
			Vector3 newPosition = Handles.FreeMoveHandle(bezierPoint.GetWorldPosition()
				, bezierPoint.transform.rotation
				, HandleUtility.GetHandleSize(bezierPoint.GetWorldPosition()) * 0.2f
				, Vector3.zero
				, Handles.CubeHandleCap);
			if (newPosition != bezierPoint.GetWorldPosition())
			{
				Undo.RegisterCompleteObjectUndo(bezierPoint.transform, "Move Point");
				bezierPoint.SetWorldPosition(newPosition);
			}

			switch (bezierPoint.MyHandleStyle)
			{
				case BezierPoint.HandleStyle.Connected:
					{
						OnSceneGUI_BezierPointHandlePoints(bezierPoint, out Vector3 newHandle1WorldPosition, out Vector3 newHandle2WorldPosition);
						if (newHandle1WorldPosition != bezierPoint.GetHandle1WorldPosition())
						{
							Undo.RegisterCompleteObjectUndo(bezierPoint, "Move Handle");
							bezierPoint.SetHandle1WorldPosition(newHandle1WorldPosition);
							bezierPoint.SetHandle2WorldPosition(-(newHandle1WorldPosition - bezierPoint.GetWorldPosition()) + bezierPoint.GetWorldPosition());
						}
						else if (newHandle2WorldPosition != bezierPoint.GetHandle2WorldPosition())
						{
							Undo.RegisterCompleteObjectUndo(bezierPoint, "Move Handle");
							bezierPoint.SetHandle1WorldPosition(-(newHandle2WorldPosition - bezierPoint.GetWorldPosition()) + bezierPoint.GetWorldPosition());
							bezierPoint.SetHandle2WorldPosition(newHandle2WorldPosition);
						}
						OnSceneGUI_BezierPointHandleLines(bezierPoint);
					}
					break;
				case BezierPoint.HandleStyle.Broken:
					{
						OnSceneGUI_BezierPointHandlePoints(bezierPoint, out Vector3 newHandle1WorldPosition, out Vector3 newHandle2WorldPosition);
						if (newHandle1WorldPosition != bezierPoint.GetHandle1WorldPosition())
						{
							Undo.RegisterCompleteObjectUndo(bezierPoint, "Move Handle");
							bezierPoint.SetHandle1WorldPosition(newHandle1WorldPosition);
						}

						if (newHandle2WorldPosition != bezierPoint.GetHandle2WorldPosition())
						{
							Undo.RegisterCompleteObjectUndo(bezierPoint, "Move Handle");
							bezierPoint.SetHandle2WorldPosition(newHandle2WorldPosition);
						}
						OnSceneGUI_BezierPointHandleLines(bezierPoint);
					}
					break;
				case BezierPoint.HandleStyle.None:
					bezierPoint.SetHandle1LocalPosition(Vector3.zero);
					bezierPoint.SetHandle2LocalPosition(Vector3.zero);
					break;
				default:
					throw new System.Exception(string.Format("not support HandleStyle({0})", bezierPoint.MyHandleStyle));
			}
		}

		private static void OnSceneGUI_BezierPointHandlePoints(BezierPoint bezierPoint, out Vector3 newHandle1WorldPosition, out Vector3 newHandle2WorldPosition)
		{
			Handles.color = Color.cyan;
			newHandle1WorldPosition = Handles.FreeMoveHandle(bezierPoint.GetHandle1WorldPosition()
			   , Quaternion.identity
			   , HandleUtility.GetHandleSize(bezierPoint.GetHandle1WorldPosition()) * 0.15f
			   , Vector3.zero
			   , Handles.SphereHandleCap);
			newHandle2WorldPosition = Handles.FreeMoveHandle(bezierPoint.GetHandle2WorldPosition()
			   , Quaternion.identity
			   , HandleUtility.GetHandleSize(bezierPoint.GetHandle2WorldPosition()) * 0.15f
			   , Vector3.zero
			   , Handles.SphereHandleCap);
		}

		private static void OnSceneGUI_BezierPointHandleLines(BezierPoint bezierPoint)
		{
			Handles.color = Color.yellow;
			Handles.DrawLine(bezierPoint.GetWorldPosition(), bezierPoint.GetHandle1WorldPosition());
			Handles.DrawLine(bezierPoint.GetWorldPosition(), bezierPoint.GetHandle2WorldPosition());
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			for (int iProperty = 0; iProperty < m_NeedDrawPropertys.Length; iProperty++)
			{
				EditorGUILayout.PropertyField(m_NeedDrawPropertys[iProperty]);
			}

			EditorGUILayout.Space();
			m_PointsProperty.isExpanded = EditorGUILayout.Foldout(m_PointsProperty.isExpanded, "Points");
			if (m_PointsProperty.isExpanded)
			{
				for (int iBezierPoint = 0; iBezierPoint < m_PointsProperty.arraySize; iBezierPoint++)
				{
					OnInspectorGUI_BezierPointInArray(m_BezierCurve.Points[iBezierPoint], iBezierPoint);
				}

				if (GUILayout.Button("Add Point"))
				{
					GameObject pointObject = new GameObject("Point " + m_PointsProperty.arraySize);
					Undo.RegisterCreatedObjectUndo(pointObject, "Add Point");
					pointObject.transform.parent = m_BezierCurve.transform;
					pointObject.transform.localPosition = Vector3.up;
					BezierPoint newPoint = pointObject.AddComponent<BezierPoint>();
					newPoint.SetHandle1LocalPosition(Vector3.right);
					newPoint.SetHandle2LocalPosition(-Vector3.right);

					m_PointsProperty.InsertArrayElementAtIndex(m_PointsProperty.arraySize);
					m_PointsProperty.GetArrayElementAtIndex(m_PointsProperty.arraySize - 1).objectReferenceValue = newPoint;
				}
			}

			if (GUI.changed)
			{
				serializedObject.ApplyModifiedProperties();
				EditorUtility.SetDirty(target);
			}
		}

		protected void OnEnable()
		{
			m_BezierCurve = (BezierCurve)target;

			m_PointsProperty = serializedObject.FindProperty("Points");

			m_NeedDrawPropertys = new SerializedProperty[]
			{
			serializedObject.FindProperty("CloseCurve"),
			serializedObject.FindProperty("Resolution"),
			serializedObject.FindProperty("EnableGizmos"),
			serializedObject.FindProperty("CurveGizmosColor"),
			};
		}

		protected void OnDisable()
		{
			m_NeedDrawPropertys = null;
		}

		protected void OnSceneGUI()
		{
			for (int iBezierPoint = 0; iBezierPoint < m_BezierCurve.Points.Length; iBezierPoint++)
			{
				OnSceneGUI_BezierPoint(m_BezierCurve.Points[iBezierPoint]);
			}
		}

		private void OnInspectorGUI_BezierPointInArray(BezierPoint bezierPoint, int index)
		{
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("X", GUILayout.Width(20)))
			{
				Undo.RegisterCompleteObjectUndo(bezierPoint.gameObject, "Remove Point");
				m_PointsProperty.MoveArrayElement(index, m_BezierCurve.Points.Length - 1);
				m_PointsProperty.arraySize--;
				DestroyImmediate(bezierPoint.gameObject);
				return;
			}
			EditorGUILayout.ObjectField(bezierPoint.gameObject, typeof(GameObject), true);

			if (index != 0 && GUILayout.Button(@"/\", GUILayout.Width(25)))
			{
				UnityEngine.Object other = m_PointsProperty.GetArrayElementAtIndex(index - 1).objectReferenceValue;
				m_PointsProperty.GetArrayElementAtIndex(index - 1).objectReferenceValue = bezierPoint;
				m_PointsProperty.GetArrayElementAtIndex(index).objectReferenceValue = other;
			}

			if (index != m_PointsProperty.arraySize - 1 && GUILayout.Button(@"\/", GUILayout.Width(25)))
			{
				UnityEngine.Object other = m_PointsProperty.GetArrayElementAtIndex(index + 1).objectReferenceValue;
				m_PointsProperty.GetArrayElementAtIndex(index + 1).objectReferenceValue = bezierPoint;
				m_PointsProperty.GetArrayElementAtIndex(index).objectReferenceValue = other;
			}
			EditorGUILayout.EndHorizontal();

			EditorGUI.indentLevel++;
			EditorGUI.indentLevel++;
			OnInspectorGUI_BezierPoint(bezierPoint);
			EditorGUI.indentLevel--;
			EditorGUI.indentLevel--;
		}
	}
}