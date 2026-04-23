using UnityEngine;

public class BoardGridVisualizer : MonoBehaviour
{
    [Header("오목판 세팅")]
    public int gridSize = 15;        // 15x15 오목판

    [Tooltip("칸 사이의 간격")]
    public float spacing = 1.0f;

    [Tooltip("전체 그리드의 중심점 미세조정 (상하좌우 쏠림 보정)")]
    public Vector2 offset = Vector2.zero;

    [Header("기즈모 표시 옵션")]
    public bool showGridLines = true;
    public bool showClickPoints = true;

    private void OnDrawGizmos()
    {
        // 1. 전체 그리드가 정중앙에 오도록 절반 크기 계산
        float halfSize = (gridSize - 1) * spacing / 2f;

        // 2. 바둑판 선 그리기 (초록색)
        if (showGridLines)
        {
            Gizmos.color = Color.green;

            for (int i = 0; i < gridSize; i++)
            {
                float pos = (i * spacing) - halfSize;

                // 세로선 그리기
                Vector3 verticalStart = new Vector3(pos, -halfSize, 0) + (Vector3)offset;
                Vector3 verticalEnd = new Vector3(pos, halfSize, 0) + (Vector3)offset;
                Gizmos.DrawLine(transform.position + verticalStart, transform.position + verticalEnd);

                // 가로선 그리기
                Vector3 horizontalStart = new Vector3(-halfSize, pos, 0) + (Vector3)offset;
                Vector3 horizontalEnd = new Vector3(halfSize, pos, 0) + (Vector3)offset;
                Gizmos.DrawLine(transform.position + horizontalStart, transform.position + horizontalEnd);
            }
        }

        // 3. 실제 돌이 놓일 교차점 그리기 (빨간색)
        if (showClickPoints)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f); // 반투명 빨간색

            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    float posX = (x * spacing) - halfSize;
                    float posY = (y * spacing) - halfSize;
                    Vector3 pointPos = new Vector3(posX, posY, 0) + (Vector3)offset;

                    // 돌이 놓일 자리에 작은 구슬을 그림
                    Gizmos.DrawSphere(transform.position + pointPos, spacing * 0.15f);
                }
            }
        }
    }
}