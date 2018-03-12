using UnityEngine;
using UnityEditor;

public class SimulationSpeed : MonoBehaviour
{
	public float timeScale
	{
		get { return Time.timeScale; }

		set
        {
			Time.timeScale = value;
			Time.fixedDeltaTime = value * 0.02f;
		}
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.KeypadMultiply))
		{
			if (timeScale > 1.0f)
			{
				timeScale = 1.0f;
			}
			else
			{
				timeScale = 10.0f;
			}
		}

		if (Input.GetKeyDown(KeyCode.KeypadPlus))
		{
			timeScale = Mathf.Min(15.0f, timeScale + 0.5f);
		}

		if (Input.GetKeyDown(KeyCode.KeypadMinus))
		{
			timeScale = Mathf.Max(0.1f, Time.timeScale - 0.5f);
		}

        if (Input.GetKeyDown(KeyCode.Pause) && Application.isEditor)
        {
            EditorApplication.isPaused = true;
        }
	}
}
