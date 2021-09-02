using Conceptiode;
using Conceptiode.Clipboards;
using Conceptiode.Components;
using Conceptiode.Interfaces;
using ConceptiodeEditor.Components;
using ConceptiodeEditor.Internals.Attributes;
using ConceptiodeEditor.Utils;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ConceptiodeDemoEditor.Brainstorming {

    [System.Serializable]
    public class BrainstormingArrow : DiagramArrow, ICopyable {

        public Color color = Color.grey;

        public bool shouldHightlight = false;

        public BrainstormingArrow ( string instanceId, DiagramElement elementA, DiagramElement elementB ) : base(instanceId, elementA, elementB) { }

        public BrainstormingArrow ( Diagram diagram, BrainstormingArrow otherArrow ) : base(diagram, otherArrow) {

        }

        public override void OnRemove ( bool isSource, Diagram diagram )  {
            if ( isSource ) {
                ((BrainstormingNode)this.elementA).arrows.Remove(this);
            }
            this.elementB.targetedByArrows.Remove(this);
            this.elementB.ApplyTargetedArrowsSerialization();
        }

        

        /// <summary>
        /// <para>Remove this arrow.</para>
        /// </summary>
        public void Remove () {
            ((BrainstormingNode)this.elementA).arrows.Remove(this);
        }

        public ICopyable Copy ( Diagram diagram, Clipboard clipboard, DiagramElement parent ) {
            BrainstormingArrow copy = new BrainstormingArrow(diagram, this);
            copy.color = this.color;
            return copy;
        }

        public void Paste ( Diagram diagram, Clipboard clipboard, Vector2 newPosition, float zoomMultiplier, List<DiagramElement> selectedElements ) { }

    }

    [CustomDiagramArrowEditor(typeof(BrainstormingArrow))]
    public class BrainstormingArrowEditor : DiagramArrowEditor {

        SerializedProperty color;

        public override void OnEnable () {
            this.name = "Selected Arrow";
        }

        public override void OnInspectorGUI () {
            this.serializedDiagram.Update();

            if (this.serializedDiagramObject != null ) {
                EditorGUILayout.PropertyField(this.serializedDiagramObject);
            }

            this.serializedDiagram.ApplyModifiedProperties();
        }

        public override void OnDiagramGUI () {
            if (this.targetedArrow.elementB == null) {
                ((BrainstormingArrow)this.targetedArrow).Remove();
                return;
            }

            if (!this.targetedArrow.elementA.isInViewBoundaries && !targetedArrow.elementB.isInViewBoundaries) {
                return;
            }

            this.color = this.serializedDiagramObject.FindPropertyRelative("color");

            Color arrowColor = this.color.colorValue;
            Color invertedArrowColor = new Color();
            invertedArrowColor.r = 1 - arrowColor.r;
            invertedArrowColor.g = 1 - arrowColor.g;
            invertedArrowColor.b = 1 - arrowColor.b;
            invertedArrowColor.a = 1.0f;

            Color color = this.targetedArrow.isHovered ? invertedArrowColor : arrowColor;
            color = ((BrainstormingArrow)this.targetedArrow).shouldHightlight ? Color.red : color;

            float lineWidth = 4.0f / this.targetedArrow.zoomMultiplier;
            ShapeDrawing.DrawBezier(this.targetedArrow, color, lineWidth);
        }
    }

    [CustomPropertyDrawer(typeof(BrainstormingArrow))]
    public class PropertyDrawerBrainstormingArrow : PropertyDrawer {

        SerializedProperty instanceId;
        SerializedProperty color;

        public override void OnGUI ( Rect position, SerializedProperty property, GUIContent label ) {
            this.instanceId = property.FindPropertyRelative("instanceId");
            this.color      = property.FindPropertyRelative("color");

            position.height = 20f;

            EditorGUI.BeginProperty(position, label, property);

            GUI.enabled = false;
            EditorGUI.PropertyField(position, instanceId);
            GUI.enabled = true;

            position.y += EditorGUI.GetPropertyHeight(instanceId);

            EditorGUI.PropertyField(position, this.color);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight ( SerializedProperty property, GUIContent label ) {
            float height = 0.0f;

            height += instanceId != null ? EditorGUI.GetPropertyHeight(instanceId) : 0.0f;
            height += color      != null ? EditorGUI.GetPropertyHeight(color)      : 0.0f;

            return height;
        }

    }
}