using Convai.Scripts.Runtime.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneReactor : MonoBehaviour
{
    private TourGuideController guideController;

    void Start()
    {
        guideController = FindObjectOfType<TourGuideController>();
    }

    public void TriggerCharacterEvent(string trigger)
    {
       guideController.TriggerNPCEvent(trigger);
    }

    public void SendContextToNPC(string context)
    {
        guideController.SendContext(context);
    }

    public void SendMessageToNPC(string message)
    {
        guideController.SendMessage(message);
    }

    public void AskNPCAboutScene(string context)
    {
        guideController.SendContext(context + "Responde a la siguiente pregunta de manera simple y corta");
        guideController.SendMessage("¿Que es lo que estoy mirando?");
    }
}
