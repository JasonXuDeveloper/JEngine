using JEngine.UI.UIKit;
using UnityEngine;
using UnityEngine.UI;

namespace JEngine.Examples
{
    public class JTestItem : AItemBase
    {
        private Text label;

        public override void setObj(GameObject obj)
        {
            base.setObj(obj);

            label = UIUtility.GetComponent<Text>(RectTrans, "名字要唯一");
        }

        public override void Refresh<T>(T data)
        {
            base.Refresh(data);

            UIUtility.Safe_UGUI(ref label, data as object);
        }
    }
}
