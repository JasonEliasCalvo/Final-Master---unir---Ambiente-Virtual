using Convai.Scripts.Runtime.Core;
using Convai.Scripts.Runtime.Features;
using System.Collections;
using UnityEngine;

public class TourGuideController : MonoBehaviour
{
    [SerializeField] private ConvaiActionsHandler _actionsHandler;
    [SerializeField] private ConvaiNPC convaiNPC;
    [SerializeField] private Transform player;

    public void Start()
    {
        ConvaiNPCManager.Instance.SetActiveConvaiNPC(convaiNPC);
        FollowPlayer(true);
    }

    public void MoveTo(GameObject @object)
    {
        FollowPlayer(false);
        StartCoroutine(_actionsHandler.MoveTo(@object));
    }

    public void FollowPlayer(bool state)
    {
        if (state)
        {
            if (player == null) player = Camera.main.transform;
            _actionsHandler.StartFollowing();
            StartCoroutine(_actionsHandler.Follow(player, this));
        }
        else
        {
            _actionsHandler.StopFollowing();
            StopAllCoroutines();
        }
    }

    public void TriggerNPCEvent(string trigger)
    {
        return;
        if (convaiNPC == null) return;

        ConvaiNPCManager.Instance.SetActiveConvaiNPC(convaiNPC);

        //Solo interrumpe si está hablando
        if (convaiNPC.isCharacterTalking)
        {
            // Interrumpe el audio
            FindAnyObjectByType<ConvaiGRPCWebAPI>()?.InterruptCharacterSpeechInternal();
            convaiNPC.InterruptCharacterSpeech();
 
            // Espera un frame para asegurar que la interrupción se procese
            StartCoroutine(TriggerAfterFrame(trigger));
        }
        else
        {
            convaiNPC.TriggerEvent(trigger);
        }
    }

    private IEnumerator TriggerAfterFrame(string trigger)
    {
        yield return null;
        yield return new WaitForSeconds(1);
        convaiNPC.TriggerEvent(trigger);
    }

    public void SendContext(string context)
    {
        return;

        if (convaiNPC == null) return;
        if (convaiNPC.isCharacterTalking)
        {
            FindAnyObjectByType<ConvaiGRPCWebAPI>()?.InterruptCharacterSpeechInternal();
            convaiNPC.InterruptCharacterSpeech();

            StartCoroutine(contextAfterFrame(context));
        }

        FindAnyObjectByType<ConvaiGRPCWebAPI>()?.RequestSendTextData(context);
    }

    private IEnumerator contextAfterFrame(string context)
    {
        yield return new WaitForSeconds(1);
        FindAnyObjectByType<ConvaiGRPCWebAPI>()?.RequestSendTextData(context);
    }

    public void SendMessage(string message)
    {
        if (convaiNPC == null) return;
        FindAnyObjectByType<ConvaiGRPCWebAPI>()?.OnUserResponseReceived(message);
    }
}
