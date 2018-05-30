using UnityEngine;
#if DEBUG
using UnityEditor;
#endif

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
		if (Gameplay.GetKeyDown(KeyCode.KeypadMultiply))
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

		if (Gameplay.GetKeyDown(KeyCode.KeypadPlus))
		{
			timeScale = Mathf.Min(15.0f, timeScale + 0.5f);
		}

		if (Gameplay.GetKeyDown(KeyCode.KeypadMinus))
		{
			timeScale = Mathf.Max(0.1f, Time.timeScale - 0.5f);
		}

#if DEBUG
        if (Gameplay.GetKeyDown(KeyCode.Pause) && Application.isEditor)
        {
            EditorApplication.isPaused = true;
        }
#endif
    }
}
