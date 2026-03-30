using UnityEngine;
using UnityEngine.SceneManagement; // 必须引入场景管理命名空间

public class MainMenuController : MonoBehaviour
{
    // 这个方法将绑定到Start按钮上
    public void PlayGame()
    {
        //SceneManager.LoadScene("SampleScene"); 
        // 先进入开场过渡场景，再由 OpeningPrologueController 加载教学关 SampleScene
        SceneManager.LoadScene("OpeningPrologue");
    }
}