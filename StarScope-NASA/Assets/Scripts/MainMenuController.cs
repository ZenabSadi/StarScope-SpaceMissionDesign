using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void Play()
    {
        SceneManager.LoadScene("MissionDirectorScene");
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OpenMissionDesign()
    {
        SceneManager.LoadScene("MissionDirectorScene");
    }

    public void PreviousStage()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        string previousScene;

        if (currentScene == "MissionDirectorScene")
            previousScene = "MainScene";
        else if (currentScene == "InstrumentsScene")
            previousScene = "MissionDirectorScene";
        else if (currentScene == "RobotsScene")
            previousScene = "InstrumentsScene";
        else if (currentScene == "SpacecraftScene")
            previousScene = "RobotsScene";
        else if (currentScene == "TrajectorySelectionScene")
            previousScene = "SpacecraftScene";
        else if (currentScene == "LaunchScene")
            previousScene = "TrajectorySelectionScene";
        else
        {
            Debug.LogError($"No previous mission stage is defined for scene '{currentScene}'.");
            return;
        }

        SceneManager.LoadScene(previousScene);
    }

    public void NextStage()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        string nextScene;

        if (currentScene == "MissionDirectorScene")
            nextScene = "InstrumentsScene";
        else if (currentScene == "InstrumentsScene")
            nextScene = "RobotsScene";
        else if (currentScene == "RobotsScene")
            nextScene = "SpacecraftScene";
        else if (currentScene == "SpacecraftScene")
            nextScene = "TrajectorySelectionScene";
        else if (currentScene == "TrajectorySelectionScene")
            nextScene = "LaunchScene";
        else
        {
            Debug.LogError($"No next mission stage is defined for scene '{currentScene}'.");
            return;
        }

        SceneManager.LoadScene(nextScene);
    }
}
