using System;
using System.Collections.Generic;
using UnityEngine;

namespace Convai.Scripts.Runtime.Features
{
    /// <summary>
    ///     This script defines global actions and settings for Convai.
    /// </summary>
    [AddComponentMenu("Convai/Convai Interactables Data")]
    public class ConvaiInteractablesData : MonoBehaviour
    {
        [Tooltip("Array of Characters in the environment")]
        [SerializeField]
        public Character[] Characters;

        [Space(5)]
        [Tooltip("Array of Objects in the environment")]
        [SerializeField]
        public Object[] Objects;

        [Space(5)]
        [Tooltip("Array of Objects in the environment")]
        [SerializeField]
        public Hall[] Halls;

        public Transform DynamicMoveTargetIndicator;

        /// <summary>
        ///     Represents a character in the environment.
        /// </summary>
        [Serializable]
        public class Character
        {
            [SerializeField] public string Name;
            [SerializeField][TextArea(1, 4)] public string Bio;
            [SerializeField] public GameObject gameObject;
        }

        [Serializable]
        public class Object
        {
            [SerializeField] public string Name;
            [SerializeField][TextArea(1, 4)] public string Description;
            [SerializeField] public GameObject gameObject;
        }

        [Serializable]
        public class Hall
        {
            [SerializeField] public string Name;
            [SerializeField][TextArea(1, 4)] public string Description;
            [SerializeField] public List<Object> Objects = new();
            [SerializeField] public GameObject gameObject;
        }
    }
}