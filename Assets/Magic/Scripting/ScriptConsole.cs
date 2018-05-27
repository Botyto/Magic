using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MoonSharp.Interpreter;

[RequireComponent(typeof(InputField))]
public class ScriptConsole : MonoBehaviour
{
    public KeyCode activationKey = KeyCode.Return;
    public KeyCode clearKey = KeyCode.F9;
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
        m_PredictionsContainer = m_PredictionsControl.Find("Viewport").Find("Content") as RectTransform;
        m_Log = transform.parent.Find("Log").GetComponent<Text>();

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
            m_Log.text = "";
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
        for (int i = 0; i < m_PredictionsContainer.childCount; ++i)
        {
            Util.Destroy(m_PredictionsContainer.GetChild(i).gameObject);
        }
        m_PredictionsControl.gameObject.SetActive(false);
    }

    public void FillPredictions(List<string> predictions)
    {
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
        if (string.IsNullOrEmpty(code))
        {
            return;
        }

        if (history.Count == 0 || history[history.Count - 1] != code)
        {
            history.Add(code);
        }
        m_HistoryIdx = -1;

        var result = environment.DoString("return " + code, "ConsoleCode");
        if (result == null)
        {
            return;
        }

        if (result.IsNil())
        {
            m_Log.text += "> " + code + "\n";
        }
        else
        {
            var resultStr = FormatValue(result);
            m_Log.text += "> " + code + "\n" + resultStr + "\n";
        }
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
        Table,
        Keys,
    }

    private PredictionResolveResult TryResolve(Table current, string key, bool canListKeys, out Table next, out List<string> keys)
    {
        if (current != null)
        {
            var nextValue = current.RawGet(key);
            if (nextValue == null && canListKeys)
            {
                var predictions = new List<string>();
                foreach (var k in current.Keys)
                {
                    if (k.Type == DataType.String && k.String.StartsWith(key))
                    {
                        predictions.Add(k.String);
                    }
                }

                if (predictions.Count > 0)
                {
                    next = null;
                    keys = predictions;
                    return PredictionResolveResult.Keys;
                }
            }
            else
            {
                if (nextValue.Type == DataType.Table)
                {
                    next = nextValue.Table;
                    keys = null;
                    return PredictionResolveResult.Table;
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
        
        Table currentTable = environment.L.Globals;
        var parts = code.Split('.');
        for (int i = 0; i < parts.Length; ++i)
        {
            var part = parts[i];
            var isLast = i == parts.Length - 1;
            Table nextTable = null;
            List<string> predictedKeys = null;
            var resolution = TryResolve(currentTable, part, isLast, out nextTable, out predictedKeys);
            if (resolution == PredictionResolveResult.Table)
            {
                currentTable = nextTable;
            }
            else if (resolution == PredictionResolveResult.Keys && isLast)
            {
                result = predictedKeys;
                break;
            }
            else
            {
                break;
            }
        }

        return result;
    }
}
