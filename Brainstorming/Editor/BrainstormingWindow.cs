using ConceptiodeEditor;
using ConceptiodeEditor.Components.Selections;
using ConceptiodeEditor.UIs;
using UnityEditor;

namespace ConceptiodeDemoEditor.Brainstorming {

    public class BrainstormingWindow : EditorWindow {

        public const string NAME = "Brainstorming";

        public static BrainstormingWindow window;

        /// <summary>
        /// <para>The diagram serialized by the window.</para>
        /// </summary>
        public Brainstorming diagram;

        #region "Window Summoning & Diagram Change Methods"
        [MenuItem("Window/Conceptiode Demos/" + BrainstormingWindow.NAME)]
        /// <summary>
        /// <para>Show this window with the new diagram by clicking a button in the toolbar.</para>
        /// </summary>
        public static void SummonWindow () {
            BrainstormingWindow.window = EditorWindow.GetWindow<BrainstormingWindow>(NAME);
            BrainstormingWindow.window.Show();
        }

        /// <summary>
        /// <para>Show this window with the new diagram by a script call.</para>
        /// </summary>
        /// <param name="brainstormingDiagram"></param>
        public static void Summon ( Brainstorming brainstormingDiagram ) {
            if ( BrainstormingWindow.window == null ) {
                BrainstormingWindow.SummonWindow();
            }

            if ( brainstormingDiagram != null ) {
                BrainstormingWindow.window.diagram = brainstormingDiagram;
            }
        }
        #endregion

        public void Update () {
            if ( this.diagram != null ) {
                Repaint();
            }
        }

        public void OnGUI () {
            DiagramEditorLayout.DiagramView<BrainstormingView>(this.diagram);
            DiagramView diagramView = DiagramEditorLayout.GetDiagramView<BrainstormingView>();

            if ( diagram != null && diagramView.currentDiagramEditor != null ) {
                // Indicate when the diagram is dirty
                if ( diagramView.currentDiagramEditor.isDirty ) {
                    this.titleContent.text = NAME + "*";
                } else {
                    this.titleContent.text = NAME;
                }
            }
        }
    }
}