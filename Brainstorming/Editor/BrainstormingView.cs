using Conceptiode.Components;
using Conceptiode.Utils;
using ConceptiodeEditor;
using ConceptiodeEditor.Components.ContextualMenus;
using ConceptiodeEditor.Components.Selections;
using ConceptiodeEditor.Components.Zooms;
using ConceptiodeEditor.UIs;
using ConceptiodeEditor.Utils;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ConceptiodeDemoEditor.Brainstorming {

    public class BrainstormingView : DiagramView {

        public BrainstormingContextualMenu contextualMenu;

        public new void OnEnable () {
            base.OnEnable();
            this.contextualMenu              = new BrainstormingContextualMenu();
            contextualMenu.onActionSelection = this.OnActionSelection;
            this.zoom                        = new DiagramMousePositionZoom();
        }

        public override void OnKeyboardInput () {
            base.OnKeyboardInput();

            if ( this.isDiagramHasBeenModified && Event.current.control && Event.current.keyCode == KeyCode.S ) {
                SaveDiagram();
            }
            if ( (DiagramObjectSelector.selectedObject != null || DiagramObjectSelector.selectedObjects != null )
              && Event.current.keyCode == KeyCode.Delete) {
                OnActionSelection(BrainstormingContextualMenu.StandardAction.DeleteDiagramObject);
            }

            this.ClipboardInput();
        }

        public override void MoveScrollNodes ( Vector2 increment ) {
            List<BrainstormingNode> nodes = ((Brainstorming)this.currentDiagram).nodes;
            IEnumerator<DiagramNode> enumerator = this.currentDiagramEditor.targetedDiagram.GetNodeEnumerator();
            while ( enumerator.MoveNext() ) {
                DiagramNode diagramNode = enumerator.Current;
                diagramNode.unzoomedPosition -= increment;
            }
            enumerator.Dispose();
        }

        /// <summary>
        /// <inheritdoc/>
        /// <para>In this diagram, we know we have one level of recursive parenting.</para>
        /// <para>So this method will do nothing, even called.</para>
        /// </summary>
        /// <param name="increment"></param>
        /// <param name="enumerator"></param>
        public override void MoveScrollChilds ( Vector2 increment, IEnumerator<DiagramElement> enumerator ) { }

        public override void OnActionSelection ( object action ) {
            DiagramGenericContextualMenu.StandardAction standardAction  = (DiagramGenericContextualMenu.StandardAction)action;
            DiagramObject                               selectedObject  = DiagramObjectSelector.selectedObject;
            Brainstorming                               castedDiagram   = (Brainstorming)this.currentDiagram;

            switch ( standardAction ) {
                case DiagramGenericContextualMenu.StandardAction.AddNode:
                    Undo.RegisterCompleteObjectUndo(this.currentDiagram, "Added node");
                    castedDiagram.OnAddNode(this.zoom.currentMultiplier, this.mousePositionOnDiagram);
                    break;
                case DiagramGenericContextualMenu.StandardAction.DeleteDiagramObject:
                    Selection.activeObject = null;
                    if ( DiagramObjectSelector.selectedObject != null && DiagramObjectSelector.selectedObjects == null ) {
                        Type selectedObjectType = selectedObject.GetType();
                        string objectTypeName = "";
                        if (selectedObjectType == typeof(BrainstormingNode)) {
                            objectTypeName = "Node";
                        } else if (selectedObjectType == typeof(BrainstormingArrow)) {
                            objectTypeName = "Arrow";
                        }
                        string undoOperationName = string.Format("Deleted {0}", objectTypeName);
                        Undo.RegisterCompleteObjectUndo(this.currentDiagram, undoOperationName);
                        selectedObject.OnRemove(this.currentDiagram);
                    } else if ( DiagramObjectSelector.selectedObjects != null ) {
                        Undo.RegisterCompleteObjectUndo(this.currentDiagram, "Deleted Brainstorming Objects");
                        foreach ( DiagramObject o in DiagramObjectSelector.selectedObjects ) {
                            o.OnRemove(this.currentDiagram);
                        }
                    }
                    this.currentDiagramEditor.serializedObject.Update();
                    break;
                case DiagramGenericContextualMenu.StandardAction.TraceArrow:
                    DiagramObjectSelector.BeginNewArrowTrace<BrainstormingArrow>((DiagramNode)selectedObject);
                    break;
            }
        }

        public override void OnDrawPreviewArrow ( Vector2[] arrowPoints ) {
            // Get mouse position
            Vector2 endPoint = this.mousePositionOnDiagram;

            // Calc start point, it must not be displayed over the start node
            DiagramElement elementA = DiagramObjectSelector.arrowStartElement;

            float angle           = GeometryCalculator.GetAngleIn360Degrees(elementA.positionByBarycenter, endPoint);
            int   sideNumber      = Mathf.RoundToInt(angle / 90) % 4;
            float magnetizedAngle = sideNumber * 90;

            Vector2 startPoint = GeometryCalculator.GetPositionAroundPoint(elementA.positionByBarycenter, elementA.radiuses, (magnetizedAngle - 180) * Mathf.Deg2Rad);
            
            // Calc and draw bezier
            Vector2 startTangent;
            Vector2 endTangent;

            if ( sideNumber % 2 == 0 ) {
                startTangent = new Vector2(endPoint.x,   startPoint.y);
                endTangent   = new Vector2(startPoint.x, endPoint.y  );
            } else {
                startTangent = new Vector2(startPoint.x, endPoint.y  );
                endTangent   = new Vector2(endPoint.x,   startPoint.y);
            }

            Handles.DrawBezier(startPoint, endPoint, startTangent, endTangent, Color.red, null, 2.0f);
        }

        public override void OnBackgroundPainting () {
            Backgrounds.PaintRect(this.layoutPosition, Color.white);
        }

        public override void OnSelection ( DiagramObject selectedElement ) {
            if (selectedElement == null) {
                Selection.activeObject = null;
                return;
            }

            SharedDiagramObjectEditorContainer editorContainer = null;
            DiagramEditorLayout.GetLoadedEditor(this.currentDiagram, selectedElement, ref editorContainer);
            Selection.activeObject = editorContainer;
        }

        public override void OnArrowCreation ( Type arrowType, DiagramElement elementA, DiagramElement elementB ) {
            Undo.RegisterCompleteObjectUndo(this.currentDiagram, "Added Arrow");
            ((Brainstorming)this.currentDiagram).OnAddArrow(arrowType, elementA, elementB);
        }

        public override void OnContextualMenuState () {
            contextualMenu.Show();
        }

        public override void OnBeforeDraggingState () {
            bool isMultiSelectionEnabled = DiagramObjectSelector.selectedObjects != null;
            Undo.RegisterCompleteObjectUndo(this.currentDiagram, "Moved Node" + (isMultiSelectionEnabled ? "s" : ""));
        }

        public override void OnPaste () {
            Undo.RegisterCompleteObjectUndo(this.currentDiagram, "Pasted New Nodes");
        }

        public new void OnDestroy () {
            base.OnDestroy();
            SaveDiagram();
        }
    }
}