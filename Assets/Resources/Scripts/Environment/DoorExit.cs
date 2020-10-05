using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;


public class DoorExit : MonoBehaviour
{
    public string SceneToLoad;

    public void LoadScene() {
        DOTween.Clear();
        SceneManager.LoadScene(SceneToLoad);
    }
}
