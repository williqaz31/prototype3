using UnityEngine;
using XCharts.Runtime;
#if INPUT_SYSTEM_ENABLED
using Input = XCharts.Runtime.InputHelper;
#endif

namespace XCharts.Example
{
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class Example_Test : MonoBehaviour
    {
        private BaseChart chart;

        private void Awake()
        {
            chart = gameObject.GetComponent<BaseChart>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                AddData();
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                chart.AnimationReset();
                chart.AnimationFadeIn();
            }
            else if (Input.GetKeyDown(KeyCode.U))
            {
                chart.UpdateData(0, 2, 99);
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                chart.UpdateData(0, 2, 22);
            }
        }

        private void AddData()
        {
            chart.AnimationReset();
            chart.AnimationFadeOut();
        }
    }
}