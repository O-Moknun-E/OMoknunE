using UnityEngine;

public class FakeStoneCleanup : MonoBehaviour
{
    private int _targetX;
    private int _targetY;

    public void LockSpot(int x, int y)
    {
        _targetX = x;
        _targetY = y;

        OmokManager.Instance.SetBoardData(_targetX, _targetY, StoneType.Fake);

        BoardInteraction bi = FindFirstObjectByType<BoardInteraction>();
        if (bi != null) bi.SetStonePlacedState(_targetX, _targetY, true);
    }

    private void OnDestroy()
    {
        if (!gameObject.scene.isLoaded) return;

        if (OmokManager.Instance != null && OmokManager.Instance.GetBoardData(_targetX, _targetY) == StoneType.Fake)
        {
            OmokManager.Instance.SetBoardData(_targetX, _targetY, StoneType.Empty);
        }

        BoardInteraction bi = FindFirstObjectByType<BoardInteraction>();
        if (bi != null) bi.SetStonePlacedState(_targetX, _targetY, false);
    }
}