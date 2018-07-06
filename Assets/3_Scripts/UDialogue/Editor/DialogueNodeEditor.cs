using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UDialogue
{
	[System.Serializable]
	public class DialogueNodeEditor
	{
		#region Types

		[System.Serializable]
		public struct Node
		{
			public DialogueNode node;
			public int rootId;
			public Rect rect;

			public static Node Blank
			{
				get { Node n = new Node() { node=null, rootId=-1 }; return n; }
			}
		}
		struct NodeAction
		{
			public bool changed;
			public bool selected;
		}

		#endregion
		#region Fields

		private Node selected = Node.Blank;
		private List<Node> nodes = null;

		private bool dragNDrop = false;
		private int selectedResponse = -1;
		private bool selectedResponseDropdown = false;
		private int responseTargetNodeId = -1;

		//...

		#endregion
		#region Methods UI

		public void setSelection(Node newSelection)
		{
			// Selection changed, so reset interaction flags:
			if(selected.node != newSelection.node)
			{
				dragNDrop = false;
			}

			selected = newSelection;

			if(selected.node != null)
			{
				Selection.activeObject = selected.node;
			}
		}
		private void toggleDragNDrop()
		{
			selectedResponseDropdown = false;
			dragNDrop = !dragNDrop;
		}
		private void toggleResponseDropdown()
		{
			selectedResponseDropdown = !selectedResponseDropdown;
			responseTargetNodeId = -1;	//<<<todo
		}

		public bool drawNodes(List<Node> inNodes)
		{
			bool changed = false;
			nodes = inNodes;

			if(nodes != null && nodes.Count != 0)
			{
				// Draw links between nodes first:
				for(int i = 0; i < nodes.Count; ++i)
				{
					Node node = nodes[i];
					DialogueNode dNode = node.node;

					// Links from dialogue root:
					if(node.rootId >= 0)
					{
						drawNodeLink(new Vector2(0, Screen.height*.5f), node);
					}

					// Links to response nodes:
					if(dNode.responses != null)
					{
						for(int j = 0; j < dNode.responses.Length; ++j)
						{
							DialogueResponse resp = dNode.responses[j];
							if(resp.nextNode == null) continue;

							Node targetNode = Node.Blank;
							foreach(Node n in nodes)
								if(n.node == resp.nextNode) targetNode = n;
							
							drawNodeLink(getResponsePosition(node, j), targetNode);
						}
					}
				}

				// Draw the actual individual nodes:
				for(int i = 0; i < nodes.Count; ++i)
				{
					Node node = nodes[i];
					// If a node is null, mark it for later deletion:
					if(node.node == null)
					{
						node.rootId = -1;
						nodes[i] = node;
						continue;
					}

					bool isSelected = selected.node == node.node;
					// Update drag&drop:
					NodeAction actions = new NodeAction() { changed=false, selected=false };
					if(isSelected && dragNDrop)
					{
						Vector2 mousePos = Event.current.mousePosition;
						node.rect.x = mousePos.x-123;
						node.rect.y = mousePos.y-5;
						actions.changed = true;
					}

					// Draw the node on screen:
					actions = drawNode(ref node, isSelected, i);
					// Raise the dirty flag if the node has been changed:
					if(actions.changed)
					{
						nodes[i] = node;
						changed = true;
					}
					// Select the node if a related action was performed:
					if(actions.selected)
					{
						setSelection(node);
					}
				}

				// Draw response dropdown overlay:
				if(selectedResponseDropdown && selectedResponse >= 0 && selectedResponse < selected.node.responses.Length)
				{
					Node respNode = selected;
					DialogueNode respDNode = respNode.node;

					Rect respRect = respNode.rect;
					float respPosY = respRect.y+34 + (-respDNode.responses.Length*.5f + selectedResponse) * 17;
					respRect = new Rect(respRect.x+138,respPosY,122,39);

					GUI.BeginGroup(respRect);

					EditorGUI.DrawRect(new Rect(0,0,122,39), Color.black);
					EditorGUI.DrawRect(new Rect(1,1,120,37), new Color(0.75f,0.75f,0.75f));

					if(GUI.Button(new Rect(2,2,118,16), "New Node"))
					{
						Debug.Log("Creating new node connected to selected node's response " + selectedResponse);
						Debug.Log("NOT IMPLEMENTED");
					}
					if(GUI.Button(new Rect(2,20,88,16), "Connect to ID"))
					{
						Debug.Log("Connecting selected node's response " + selectedResponse +
							" to node " + responseTargetNodeId);
						Debug.Log("NOT IMPLEMENTED");
					}
					responseTargetNodeId = EditorGUI.DelayedIntField(new Rect(91,20,29,16), responseTargetNodeId);

					GUI.EndGroup();
				}
			}

			return changed;
		}

		private NodeAction drawNode(ref Node node, bool isSelected, int nodeIndex)
		{
			NodeAction actions = new NodeAction() { changed=false, selected=false };

			bool prevChanged = GUI.changed;
			GUI.changed = false;

			EditorGUI.DrawRect(new Rect(node.rect.x-1,node.rect.y-1,130,70), Color.black);
			GUI.BeginGroup(new Rect(node.rect));
			EditorGUI.DrawRect(new Rect(0,0,128,68), new Color(0.75f,0.75f,0.75f));
			EditorGUI.DrawRect(new Rect(0,15,128,1), Color.black);
			
			DialogueNode dNode = node.node;

			// HEADER:

			EditorGUI.LabelField(new Rect(0,0,106,16), nodeIndex + ") " + dNode.name);
			if(isSelected) dNode.name = (EditorGUI.DelayedTextField(new Rect(13,0,105,16), dNode.name));
			if(GUI.Button(new Rect(119,1,8,8), ""))
			{
				toggleDragNDrop();
				actions.selected = true;
			}

			// EDITING:

			string contTxt = dNode.content != null && !string.IsNullOrEmpty(dNode.content[0].text) ?
				dNode.content[0].text : "<empty>";
			EditorGUI.LabelField(new Rect(0,17,106,16), contTxt, EditorStyles.miniLabel);
			EditorGUI.LabelField(new Rect(0,34,106,16), "Contents: " + dNode.content.Length.ToString());
			EditorGUI.LabelField(new Rect(0,51,106,16), "Responses: " + dNode.responses.Length.ToString());

			if(GUI.Button(new Rect(107,17,20,16), ">")) actions.selected = true;
			if(GUI.Button(new Rect(107,34,20,16), "+"))
			{
				actions.changed = true;
				addNodeContent(dNode, dNode.content.Length);
			}
			if(GUI.Button(new Rect(107,51,20,16), "+"))
			{
				actions.changed = true;
				Debug.Log("TODO: add response helper methods");
			}

			GUI.EndGroup();

			if(dNode.responses != null)
			{
				int respCount = dNode.responses.Length;
				float respSize = respCount * 17.0f;
				Rect respRect = new Rect(node.rect.x+129, node.rect.y+34-respSize*.5f, 100, respSize);

				GUI.BeginGroup(respRect);
				for(int i = 0; i < respCount; ++i)
				{
					DialogueResponse response = dNode.responses[i];
					if(GUI.Button(new Rect(0,i*17-1,20,16), i.ToString()))
					{
						setSelection(node);
						int prevSelectedResponse = selectedResponse;
						selectedResponse = i;
						if(Event.current.button == 1)
						{
							if(selectedResponse == prevSelectedResponse) toggleResponseDropdown();
						}
						else selectedResponseDropdown = false;
					}
					EditorGUI.LabelField(new Rect(20,i*17-1,80,16), response.responseText, EditorStyles.miniLabel);
				}
				GUI.EndGroup();
			}

			actions.changed = GUI.changed;
			GUI.changed = prevChanged || GUI.changed;

			return actions;
		}

		private void drawNodeLink(Vector2 p0, Node target)
		{
			Vector2 p1 = target.rect.position + Vector2.up * target.rect.height * 0.5f;
			Vector2 tan = Vector2.right * 32;

			Color c0 = new Color(0.0f, 0.9f, 1.0f);
			Color c1 = new Color(0.0f, 0.9f, 1.0f, 0.5f);
			Handles.DrawBezier(p0, p1, p0 + tan, p1 - tan, c0, null, 1);
			Handles.DrawBezier(p0, p1, p0 + tan, p1 - tan, c1, null, 2);
		}
		private Vector2 getResponsePosition(Node node, int responseIndex)
		{
			if(node.node == null || node.node.responses == null) return node.rect.center;

			int respCount = node.node.responses.Length;
			float respSize = (respCount + 1) * 17.0f;
			float basePosY = node.rect.center.y - 0.5f * respSize;

			float x = node.rect.x + node.rect.width+16;
			float y = basePosY + responseIndex * 17.0f+16;

			return new Vector2(x, y);
		}

		#endregion
		#region Methods Node Helpers

		public static void addNodeContent(DialogueNode node, int index)
		{
			int newContentCount = node != null ? node.content.Length + 1 : 1;
			index = Mathf.Clamp(index, 0, newContentCount);

			DialogueContent[] newContents = new DialogueContent[newContentCount];

			// Copy contents from previous array:
			if(node.content != null)
			{
				for(int i = 0; i < newContentCount; ++i)
				{
					int srcIndex = Mathf.Max(i < index ? i : i - 1, 0);
					newContents[i] = node.content[srcIndex];
				}
			}

			// Create new blank content at index:
			newContents[index] = DialogueContent.Blank;

			// Assign array back to the node:
			node.content = newContents;

			EditorUtility.SetDirty(node);
		}
		public static bool removeNodeContent(DialogueNode node, int index)
		{
			// Make sure the parameters for this call are in any way reasonable:
			if(node == null || node.content == null || index < 0 || index >= node.content.Length)
			{
				return false;
			}
			int newContentCount = node.content.Length - 1;

			// If there's only one content block remaining, kill the array:
			if(newContentCount <= 0)
			{
				node.content = null;
				return true;
			}

			// Generate new content array:
			DialogueContent[] newContents = new DialogueContent[newContentCount];
			// Copy contents from previous content array:
			for(int i = 0; i < newContentCount; ++i)
			{
				int srcIndex = i < index ? i : i + 1;
				newContents[i] = node.content[srcIndex];
			}

			// Assign array back to the node:
			node.content = newContents;

			EditorUtility.SetDirty(node);
			return true;
		}

		#endregion
		#region Methods Layout

		public void autoLayoutNodes()
		{
			if(nodes == null) return;

			int[] depths = new int[nodes.Count];
			Dictionary<DialogueNode, int> nodeIndices = new Dictionary<DialogueNode, int>();
			for(int i = 0; i < depths.Length; ++i)
			{
				depths[i] = -1;
				DialogueNode n = nodes[i].node;
				if(n != null) nodeIndices.Add(n, i);
			}

			List<int> currentNodeIndices = new List<int>();
			List<int> nextNodeIndices = new List<int>();
			foreach(Node n in nodes)
			{
				if(n.node != null && n.rootId >= 0)
				{
					int nIndex;
					nodeIndices.TryGetValue(n.node, out nIndex);
					currentNodeIndices.Add(nIndex);
				}
			}

			int currentDepth = 0;
			List<int> depthCounts = new List<int>();
			while(currentNodeIndices.Count != 0 && currentDepth < 256)
			{
				// NOTE: 256 was set as an arbitrary limit to prevent crashes from loops in dialogue structure.
				depthCounts.Add(0);
				for(int i = 0; i < currentNodeIndices.Count; i++)
				{
					int currentIndex = currentNodeIndices[i];
					Node node = nodes[currentIndex];
					DialogueNode dNode = node.node;
					if(dNode == null)
					{
						depths[currentIndex] = 255;
						continue;
					}

					depths[currentIndex] = currentDepth;
					depthCounts[currentDepth] += 1;

					for(int j = 0; j < dNode.responses.Length; ++j)
					{
						DialogueResponse resp = dNode.responses[j];
						if(resp.nextNode == null) continue;

						int respNodeIndex;
						if(!nodeIndices.TryGetValue(resp.nextNode, out respNodeIndex)) continue;

						Node respNode = nodes[respNodeIndex];
						if(respNode.node != null && respNode.node != dNode && depths[respNodeIndex] < 0)
						{
							nextNodeIndices.Add(respNodeIndex);
						}
					}
				}

				List<int> newNextIndices = currentNodeIndices;
				currentNodeIndices = nextNodeIndices;
				nextNodeIndices = newNextIndices;
				nextNodeIndices.Clear();
				currentDepth++;
			}

			// Push any disconnected, invalid or new nodes to the very end of the depth spectrum:
			int maxDepth = depthCounts.Count;
			depthCounts.Add(0);
			foreach(Node n in nodes)
			{
				if(n.node == null) continue;
				int nIndex;
				if(!nodeIndices.TryGetValue(n.node, out nIndex)) continue;

				int nDepth = depths[nIndex];
				if(nDepth < 0)
				{
					depths[nIndex] = maxDepth;
					depthCounts[maxDepth]++;
				}
			}

			// Calculate screen positions for all nodes:
			int[] depthCountsDone = new int[depthCounts.Count];
			for(int i = 0; i < nodes.Count; ++i)
			{
				const float nodeOffsetX = 32;
				const float nodeOffsetY0 = 10;
				const float nodeHeight0 = 70;
				float nodeOffsetY = nodeOffsetY0 + (Screen.height - nodeHeight0) * 0.5f;

				const float nodeWidth = 130 + 64 + 31;
				const float nodeHeight = nodeHeight0 + 10;

				Node node = nodes[i];
				int nIndex;
				if(!nodeIndices.TryGetValue(node.node, out nIndex)) continue;

				int nDepth = depths[nIndex];
				int nDepthCountAll = depthCounts[Mathf.Clamp(nDepth, 0, maxDepth)];
				int nDepthCount = depthCountsDone[nDepth];

				float countOffset = -0.5f * (nDepthCountAll - 1) + nDepthCount;
				float posX = nodeOffsetX + nDepth * nodeWidth;
				float posY = nodeOffsetY + countOffset * nodeHeight;

				node.rect = new Rect(posX, posY, 130, 70);
				nodes[i] = node;

				depthCountsDone[nDepth]++;
			}
		}

		#endregion
	}
}
