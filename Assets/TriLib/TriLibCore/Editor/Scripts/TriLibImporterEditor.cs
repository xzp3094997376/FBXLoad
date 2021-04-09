﻿using System;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using Object = UnityEngine.Object;

namespace TriLibCore.Editor
{
    [CustomEditor(typeof(TriLibScriptedImporter))]
    public class TriLibImporterEditor : ScriptedImporterEditor
    {
        private int _currentTab;

        protected override Type extraDataType => typeof(AssetLoaderOptions);

        protected override void InitializeExtraDataInstance(Object extraData, int targetIndex)
        {
            var scriptedImporter = (TriLibScriptedImporter) target;
            var existingAssetLoaderOptions = scriptedImporter.AssetLoaderOptions;
            EditorUtility.CopySerializedIfDifferent(existingAssetLoaderOptions, extraData);
        }

        protected override void Apply()
        {
            base.Apply();
            var assetLoaderOptions = (AssetLoaderOptions) extraDataTarget;
            var scriptedImporter = (TriLibScriptedImporter) target;
            scriptedImporter.AssetLoaderOptions = assetLoaderOptions;
        }

        public override void OnInspectorGUI()
        {
            AssetLoaderOptionsEditor.ShowInspectorGUI(extraDataSerializedObject, ref _currentTab);
            ApplyRevertGUI();
        }
    }
}
