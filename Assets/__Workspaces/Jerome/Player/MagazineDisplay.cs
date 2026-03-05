using UnityEngine;
using UnityEngine.UI;

public class MagazineDisplay : MonoBehaviour
{
    [SerializeField] private Image _magImg;

    private float _fillLevel;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

// public class FloatingAlertIndicator : MonoBehaviour
// {
//     private List<Image> _alertIcons = new();
//
//     private readonly BlackboardKey _visibilityLevel = EnemyController.Blackboard.GetOrRegisterKey("PlayerVisibility");
//     private readonly BlackboardKey _isPlayerVisible = EnemyController.Blackboard.GetOrRegisterKey("PlayerVisible");
//
//     private void Start()
//     {
//         foreach (Image img in GetComponentsInChildren<Image>(true))
//         {
//             _alertIcons.Add(img);
//         }
//     }
//
//     private void Update()
//     {
//         EnemyController.Blackboard.TryGetValue(_visibilityLevel, out float visibilityLevel);
//         EnemyController.Blackboard.TryGetValue(_isPlayerVisible, out bool isPlayerVisible);
//         if (visibilityLevel == 0f) NoSus();
//         if (!isPlayerVisible) return;
//
//         switch (visibilityLevel)
//         {
//             case < 1f :
//                 Sus(visibilityLevel);
//                 break;
//             case 1f:
//                 Alerted();
//                 break;
//                 
//         }
//     }
//     
//     private void NoSus()
//     {
//         _alertIcons[0].enabled = true;
//         EnemyController.Blackboard.SetValue(_isPlayerVisible, false);
//         
//         _alertIcons[1].enabled = false;
//         _alertIcons[1].fillAmount = 0;
//         _alertIcons[2].enabled = false;
//     }
//
//     private void Sus(float level)
//     {
//         _alertIcons[1].enabled = true;
//         _alertIcons[1].fillAmount = level;
//         
//         _alertIcons[0].enabled = false;
//         _alertIcons[2].enabled = false;
//     }
//
//     private void Alerted()
//     {
//         _alertIcons[2].enabled = true;
//         
//         _alertIcons[0].enabled = false;
//         _alertIcons[1].enabled = false;
//     }
//     
// }
