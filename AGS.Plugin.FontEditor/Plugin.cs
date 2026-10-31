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
		private Dictionary<int, FontPane>			FontPaneList			= new Dictionary<int, FontPane>();
		private Dictionary<int, ContentDocument>	PaneDictionary			= new Dictionary<int, ContentDocument>();
		private MenuCommand							MenuCommand;
        private MenuCommand InfoMenuCommand;
        private int									MenuEntry;
        private Dictionary<int, string> FontFileList = new Dictionary<int, string>();
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

        void ProjectTree_BeforeShowContextMenu(BeforeShowContextMenuEventArgs evArgs)
        {
            string nodeID = evArgs.SelectedNodeID;

            // AGS 4.x: actual WFN filename
            if (nodeID.StartsWith("FntFile:", System.StringComparison.OrdinalIgnoreCase))
            {
                string fileName = nodeID.Substring("FntFile:".Length);

                if (!fileName.EndsWith(".wfn", System.StringComparison.OrdinalIgnoreCase))
                    return;

                foreach (KeyValuePair<int, string> item in FontFileList)
                {
                    if (System.String.Equals(
                        System.IO.Path.GetFileName(item.Value),
                        fileName,
                        System.StringComparison.OrdinalIgnoreCase))
                    {
                        MenuEntry = item.Key;
                        evArgs.MenuCommands.Commands.Add(MenuCommand);
                        return;
                    }
                }

                return;
            }

            // AGS 3.6.x: legacy font number
            if (nodeID.StartsWith("Fnt", System.StringComparison.OrdinalIgnoreCase))
            {
                string fontNumber = nodeID.Substring(3);
                int entry;

                if (int.TryParse(fontNumber, out entry) &&
                    FontFileList.ContainsKey(entry))
                {
                    MenuEntry = entry;
                    evArgs.MenuCommands.Commands.Add(MenuCommand);
                }
            }
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

            int entry = -1;

            if (controlID == COMPONENT_MENU_COMMAND)
            {
                entry = MenuEntry;
            }

            if (controlID.StartsWith(CONTROL_ID_ROOT_NODE))
            {
                int.TryParse(
                    controlID.Substring(CONTROL_ID_ROOT_NODE.Length),
                    out entry);
            }

            if (entry < 0 || !FontFileList.ContainsKey(entry))
                return;

            ContentDocument editpane;

            if (PaneDictionary.TryGetValue(entry, out editpane))
            {
                if (editpane == null)
                {
                    string filePath = FontFileList[entry];
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

                        if (PaneDictionary.ContainsKey(entry) &&
                            PaneDictionary[entry] == closedDocument)
                        {
                            PaneDictionary[entry] = null;
                        }
                    };

                    PaneDictionary[entry] = editpane;
                }

                LocalEditor.GUIController.AddOrShowPane(editpane);
            }
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

            FontPaneList.Clear();
            PaneDictionary.Clear();
            FontFileList.Clear();

            string gameDirectory = LocalEditor.CurrentGame.DirectoryPath;

            string fontsDirectory = System.IO.Path.Combine(gameDirectory, "Fonts");

            if (System.IO.Directory.Exists(fontsDirectory))
                gameDirectory = fontsDirectory;

            string[] fontFiles = System.IO.Directory.GetFiles(
                gameDirectory,
                "*.wfn",
                System.IO.SearchOption.TopDirectoryOnly);

            System.Array.Sort(
                fontFiles,
                System.StringComparer.OrdinalIgnoreCase);

            int fontcounter = 0;

            foreach (string filePath in fontFiles)
            {
                string fileName = System.IO.Path.GetFileName(filePath);

                // Temporarily retain the existing dictionary structure.
                FontFileList.Add(fontcounter, filePath);
                PaneDictionary.Add(fontcounter, null);

                fontcounter++;
            }
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
