using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop.BasicDescriptors;
using System.Text.RegularExpressions;
using System.Linq;

public class UIDebugConsole : Dialog
{
    #region Members

    public KeyCode activationKey = KeyCode.Return;
    public KeyCode clearKey = KeyCode.F9;
    public KeyCode repeatKey = KeyCode.Period;
    
    private ScriptEnvironment environment;
    private List<string> history;
    private int m_HistoryIdx = -1;

    #endregion

    #region Dialog Implementation

    protected override void OnOpen()
    {
        environment = new ScriptEnvironment();
        history = new List<string>();
        SetActive(false);
        ClearPredictions();
    }

    public override bool OnKeyDown(KeyCode keyCode)
    {
        //cancel
        if (keyCode == KeyCode.Escape)
        {
            SetActive(false);
            return true;
        }

        //activate console or execute statement
        var input = FindRecursive("InputField");
        var inputField = input.GetComponent<InputField>();
        if (keyCode == KeyCode.Return)
        {
            if (isActive)
            {
                Execute(inputField.text);
                ClearPredictions();
                SetActive(false);
            }
            else
            {
                SetActive(true);
                ClearPredictions();
                SetCode("");
            }
            return true;
        }

        //clear console log
        if (keyCode == clearKey)
        {
            var log = FindRecursive<Text>("Log");
            if (log != null) { log.text = ""; }
            return true;
        }

        //prediction
        if (input != null && input.gameObject.activeSelf)
        {
            if (keyCode == KeyCode.Tab)
            {
                var predictions = GeneratePredictions(inputField.text);
                if (predictions.Count == 0)
                {
                    ClearPredictions();
                }
                else if (predictions.Count == 1)
                {
                    var parts = inputField.text.Split('.');
                    parts[parts.Length - 1] = predictions[0];
                    SetCode(string.Join(".", parts));
                    ClearPredictions();
                }
                else
                {
                    FillPredictions(predictions);
                }
                return true;
            }

            if (keyCode == KeyCode.UpArrow)
            {
                SetCode(HistoryBack());
                return true;
            }

            if (keyCode == KeyCode.DownArrow)
            {
                SetCode(HistoryForward());
                return true;
            }
        }

        //repetition
        if (keyCode == repeatKey && history.Count > 0)
        {
            Execute(history[history.Count - 1]);
            return true;
        }

        return false;
    }

    #endregion

    #region Console

    public bool isActive { get { return FindRecursive("InputField").gameObject.activeSelf; } }

    public void SetCode(string code)
    {
        var input = FindRecursive<InputField>("InputField");
        input.ActivateInputField();
        input.text = code;
        input.caretPosition = code.Length;
    }

    public void SetActive(bool active)
    {
        var input = FindRecursive<InputField>("InputField");
        input.gameObject.SetActive(active);
        ClearPredictions();

        if (active)
        {
            input.ActivateInputField();
        }
    }

    public string HistoryBack()
    {
        if (history.Count == 0) { return ""; }
        if (m_HistoryIdx < history.Count - 1) { ++m_HistoryIdx; }
        return history[history.Count - m_HistoryIdx - 1];
    }

    public string HistoryForward()
    {
        if (history.Count == 0) { return ""; }
        if (m_HistoryIdx > 0) { --m_HistoryIdx; }
        return history[history.Count - m_HistoryIdx - 1];
    }

    public void ExecuteInput(string _)
    {
        var input = FindRecursive<InputField>("InputField");
        Execute(input.text);
        input.text = "";
    }
    
    public void Log(string message)
    {
        var log = FindRecursive<Text>("Log");
        if (log == null) { return; }
        
        log.text += message + "\n";
    }

    #endregion

    #region Execution

    public void Execute(string code)
    {
        //no code to execute?
        if (string.IsNullOrEmpty(code))
        {
            return;
        }

        //deal with history
        if (history.Count == 0 || history[history.Count - 1] != code)
        {
            history.Add(code);
        }
        m_HistoryIdx = -1;

        //execute the code
        var loadedCode = LoadCode(code);
        if (loadedCode.IsNil())
        {
            MagicLog.LogErrorFormat("Couldn't understand `{0}`", code);
            return;
        }
        var result = loadedCode.Function.Call();

        //format the result
        string resultText;
        if (result.IsNil())
        {
            resultText = "> " + code;
        }
        else
        {
            var resultStr = FormatValue(result);
            resultText = "> " + code + "\n" + resultStr;
        }

        //print the result
        var log = FindRecursive<Text>("Log");
        if (log != null)
        {
            Log(resultText);
        }
        else
        {
            MagicLog.LogFormat("[Script] {0}", resultText);
        }
    }

    private DynValue LoadCode(string code)
    {
        var formats = new KeyValuePair<Regex, string>[]
        {
            new KeyValuePair<Regex, string>(new Regex(@"(.*)"), "return {0}" ),
            new KeyValuePair<Regex, string>(new Regex(@"(.*)"), "{0}" ),
        };

        foreach (var format in formats)
        {
            var match = format.Key.Match(code);
            if (match.Success)
            {
                //copy captures
                var captures = new string[match.Captures.Count];
                for (int i = 0; i < match.Captures.Count; ++i)
                {
                    captures[i] = match.Captures[i].Value;
                }

                //try loading the code
                var formattedCode = string.Format(format.Value, captures);
                try
                {
                    var loaded = environment.L.LoadString(formattedCode, null, "ConsoleExecute");
                    return loaded;
                }
                catch (InterpreterException)
                {
                    continue;
                }
            }
        }

        return DynValue.Nil;
    }

    public string FormatValue(DynValue value)
    {
        string str;
        if (value.Type == DataType.Table)
        {
            str = "{ ";
            foreach (var entry in value.Table.Pairs)
            {
                str += string.Format("{0}={1} ", entry.Key.ToPrintString(), entry.Value.ToPrintString());
            }
            str += "}";
        }
        else
        {
            str = value.ToPrintString();
        }

        return str;
    }

    #endregion

    #region Predictions

    public void ClearPredictions()
    {
        var predictionsCtrl = FindRecursive<ScrollRect>("Predictions").content;
        if (predictionsCtrl == null)
        {
            return;
        }

        for (int i = 0; i < predictionsCtrl.childCount; ++i)
        {
            Gameplay.Destroy(predictionsCtrl.GetChild(i).gameObject);
        }

        predictionsCtrl.gameObject.SetActive(false);
    }

    public void FillPredictions(List<string> predictions)
    {
        var predictionsCtrl = FindRecursive<ScrollRect>("Predictions").content;
        if (predictionsCtrl == null) { return; }

        predictionsCtrl.gameObject.SetActive(true);
        for (int i = 0; i < predictionsCtrl.childCount; ++i)
        {
            Gameplay.Destroy(predictionsCtrl.GetChild(i).gameObject);
        }

        var dy = 18.0f;
        var height = 16.0f;
        var spacing = 2.0f;
        var width = 200.0f;
        var font = Font.CreateDynamicFontFromOSFont("Aerial", 14);
        predictionsCtrl.sizeDelta = new Vector2(0.0f, (height + spacing) * predictions.Count);
        foreach (var prediction in predictions)
        {
            //resolve predicted line
            var input = FindRecursive<InputField>("InputField");
            var parts = input.text.Split('.');
            parts[parts.Length - 1] = prediction;
            var predictedLine = string.Join(".", parts);

            //create main element
            var predictionElement = new GameObject("ConsolePrediction:" + prediction);
            predictionElement.transform.SetParent(predictionsCtrl);

            //create button
            var btn = predictionElement.AddComponent<Button>();
            btn.onClick.AddListener(new UnityEngine.Events.UnityAction(() => { SetCode(predictedLine); ClearPredictions(); }));
            var img = predictionElement.AddComponent<Image>();
            img.enabled = false;
            btn.targetGraphic = img;
            //create button text
            var txt = new GameObject("ConsolePredictionText:" + prediction);
            var uiText = txt.AddComponent<Text>();
            uiText.text = prediction;
            uiText.color = Color.black;
            uiText.font = font;
            txt.transform.SetParent(btn.transform);
            var txtTransform = txt.transform as RectTransform;
            txtTransform.sizeDelta = new Vector2(width, height);
            txtTransform.anchorMin = new Vector2(0.0f, 0.0f);
            txtTransform.anchorMax = new Vector2(0.0f, 0.0f);
            txtTransform.pivot = new Vector2(0.0f, 0.0f);
            //txtTransform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);

            //align element properly
            var rectTransform = predictionElement.transform as RectTransform;
            rectTransform.pivot = new Vector2(0.0f, 0.0f);
            rectTransform.localPosition = new Vector3(10.0f, -dy, 0.0f);
            rectTransform.sizeDelta = new Vector2(width, height);

            //Fix text alignment
            txtTransform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);

            dy += height + spacing;
        }
    }

    private enum PredictionResolution
    {
        None,
        Value,
        Keys,
    }

    private PredictionResolution TryResolve(DynValue value, string key, bool canListKeys, out DynValue next, out List<string> keys)
    {
        if (value != null)
        {
            var nextValue = value.Type == DataType.Table ? value.Table.RawGet(key) : null;
            if (nextValue == null)
            {
                if (canListKeys)
                {
                    var predictions = new List<string>();
                    if (value.Type == DataType.Table)
                    {
                        foreach (var k in value.Table.Keys)
                        {
                            if (k.Type == DataType.String && k.String.StartsWith(key))
                            {
                                predictions.Add(k.String);
                            }
                        }
                    }
                    else if (value.Type == DataType.UserData)
                    {
                        var descriptor = UserData.GetDescriptorForObject(value.UserData.Object) as DispatchingUserDataDescriptor;
                        if (descriptor != null)
                        {
                            var members = descriptor.MemberNames;
                            foreach (var k in members)
                            {
                                if (k.StartsWith(key))
                                {
                                    predictions.Add(k);
                                }
                            }
                        }
                    }

                    if (predictions.Count > 0)
                    {
                        next = null;
                        keys = predictions;
                        return PredictionResolution.Keys;
                    }
                }
            }
            else
            {
                if (nextValue.Type == DataType.Table || nextValue.Type == DataType.UserData)
                {
                    next = nextValue;
                    keys = null;
                    return PredictionResolution.Value;
                }
            }
        }

        next = null;
        keys = null;
        return PredictionResolution.None;
    }

    public List<string> GeneratePredictions(string code)
    {
        var result = new HashSet<string>();

        var partsLastPeriod = code.LastIndexOf('.');
        var basePart = partsLastPeriod != -1 ? code.Substring(0, partsLastPeriod) : "";
        var lastPart = partsLastPeriod != -1 ? code.Substring(partsLastPeriod + 1) : code;

        var baseValues = new List<DynValue>();
        try { baseValues.Add(environment.L.DoString("return " + basePart)); } catch (InterpreterException) { }
        try { baseValues.Add(environment.L.DoString("return _G." + basePart)); } catch (InterpreterException) { }

        for (int i = 0; i < baseValues.Count; ++i)
        {
            var baseValue = baseValues[i];
            DynValue nextValue;
            List<string> predictedKeys;
            var resolution = TryResolve(baseValue, lastPart, true, out nextValue, out predictedKeys);
            if (resolution == PredictionResolution.Value)
            {
                baseValues.Add(nextValue);
                if (nextValue.Type == DataType.Table && nextValue.Table.MetaTable != null)
                {
                    var __index = nextValue.Table.MetaTable.RawGet("__index");
                    if (__index != null && !__index.IsNil())
                    {
                        baseValues.Add(DynValue.FromObject(environment.L, __index));
                    }
                }
            }
            else if (resolution == PredictionResolution.Keys)
            {
                foreach (var prediction in predictedKeys)
                {
                    result.Add(prediction);
                }
            }
        }

        var list = result.ToList();
        list.Sort();
        return list;
    }

    #endregion
}
