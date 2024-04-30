// SceneChangeManager.cs
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class SceneChangeManager : Singleton<SceneChangeManager>
{

    //초기화 세팅 
    [SerializeField] private string sceneToLoad = "map1";
    [SerializeField] GameObject monsterPrefab;
    [SerializeField] private int MonLevel = 1;
    [SerializeField] public MonsterData getMonsterData;

    // 노드 별 레벨 정보를 담는 딕셔너리
    public Dictionary<LinkedNode, int> nodeByLevel = new Dictionary<LinkedNode, int>();

    // 방의 중심 위치 리스트
    public List<Vector3Int> roomCenterList = new List<Vector3Int>();
    Coroutine monsterSpawnCoroutine;

    // SceneToLoad 속성
    public string SceneToLoad
    {
        get
        {
            return sceneToLoad;
        }
        set
        {
            sceneToLoad = value;
        }
    }

    //scene To Load 필드에 적힌 곳으로 이동
    public void StartButton()
    {
        // SceneToLoad에 지정된 씬을 비동기로 로드
        SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Single).completed += OnSceneLoaded;
    } 
    // 씬 로드 완료 시 호출되는 콜백 메서드
    private void OnSceneLoaded(AsyncOperation asyncOperation)
    {
        if (monsterSpawnCoroutine != null)
        {
            // 몬스터 스폰 코루틴 중지
            StopCoroutine(monsterSpawnCoroutine);
          
        }
        // 로드가 완료되면
        if (asyncOperation.isDone)
            //Debug.Log("Test1");
        {
            // ★플레이어 데이터 가져오기 중간에 동료 데이터도 가져옴
            PlayerData playerData = GameDB.Instance.playerDictionary.GetValueOrDefault("Player");
            if (playerData != null)
            {
                // 플레이어 프리팹 로드 및 생성
                GameObject playerPrefab = Resources.Load<GameObject>("Prefabs/Player");
                if (playerPrefab != null)
                {

                    GameObject playerInstance = Instantiate(playerPrefab);
                    GameManager.Instance.SetPlayer(playerInstance);

                    //동료데이터도 가져오기..                   
                    if (sceneToLoad != "map1")
                    {
                        //팀 딕셔너리에서 키로 돌리기 
                        foreach (string teamKey in GameDB.Instance.TeamDictionary.Keys)
                        {                            
                            //키에 해당하는 값을 보기 위한 코드
                            TeamData teamData = GameDB.Instance.TeamDictionary[teamKey];
                            
                            //값에서 state가 거짓이면 종료 왜냐하면 장착하지 않은 것이라 
                            if (!teamData.state) continue;

                            //키를 프리팹 이름과 맵핑해서 프리팹가져오기 
                            GameObject teamMemberPrefab = Resources.Load<GameObject>($"Prefabs/{teamKey}");
                            //팀 컴포먼트 부착
                            
                            
                            if (teamKey == "team1") teamMemberPrefab.tag = "team1";
                            else if (teamKey == "team2") teamMemberPrefab.tag = "team2";
                            else if (teamKey == "team3") teamMemberPrefab.tag = "team3";
                            else Debug.LogError("NoTag");


                            if (teamMemberPrefab != null)
                            {   
                                //가져온 프리펩에서 monstercontrolloer는 필요없어서 지우기 위해서 가져옴
                                MonsterController mcDelete = teamMemberPrefab.GetComponent<MonsterController>();

                                if (mcDelete != null)
                                {
                                    mcDelete.enabled = false;
                                }


                                //오브젝트로 만듬 
                                GameObject teamMemberInstance = Instantiate(teamMemberPrefab);
                                if (teamMemberInstance.GetComponent<Playerteam>() == null)
                                {
                                    // If it doesn't have the component, add it
                                    teamMemberInstance.AddComponent<Playerteam>();
                                    Debug.Log("YourComponent added.");
                                }
                                else
                                {
                                    Debug.Log(teamKey);
                                }
                                //플레이어 아래에 들어가도록
                                teamMemberInstance.GetComponent<Playerteam>().Init(teamData);
                                teamMemberInstance.transform.parent = playerInstance.transform;
                                
                            }
                            else
                            {
                                // Log an error if the team member prefab is not found
                                Debug.LogError("Team member prefab not found!");
                            }
                        }
                    }


                    if (sceneToLoad != "map1")
                    {
                        //프리팹 생성 요기서 세팅을
                        ObjectPool.Instance.CreateInstance("mon1", null, 3);
                        ObjectPool.Instance.CreateInstance("mon2", null, 20);
                        ObjectPool.Instance.CreateInstance("mon3", null, 20);
                        ObjectPool.Instance.CreateInstance("mon4", null, 20);
                        ObjectPool.Instance.CreateInstance("mon5", null, 20);
                        ObjectPool.Instance.CreateInstance("mon6", null, 20);


                        //GameObject monster = ObjectPool.Instance.GetInactiveObject("mon1");
                        //활성화
                        //monster.SetActive(true);
                        //반환
                        //ObjectPool.Instance.AddInactiveObject(monster);

                    }
                    Vector3Int firstRoomCenter = Vector3Int.zero;
                    Tilemap tilemap = null;

                    // 씬이 map1이 아닌 경우 몬스터 프리팹 생성 및 활성화
                    if (sceneToLoad != "map1")
                    {
                        // 연결된 데이터에서 첫 번째 방의 중심을 가져옵니다.
                        Vector3 vec = GameManager.Instance.linkData.Head.room.center;
                        firstRoomCenter = new Vector3Int((int)vec.x, (int)vec.y);//roomCenterList[0];

                        // 현재 Scene의 타일맵을 가져옵니다.
                        tilemap = FindObjectOfType<CreateMap>().GetTilemap();
                        GameManager.Instance.tilemap = tilemap;
                    }
                    // 카메라가 플레이어를 따라다니도록 설정
                    Camera_fix cameraFollowScript = Camera.main.GetComponent<Camera_fix>();
                    cameraFollowScript.player = playerInstance;

                    // 타일맵이 null이 아닌 경우
                    if (tilemap != null)
                    {
                        // 그리드 좌표를 월드 좌표로 변환합니다.
                        Vector3 worldPosition = tilemap.CellToWorld(firstRoomCenter);
                        // 플레이어 위치를 월드 좌표로 설정합니다.
                        playerInstance.transform.position = worldPosition;
                        // 플레이어 주변에 몬스터를 스폰합니다.
                        monsterSpawnCoroutine = StartCoroutine(CheakOnRect());

                    }
                    // 플레이어 데이터를 기반으로 플레이어의 상태를 설정합니다.
                    Statusinfo statusComponent = playerInstance.GetComponent<Statusinfo>();
                    if (statusComponent != null)
                    {
                        statusComponent.hp = playerData.health;
                        statusComponent.str = playerData.str;
                        statusComponent.name = playerData.name;
                    }
                    else
                    {
                        // 플레이어 프리팹 인스턴스에서 Statusinfo 컴포넌트를 찾을 수 없는 경우 오류를 기록합니다.
                        Debug.LogError("Statusinfo component not found on the player prefab instance");
                    }
                }
                else
                {
                    // 플레이어 프리팹이 발견되지 않은 경우 오류를 기록합니다.
                    Debug.LogError("Player prefab not found!");
                }
            }
            else
            {
                // 게임 데이터베이스에서 플레이어 데이터를 찾을 수 없는 경우 경고를 기록합니다.
                Debug.LogWarning("Player data not found in GameDB");
            }
        }
    }
    // 노드 레벨을 설정합니다.
    public void SetNodeLevel()
    {
        int level = 1;
        List<LinkedNode> currentNodes = new List<LinkedNode>();
        List<LinkedNode> recordNodes = new List<LinkedNode>();
        currentNodes.Add(GameManager.Instance.linkData.Head);

        while (true)
        {
            recordNodes.AddRange(currentNodes);
            foreach (LinkedNode node in currentNodes)
            {
                nodeByLevel.Add(node, level);
            }
            List<LinkedNode> temp = new List<LinkedNode>();
            currentNodes.ForEach(node => temp.AddRange(node.linkedNodeDic.Values.ToList()));

            currentNodes = temp.Where(node => !recordNodes.Contains(node)).Distinct().ToList();
            if (currentNodes.Count == 0)
                return;
            level++;
            
        }
        //nodeByLevel;
    }
    // 몬스터를 인스턴스화합니다.
    List<GameObject> InstantiateMonster(int def = 0)
    {

        //말 그래도 몬스터 데이터
        MonsterData monsterData = GameDB.Instance.GetMonster($"mon{def}");

        //GameObject monster = ObjectPool.Instance.GetInactiveObject($"mon{def}");
        //monster.GetComponent<monsterinfo>().Init(monsterData);

        // 키에 해당하는 모든 몬스터를 활성화합니다.
        List<GameObject> allMonsters = ObjectPool.Instance.GetAllInactiveObjects($"mon{def}");

        // 활성화된 모든 몬스터에 몬스터 데이터를 적용합니다.
        allMonsters.ForEach(x => x.GetComponent<MonsterController>().Init(monsterData));

        return allMonsters;
    }

    // 사각형 내에서 노드를 확인하고 몬스터를 스폰하는 코루틴입니다.
    IEnumerator CheakOnRect()
    {
        // 노드 레벨을 설정합니다.
        SetNodeLevel();
        List<GameObject> monsterGroup = new List<GameObject>();
        while (true)
        {
            // 플레이어 위치를 기반으로 노드를 확인합니다.
            Vector3Int vec = GameManager.Instance.tilemap.WorldToCell(GameManager.Instance.player.transform.position);
            var linkedNode = GameManager.Instance.CheakOnNode(vec);
            if (linkedNode == null)
            {
                yield return new WaitForSeconds(0.1f);
                continue;
            }
            /*if (GameManager.Instance.nodes.Contains(linkedNode))
            {
                yield return new WaitForSeconds(0.1f);
                continue;
            }
            GameManager.Instance.nodes.Add(linkedNode);*/
            if (GameManager.Instance.currentNode == linkedNode)
            {
                yield return new WaitForSeconds(0.1f);
                continue;
            }
            GameManager.Instance.currentNode = linkedNode;
            //Debug.Log(MonLevel);

            if (monsterGroup.Count > 0)
                monsterGroup.ForEach(x => ObjectPool.Instance.AddInactiveObject(x));
            // 해당 노드의 레벨에 따라 몬스터를 인스턴스화합니다.
            List<GameObject> monsters = InstantiateMonster(nodeByLevel[linkedNode]);

            monsterGroup.AddRange(monsters);

            /*MonLevel++;
            if (MonLevel > 5)
                MonLevel = 5;*/

            monsters.ForEach(x => x.transform.position = linkedNode.CellToPosition(GameManager.Instance.tilemap));


            yield return new WaitForSeconds(0.1f);
        }
    }
}