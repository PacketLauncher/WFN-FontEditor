using System.Collections.Generic;
using AGS.Types;

namespace AGS.Plugin.FontEditor
{
	public class Component: IEditorComponent
	{
		private const string						COMPONENT_ID			= "WFN-FontEditor";
		private const string						COMPONENT_MENU_COMMAND	= "WFN-FontEditorMenuCommand";
		private const string						CONTROL_ID_ROOT_NODE	= "FontEditorRoot";
		private IAGSEditor							LocalEditor;
        //private ContentDocument						Pane;
        private Dictionary<string, ContentDocument> PaneDictionary =
            new Dictionary<string, ContentDocument>(System.StringComparer.OrdinalIgnoreCase);
        private MenuCommand							MenuCommand;
        private MenuCommand InfoMenuCommand;
        private string _selectedFontPath;
        private const string INFO_MENU_COMMAND = "WFN-FontEditorInfo";
        private const string INFO_MESSAGE =
            "To edit fonts, right-click on the relevant font file under the Fonts section and select \"Edit Font (with WFN-FontEditor)\".";

        public Component(IAGSEditor editor)
        {
            LocalEditor = editor;

            LocalEditor.GUIController.RegisterIcon(
                "FontEditorIcon",
                Properties.Resources.FontEditor);

            LocalEditor.GUIController.ProjectTree.AddTreeRoot(
                this,
                CONTROL_ID_ROOT_NODE,
                "FontEditor",
                "FontEditorIcon");

            LocalEditor.GUIController.ProjectTree.BeforeShowContextMenu +=
                new BeforeShowContextMenuHandler(ProjectTree_BeforeShowContextMenu);

            MenuCommand = LocalEditor.GUIController.CreateMenuCommand(
                this,
                COMPONENT_MENU_COMMAND,
                "Edit Font (with WFN-FontEditor)");

            InfoMenuCommand = LocalEditor.GUIController.CreateMenuCommand(
                this,
                INFO_MENU_COMMAND,
                "How to Edit Fonts");
        }

        private string ResolveFontPath(string nodeID)
        {
            if (string.IsNullOrEmpty(nodeID) || LocalEditor.CurrentGame == null)
                return null;

            string gameDirectory = LocalEditor.CurrentGame.DirectoryPath;
            string fontsDirectory = System.IO.Path.Combine(gameDirectory, "Fonts");

            if (System.IO.Directory.Exists(fontsDirectory))
                gameDirectory = fontsDirectory;

            string fileName;

            // AGS 4.x: the node contains the actual WFN filename.
            if (nodeID.StartsWith("FntFile:", System.StringComparison.OrdinalIgnoreCase))
            {
                fileName = nodeID.Substring("FntFile:".Length);

                if (!fileName.EndsWith(".wfn", System.StringComparison.OrdinalIgnoreCase))
                    return null;

                // Reject paths; the node should contain only a filename.
                if (System.IO.Path.GetFileName(fileName) != fileName)
                    return null;
            }
            // AGS 3.6.x: the node contains the font number.
            else if (nodeID.StartsWith("Fnt", System.StringComparison.OrdinalIgnoreCase))
            {
                int fontNumber = -1;

                if (!int.TryParse(nodeID.Substring(3), out fontNumber) || fontNumber < 0)
                    return null;

                fileName = "AGSFNT" + fontNumber + ".WFN";
            }
            else
            {
                return null;
            }

            string filePath = System.IO.Path.Combine(gameDirectory, fileName);

            return System.IO.File.Exists(filePath) ? filePath : null;
        }

        void ProjectTree_BeforeShowContextMenu(BeforeShowContextMenuEventArgs evArgs)
        {
            _selectedFontPath = null;

            string nodeID = evArgs.SelectedNodeID;

            if (string.IsNullOrEmpty(nodeID))
                return;

            string filePath = ResolveFontPath(nodeID);

            if (filePath == null)
                return;

            _selectedFontPath = filePath;

            evArgs.MenuCommands.Commands.Add(MenuCommand);
        }

        string IEditorComponent.ComponentID
		{
			get { return COMPONENT_ID; }
		}
        IList<MenuCommand> IEditorComponent.GetContextMenu(string controlID)
        {
            if (controlID == CONTROL_ID_ROOT_NODE)
            {
                return new List<MenuCommand> { InfoMenuCommand };
            }

            return null;
        }
        void IEditorComponent.CommandClick(string controlID)
        {
            if (controlID == INFO_MENU_COMMAND || controlID == CONTROL_ID_ROOT_NODE)
            {
                System.Windows.Forms.MessageBox.Show(
                    INFO_MESSAGE,
                    "WFN-FontEditor",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Information);

                return;
            }

            string filePath = null;

            if (controlID == COMPONENT_MENU_COMMAND)
            {
                filePath = _selectedFontPath;
            }

            if (string.IsNullOrEmpty(filePath) ||
                !System.IO.File.Exists(filePath))
                return;

            ContentDocument editpane;

            if (!PaneDictionary.TryGetValue(filePath, out editpane) || editpane == null)
            {
                string fileName = System.IO.Path.GetFileName(filePath);
                string directory = System.IO.Path.GetDirectoryName(filePath);

                FontEditorPane fep = new FontEditorPane(
                    directory,
                    fileName,
                    fileName);

                fep.OnFontModified += new System.EventHandler(fep_OnFontModified);

                editpane = new ContentDocument(fep, fileName, this);

                fep.Tag = editpane;

                editpane.PanelClosed += delegate (object sender, System.EventArgs e)
                {
                    ContentDocument closedDocument = (ContentDocument)sender;

                    ContentDocument currentDocument;

                    if (PaneDictionary.TryGetValue(filePath, out currentDocument) &&
                        currentDocument == closedDocument)
                    {
                        PaneDictionary.Remove(filePath);
                    }
                };

                PaneDictionary[filePath] = editpane;
            }

            LocalEditor.GUIController.AddOrShowPane(editpane);
        }

        void IEditorComponent.PropertyChanged(string propertyName, object oldValue) { }

        void IEditorComponent.BeforeSaveGame()
        {
            foreach (ContentDocument item in PaneDictionary.Values)
            {
                if (item != null)
                {
                    FontEditorPane pane = (FontEditorPane)item.Control;

                    if (pane.HasUnsavedChanges)
                    {
                        pane.Save();
                    }
                }
            }
        }

        void IEditorComponent.RefreshDataFromGame()
        {
            LocalEditor.GUIController.ProjectTree.RemoveAllChildNodes(
                this, CONTROL_ID_ROOT_NODE);

            LocalEditor.GUIController.ProjectTree.StartFromNode(
                this, CONTROL_ID_ROOT_NODE);

            PaneDictionary.Clear();
        }
        void IEditorComponent.GameSettingsChanged() { }
		void IEditorComponent.ToXml(System.Xml.XmlTextWriter writer) { }
		void IEditorComponent.FromXml(System.Xml.XmlNode node) { }
		void IEditorComponent.EditorShutdown() { }

        void fep_OnFontModified(object sender, System.EventArgs e)
        {
            ContentDocument tp = (ContentDocument)((FontEditorPane)sender).Tag;
            MyEventArgs me = (MyEventArgs)e;

            const string modifiedMarker = " *";

            if (me.Modified)
            {
                if (!tp.Name.EndsWith(modifiedMarker))
                    tp.Name += modifiedMarker;
            }
            else
            {
                if (tp.Name.EndsWith(modifiedMarker))
                    tp.Name = tp.Name.Substring(
                        0, tp.Name.Length - modifiedMarker.Length);
            }

            tp.Control.DockingContainer.Text = tp.Name;

            System.Reflection.MethodInfo refreshMethod =
                LocalEditor.GUIController.GetType().GetMethod("DocumentTitlesChanged");

            if (refreshMethod != null)
            {
                refreshMethod.Invoke(LocalEditor.GUIController, null);
            }
        }
    }
}
