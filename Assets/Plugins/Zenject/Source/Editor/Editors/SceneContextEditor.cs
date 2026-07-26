#if !ODIN_INSPECTOR

#region

using UnityEditor;

#endregion

namespace Zenject
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SceneContext))]
    [NoReflectionBaking]
    public class SceneContextEditor : RunnableContextEditor
    {
        private SerializedProperty _contractNameProperty;
        private SerializedProperty _parentNamesProperty;
        private SerializedProperty _parentNewObjectsUnderSceneContextProperty;
        private SerializedProperty _kernel;

        public override void OnEnable()
        {
            base.OnEnable();

            _contractNameProperty = serializedObject.FindProperty("_contractNames");
            _parentNamesProperty = serializedObject.FindProperty("_parentContractNames");
            _parentNewObjectsUnderSceneContextProperty =
                serializedObject.FindProperty("_parentNewObjectsUnderSceneContext");

            _kernel = serializedObject.FindProperty("_kernel");
        }

        protected override void OnGui()
        {
            base.OnGui();

            EditorGUILayout.PropertyField(_contractNameProperty, true);
            EditorGUILayout.PropertyField(_parentNamesProperty, true);
            EditorGUILayout.PropertyField(_parentNewObjectsUnderSceneContextProperty);
            EditorGUILayout.PropertyField(_kernel);
        }
    }
}


#endif