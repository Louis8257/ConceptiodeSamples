using Conceptiode;
using Conceptiode.Components;
using ConceptiodeEditor;
using ConceptiodeEditor.UIs;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ConceptiodeDemoEditor.Brainstorming {

    [CreateAssetMenu(fileName = "New Brainstorming Diagram", menuName = "Conceptiode Demos/" + BrainstormingWindow.NAME, order = 0)]
    public class Brainstorming : Diagram {

        public List<BrainstormingNode> nodes = new List<BrainstormingNode>();

        public override IEnumerator<DiagramNode> GetNodeEnumerator () {
            foreach ( BrainstormingNode node in nodes ) {
                yield return node;
            }
        }

        public void OnAddNode ( float zoomMultiplier, Vector2 mousePosition ) {
            BrainstormingNode node    = new BrainstormingNode(this.nextObjectId);
            node.text                 = "New Node";
            node.zoomMultiplier       = zoomMultiplier;
            node.positionByBarycenter = mousePosition;
            this.nodes.Add(node);
        }

        public void OnAddArrow ( Type arrowType, DiagramElement elementA, DiagramElement elementB ) {
            ((BrainstormingNode)elementA).arrows.Add(new BrainstormingArrow(this.nextObjectId, (BrainstormingNode)elementA, (BrainstormingNode)elementB));
        }

        public void OnRemoveNode ( DiagramNode node ) {
            this.nodes.Remove((BrainstormingNode)node);
        }

        public void OnRemoveArrow ( DiagramArrow arrow ) {
            ((BrainstormingNode)arrow.elementA).arrows.Remove((BrainstormingArrow)arrow);
        }
    }

    [CustomEditor(typeof(Brainstorming))]
    public class BrainstormingEditor : DiagramEditor {

        Brainstorming castedTarget;

        public SerializedProperty nodes;

        public override void OnInspectorGUI () {
            if ( BrainstormingWindow.window == null && GUILayout.Button("Open Window") ) {
                BrainstormingWindow.Summon(this.castedTarget);
            } else if ( BrainstormingWindow.window != null ) {
                BrainstormingWindow.Summon(this.castedTarget);
            }
        }
        
        public new void OnEnable () {
            base.OnEnable();
            this.castedTarget = (Brainstorming)this.targetedDiagram;
            this.nodes        = this.serializedObject.FindProperty("nodes");
        }

        public override void OnDiagramGUI ( DiagramView diagramView ) {
            this.serializedObject.Update();

            this.StandardDrawing(ref diagramView, this.castedTarget.nodes, this.nodes);

            this.serializedObject.ApplyModifiedProperties();
        }
    }
}