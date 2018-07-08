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
		private Vector2 offset = Vector2.zero;

		private bool dragNDrop = false;
		private int selectedResponse = -1;
		private bool selectedResponseDropdown = false;
		private int responseTargetNodeId = -1;

		//...

		#endregion
		#region Properties

		public Node Selected
		{
			get { return selected; }
		}

		#endregion
		#region Methods UI

		public void setSelection(Node newSelection)
		{
			// Selection changed, so reset interaction flags:
			if(selected.node != newSelection.node)
			{
				dragNDrop = false;
				selectedResponse = -1;
				selectedResponseDropdown = false;
			}

			// Set selection:
			selected = newSelection;

			// Select dialogue node asset for editing in inspector:
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

		public bool drawNodes(List<Node> inNodes, Vector2 inOffet)
		{
			offset = inOffet;

			bool changed = false;
			nodes = inNodes;

			Vector2 rootPos = new Vector2(0, Screen.height * 0.5f);
			Rect rootRect = new Rect(rootPos.x - offset.x - 39, rootPos.y - offset.y - 7, 38, 14);
			EditorGUI.DrawRect(new Rect(rootRect.x-1, rootRect.y-1, rootRect.width+2, rootRect.height+2), Color.black);
			EditorGUI.DrawRect(rootRect, new Color(0.75f, 0.75f, 0.75f));
			EditorGUI.LabelField(rootRect, "Root");

			if (nodes != null && nodes.Count != 0)
			{
				// Draw links between nodes first:
				for(int i = 0; i < nodes.Count; ++i)
				{
					Node node = nodes[i];
					DialogueNode dNode = node.node;

					// Links from dialogue root:
					if(node.rootId >= 0)
					{
						drawNodeLink(rootPos, node);
					}

					// Links to response nodes:
					if(dNode.responses != null)
					{
						for(int j = 0; j < dNode.responses.Length; ++j)
						{
							DialogueResponse resp = dNode.responses[j];
							Vector2 respPos = getResponsePosition(node, j);

							// Draw short red lines to indicate a response is no linked to any node:
							if (resp.nextNode == null)
							{
								respPos -= offset;
								Handles.color = Color.red;
								Handles.DrawLine(respPos, respPos + Vector2.right * 32);
								continue;
							}

							// Figure out the editor node representing the response's connected node asset:
							Node targetNode = Node.Blank;
							foreach(Node n in nodes)
							{
								if(n.node == resp.nextNode)
								{
									targetNode = n;
									break;
								}
							}
							
							// Draw a bezier curve starting at the response button and leading to the connected node:
							drawNodeLink(respPos, targetNode);
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
						node.rect.x = mousePos.x - 123;
						node.rect.y = mousePos.y - 5;
						actions.changed = true;
					}

					// Round positions to whole numbers, 'cause without it the damn thing looks ugly as hell:
					node.rect.x = Mathf.Round(node.rect.x);
					node.rect.y = Mathf.Round(node.rect.y);

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
					respRect = new Rect(respRect.x-offset.x+138,respPosY-offset.y,122,57);

					GUI.BeginGroup(respRect);

					EditorGUI.DrawRect(new Rect(0,0,122,57), Color.black);
					EditorGUI.DrawRect(new Rect(1,1,120,55), new Color(0.75f,0.75f,0.75f));

					if(GUI.Button(new Rect(2,2,118,16), "New Node"))
					{
						string assetPath = AssetDatabase.GetAssetPath(selected.node);
						Dialogue asset = AssetDatabase.LoadMainAssetAtPath(assetPath) as Dialogue;

						if (createNewNode(asset, ref nodes))
						{
							int newNodeId = nodes.Count - 1;
							if(createNodeLink(selected, selectedResponse, newNodeId))
							{
								Debug.Log("Creating new node connected to selected node's response " + selectedResponse);
								setSelection(nodes[newNodeId]);
							}
						}
					}

					bool uiShowClearLink = selected.node.responses != null &&
						selectedResponse >= 0 &&
						selectedResponse < selected.node.responses.Length &&
						selected.node.responses[selectedResponse].nextNode == null;
					EditorGUI.BeginDisabledGroup(uiShowClearLink);
					if (GUI.Button(new Rect(2, 20, 118, 16), "Clear Link"))
					{
						Debug.Log("Resetting response " + selectedResponse + " in selected node.");
						createNodeLink(respNode, selectedResponse, -1);
					}
					EditorGUI.EndDisabledGroup();

					if (GUI.Button(new Rect(2,38,88,16), "Link to ID"))
					{
						if(createNodeLink(respNode, selectedResponse, responseTargetNodeId))
							Debug.Log("Connecting selected node's response " + selectedResponse + " to node " + responseTargetNodeId);
					}
					responseTargetNodeId = EditorGUI.DelayedIntField(new Rect(91,38,29,16), responseTargetNodeId);

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

			const float nodeHeight = 70;
			Rect nRect = node.rect;
			nRect.position -= offset;

			// Draw incoming link endpoint:
			EditorGUI.DrawRect(new Rect(nRect.x - 4, nRect.y + nodeHeight * 0.5f - 2, 4, 5), Color.black);
			// Draw black outline:
			EditorGUI.DrawRect(new Rect(nRect.x-1, nRect.y-1,130,nodeHeight), Color.black);

			// Draw content rect:
			GUI.BeginGroup(new Rect(nRect));
			EditorGUI.DrawRect(new Rect(0,0,128,68), new Color(0.75f,0.75f,0.75f));
			EditorGUI.DrawRect(new Rect(0,15,128,1), Color.black);
			
			DialogueNode dNode = node.node;

			// HEADER:

			string nodeTitleTxt = nodeIndex + ") ";
			if (isSelected)
				dNode.name = (EditorGUI.DelayedTextField(new Rect(13, 0, 105, 16), dNode.name));
			else
				nodeTitleTxt += dNode.name;

			EditorGUI.LabelField(new Rect(0,0,106,16), nodeTitleTxt);

			if(GUI.Button(new Rect(119,1,8,8), ""))
			{
				toggleDragNDrop();
				actions.selected = true;
			}

			// EDITING:

			string contTxt = dNode.content != null && dNode.content.Length != 0 && !string.IsNullOrEmpty(dNode.content[0].text) ?
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
				addNodeResponse(dNode);
			}

			GUI.EndGroup();

			if(dNode.responses != null)
			{
				int respCount = dNode.responses.Length;
				float respSize = respCount * 17.0f;
				Rect respRect = new Rect(nRect.x+129, nRect.y+34-respSize*.5f, 100, respSize);

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
			p0 -= offset;
			Vector2 p1 = target.rect.position + Vector2.up * target.rect.height * 0.5f - offset;

			float tanLength = 32 * Mathf.Max(Vector2.Distance(p0, p1) / 74.0f, 1.0f);
			Vector2 t0 = new Vector2(tanLength, 0);
			Vector2 t1 = new Vector2(-tanLength, 0);
			if (p0.x > p1.x)
			{
				float vertTanLength = tanLength * 0.45f;
				t0.y += vertTanLength;
				t1.y += vertTanLength;
			}

			Color c0 = new Color(0.0f, 0.9f, 1.0f);
			Color c1 = new Color(0.0f, 0.9f, 1.0f, 0.5f);
			Handles.DrawBezier(p0, p1, p0 + t0, p1 + t1, c0, null, 1);
			Handles.DrawBezier(p0, p1, p0 + t0, p1 + t1, c1, null, 2);
		}

		private Vector2 getResponsePosition(Node node, int responseIndex)
		{
			Rect nRect = node.rect;
			//nRect.position -= offset;

			if(node.node == null || node.node.responses == null) return nRect.center;

			int respCount = node.node.responses.Length;
			float respSize = (respCount + 1) * 17.0f;
			float basePosY = nRect.center.y - 0.5f * respSize;

			float x = nRect.x + nRect.width+16;
			float y = basePosY + responseIndex * 17.0f+15;

			return new Vector2(x, y);
		}

		#endregion
		#region Methods Node Helpers

		public static void createNodeList(Dialogue dialogue, ref List<Node> nodes)
		{
			// Initialize list if it hasn't been done already:
			if (nodes == null) nodes = new List<DialogueNodeEditor.Node>();
			nodes.Clear();

			string assetPath = AssetDatabase.GetAssetPath(dialogue);
			object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
			foreach (object asset in allAssets)
			{
				DialogueNode dNode = asset as DialogueNode;
				if (dNode == null) continue;
				int newRootId = -1;
				for (int i = 0; i < dialogue.rootNodes.Length; ++i)
					if (dNode == dialogue.rootNodes[i].node) newRootId = i;
				DialogueNodeEditor.Node newNode = new DialogueNodeEditor.Node()
				{ node = dNode, rootId = newRootId, rect = new Rect(100, 100, 128, 64) };
				nodes.Add(newNode);
			}
		}
		public static bool createNewNode(Dialogue dialogue, ref List<Node> nodes)
		{
			// Make sure the dialogue asset is non-null:
			if (dialogue == null) return false;
			// Update node list: (initialize list and load existing nodes from dialogue asset)
			if (nodes == null || nodes.Count == 0) createNodeList(dialogue, ref nodes);

			// Create a new node asset in dialogue:
			DialogueNode newDNode = DialogueEditorHelper.createNewNode(dialogue);
			// Create an editor node representation:
			DialogueNodeEditor.Node newNode = DialogueNodeEditor.Node.Blank;
			newNode.node = newDNode;

			// Set newly created node as root node if no root has been assigned yet:
			if (dialogue.rootNodes == null || dialogue.rootNodes.Length == 0)
			{
				// Create new root node in dialogue with no conditions:
				DialogueConditions newRootConds = DialogueConditions.None;
				DialogueRoot newRoot = new DialogueRoot() { node = newDNode, conditions = newRootConds };
				dialogue.rootNodes = new DialogueRoot[1] { newRoot };
				newNode.rootId = 0;

				// Save changes to asset:
				DialogueEditorHelper.saveDialogueAsset(dialogue);
			}

			// Add the new node representation to nodes list:
			nodes.Add(newNode);
			return true;
		}

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

		public static bool addNodeResponse(DialogueNode node)
		{
			if (node == null) return false;

			DialogueResponse[] oldResponses = node.responses;
			bool oldWasEmpty = oldResponses == null || oldResponses.Length == 0;
			int oldResponseCount = oldWasEmpty ? 0 : oldResponses.Length;

			DialogueResponse[] newResponses = new DialogueResponse[oldWasEmpty ? 1 : oldResponseCount + 1];
			if (!oldWasEmpty)
			{
				for (int i = 0; i < oldResponseCount; ++i)
					newResponses[i] = oldResponses[i];
			}

			DialogueResponse newResponse = DialogueResponse.Blank;
			newResponse.responseText = "[...]";
			newResponses[oldResponseCount] = newResponse;
			node.responses = newResponses;

			EditorUtility.SetDirty(node);
			return true;
		}

		private bool createNodeLink(Node source, int responseIndex, int targetNodeIndex)
		{
			DialogueNode dNode = source.node;
			if (dNode == null) return false;
			if (dNode.responses == null || responseIndex < 0 || responseIndex >= dNode.responses.Length) return false;

			DialogueNode targetNode = (targetNodeIndex < 0 || targetNodeIndex >= nodes.Count) ? null : nodes[targetNodeIndex].node;

			dNode.responses[responseIndex].nextNode = targetNode;

			EditorUtility.SetDirty(dNode);
			if(targetNode != null) EditorUtility.SetDirty(targetNode);
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
			List<List<Node>> depthCounts = new List<List<Node>>();
			while(currentNodeIndices.Count != 0 && currentDepth < 256)
			{
				// NOTE: 256 was set as an arbitrary limit to prevent crashes from loops in dialogue structure.
				List<Node> depthList = new List<Node>();
				depthCounts.Add(depthList);

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
					// Skip nodes that have already been sorted (for whatever reason):
					if (depths[currentIndex] >= 0)
					{
						continue;
					}

					depths[currentIndex] = currentDepth;
					depthList.Add(node);

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
			depthCounts.Add(new List<Node>());
			foreach(Node n in nodes)
			{
				if(n.node == null) continue;
				int nIndex;
				if(!nodeIndices.TryGetValue(n.node, out nIndex)) continue;

				int nDepth = depths[nIndex];
				if(nDepth < 0)
				{
					depths[nIndex] = maxDepth;
					depthCounts[maxDepth].Add(n);
				}
			}

			// Sort nodes vertically, to avoid overly spaghetti-like links:
			float[] weights = new float[nodes.Count];
			for (int i = 0; i < weights.Length; ++i)
				weights[i] = 0.0f;
			for(int i = 0; i < depthCounts.Count - 1; ++i)
			{
				List<Node> depthNodes = depthCounts[i];
				int[] depthNodeIndices = new int[depthNodes.Count];

				for(int j = 0; j < depthNodes.Count; ++j)
				{
					Node node = depthNodes[j];
					if (node.node == null) continue;

					int sourceIndex;
					nodeIndices.TryGetValue(node.node, out sourceIndex);
					depthNodeIndices[j] = sourceIndex;
					float sourceWeight = weights[sourceIndex];

					for (int k = 0; k < node.node.responses.Length; ++k)
					{
						// Calculate weighting based on precursing nodes' weight and the responses' indices:
						float weight = sourceWeight * 1.317f + k;	// note: odd multiplier value makes identical weightings less likely.

						DialogueResponse resp = node.node.responses[k];
						if (resp.nextNode == null) continue;

						int targetIndex;
						if (!nodeIndices.TryGetValue(resp.nextNode, out targetIndex)) continue;
						weights[targetIndex] += weight;
					}
				}

				// Sort nodes on a same depth based on their cumulative weighting:
				float lowestWeight = 1.0e+8f;
				for (int j = 0; j < depthNodes.Count; ++j)
				{
					for (int k = j; k < depthNodes.Count; ++k)
					{
						int sourceIndex = depthNodeIndices[k];
						float kWeight = weights[sourceIndex];
						if (kWeight < lowestWeight)
						{
							Node jNode = depthNodes[j];
							depthNodes[j] = depthNodes[k];
							depthNodes[k] = jNode;

							lowestWeight = kWeight;
						}
					}
				}
			}

			// Calculate screen positions for all nodes:
			//int[] depthCountsDone = new int[depthCounts.Count];
			for (int i = 0; i < nodes.Count; ++i)
			{
				const float nodeOffsetX = 32;
				const float nodeOffsetY0 = 10;
				const float nodeHeight0 = 70;
				float nodeOffsetY = nodeOffsetY0 + (Screen.height - nodeHeight0) * 0.5f;

				const float nodeWidth = 130 + 64 + 40;
				const float nodeHeight = nodeHeight0 + 18;

				Node node = nodes[i];
				int nIndex;
				if (!nodeIndices.TryGetValue(node.node, out nIndex)) continue;

				int nDepth = depths[nIndex];
				List<Node> depthNodes = depthCounts[nDepth];
				int nDepthIndex = depthNodes.IndexOf(node);

				float countOffset = -0.5f * (depthNodes.Count - 1) + nDepthIndex;
				float posX = nodeOffsetX + nDepth * nodeWidth;
				float posY = nodeOffsetY + countOffset * nodeHeight;

				node.rect = new Rect(posX, posY, 130, 70);
				nodes[i] = node;

				//depthCountsDone[nDepth]++;
			}
		}

		#endregion
	}
}
