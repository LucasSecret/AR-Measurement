using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class AlertBox
{
    private static CoroutineExecuter _coroutineExecuter;
    private static GameObject _alertBoxObject;


    public static void Alert(string message)
    {

        Debug.Log(message);
        if (!_coroutineExecuter)
        {
            _coroutineExecuter = GameObject.FindObjectOfType<CoroutineExecuter>();

            if (!_coroutineExecuter)
            {
                _coroutineExecuter = new GameObject("CoroutineExecuter").AddComponent<CoroutineExecuter>();
            }
        }


        if (!_alertBoxObject)
        {
            _alertBoxObject = GameObject.Find("AlertBox");
            if (!_alertBoxObject)
                return;
        }

        if (_alertBoxObject.activeSelf)
            _alertBoxObject.transform.GetComponentInChildren<Text>().text += "\n" + message;

        else
        {
            _alertBoxObject.SetActive(true);
            _coroutineExecuter.StartCoroutine(CloseAfter(3));
        }
    }

    public static IEnumerator CloseAfter(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        _alertBoxObject.SetActive(false);
    }
}
