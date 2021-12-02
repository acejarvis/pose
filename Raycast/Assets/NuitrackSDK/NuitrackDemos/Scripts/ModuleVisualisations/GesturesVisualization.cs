using UnityEngine;
using System;
using nuitrack;

namespace NuitrackSDK.NuitrackDemos
{
    public class GesturesVisualization : MonoBehaviour
    {
        ExceptionsLogger exceptionsLogger;
        NuitrackModules nuitrackModules;

        private void OnEnable()
        {
            NuitrackManager.onNewGesture += OnNewGesture;
        }

        private void OnNewGesture(Gesture gesture)
        {
            string newEntry =
                "User " + gesture.UserID + ": " +
                Enum.GetName(typeof(nuitrack.GestureType), (int)gesture.Type);
            exceptionsLogger.AddEntry(newEntry);
        }

        private void OnDisable()
        {
            NuitrackManager.onNewGesture -= OnNewGesture;
        }

        void Start()
        {
            exceptionsLogger = FindObjectOfType<ExceptionsLogger>();
            nuitrackModules = FindObjectOfType<NuitrackModules>();
        }
    }
}