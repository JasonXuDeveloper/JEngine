using System.Collections.Generic;

namespace JEngine.UI
{
    public class ItemSizeGroup
{

    public float[] mItemSizeArray;
    public float[] mItemStartPosArray;
    public int mItemCount;
    int mDirtyBeginIndex = ItemPosMgr.mItemMaxCountPerGroup;
    public float mGroupSize;
    public float mGroupStartPos;
    public float mGroupEndPos;
    public int mGroupIndex;
    float mItemDefaultSize;
    public ItemSizeGroup(int index, float itemDefaultSize)
    {
        mGroupIndex = index;
        mItemDefaultSize = itemDefaultSize;
        Init();
    }

    public void Init()
    {
        mItemSizeArray = new float[ItemPosMgr.mItemMaxCountPerGroup];
        if (mItemDefaultSize != 0)
        {
            for (int i = 0; i < mItemSizeArray.Length; ++i)
            {
                mItemSizeArray[i] = mItemDefaultSize;
            }
        }
        mItemStartPosArray = new float[ItemPosMgr.mItemMaxCountPerGroup];
        mItemStartPosArray[0] = 0;
        mItemCount = ItemPosMgr.mItemMaxCountPerGroup;
        mGroupSize = mItemDefaultSize * mItemSizeArray.Length;
        if (mItemDefaultSize != 0)
        {
            mDirtyBeginIndex = 0;
        }
        else
        {
            mDirtyBeginIndex = ItemPosMgr.mItemMaxCountPerGroup;
        }
    }

    public float GetItemStartPos(int index)
    {
        return mGroupStartPos + mItemStartPosArray[index];
    }

    public bool IsDirty
    {
        get
        {
            return (mDirtyBeginIndex < mItemCount);
        }
    }
    public float SetItemSize(int index, float size)
    {
        float old = mItemSizeArray[index];
        if (old == size)
        {
            return 0;
        }
        mItemSizeArray[index] = size;
        if (index < mDirtyBeginIndex)
        {
            mDirtyBeginIndex = index;
        }
        float ds = size - old;
        mGroupSize = mGroupSize + ds;
        return ds;
    }

    public void SetItemCount(int count)
    {
        if (mItemCount == count)
        {
            return;
        }
        mItemCount = count;
        RecalcGroupSize();
    }

    public void RecalcGroupSize()
    {
        mGroupSize = 0;
        for (int i = 0; i < mItemCount; ++i)
        {
            mGroupSize += mItemSizeArray[i];
        }
    }

    public int GetItemIndexByPos(float pos)
    {
        if (mItemCount == 0)
        {
            return -1;
        }
        int low = 0;
        int high = mItemCount - 1;
        while (low <= high)
        {
            int mid = (low + high) / 2;
            float startPos = mItemStartPosArray[mid];
            float endPos = startPos + mItemSizeArray[mid];
            if (startPos <= pos && endPos >= pos)
            {
                return mid;
            }

            if (pos > endPos)
            {
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }
        return -1;
    }

    public void UpdateAllItemStartPos()
    {
        if (mDirtyBeginIndex >= mItemCount)
        {
            return;
        }
        int startIndex = (mDirtyBeginIndex < 1) ? 1 : mDirtyBeginIndex;
        for (int i = startIndex; i < mItemCount; ++i)
        {
            mItemStartPosArray[i] = mItemStartPosArray[i - 1] + mItemSizeArray[i - 1];
        }
        mDirtyBeginIndex = mItemCount;
    }
}

public class ItemPosMgr
{
    public const int mItemMaxCountPerGroup = 100;
    List<ItemSizeGroup> mItemSizeGroupList = new List<ItemSizeGroup>();
    int mDirtyBeginIndex = int.MaxValue;
    public float mTotalSize;
    public float mItemDefaultSize = 20;

    public ItemPosMgr(float itemDefaultSize)
    {
        mItemDefaultSize = itemDefaultSize;
    }

    public void SetItemMaxCount(int maxCount)
    {
        mDirtyBeginIndex = 0;
        mTotalSize = 0;
        int st = maxCount % mItemMaxCountPerGroup;
        int lastGroupItemCount = st;
        int needMaxGroupCount = maxCount / mItemMaxCountPerGroup;
        if (st > 0)
        {
            needMaxGroupCount++;
        }
        else
        {
            lastGroupItemCount = mItemMaxCountPerGroup;
        }
        int count = mItemSizeGroupList.Count;
        if (count > needMaxGroupCount)
        {
            int d = count - needMaxGroupCount;
            mItemSizeGroupList.RemoveRange(needMaxGroupCount, d);
        }
        else if (count < needMaxGroupCount)
        {
            int d = needMaxGroupCount - count;
            for (int i = 0; i < d; ++i)
            {
                ItemSizeGroup tGroup = new ItemSizeGroup(count + i, mItemDefaultSize);
                mItemSizeGroupList.Add(tGroup);
            }
        }
        count = mItemSizeGroupList.Count;
        if (count == 0)
        {
            return;
        }
        for (int i = 0; i < count - 1; ++i)
        {
            mItemSizeGroupList[i].SetItemCount(mItemMaxCountPerGroup);
        }
        mItemSizeGroupList[count - 1].SetItemCount(lastGroupItemCount);
        for (int i = 0; i < count; ++i)
        {
            mTotalSize = mTotalSize + mItemSizeGroupList[i].mGroupSize;
        }

    }

    public void SetItemSize(int itemIndex, float size)
    {
        int groupIndex = itemIndex / mItemMaxCountPerGroup;
        int indexInGroup = itemIndex % mItemMaxCountPerGroup;
        ItemSizeGroup tGroup = mItemSizeGroupList[groupIndex];
        float changedSize = tGroup.SetItemSize(indexInGroup, size);
        if (changedSize != 0f)
        {
            if (groupIndex < mDirtyBeginIndex)
            {
                mDirtyBeginIndex = groupIndex;
            }
        }
        mTotalSize += changedSize;
    }

    public float GetItemPos(int itemIndex)
    {
        Update(true);
        int groupIndex = itemIndex / mItemMaxCountPerGroup;
        int indexInGroup = itemIndex % mItemMaxCountPerGroup;
        return mItemSizeGroupList[groupIndex].GetItemStartPos(indexInGroup);
    }

    public void GetItemIndexAndPosAtGivenPos(float pos, ref int index, ref float itemPos)
    {
        Update(true);
        index = 0;
        itemPos = 0f;
        int count = mItemSizeGroupList.Count;
        if (count == 0)
        {
            return;
        }
        ItemSizeGroup hitGroup = null;

        int low = 0;
        int high = count - 1;
        while (low <= high)
        {
            int mid = (low + high) / 2;
            ItemSizeGroup tGroup = mItemSizeGroupList[mid];
            if (tGroup.mGroupStartPos <= pos && tGroup.mGroupEndPos >= pos)
            {
                hitGroup = tGroup;
                break;
            }

            if (pos > tGroup.mGroupEndPos)
            {
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }
        int hitIndex = -1;
        if (hitGroup != null)
        {
            hitIndex = hitGroup.GetItemIndexByPos(pos - hitGroup.mGroupStartPos);
        }
        else
        {
            return;
        }
        if (hitIndex < 0)
        {
            return;
        }
        index = hitIndex + hitGroup.mGroupIndex * mItemMaxCountPerGroup;
        itemPos = hitGroup.GetItemStartPos(hitIndex);
    }

    public void Update(bool updateAll)
    {
        int count = mItemSizeGroupList.Count;
        if (count == 0)
        {
            return;
        }
        if (mDirtyBeginIndex >= count)
        {
            return;
        }
        int loopCount = 0;
        for (int i = mDirtyBeginIndex; i < count; ++i)
        {
            loopCount++;
            ItemSizeGroup tGroup = mItemSizeGroupList[i];
            mDirtyBeginIndex++;
            tGroup.UpdateAllItemStartPos();
            if (i == 0)
            {
                tGroup.mGroupStartPos = 0;
                tGroup.mGroupEndPos = tGroup.mGroupSize;
            }
            else
            {
                tGroup.mGroupStartPos = mItemSizeGroupList[i - 1].mGroupEndPos;
                tGroup.mGroupEndPos = tGroup.mGroupStartPos + tGroup.mGroupSize;
            }
            if (!updateAll && loopCount > 1)
            {
                return;
            }

        }
    }

}
}