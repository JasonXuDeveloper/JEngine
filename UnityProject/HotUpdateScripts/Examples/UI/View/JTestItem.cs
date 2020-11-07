using JEngine.UI.UIKit;
using UnityEngine;

namespace JEngine.Examples
{
    public class JTestItem : AItemBase
    {
        public override void setObj(GameObject obj)
        {
            base.setObj(obj);

            //持有 GameObject，Transform，RectTransform
        }
    }
}
