using Platformer.Mechanics;
using Platformer.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Platformer.UI
{
    public class MetaGameController : MonoBehaviour
    {
      
        public MainUIController mainMenu;

       
        public Canvas[] gamePlayCanvasii;

        
        public GameController gameController;

        bool showMainCanvas = false;
        private InputAction m_MenuAction;

        void OnEnable()
        {
            _ToggleMainMenu(showMainCanvas);
            m_MenuAction = InputSystem.actions.FindAction("Player/Menu");
        }

        public void ToggleMainMenu(bool show)
        {
            if (this.showMainCanvas != show)
            {
                _ToggleMainMenu(show);
            }
        }

        void _ToggleMainMenu(bool show)
        {
            if (show)
            {
                Time.timeScale = 0;
                mainMenu.gameObject.SetActive(true);
                foreach (var i in gamePlayCanvasii) i.gameObject.SetActive(false);
            }
            else
            {
                Time.timeScale = 1;
                mainMenu.gameObject.SetActive(false);
                foreach (var i in gamePlayCanvasii) i.gameObject.SetActive(true);
            }
            this.showMainCanvas = show;
        }

        void Update()
        {
            if (m_MenuAction.WasPressedThisFrame())
            {
                ToggleMainMenu(show: !showMainCanvas);
            }
        }

    }
}
