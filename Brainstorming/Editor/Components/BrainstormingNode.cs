using Conceptiode;
using Conceptiode.Clipboards;
using Conceptiode.Components;
using Conceptiode.Interfaces;
using ConceptiodeEditor;
using ConceptiodeEditor.Components;
using ConceptiodeEditor.Components.Selections;
using ConceptiodeEditor.Internals.Attributes;
using ConceptiodeEditor.UIs;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ConceptiodeDemoEditor.Brainstorming {

    [System.Serializable]
    public class BrainstormingNode : DiagramNode, ICopyable {

        [SerializeField]
        public Color textColor = Color.black;

        [SerializeField, TextArea(3, 5)]
        public string text;

        [SerializeField]
        public List<BrainstormingArrow> arrows = new List<BrainstormingArrow>();

        [SerializeField]
        public bool textModeEnabled = false;

        public BrainstormingNode ( string instanceId ) : base(instanceId) { }

        public BrainstormingNode ( Diagram diagram, BrainstormingNode otherNode ) : base(diagram, otherNode) {
            this.textColor = otherNode.textColor;
            this.text      = otherNode.text;
        }

        public override IEnumerator<DiagramArrow> GetArrowEnumerator () {
            foreach ( DiagramArrow arrow in arrows ) {
                yield return arrow;
            }
        }

        public override IEnumerator<DiagramElement> GetChildEnumerator () {
            IEnumerator<DiagramArrow> enumerator = GetArrowEnumerator();

            if ( DiagramObjectSelector.currentMode == DiagramObjectSelector.Mode.Dragging ) {
                while ( enumerator.MoveNext() ) {
                    DiagramArrow arrow = enumerator.Current;
                    yield return arrow.elementB;
                }
            }
        }

        public override void OnRemove ( bool isSource, Diagram diagram ) {
            // Delete the arrows that targets this element
            foreach ( BrainstormingArrow targetedArrow in this.targetedByArrows ) {
                ((BrainstormingNode)targetedArrow.elementA).arrows.Remove(targetedArrow);
            }
            ((Brainstorming)diagram).nodes.Remove(this);
        }

        public ICopyable Copy ( Diagram diagram, Clipboard clipboard, DiagramElement parent ) {
            BrainstormingNode copy = new BrainstormingNode(diagram, this);
            CopyArrows(diagram, clipboard, ref copy);
            return copy;
        }

        public void CopyArrows ( Diagram diagram, Clipboard clipboard, ref BrainstormingNode copy ) {
            foreach ( BrainstormingArrow arrow in this.arrows ) {
                // Check if arrow is pointing to an element of the selection list
                if ( clipboard.ContainsOriginal((BrainstormingNode)arrow.elementB) ) {
                    BrainstormingNode  copiedElementB = (BrainstormingNode)clipboard.MakeCopy(diagram, (ICopyable)arrow.elementB);
                    BrainstormingArrow copiedArrow    = (BrainstormingArrow)arrow.Copy(diagram, clipboard, null);
                    copiedArrow.elementA = copy;
                    copiedArrow.elementB = copiedElementB;
                    copiedElementB.targetedByArrows.Add(copiedArrow);
                    copiedElementB.ApplyTargetedArrowsSerialization();
                    copy.arrows.Add(copiedArrow);
                }
            }
        }

        public void Paste ( Diagram diagram, Clipboard clipboard, Vector2 newPosition, float zoomMultiplier, List<DiagramElement> selectedElements ) {
            this.position = newPosition;
            ((Brainstorming)diagram).nodes.Add(this);
        }
    }

    [CustomDiagramNodeEditor(typeof(BrainstormingNode))]
    public class BrainstormingNodeEditor : DiagramNodeEditor {

        public const int DEFAULT_FONT_SIZE = 3;

        BrainstormingNode castedTarget;

        public override void OnInitialization ( Diagram diagram ) {
            this.castedTarget = (BrainstormingNode)this.targetedNode;
        }

        public override void OnEnable () {
            this.name = "Selected Node";
        }

        public override void OnInspectorGUI () {
            this.serializedDiagram.Update();

            if ( this.serializedDiagramObject != null ) {
                EditorGUILayout.PropertyField(this.serializedDiagramObject);
            }

            this.serializedDiagram.ApplyModifiedProperties();
        }

        public override void OnDiagramGUI () {
            SerializedProperty text = this.serializedDiagramObject.FindPropertyRelative("text");

            // Don't render this node when out of view boundaries
            if ( !this.castedTarget.isInViewBoundaries ) {
                return;
            }

            // Set dimensions according to the text style and text.
            this.defaultTextStyle.fontSize = (int)(DEFAULT_FONT_SIZE * this.targetedNode.zoomMultiplier);
            this.targetedNode.dimensions   = this.defaultTextStyle.CalcSize(new GUIContent(this.castedTarget.text)) / this.targetedNode.zoomMultiplier;
            this.targetedNode.grabPoint    = this.targetedNode.radiuses;

            if ( this.targetedNode.isHovered ) {
                Color selectionColor =  Color.red;
                selectionColor.a     *= 0.333f;
                EditorGUI.DrawRect(this.targetedNode.bounds, selectionColor);
            }

            if ( this.targetedNode.isSelected && Event.current.clickCount == 2 && !this.castedTarget.textModeEnabled ) {
                this.castedTarget.textModeEnabled = true;
            } else if ( DiagramObjectSelector.selectedObject != this.castedTarget && Event.current.clickCount != 2 ) {
                this.castedTarget.textModeEnabled = false;
            }

            // Set style
            this.defaultTextStyle.alignment = TextAnchor.MiddleCenter;

            // Draw GUI
            foreach ( BrainstormingArrow arrow in this.castedTarget.arrows ) {
                arrow.shouldHightlight = this.targetedNode.isHovered;
            }

            defaultTextStyle.normal.textColor  = this.castedTarget.textColor;
            defaultTextStyle.focused.textColor = this.castedTarget.textColor;
            GUI.enabled = this.castedTarget.textModeEnabled;
            text.stringValue = EditorGUI.TextArea(this.targetedNode.bounds, text.stringValue, this.defaultTextStyle);
            GUI.enabled = true;
        }

        public override void OnDrawArrows ( ref DiagramView diagramView ) {
            SerializedProperty serializedArrows = this.serializedDiagramObject.FindPropertyRelative("arrows");

            for ( int ind = 0; ind < serializedArrows.arraySize; ind++ ) {
                BrainstormingArrow arrow = this.castedTarget.arrows[ind];
                DiagramEditorLayout.Arrow(ref diagramView, arrow, serializedArrows.GetArrayElementAtIndex(ind));
            }
        }
    }

    [CustomPropertyDrawer(typeof(BrainstormingNode))]
    public class PropertyDrawerBrainstormingNode : PropertyDrawer {

        SerializedProperty instanceId;
        SerializedProperty textColor;

        public override void OnGUI ( Rect position, SerializedProperty property, GUIContent label ) {
            this.instanceId = property.FindPropertyRelative("instanceId");
            this.textColor  = property.FindPropertyRelative("textColor");

            position.height = 20f;

            EditorGUI.BeginProperty(position, label, property);

            GUI.enabled = false;
            EditorGUI.PropertyField(position, instanceId);
            GUI.enabled = true;

            position.y += EditorGUI.GetPropertyHeight(instanceId);

            EditorGUI.PropertyField(position, this.textColor);

            position.height += EditorGUI.GetPropertyHeight(textColor);

            position.y += position.height;
            position.height = 20f;
        
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight ( SerializedProperty property, GUIContent label ) {
            float height = 0.0f;

            height += instanceId != null ? EditorGUI.GetPropertyHeight(instanceId) : 0.0f;
            height += textColor  != null ? EditorGUI.GetPropertyHeight(textColor)  : 0.0f;

            return height;
        }
    }
}