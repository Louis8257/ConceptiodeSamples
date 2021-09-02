using ConceptiodeEditor.Components.ContextualMenus;

namespace ConceptiodeDemoEditor.Brainstorming {

    public class BrainstormingContextualMenu : DiagramGenericContextualMenu {
        protected override void OnDisplayActionList () {
            this.ListStandardActions();
        }
    }

}
