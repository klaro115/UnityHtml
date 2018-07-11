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

		private Vector2 scrollPosition = Vector2.zero;

		private List<DialogueNodeEditor.Node> nodes = null;
		private int selectedNodeIndex = -1;

		private string findNodeTxt = "Find node by name";
		private Vector2 findPos = Vector2.zero;

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

			// Make sure there is always a node editor instance at hand:
			if(nodeEditor == null)
			{
				nodeEditor = new DialogueNodeEditor();
			}


			// B) HEADER:

			EditorGUI.DrawRect(new Rect(0, uiHeaderHeight,Screen.width, 1), Color.black);
			EditorGUI.DrawRect(areaRect, Color.gray);

			Dialogue newDialogue = (Dialogue)EditorGUI.ObjectField(new Rect(0,0,300,16), "Dialogue asset", dialogue, typeof(Dialogue), false);
			if(newDialogue != dialogue)
			{
				// Assign new asset and reset selections:
				dialogue = newDialogue;
				assetChanged = true;
				nodeEditor.setSelection(DialogueNodeEditor.Node.Blank);

				// Rebuild node graph:
				if(dialogue != null)
				{
					DialogueNodeEditor.createNodeList(dialogue, ref nodes);
					nodeEditor.autoLayoutNodes();
				}
				return;
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
				if(GUILayout.Button("Rebuild graph"))
				{
					DialogueNodeEditor.createNodeList(dialogue, ref nodes);
					nodeEditor.autoLayoutNodes();
				}
				if(GUILayout.Button("Auto-Layout"))
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
					if(nodeEditor.createNewNode(dialogue, ref nodes))
					{
						selectNode(nodes.Count - 1);
						Repaint();
					}
				}
				EditorGUI.BeginDisabledGroup(nodeEditor.Selected.node == null);
				if(GUILayout.Button("Delete Node"))
				{
					deleteNode(nodeEditor.Selected.node);
				}
				if(GUILayout.Button("Duplicate"))
				{
					duplicateNode(nodeEditor.Selected);
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
				DialogueNodeEditor.createNodeList(dialogue, ref nodes);
			}

			// Draw existing nodes in their relative positions here with all their dependencies.
			if(dialogue != null && nodes != null)
			{
				assetChanged = assetChanged || nodeEditor.drawNodes(dialogue, nodes, scrollPosition);
			}

			// Navigation elements:
			{
				Rect navRect = new Rect(10, 10, 200, 70);
				GUI.BeginGroup(navRect);
				EditorGUI.DrawRect(new Rect(0, 0, navRect.width, navRect.height), Color.black);
				EditorGUI.DrawRect(new Rect(1, 1, navRect.width-2, navRect.height-2), new Color(0.75f, 0.75f, 0.75f));

				// Draw navigation buttons at the top left of the work space:
				if (GUI.RepeatButton(new Rect(5, 25, 20, 20), "L")) scrollPosition.x -= 10.0f;
				if (GUI.RepeatButton(new Rect(45, 25, 20, 20), "R")) scrollPosition.x += 10.0f;
				if (GUI.RepeatButton(new Rect(25, 5, 20, 20), "D")) scrollPosition.y -= 10.0f;
				if (GUI.RepeatButton(new Rect(25, 45, 20, 20), "U")) scrollPosition.y += 10.0f;
				if (GUI.RepeatButton(new Rect(25, 25, 20, 20), "O")) scrollPosition = Vector2.zero;

				findNodeTxt = EditorGUI.TextField(new Rect(70, 5, 85, 16), findNodeTxt);
				if(GUI.Button(new Rect(156, 5, 39, 16), "Find") && !string.IsNullOrEmpty(findNodeTxt))
				{
					int findNodeIndex = -1;
					try
					{
						findNodeIndex = System.Convert.ToInt32(findNodeTxt);
					}
					catch (System.Exception) { }
					for(int i = 0; i < nodes.Count; ++i)
					{
						DialogueNodeEditor.Node fNode = nodes[i];
						DialogueNode fdNode = fNode.node;
						if (fdNode == null) continue;
						if(i == findNodeIndex || fdNode.name.Contains(findNodeTxt))
						{
							selectNode(i);
							centerSelected();
							break;
						}
					}
				}
				findPos.x = EditorGUI.FloatField(new Rect(70, 22, 42, 16), findPos.x);
				findPos.y = EditorGUI.FloatField(new Rect(113, 22, 42, 16), findPos.y);
				if (GUI.Button(new Rect(156, 22, 39, 16), "Goto"))
				{
					scrollPosition.x = Mathf.Round(findPos.x - Screen.width * 0.5f);
					scrollPosition.y = Mathf.Round(findPos.y - Screen.height * 0.5f);
				}

				GUI.EndGroup();
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

		public void centerSelected()
		{
			if (nodes == null || selectedNodeIndex < 0 || selectedNodeIndex >= nodes.Count) return;

			DialogueNodeEditor.Node node = nodes[selectedNodeIndex];
			Rect nodeRect = node.rect;
			Vector2 nodePos = nodeRect.center;
			scrollPosition = nodePos - new Vector2(Screen.width, Screen.height) * 0.5f;
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

			Repaint();

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

			Repaint();
			return true;
		}

		#endregion
	}
}
