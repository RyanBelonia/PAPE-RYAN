using UnityEngine;
using InteriorPlanner.Core;
using InteriorPlanner.Data;

namespace InteriorPlanner.Systems.Room
{
    /// <summary>
    /// Motor de Geração Procedural. Constrói o esqueleto físico da sala 3D (Chão e 4 Paredes) 
    /// com base nas medidas exatas que o utilizador digitou no menu inicial.
    /// </summary>
    public class RoomGenerator : MonoBehaviour
    {
        [SerializeField] private Transform roomRoot; // O objeto-pai onde as paredes geradas vão ficar organizadas
        [SerializeField] private Material wallMaterial;
        [SerializeField] private Material floorMaterial;
        
        // Espessuras físicas dos blocos para terem volume em vez de serem planos (Planes) finos de papel
        [SerializeField] private float wallThickness = 0.15f;
        [SerializeField] private float floorThickness = 0.1f;

        private void Start()
        {
            // Arranca a construção assim que a cena de planeamento é carregada
            GenerateRoom();
        }

        private void GenerateRoom()
        {
            // Segurança: Aborta a criação se alguém abrir a cena diretamente no Editor sem passar pelo Menu Inicial
            if (AppManager.Instance == null || !AppManager.Instance.ProjectSession.HasProjectLoaded())
            {
                Debug.LogWarning("Nenhum projeto carregado.");
                return;
            }

            // Vai buscar as medidas à memória global
            RoomData room = AppManager.Instance.ProjectSession.CurrentProject.Room;

            if (room == null) return;

            // Limpa lixo residual, cria a base e depois levanta as paredes
            ClearRoom();
            CreateFloor(room);
            CreateWalls(room);
        }

        private void CreateFloor(RoomData room)
        {
            // Gera um cubo primitivo através de código puro (não precisa de Prefabs)
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(roomRoot, true);
            
            // Rebaixa o chão pela metade da sua espessura para que o topo fique exatamente no nível Y = 0 (o nível dos pés dos móveis)
            floor.transform.position = new Vector3(0f, -floorThickness / 2f, 0f);
            
            // Estica o cubo para as medidas exatas do utilizador
            floor.transform.localScale = new Vector3(room.Width, floorThickness, room.Length);

            // Coloca o chão na sua camada física correta para o raio do rato não o confundir com mobília
            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer != -1) floor.layer = groundLayer;

            Renderer rend = floor.GetComponent<Renderer>();
            if (floorMaterial != null) rend.material = floorMaterial;

            // --- INJEÇÃO DINÂMICA DE INTELIGÊNCIA ---
            // Como este cubo foi gerado do nada, ele não tem os nossos scripts de seleção.
            // Aqui injetamos o 'PlaceableObject' e configuramo-lo via código!
            var placeable = floor.AddComponent<InteriorPlanner.Systems.Placement.PlaceableObject>();
            placeable.originalPrefabID = "Floor"; 
            
            // Dizemos ao PlaceableObject exatamente qual é o Renderer que ele tem de pintar
            // e bloqueamos o movimento (false, false, false) para o utilizador não arrastar o chão!
            placeable.Configure(InteriorPlanner.Systems.Placement.PlaceableObjectType.Furniture, false, false, false, false, new Renderer[] { rend });
            
            // Injeta a etiqueta para permitir que o balde de tinta pinte este chão
            floor.AddComponent<InteriorPlanner.Systems.Tools.Paintable>();
        }

        private void CreateWalls(RoomData room)
        {
            // Matemática espacial: A parede é movida para metade da largura/comprimento total da sala para ficar nas "bordas"
            // e elevada para metade da altura para ficar alinhada com o chão.
            CreateWall("Wall_Front", new Vector3(0f, room.Height / 2f, room.Length / 2f), new Vector3(room.Width, room.Height, wallThickness));
            CreateWall("Wall_Back", new Vector3(0f, room.Height / 2f, -room.Length / 2f), new Vector3(room.Width, room.Height, wallThickness));
            
            // As paredes laterais trocam as proporções matemáticas (A largura vira espessura, o comprimento estica)
            CreateWall("Wall_Left", new Vector3(-room.Width / 2f, room.Height / 2f, 0f), new Vector3(wallThickness, room.Height, room.Length));
            CreateWall("Wall_Right", new Vector3(room.Width / 2f, room.Height / 2f, 0f), new Vector3(wallThickness, room.Height, room.Length));
        }

        private void CreateWall(string wallName, Vector3 position, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = wallName;
            wall.transform.SetParent(roomRoot, true);
            wall.transform.position = position;
            wall.transform.localScale = scale;

            // Define a camada exclusiva de parede para que as portas e janelas a reconheçam como hospedeiro (Magnetismo)
            int wallLayer = LayerMask.NameToLayer("Wall");      
            if (wallLayer != -1) wall.layer = wallLayer;

            Renderer rend = wall.GetComponent<Renderer>();
            if (wallMaterial != null) rend.material = wallMaterial;

            // --- INJEÇÃO DINÂMICA DE INTELIGÊNCIA ---
            var placeable = wall.AddComponent<InteriorPlanner.Systems.Placement.PlaceableObject>();
            placeable.originalPrefabID = wallName; 
            
            // Paredes fixas, sem possibilidade de serem movidas pelo rato
            placeable.Configure(InteriorPlanner.Systems.Placement.PlaceableObjectType.Furniture, false, false, false, false, new Renderer[] { rend });
            
            // Injeta a permissão de pintura
            wall.AddComponent<InteriorPlanner.Systems.Tools.Paintable>();
        }

        private void ClearRoom()
        {
            // Iteração inversa (de trás para a frente) é mandatória ao destruir filhos 
            // de um Transform para evitar que os índices da lista "escorreguem" causando erros.
            for (int i = roomRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(roomRoot.GetChild(i).gameObject);
            }
        }
    }
}