#region

using UnityEngine;

#endregion

namespace WaystoneMason.Agents
{
    public class TestAgent : WMAgent
    {
        protected override void Update()
        {
            base.Update();
            if (Input.GetMouseButton(0))
            { 
                SetGoal(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            }
        }
    }
}