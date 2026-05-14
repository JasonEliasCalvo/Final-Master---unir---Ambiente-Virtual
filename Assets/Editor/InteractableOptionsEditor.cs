using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(InteractableOptions))]
public class InteractableOptionsEditor : Editor
{
    private SerializedProperty interactionTypesProp;
    private SerializedProperty movableObjectProp;
    private SerializedProperty onEventProp;
    private SerializedProperty endEventProp;
    private SerializedProperty nameProp;
    private SerializedProperty idProp;

    private void OnEnable()
    {
        if (serializedObject == null) return;

        interactionTypesProp = serializedObject.FindProperty("interactionTypes");
        movableObjectProp = serializedObject.FindProperty("selectecObject");
        onEventProp = serializedObject.FindProperty("onInteract");
        endEventProp = serializedObject.FindProperty("endInteract");
        nameProp = serializedObject.FindProperty("itemName");
        idProp = serializedObject.FindProperty("ID");
    }

    public override void OnInspectorGUI()
    {
        if (serializedObject == null) return;

        serializedObject.Update();

        if (interactionTypesProp == null)
        {
            EditorGUILayout.HelpBox("No se encontró la propiedad 'interactionTypes'.", MessageType.Warning);
            return;
        }

        EditorGUILayout.PropertyField(interactionTypesProp, new GUIContent("Tipos de interacción"));

        var interactionValue = (InteractionType)interactionTypesProp.intValue;

        if (interactionValue.HasFlag(InteractionType.InvokeEvent))
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Eventos", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(onEventProp, new GUIContent("Evento inicial"));
            EditorGUILayout.PropertyField(endEventProp, new GUIContent("Evento final"));
        }

        if (interactionValue.HasFlag(InteractionType.ShowBook))
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Mostrar Libro", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(nameProp, new GUIContent("Nombre del libro"));
        }

        if (interactionValue.HasFlag(InteractionType.StartMoving))
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Movimiento", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(movableObjectProp, new GUIContent("Objeto a mover"));
            EditorGUILayout.PropertyField(idProp, new GUIContent("ID"));
        }

        if (interactionValue.HasFlag(InteractionType.SelectMaterial))
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Seleccionar material", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(nameProp, new GUIContent("Nombre del material"));
            SerializedProperty dontDestroyProp = serializedObject.FindProperty("dontDestroyOnSelect");
            if (dontDestroyProp != null)
                EditorGUILayout.PropertyField(dontDestroyProp, new GUIContent("No destruir al seleccionar"));
        }

        if(interactionValue.HasFlag(InteractionType.ShowDescription))
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Mostrar Descripción", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(movableObjectProp, new GUIContent("Maquina a describir"));
        }

        SerializedProperty justOneInteraction = serializedObject.FindProperty("justOneInteraction");
        if (justOneInteraction != null)
            EditorGUILayout.PropertyField(justOneInteraction, new GUIContent("Solo una interacción"));

        serializedObject.ApplyModifiedProperties();

        if (interactionValue.HasFlag(InteractionType.InvokeEvent))
        {
            EditorGUILayout.HelpBox("La opción 'InvokeEvent' requiere que se asignen eventos para interacción inicial y final.", MessageType.Info);
        }

        if (interactionValue.HasFlag(InteractionType.ShowBook))
        {
            EditorGUILayout.HelpBox("La opción 'ShowBook' requiere un nombre de libro que se abrirá cuando se active la interacción.", MessageType.Info);
        }

        if (interactionValue.HasFlag(InteractionType.StartMoving))
        {
            EditorGUILayout.HelpBox("La opción 'StartMoving' requiere que el objeto tenga un componente de movimiento, como XRSlider o XRKnob, para controlar el movimiento.", MessageType.Info);
        }

        if (interactionValue.HasFlag(InteractionType.SelectMaterial))
        {
            EditorGUILayout.HelpBox("La opción 'SelectMaterial' requiere un nombre de material para seleccionar, y puede que sea necesario un componente para manejar la selección y destrucción del objeto.", MessageType.Info);
        }

        if (interactionValue.HasFlag(InteractionType.ShowDescription))
        {
            EditorGUILayout.HelpBox("La opción 'ShowDescription' mostrará la descripción asociada al objeto, y requiere que el objeto tenga un componente 'MachineDescription'.", MessageType.Info);
        }
    }
}