using System.Collections.Generic;
using UnityEngine;
using MoonSharp.Interpreter;

public class ScriptConsole : MonoBehaviour
{
    public List<string> history = new List<string>();
    public ScriptEnvironment environment;

    private int historyIdx = -1;

	public void Start()
    {
        environment = new ScriptEnvironment();

        var input = GetComponent<UnityEngine.UI.InputField>();
        input.onEndEdit.AddListener(ExecuteInput);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            var input = GetComponent<UnityEngine.UI.InputField>();
            input.text = HistoryBack();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            var input = GetComponent<UnityEngine.UI.InputField>();
            input.text = HistoryForward();
        }
    }

    public string HistoryBack()
    {
        if (history.Count == 0)
        {
            return "";
        }

        if (historyIdx < history.Count - 1)
        {
            ++historyIdx;
        }


        return history[history.Count - historyIdx - 1];
    }

    public string HistoryForward()
    {
        if (history.Count == 0)
        {
            return "";
        }

        if (historyIdx > 0)
        {
            --historyIdx;
        }

        return history[history.Count - historyIdx - 1];
    }

    public void ExecuteInput(string _)
    {
        var input = GetComponent<UnityEngine.UI.InputField>();
        Execute(input.text);
        input.text = "";
    }

    public void Execute(string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            return;
        }

        history.Add(code);
        historyIdx = -1;

        var result = environment.DoString("return " + code, "ConsoleCode");
        if (result == null)
        {
            return;
        }

        if (result.IsNil())
        {
            Debug.Log("Console Input: " + code);
        }
        else
        {
            string resultStr;
            if (result.Type == DataType.Table)
            {
                resultStr = "{ ";
                foreach (var entry in result.Table.Pairs)
                {
                    resultStr += string.Format("{0}={1} ", entry.Key.ToPrintString(), entry.Value.ToPrintString());
                }
                resultStr += "}";
            }
            else
            {
                resultStr = result.ToPrintString();
            }

            Debug.Log("Console Input: " + code + "\nResult: " + resultStr);
        }
    }
}
