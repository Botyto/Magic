﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using UnityEngine.EventSystems;

[RequireComponent(typeof(InputField))]
[RequireComponent(typeof(Image))]
public class ScriptConsole : MonoBehaviour
{
    public KeyCode activationKey = KeyCode.Return;
    public KeyCode clearKey = KeyCode.F9;
    public KeyCode repeatKey = KeyCode.Period;
    [HideInInspector]
    [SerializeField]
    public List<string> history = new List<string>();
    [SerializeField]
    public ScriptEnvironment environment;

    private InputField m_Input;
    private Image m_Image;
    private int m_HistoryIdx = -1;
    private RectTransform m_PredictionsControl;
    private RectTransform m_PredictionsContainer;
    private Text m_Log;

    public bool isActive { get { return m_Image.enabled; } }

    public void Start()
    {
        environment = new ScriptEnvironment();

        m_Input = GetComponent<InputField>();
        m_Image = GetComponent<Image>();
        m_Input.onEndEdit.AddListener(ExecuteInput);
        m_PredictionsControl = transform.Find("Predictions") as RectTransform;
        if (m_PredictionsControl != null)
        {
            m_PredictionsContainer = m_PredictionsControl.Find("Viewport").Find("Content") as RectTransform;
        }
        if (transform.parent != null)
        {
            var logObject = transform.parent.Find("Log");
            if (logObject != null)
            {
                m_Log = logObject.GetComponent<Text>();
            }
        }

        SetActive(false);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && isActive)
        {
            SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            if (isActive)
            {
                Execute(m_Input.text);
                ClearPredictions();
                SetActive(false);
            }
            else
            {
                SetActive(true);
                ClearPredictions();
                SetCode("");
            }
        }
        else if (Input.GetKeyDown(clearKey))
        {
            if (m_Log != null) { m_Log.text = ""; }
        }
        else if (m_Input.enabled)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                var predictions = GeneratePredictions(m_Input.text);
                if (predictions.Count == 0)
                {
                    ClearPredictions();
                }
                else if (predictions.Count == 1)
                {
                    var parts = m_Input.text.Split('.');
                    parts[parts.Length - 1] = predictions[0];
                    SetCode(string.Join(".", parts));
                    ClearPredictions();
                }
                else
                {
                    FillPredictions(predictions);
                }
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                SetCode(HistoryBack());
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                SetCode(HistoryForward());
            }
        }
        else //if m_Input.enabled is false
        {
            if (Input.GetKeyDown(repeatKey))
            {
                if (history.Count > 0)
                {
                    Execute(history[history.Count - 1]);
                }
            }
        }
    }

    public void SetCode(string code)
    {
        m_Input.ActivateInputField();
        m_Input.text = code;
        m_Input.caretPosition = code.Length;
    }

    public void SetActive(bool active)
    {
        m_Image.enabled = active;
        m_Input.enabled = active;
        for (int i = 0; i < transform.childCount; ++i)
        {
            transform.GetChild(i).gameObject.SetActive(active);
        }

        if (active)
        {
            m_Input.ActivateInputField();
        }
    }

    public void ClearPredictions()
    {
        if (m_PredictionsControl == null)
        {
            return;
        }

        for (int i = 0; i < m_PredictionsContainer.childCount; ++i)
        {
            Util.Destroy(m_PredictionsContainer.GetChild(i).gameObject);
        }
        m_PredictionsControl.gameObject.SetActive(false);
    }

    public void FillPredictions(List<string> predictions)
    {
        if (m_PredictionsControl == null)
        {
            return;
        }

        m_PredictionsControl.gameObject.SetActive(true);
        for (int i = 0; i < m_PredictionsContainer.childCount; ++i)
        {
            Util.Destroy(m_PredictionsContainer.GetChild(i).gameObject);
        }

        var dy = 18.0f;
        var height = 16.0f;
        var spacing = 2.0f;
        var width = 200.0f;
        var font = Font.CreateDynamicFontFromOSFont("Aerial", 14);
        m_PredictionsContainer.sizeDelta = new Vector2(0.0f, (height + spacing) * predictions.Count);
        foreach (var prediction in predictions)
        {
            //create main element
            var predictionElement = new GameObject("ConsolePrediction:" + prediction);
            predictionElement.transform.SetParent(m_PredictionsContainer);
            
            //create button
            var btn = predictionElement.AddComponent<Button>();
            btn.onClick.AddListener(new UnityEngine.Events.UnityAction(() => { SetCode(prediction); ClearPredictions(); }));
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
        Execute(m_Input.text);
        m_Input.text = "";
    }

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
            Debug.LogErrorFormat("Couldn't understand `{0}`", code);
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
        if (m_Log != null)
        {
            m_Log.text += resultText + "\n";
        }
        else
        {
            Debug.LogFormat("[Script] {0}", resultText);
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

    private enum PredictionResolveResult
    {
        None,
        Value,
        Keys,
    }

    private PredictionResolveResult TryResolve(DynValue current, string key, bool canListKeys, out DynValue next, out List<string> keys)
    {
        if (current != null)
        {
            var nextValue = current.Type == DataType.Table ? current.Table.RawGet(key) : null;
            if (nextValue == null)
            {
                if (canListKeys)
                {
                    var predictions = new List<string>();
                    if (current.Type == DataType.Table)
                    {
                        foreach (var k in current.Table.Keys)
                        {
                            if (k.Type == DataType.String && k.String.StartsWith(key))
                            {
                                predictions.Add(k.String);
                            }
                        }
                    }
                    else if (current.Type == DataType.UserData)
                    {
                        var descriptor = UserData.GetDescriptorForObject(current.UserData.Object) as StandardUserDataDescriptor;
                        var members = descriptor.MemberNames;
                        foreach (var k in members)
                        {
                            if (k.StartsWith(key))
                            {
                                predictions.Add(k);
                            }
                        }
                    }
                    
                    if (predictions.Count > 0)
                    {
                        next = null;
                        keys = predictions;
                        return PredictionResolveResult.Keys;
                    }
                }
            }
            else
            {
                if (nextValue.Type == DataType.Table || nextValue.Type == DataType.UserData)
                {
                    next = nextValue;
                    keys = null;
                    return PredictionResolveResult.Value;
                }
            }
        }

        next = null;
        keys = null;
        return PredictionResolveResult.None;
    }

    public List<string> GeneratePredictions(string code)
    {
        var result = new List<string>();

        var partsLastPeriod = code.LastIndexOf('.');
        var basePart = code.Substring(0, partsLastPeriod);
        var lastPart = code.Substring(partsLastPeriod + 1);

        var baseValues = new List<DynValue>();
        try { baseValues.Add(environment.L.DoString("return " + basePart)); } catch (InterpreterException) { }
        try { baseValues.Add(environment.L.DoString("return _G." + basePart)); } catch (InterpreterException) { }

        for (int i = 0; i < baseValues.Count; ++i)
        {
            var baseValue = baseValues[i];
            DynValue nextValue;
            List<string> predictedKeys;
            var resolution = TryResolve(baseValue, lastPart, true, out nextValue, out predictedKeys);
            if (resolution == PredictionResolveResult.Value)
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
            else if (resolution == PredictionResolveResult.Keys)
            {
                result.AddRange(predictedKeys);
            }
        }

        return result;
    }
}
