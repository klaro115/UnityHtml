using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace UDialogue
{
	[System.Serializable]
	public class DialogueEditor : EditorWindow
	{
		#region Types

		public enum DialogueStatus : int
		{
			Ok		= 0,
			NoRoot	= 1,
		}

		#endregion
		#region Fields

		public Dialogue dialogue = null;
		private bool assetChanged = false;

//		private Vector2 scrollPosition = Vector2.zero;

		//...

		private List<DialogueNodeEditor.Node> nodes = null;
		private int selectedNodeIndex = -1;

		[SerializeField]
		private DialogueNodeEditor nodeEditor = null;
		private System.Diagnostics.Stopwatch stopwatch = null;

		private DialogueStatus assetStatus = DialogueStatus.Ok;
		private static readonly string[] assetStatusLines = new string[2] {
			"ok", "No root nodes assigned!",
		};

		#endregion
		#region Methods Static

		[MenuItem("Window/Dialogue Editor")]
		public static DialogueEditor showEditorWindow()
		{
			DialogueEditor de = (DialogueEditor)EditorWindow.GetWindow<DialogueEditor>("Dialogue Editor");

			return de;
		}

		#endregion
		#region Methods

		void Update()
		{
			if(stopwatch == null) stopwatch = new System.Diagnostics.Stopwatch();
			if(!stopwatch.IsRunning) stopwatch.Start();
			if(stopwatch.ElapsedMilliseconds > 16)
			{
				Repaint();
				stopwatch.Reset();
			}
		}

		void OnGUI()
		{
			// A) CONSTANTS & REFERENCES:

			// UI constants:
			const float uiHeaderHeight = 38f;
			Rect areaRect = new Rect(0, uiHeaderHeight+1, Screen.width, Screen.height-uiHeaderHeight-1);

			// Get the currently selected node:
			DialogueNodeEditor.Node selected = DialogueNodeEditor.Node.Blank;
			if(dialogue != null && nodes != null && selectedNodeIndex >= 0 && selectedNodeIndex < nodes.Count)
			{
				selected = nodes[selectedNodeIndex];
			}


			// B) HEADER:

			EditorGUI.DrawRect(new Rect(0, uiHeaderHeight,Screen.width, 1), Color.black);
			EditorGUI.DrawRect(areaRect, Color.gray);

			Dialogue newDialogue = (Dialogue)EditorGUI.ObjectField(new Rect(0,0,300,16), "Dialogue asset",
				dialogue, typeof(Dialogue), false);
			if(newDialogue != dialogue)
			{
				dialogue = newDialogue;
				assetChanged = true;
				if(nodeEditor != null) nodeEditor.setSelection(DialogueNodeEditor.Node.Blank);
			}

			//Buttons for layouting:
			GUILayout.BeginArea(new Rect(Screen.width-303,0,300,18));
			{
				EditorGUILayout.BeginHorizontal();
				if(GUILayout.Button("Save changes") && dialogue != null)
				{
					DialogueEditorHelper.saveDialogueAsset(dialogue);
					assetChanged = false;
				}
				if(GUILayout.Button("Rebuild graph") && nodeEditor != null)
				{
					createNodeList();
					nodeEditor.autoLayoutNodes();
				}
				if(GUILayout.Button("Auto-Layout") && nodeEditor != null)
				{
					nodeEditor.autoLayoutNodes();
				}
				EditorGUILayout.EndHorizontal();
			}
			GUILayout.EndArea();
			// Buttons for creating, deleting and duplicating node assets:
			GUILayout.BeginArea(new Rect(Screen.width-303,19,300,18));
			{
				EditorGUILayout.BeginHorizontal();
				if(GUILayout.Button("New Node"))
				{
					// Create new node and select it right away:
					if(createNewNode())
						selectNode(nodes.Count - 1);
				}
				EditorGUI.BeginDisabledGroup(selected.node == null);
				if(GUILayout.Button("Delete Node"))
				{
					deleteNode(selected.node);
				}
				if(GUILayout.Button("Duplicate"))
				{
					duplicateNode(selected);
				}
				EditorGUI.EndDisabledGroup();
				EditorGUILayout.EndHorizontal();
			}
			GUILayout.EndArea();

			EditorGUI.LabelField(new Rect(0,16,300,16), "Asset status: ", assetStatusLines[(int)assetStatus]);

			if(dialogue == null) return;


			// C) WORK AREA:

			GUI.BeginGroup(areaRect);
			GUI.BeginClip(new Rect(0, 0, areaRect.width, areaRect.height));

			if(nodes == null && dialogue != null && dialogue.rootNodes != null)
			{
				createNodeList();
			}

			// Draw existing nodes in their relative positions here with all their dependencies.
			if(dialogue != null && nodes != null)
			{
				if(nodeEditor == null) nodeEditor = new DialogueNodeEditor();

				assetChanged = assetChanged || nodeEditor.drawNodes(nodes);
			}

			GUI.EndClip();
			GUI.EndGroup();


			// D) ASSET CHANGES:

			// Do some post-checks if the asset was changed:
			if(assetChanged)
			{
				// Drop current selection if it has expired:
				if(selected.node == null) selectedNodeIndex = -1;

				// Remove any invalid nodes from dialogue:
				nodes.RemoveAll(o => o.node == null);
			}

			//Reset flags:
			assetChanged = false;
		}

		#endregion
		#region Methods Nodes

		private bool selectNode(int index)
		{
			// Don't allow selection of invalid, inexistant or out-of-bounds nodes:
			if(dialogue == null || nodes == null) return false;
			if(index < 0 || index >= nodes.Count) return false;
			if(nodes[index].node == null) return false;

			// Set the node selected and return success:
			selectedNodeIndex = index;
			return true;
		}

		private void createNodeList()
		{
			// Initialize list if it hasn't been done already:
			if(nodes == null) nodes = new List<DialogueNodeEditor.Node>();
			nodes.Clear();

			string assetPath = AssetDatabase.GetAssetPath(dialogue);
			object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
			foreach(object asset in allAssets)
			{
				DialogueNode dNode = asset as DialogueNode;
				if(dNode == null) continue;
				int newRootId = -1;
				for(int i = 0; i < dialogue.rootNodes.Length; ++i)
					if(dNode == dialogue.rootNodes[i].node) newRootId = i;
				DialogueNodeEditor.Node newNode = new DialogueNodeEditor.Node()
					{ node=dNode, rootId=newRootId, rect=new Rect(100,100,128,64) };
				nodes.Add(newNode);
			}
		}
		private bool createNewNode()
		{
			// Make sure the dialogue asset is non-null:
			if(dialogue == null) return false;
			// Update node list: (initialize list and load existing nodes from dialogue asset)
			if(nodes == null || nodes.Count == 0) createNodeList();

			// Create a new node asset in dialogue:
			DialogueNode newDNode = DialogueEditorHelper.createNewNode(dialogue);
			// Create an editor node representation:
			DialogueNodeEditor.Node newNode = DialogueNodeEditor.Node.Blank;
			newNode.node = newDNode;

			// Set newly created node as root node if no root has been assigned yet:
			if(dialogue.rootNodes == null || dialogue.rootNodes.Length == 0)
			{
				// Create new root node in dialogue with no conditions:
				DialogueConditions newRootConds = DialogueConditions.None;
				DialogueRoot newRoot = new DialogueRoot() { node=newDNode, conditions=newRootConds };
				dialogue.rootNodes = new DialogueRoot[1] { newRoot };
				newNode.rootId = 0;

				// Save changes to asset:
				DialogueEditorHelper.saveDialogueAsset(dialogue);
			}

			// Add the new node representation to nodes list:
			nodes.Add(newNode);
			return true;
		}
		private bool deleteNode(DialogueNode dNode)
		{
			if(dialogue == null || dNode == null) return false;

			// Mark the deleted and any deprecated/dead nodes for deletion:
			for(int i = 0; i < nodes.Count; ++i)
			{
				DialogueNodeEditor.Node n = nodes[i];
				if(n.node == null || n.node == dNode)
				{
					n.node = null;
					n.rootId = -1;
					nodes[i] = n;
				}
			}

			// Remove all marked nodes from list:
			nodes.RemoveAll(o => o.node == null);

			// Destroy node asset in dialogue:
			return DialogueEditorHelper.destroyNodeAsset(dNode);
		}
		private bool duplicateNode(DialogueNodeEditor.Node node)
		{
			if(dialogue == null || node.node == null) return false;

			// Duplicate node asset:
			DialogueNode newDNode = DialogueEditorHelper.duplicateNode(node.node);
			if(newDNode == null)
			{
				Debug.LogError("[DialogueEditor] Error! Failed to duplicate node in dialogue asset!");
				return false;
			}

			// Create new node from prefab one, but using the newly cloned asset:
			DialogueNodeEditor.Node newNode = node;
			newNode.node = newDNode;
			newNode.rect = new Rect(node.rect.x+30,node.rect.y+30,node.rect.width,node.rect.height);

			// Add to nodes list and return success:
			nodes.Add(newNode);
			return true;
		}

		#endregion
	}
}
