using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashScreenManager : MonoBehaviour
{
	[HideInInspector]
	public string TutorialTargetScene;
	[HideInInspector]
	public string SimulationTargetScene;

#if UNITY_EDITOR

	public SceneAsset TutorialSequence;
	public SceneAsset SimulationSequence;

	private void OnValidate()
	{
		TutorialTargetScene = "";
		if (TutorialSequence != null)
			TutorialTargetScene = TutorialSequence.name;

		SimulationTargetScene = "";
		if (SimulationSequence != null)
			SimulationTargetScene = SimulationSequence.name;
	}

#endif

	public void EnterTutorial()
	{
		SceneManager.LoadSceneAsync(TutorialTargetScene, LoadSceneMode.Single);
	}

	public void EnterSimulation()
	{
		SceneManager.LoadSceneAsync(SimulationTargetScene, LoadSceneMode.Single);
	}
}
