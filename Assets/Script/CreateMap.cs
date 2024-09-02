using NavMeshPlus.Components;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class CreateMap : MonoBehaviour
{

    MapTree map;
    [SerializeField]
    public Vector2Int mapSize;// 맵의 크기를 정의하는 변수 (가로 x, 세로 y)
    [SerializeField]
    Tilemap tilemap;
    [SerializeField]
    Tilemap tilemapWall;
    [SerializeField]
    Tilemap tilemapWall1;
    [SerializeField]
    Tilemap tilemapWall2;
    [SerializeField]
    TileBase tileBase1;

    [SerializeField]
    TileBase tileBase2;

    [SerializeField]
    TileBase tileBase3;

    [SerializeField]
    TileBase tileBase4;

    [SerializeField]
    TileBase tileBase5;

    [SerializeField]
    TileBase tileBase6;

    [SerializeField]
    List<Vector3Int> roomCenterList = new List<Vector3Int>();// 각 방의 중심 좌표를 저장할 리스트

    List<RectInt> roomList = new List<RectInt>();

    LinkData linkData = new LinkData();


    public Tilemap GetTilemap()
    {
        return tilemap;
    }
    public NavMeshSurface Surface2D;
    private void Start()
    {
        Surface2D.BuildNavMeshAsync(); // 비동기적으로 NavMesh를 구축
    }
    // 1. Awake 순서 정리 스크립트 호출 시 먼저 실행됨. 
    // 2. 맵의 크기를 기반으로 전체 지도 영역을 나타내는 MapTree 객체를 생성 / 방을 재귀적으로 쪼개기 위해 사이즈를 매개변수로 넘김
    // 3. divideMap() 함수는 재귀적으로 호출되며, 각 호출에서는 현재 노드가 나뉜 후에도 충분히 큰지 확인한 다음, left와 right 노드로 구성됩니다.
    // 4. GetLastNode() 이진 트리 구조의 모든 리프 노드를 포함하는 목록을 반환합니다.  함수는 재귀적인 방 나누기 과정에서 각 분할의 최종 단계에 있는 노드들을 수집하는 코드입니다. 이 메소드는 분할 과정에서 생성된 이진 트리의 모든 리프 노드를 리스트로 반환하는 역할을 합니다. 리프 노드란 더 이상 자식 노드가 없는 노드를 의미합니다.
    // 5. //GenerationRoute() 함수를 호출하여 이미 생성된 방들 간의 연결(방향)을 설정하고, 이에 따른 LinkData 클래스에 정보를 저장합니다.
    // 5.1GetNearNodes() 왼쪽과 오른쪽 서브트리의 모든 노드 쌍을 검사하고 최단 거리를 찾습니다.    마지막으로, 왼쪽과 오른쪽 서브트리에서 반환된 최단 거리를 가지는 노드 쌍들을 합친 후, 이를 리스트로 반환합니다.
    // 5.2아래 함수에서 다리연결 방끼리 연결은 좌표로 이미 되어있음 다리만 그리는 함수임
    // 6 부터 실제로 타일 그림
    private void Awake()
    {
        //2.
        map = new MapTree(mapSize);
        //4.
        var nodeList = map.GetLastNode();
        //5.
        GenerationRoute();

        GameManager.Instance.SetLinkData(linkData);/// GameManager 인스턴스에 각 방 연결관계를 담은 linkData 전달


        //6. 위 가공데이터를 토대로 타일을 화면에 그리기 시작하는 foreach
        foreach (var (room, index) in nodeList.Select((value, index) => (value, index)))

        {
            TileBase tileBase;
            tileBase = tileBase1;
           
            int roomWidth = Mathf.RoundToInt(room.data.width * 0.8f); // 방 넓이의 80%
            int roomHeight = Mathf.RoundToInt(room.data.height * 0.8f); // 방 넓이의 80% 
            int offsetX = Mathf.RoundToInt((room.data.width - roomWidth) / 2f);//전체 길이 - 전체길이의 80% / 나누기 2  즉 시작위치 더해주는 오프셋
            int offsetY = Mathf.RoundToInt((room.data.height - roomHeight) / 2f);
            for (int h = room.data.position.y + offsetY; h < room.data.position.y + offsetY + roomHeight; h++)
            {
                for (int w = room.data.position.x + offsetX; w < room.data.position.x + offsetX + roomWidth; w++)
                {
                    tilemap.SetTile(new Vector3Int(w, h, 0), tileBase); // 방의 바닥 타일을 채움
                }
            }
            /*80%기존*/
            Vector3 bottomLeft = new Vector3(room.data.position.x + offsetX, room.data.position.y + offsetY, 0);
            Vector3 topLeft = new Vector3(room.data.position.x + offsetX, room.data.position.y + offsetY + roomHeight, 0);
            Vector3 bottomRight = new Vector3(room.data.position.x + offsetX + roomWidth, room.data.position.y + offsetY, 0);
            Vector3 topRight = new Vector3(room.data.position.x + offsetX + roomWidth, room.data.position.y + offsetY + roomHeight, 0);

            Debug.DrawLine(bottomLeft, topLeft, Color.green, 1000f);
            Debug.DrawLine(topLeft, topRight, Color.green, 1000f);
            Debug.DrawLine(topRight, bottomRight, Color.green, 1000f);
            Debug.DrawLine(bottomRight, bottomLeft, Color.green, 1000f);

            /*##3 위치이동*/
            float moveAmount = 200.0f; // 예시로 5라는 값을 사용합니다. 
            float moveAmountY = 300.0f; // 예시로 5라는 값을 사용합니다. 
            Vector3 bottomLeft1 = new Vector3(room.data.position.x + offsetX + moveAmount, room.data.position.y + offsetY + moveAmountY, 0);
            Vector3 topLeft1 = new Vector3(room.data.position.x + offsetX + moveAmount, room.data.position.y + offsetY + moveAmountY + roomHeight, 0);
            Vector3 bottomRight1 = new Vector3(room.data.position.x + offsetX + roomWidth + moveAmount, room.data.position.y + offsetY + moveAmountY, 0);
            Vector3 topRight1 = new Vector3(room.data.position.x + offsetX + roomWidth + moveAmount, room.data.position.y + offsetY + moveAmountY + roomHeight, 0); 
            Debug.DrawLine(bottomLeft1, topLeft1, Color.green, 1000f);
            Debug.DrawLine(topLeft1, topRight1, Color.green, 1000f);
            Debug.DrawLine(topRight1, bottomRight1, Color.green, 1000f);
            Debug.DrawLine(bottomRight1, bottomLeft1, Color.green, 1000f);



            // 2층 3층 4층
            foreach (var rooms in nodeList)
            {
                int roomLeft = room.data.position.x + offsetX;
                int roomRight = roomLeft + roomWidth;
                int roomTop = room.data.position.y + offsetY;
                int roomBottom = roomTop + roomHeight;

                RectInt roomRect = new RectInt(roomLeft, roomTop, roomWidth, roomHeight);

                roomList.Add(roomRect);
                for (int w = roomLeft; w <= roomRight; w++)
                {
                    tilemapWall.SetTile(new Vector3Int(w, roomTop, 0), tileBase2);
                    tilemapWall1.SetTile(new Vector3Int(w + 1, roomTop + 1, 0), tileBase5);
                    tilemapWall2.SetTile(new Vector3Int(w + 2, roomTop + 2, 0), tileBase6);
                }

                // Draw bottom border
                for (int w = roomLeft; w <= roomRight; w++)
                {
                    tilemapWall.SetTile(new Vector3Int(w, roomBottom, 0), tileBase2);
                    tilemapWall1.SetTile(new Vector3Int(w + 1, roomBottom + 1, 0), tileBase5);
                    tilemapWall2.SetTile(new Vector3Int(w + 2, roomBottom + 2, 0), tileBase6);
                }

                // Draw left border
                for (int h = roomTop + 1; h < roomBottom; h++)
                {
                    tilemapWall.SetTile(new Vector3Int(roomLeft, h, 0), tileBase2);
                    tilemapWall1.SetTile(new Vector3Int(roomLeft + 1, h + 1, 0), tileBase5);
                    tilemapWall2.SetTile(new Vector3Int(roomLeft + 2, h + 2, 0), tileBase6);
                }

                // Draw right border
                for (int h = roomTop + 1; h < roomBottom; h++)
                {
                    tilemapWall.SetTile(new Vector3Int(roomRight, h, 0), tileBase2);
                    tilemapWall1.SetTile(new Vector3Int(roomRight + 1, h + 1, 0), tileBase5);
                    tilemapWall2.SetTile(new Vector3Int(roomRight + 2, h + 2, 0), tileBase6);
                }
            }
           
            if (SceneChangeManager.Instance != null) // 각 방의 중심 좌표를 저장
            {
                SceneChangeManager.Instance.roomCenterList.Add(new Vector3Int(room.data.position.x + offsetX + roomWidth / 2, room.data.position.y + offsetY + roomHeight / 2, 0));
                roomCenterList.Add(new Vector3Int(room.data.position.x + offsetX + roomWidth / 2, room.data.position.y + offsetY + roomHeight / 2, 0));
            }
            else
            {
                Debug.LogError("SceneChangeManager instance is not available.");
            }
        }

       
        var data = map.map.GetNearNodes(); //입구 만들기 위해 벽을 지우는 DeleteWall 함수 코드는 기존것 가져옴
        foreach (List<MapNode> node in data)
        {
            var temp1 = new Vector2Int((int)node[0].data.center.x, (int)node[0].data.center.y);
            var temp2 = new Vector2Int((int)node[1].data.center.x, (int)node[1].data.center.y);
            DeleteWall(temp1, temp2);
        }

       

        foreach (var roomCenter in roomCenterList) // 각 방의 중심이 잘 찍히는지 테스트용 
        {
            tilemap.SetTile(roomCenter, tileBase2);
        }

        // 맵 view 출력 후 동적으로 베이크 하여 길 생성.
        NavMeshSurface navMeshSurface = GetComponent<NavMeshSurface>();
        navMeshSurface.BuildNavMesh();
    }


    // GenerationRoute 연결 수립, 다리 그리기, 수립된 데이터 linkData 저장
    public void GenerationRoute()
    {

        //5.1
        var data = map.map.GetNearNodes();

        foreach (List<MapNode> node in data)
        {
            var temp1 = new Vector2Int((int)node[0].data.center.x, (int)node[0].data.center.y);
            var temp2 = new Vector2Int((int)node[1].data.center.x, (int)node[1].data.center.y);
            //5.2
            DrawLine(temp1, temp2);

            Vector2 vec = (node[1].data.center - node[0].data.center).normalized;
            Direction dir = Direction.none;
            if (vec.x >= 0 && vec.y >= 0)
            {
                //오른쪽or위
                if (Mathf.Abs(vec.x) > Mathf.Abs(vec.y))
                    dir = Direction.right;
                else
                    dir = Direction.up;
            }
            if (vec.x >= 0 && vec.y <= 0)
            {
                //오른쪽or아래
                if (Mathf.Abs(vec.x) > Mathf.Abs(vec.y))
                    dir = Direction.right;
                else
                    dir = Direction.down;
            }
            if (vec.x <= 0 && vec.y >= 0)
            {
                //왼쪽or위
                if (Mathf.Abs(vec.x) > Mathf.Abs(vec.y))
                    dir = Direction.left;
                else
                    dir = Direction.up;
            }
            if (vec.x <= 0 && vec.y <= 0)
            {
                //왼쪽or아래
                if (Mathf.Abs(vec.x) > Mathf.Abs(vec.y))
                    dir = Direction.left;
                else
                    dir = Direction.down;
            }

            if (dir == Direction.none)
                continue;


            linkData.AddToNodeDir(node[0].data, dir, node[1].data);
        }
    }

    //1층
    void DrawLine(Vector2Int start, Vector2Int end)
    {
        int x1 = start.x;
        int y1 = start.y;
        int x2 = end.x;
        int y2 = end.y;


        //#2 디버그용2
        Vector3 startPos = new Vector3(start.x, start.y, 0);
        Vector3 endPos = new Vector3(end.x, end.y, 0);

        float distance = Vector3.Distance(startPos, endPos);
        int segmentCount = Mathf.CeilToInt(distance);

        Vector3 step = (endPos - startPos) / segmentCount;   

        for (int i = 0; i < segmentCount; i++)
        {
            Vector3 segmentStart = startPos + step * i;
            Vector3 segmentEnd = startPos + step * (i + 1);
            Debug.DrawLine(segmentStart, segmentEnd, Color.red, Mathf.Infinity);
        }
        //##2 디버그용 2end

        int  offsetX = 0;
        int offsetY = 300;
        Vector3 startPos1 = new Vector3(start.x + offsetX, start.y + offsetY, 0);
        Vector3 endPos1 = new Vector3(end.x + offsetX, end.y+ offsetY, 0); 

        float distance1 = Vector3.Distance(startPos1, endPos1);
        int segmentCount1 = Mathf.CeilToInt(distance1);

        Vector3 step1 = (endPos1 - startPos1) / segmentCount1;

        for (int i = 0; i < segmentCount1; i++)
        {
            Vector3 segmentStart1 = startPos1 + step1 * i;
            Vector3 segmentEnd1 = startPos1 + step1 * (i + 1);
            Debug.DrawLine(segmentStart1, segmentEnd1, Color.red, Mathf.Infinity);
        }



        if (x1 == x2)
        {
            int minY = Mathf.Min(y1, y2);
            int maxY = Mathf.Max(y1, y2);
            for (int y = minY; y <= maxY; y++)
            {
                tilemap.SetTile(new Vector3Int(x1, y, 0), tileBase1);
                tilemap.SetTile(new Vector3Int(x1 + 1, y, 0), tileBase1);
            }
        }
        else if (y1 == y2)
        {
            int minX = Mathf.Min(x1, x2);
            int maxX = Mathf.Max(x1, x2);
            for (int x = minX; x <= maxX; x++)
            {
                tilemap.SetTile(new Vector3Int(x, y1, 0), tileBase1);
                tilemap.SetTile(new Vector3Int(x, y1 + 1, 0), tileBase1);
            }
        }
        else
        {
            int minX = Mathf.Min(x1, x2);
            int maxX = Mathf.Max(x1, x2);
            int minY = Mathf.Min(y1, y2);
            int maxY = Mathf.Max(y1, y2);
            for (int x = minX; x <= maxX; x++)
            {
                tilemap.SetTile(new Vector3Int(x, y1, 0), tileBase1);
                tilemap.SetTile(new Vector3Int(x, y1 + 1, 0), tileBase1);
            }
            for (int y = minY; y <= maxY; y++)
            {
                tilemap.SetTile(new Vector3Int(x2, y, 0), tileBase1);
                tilemap.SetTile(new Vector3Int(x2 + 1, y, 0), tileBase1);
            }
        }
    }

    //방입구 지우기)

    void DeleteWall(Vector2Int start, Vector2Int end)
    {
        int x1 = start.x;
        int y1 = start.y;
        int x2 = end.x;
        int y2 = end.y;

        if (x1 == x2)
        {
            int minY = Mathf.Min(y1, y2);
            int maxY = Mathf.Max(y1, y2);
            for (int y = minY - 1; y <= maxY + 1; y++)
            {
                tilemapWall.SetTile(new Vector3Int(x1 + 1, y, 0), null);
                tilemapWall.SetTile(new Vector3Int(x1 + 2, y, 0), null);
                tilemapWall1.SetTile(new Vector3Int(x1 + 2, y, 0), null);
                tilemapWall1.SetTile(new Vector3Int(x1 + 3, y, 0), null);
                tilemapWall2.SetTile(new Vector3Int(x1 + 3, y, 0), null);
                tilemapWall2.SetTile(new Vector3Int(x1 + 4, y, 0), null);
            }
        }
        else if (y1 == y2)
        {
            int minX = Mathf.Min(x1, x2);
            int maxX = Mathf.Max(x1, x2);
            for (int x = minX - 1; x <= maxX + 1; x++)
            {
                tilemapWall.SetTile(new Vector3Int(x, y1 + 1, 0), null);
                tilemapWall.SetTile(new Vector3Int(x, y1 + 2, 0), null);
                tilemapWall1.SetTile(new Vector3Int(x, y1 + 2, 0), null);
                tilemapWall1.SetTile(new Vector3Int(x, y1 + 3, 0), null);
                tilemapWall2.SetTile(new Vector3Int(x, y1 + 3, 0), null);
                tilemapWall2.SetTile(new Vector3Int(x, y1 + 4, 0), null);
            }
        }
        else
        {
            int minX = Mathf.Min(x1, x2);
            int maxX = Mathf.Max(x1, x2);
            int minY = Mathf.Min(y1, y2);
            int maxY = Mathf.Max(y1, y2);
            for (int x = minX - 1; x <= maxX + 1; x++)
            {
                tilemapWall.SetTile(new Vector3Int(x, y1 + 1, 0), null);
                tilemapWall.SetTile(new Vector3Int(x, y1 + 2, 0), null);
                tilemapWall1.SetTile(new Vector3Int(x, y1 + 2, 0), null);
                tilemapWall1.SetTile(new Vector3Int(x, y1 + 3, 0), null);
                tilemapWall2.SetTile(new Vector3Int(x, y1 + 3, 0), null);
                tilemapWall2.SetTile(new Vector3Int(x, y1 + 4, 0), null);
            }
            for (int y = minY - 1; y <= maxY + 1; y++)
            {
                tilemapWall.SetTile(new Vector3Int(x2 + 1, y, 0), null);
                tilemapWall.SetTile(new Vector3Int(x2 + 2, y, 0), null);
                tilemapWall1.SetTile(new Vector3Int(x2 + 2, y, 0), null);
                tilemapWall1.SetTile(new Vector3Int(x2 + 3, y, 0), null);
                tilemapWall2.SetTile(new Vector3Int(x2 + 3, y, 0), null);
                tilemapWall2.SetTile(new Vector3Int(x2 + 4, y, 0), null);
            }
        }
    }


}
public class MapTree
{
    public Vector2Int mapSize;
    public MapNode map;

    public MapTree(Vector2Int size)
    {
        mapSize = size;
        map = new MapNode(new RectInt(new Vector2Int(0, 0), mapSize));//방을 조각조각 나누기위해 맵 사이즈를 매배견수로 전달
        map.divideMap();
    }
    public List<MapNode> GetLastNode()
    {
        return map.GetLastNode();
    }
}

public class MapNode
{
    public RectInt data;
    public MapNode left;
    public MapNode right;
    public MapNode(RectInt ri)
    {
        //x y ,w h 초기화
        data = ri;
    }

    //3.
    public void divideMap()
    {
        if (data.width <= 70 && data.height <= 70)
            return;
        RectInt leftRect;
        RectInt rightRect;
        if (data.width >= data.height)
        {
            int length = data.width / 3;
            var width = Random.Range(length, length * 2);
            leftRect = new RectInt(data.position, new Vector2Int(width, data.height));
            rightRect = new RectInt(new Vector2Int(data.position.x + width, data.position.y), new Vector2Int(data.width - width, data.height));
        }
        else
        {
            int length = data.height / 3;
            var height = Random.Range(length, length * 2);
            leftRect = new RectInt(data.position, new Vector2Int(data.width, height));
            rightRect = new RectInt(new Vector2Int(data.position.x, data.position.y + height), new Vector2Int(data.width, data.height - height));
        }
        left = new MapNode(leftRect);
        right = new MapNode(rightRect);

        /* 디버그용 - 이 디버그 라인들은 방 분할의 결과만을 시각화하는 데 사용되며, 방의 연결 상태는 표현하지 않습니다. 
         각 Debug.DrawLine 호출은 나누어진 각 방의 경계선을 그리기 위해 사용됩니다.
        색깔이 다른 두 세트의 선(빨간색과 파란색)은 맵을 두 부분으로 나눌 때 각각 왼쪽 부분과 오른쪽 부분을 나타냅니다.
        빨간색 선들은 왼쪽 부분의 상하좌우 경계를 표시하며, 파란색 선들은 오른쪽 부분의 상하좌우 경계를 표시합니다.
        이 라인들은 방이 완전히 나눠진 후 해당 경계를 나타내므로, 이 시점에서는 방들이 연결되지 않았음을 의미합니다. 
         */
        Debug.DrawLine(new Vector3(leftRect.x, leftRect.y), new Vector3(leftRect.x + leftRect.width, leftRect.y), Color.blue, 1000f);
        Debug.DrawLine(new Vector3(leftRect.x + leftRect.width, leftRect.y), new Vector3(leftRect.x + leftRect.width, leftRect.y + leftRect.height), Color.blue, 1000f);
        Debug.DrawLine(new Vector3(leftRect.x, leftRect.y + leftRect.height), new Vector3(leftRect.x + leftRect.width, leftRect.y + leftRect.height), Color.blue, 1000f);
        Debug.DrawLine(new Vector3(leftRect.x, leftRect.y), new Vector3(leftRect.x, leftRect.y + leftRect.height), Color.blue, 100);

        Debug.DrawLine(new Vector3(rightRect.x, rightRect.y), new Vector3(rightRect.x + rightRect.width, rightRect.y), Color.blue, 1000f);
        Debug.DrawLine(new Vector3(rightRect.x + rightRect.width, rightRect.y), new Vector3(rightRect.x + rightRect.width, rightRect.y + rightRect.height), Color.blue, 1000f);
        Debug.DrawLine(new Vector3(rightRect.x, rightRect.y + rightRect.height), new Vector3(rightRect.x + rightRect.width, rightRect.y + rightRect.height), Color.blue, 1000f);
        Debug.DrawLine(new Vector3(rightRect.x, rightRect.y), new Vector3(rightRect.x, rightRect.y + rightRect.height), Color.blue, 1000f);


        /*##1위치이동*/
        int offsetX = -300;
        int offsetY = 300;
        leftRect.x += offsetX;
        rightRect.x += offsetX;
        leftRect.y += offsetY;
        rightRect.y += offsetY;
        Debug.DrawLine(new Vector3(leftRect.x, leftRect.y), new Vector3(leftRect.x + leftRect.width, leftRect.y), Color.blue, 1000f);
        Debug.DrawLine(new Vector3(leftRect.x + leftRect.width, leftRect.y), new Vector3(leftRect.x + leftRect.width, leftRect.y + leftRect.height), Color.blue, 1000f);
        Debug.DrawLine(new Vector3(leftRect.x, leftRect.y + leftRect.height), new Vector3(leftRect.x + leftRect.width, leftRect.y + leftRect.height), Color.blue, 1000f);
        Debug.DrawLine(new Vector3(leftRect.x, leftRect.y), new Vector3(leftRect.x, leftRect.y + leftRect.height), Color.blue, 1000f);

        Debug.DrawLine(new Vector3(rightRect.x, rightRect.y), new Vector3(rightRect.x + rightRect.width, rightRect.y), Color.blue, 1000f);
        Debug.DrawLine(new Vector3(rightRect.x + rightRect.width, rightRect.y), new Vector3(rightRect.x + rightRect.width, rightRect.y + rightRect.height), Color.blue, 1000f);
        Debug.DrawLine(new Vector3(rightRect.x, rightRect.y + rightRect.height), new Vector3(rightRect.x + rightRect.width, rightRect.y + rightRect.height), Color.blue, 1000f);
        Debug.DrawLine(new Vector3(rightRect.x, rightRect.y), new Vector3(rightRect.x, rightRect.y + rightRect.height), Color.blue, 1000f);



        left.divideMap();
        right.divideMap();
    }
    public List<MapNode> GetLastNode()
    {
        List<MapNode> mapNodes = new List<MapNode>();
        if (left != null)
            mapNodes.AddRange(left.GetLastNode());
        if (right != null)
            mapNodes.AddRange(right.GetLastNode());
        if (left == null && right == null)
            mapNodes.Add(this);
        return mapNodes;
    }
    //왼쪽과 오른쪽 서브트리의 모든 노드 쌍을 검사하고 최단 거리를 찾습니다.    마지막으로, 왼쪽과 오른쪽 서브트리에서 반환된 최단 거리를 가지는 노드 쌍들을 합친 후, 이를 리스트로 반환합니다.
    public List<List<MapNode>> GetNearNodes()
    {
        if (left == null && right == null)
            return null;
        var leftList = left.GetLastNode();
        var rightList = right.GetLastNode();

        MapNode node1 = null;
        MapNode node2 = null;

        List<MapNode> nearPair = new List<MapNode>();

        float gap = float.MaxValue;

        foreach (var lNode in leftList)
        {
            foreach (var rNode in rightList)
            {
                float tempGap = CalculateDistance(lNode.data, rNode.data);
                if (gap > tempGap)
                {
                    node1 = lNode;
                    node2 = rNode;
                    gap = tempGap;
                }
            }
        }
        nearPair.Add(node1);
        nearPair.Add(node2);

        var PairList = new List<List<MapNode>>() { nearPair };

        var leftTemp = left.GetNearNodes();
        var rightTemp = right.GetNearNodes();
        if (leftTemp != null)
            PairList.AddRange(leftTemp);
        if (rightTemp != null)
            PairList.AddRange(rightTemp);

        return PairList;
    }
    private float CalculateDistance(RectInt rectA, RectInt rectB)
    {
        return new Vector2(Mathf.Abs(rectA.center.x - rectB.center.x), Mathf.Abs(rectA.center.y - rectB.center.y)).magnitude;
    }
}
public class LinkData
{
    public LinkedNode Head = null;
    public List<LinkedNode> nodeList = new List<LinkedNode>();


    public LinkedNode FindOnRect(Vector3Int position)
    {
        foreach (var node in nodeList)
        {
            if (node.room.position.x < position.x
                && node.room.position.y < position.y
                && node.room.position.x + node.room.width > position.x
                && node.room.position.y + node.room.height > position.y)
            {
                return node;
            }
        }
        return null;
    }
    public void AddToNodeDir(RectInt origin, Direction dir, RectInt added)
    {

        LinkedNode temp = Find(added);
        if (temp == null)
            temp = new LinkedNode(added);


        LinkedNode findNode = Find(origin);
        if (findNode == null)
        {
            if (Head != null)
            {
                findNode = new LinkedNode(origin);
                nodeList.Add(findNode);
            }
            else
            {
                Head = new LinkedNode(origin);
                findNode = Head;
                nodeList.Add(findNode);
            }
        }
        findNode.linkedNodeDic.Add(dir, temp);
        switch (dir)
        {
            case Direction.left:
                dir = Direction.right;
                break;
            case Direction.right:
                dir = Direction.left;
                break;
            case Direction.up:
                dir = Direction.down;
                break;
            case Direction.down:
                dir = Direction.up;
                break;
        }
        temp.linkedNodeDic.Add(dir, findNode);
        nodeList.Add(temp);
    }
    public LinkedNode Find(RectInt data)
    {
        foreach (var node in nodeList)
        {
            if (node.room.x == data.x && node.room.y == data.y && node.room.width == data.width && node.room.height == data.height)
            {
                return node;
            }
        }
        return null;
    }

}
public class LinkedNode
{
    public RectInt room;
    public Dictionary<Direction, LinkedNode> linkedNodeDic = new Dictionary<Direction, LinkedNode>();

    public LinkedNode(RectInt data)
    {
        room = data;
    }
    public Vector2 CellToPosition(Tilemap tileMap)
    {
        //  Debug.Log(room.center);
        return tileMap.CellToWorld(new Vector3Int((int)room.center.x, (int)room.center.y));
    }
}
public enum Direction
{
    none,
    left,
    right,
    up,
    down
}