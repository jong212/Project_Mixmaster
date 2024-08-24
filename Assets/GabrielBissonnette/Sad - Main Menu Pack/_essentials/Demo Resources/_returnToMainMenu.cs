using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GabrielBissonnette.SAD
{
    public class _returnToMainMenu : MonoBehaviour
    {
        public void LoadMainMenu()
        {
            SceneManager.LoadScene("Demo");
        }
    }
}
