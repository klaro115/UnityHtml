using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using UBindings;

namespace UDialogue
{
	/// <summary>
	/// Editor helper class for creating and updating dialogue nodes.
	/// </summary>
	public static class DialogueEditorHelper
	{
		#region Methods

		public static Dialogue createNewDialogue(string name, string filePath)
		{
			if(string.IsNullOrEmpty(name) || string.IsNullOrEmpty(filePath))
			{
				Debug.LogError("[DialogueEditorHelper] Error! Please provide a valid asset name and file path!");
				return null;
			}

			// Create new dialogue instance:
			Dialogue instance = ScriptableObject.CreateInstance<Dialogue>();

			// Set some default parameters and values for the new instance:
			instance.name = name;

			instance.characters = new DialogueCharacter[1] { DialogueCharacter.Default };
			instance.startBinding = Binding.Blank;
			instance.endBinding = Binding.Blank;
			instance.rootNodes = new DialogueRoot[1] { DialogueRoot.Blank };

			// Create new main asset and save changes:
			AssetDatabase.CreateAsset(instance, filePath);
			AssetDatabase.SaveAssets();

			return instance;
		}

		public static bool saveDialogueAsset(Dialogue asset)
		{
			if(asset == null)
			{
				Debug.LogError("[DialogueEditorHelper] Error! Unable to save changes of null asset.");
				return false;
			}
			if(!AssetDatabase.IsMainAsset(asset))
			{
				Debug.LogError("[DialogueEditorHelper] Error! Can only save changes from root asset.");
				return false;
			}

			// Raise the dirty flag on the asset, then save all changes:
			EditorUtility.SetDirty(asset);
			AssetDatabase.SaveAssets();

			return true;
		}

		public static DialogueNode createNewNode(Dialogue dialogue)
		{
			if(dialogue == null)
			{
				Debug.LogError("[DialogueEditorHelper] Error! Unable to save changes of null asset.");
				return null;
			}

			// Create a new dialogue node instance:
			DialogueNode instance = ScriptableObject.CreateInstance<DialogueNode>();

			// Set some default parameters and values for the new instance:
			instance.name = "Dialogue Node";

			instance.content = new DialogueContent[1] { DialogueContent.Blank };
			instance.responses = new DialogueResponse[1] { DialogueResponse.Blank };

			// Create subasset for instance and save changes to dialogue asset:
			AssetDatabase.AddObjectToAsset(instance, dialogue);
			saveDialogueAsset(dialogue);

			return instance;
		}

		public static bool destroyNodeAsset(DialogueNode node)
		{
			if(node == null)
			{
				Debug.LogError("[DialogueEditorHelper] Error! Unable to destroy null node!");
				return false;
			}
			if(!AssetDatabase.IsSubAsset(node))
			{
				Debug.LogError("[DialogueEditorHelper] Error! Node is not part of a dialogue asset!");
				return false;
			}

			// Retrieve the asset path leading to the dialogue node subasset, then delete it:
			string assetPath = AssetDatabase.GetAssetPath(node);
			/*
			bool result = AssetDatabase.DeleteAsset(assetPath);
			if(!result)
			{
				Debug.LogError("[DialogueEditorHelper] Error! Failed to delete asset for node!");
			}
			*/

			// Remove the instance as well, then return result:
			Object.DestroyImmediate(node, true);

			// Raise the dirty flag on the asset, then save all changes:
			EditorUtility.SetDirty(AssetDatabase.LoadMainAssetAtPath(assetPath));
			AssetDatabase.SaveAssets();

			return true;//result;
		}

		public static DialogueNode duplicateNode(DialogueNode sourceNode)
		{
			if(sourceNode == null)
			{
				Debug.LogError("[DialogueEditorHelper] Error! Unable to duplicate null node!");
				return null;
			}
			if(!AssetDatabase.IsSubAsset(sourceNode))
			{
				Debug.LogError("[DialogueEditorHelper] Error! Node is not part of a dialogue asset!");
				return null;
			}

			// Fetch dialogue asset the node belongs to:
			string assetPath = AssetDatabase.GetAssetPath(sourceNode);
			Dialogue baseAsset = AssetDatabase.LoadMainAssetAtPath(assetPath) as Dialogue;
			if(baseAsset == null)
			{
				Debug.LogError("[DialogueEditorHelper] Error! Dialogue base asset could not be found!");
				return null;
			}

			// Create a new node in dialogue:
			DialogueNode newNode = createNewNode(baseAsset);
			newNode.name = sourceNode.name + " (clone)";
			newNode.content = null;
			newNode.responses = null;

			// Copy all data and dependencies from source node:
			if(sourceNode.content != null)
			{
				newNode.content = new DialogueContent[sourceNode.content.Length];
				for(int i = 0; i < sourceNode.content.Length; ++i)
					newNode.content[i] = sourceNode.content[i];
			}
			if(sourceNode.responses != null)
			{
				newNode.responses = new DialogueResponse[sourceNode.responses.Length];
				for(int i = 0; i < sourceNode.responses.Length; ++i)
					newNode.responses[i] = sourceNode.responses[i];
			}

			// Save all changes to dialogue asset:
			saveDialogueAsset(baseAsset);

			// Return newly created node asset:
			return newNode;
		}

		#endregion
	}
}
