using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderInputReciever : InputReciever 
{
    private Vector3 clickPosition;
    private bool continueAudio = false;

    public AudioInputHandler audioInputHandler;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            audioInputHandler.stopInput = true;
            
            Debug.Log("Stop input is true");
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                clickPosition = hit.point;
                OnInputRecieved();
            }
        }
        else if(continueAudio)
        {
            audioInputHandler.startInput = true;
            Debug.Log("Start input is true");
            continueAudio = false;
        }
    }
    public override void OnInputRecieved()
    {
        foreach (var handler in inputHandlers)
        {
            handler.ProcessInput(clickPosition, null, null);
        }
        continueAudio = true;
    }
}
