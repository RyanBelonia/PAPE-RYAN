using System.Collections.Generic;

namespace InteriorPlanner.Data
{
    /// <summary>
    /// Estrutura central de dados que armazena a planta baixa (Floor Plan) do projeto.
    /// A etiqueta [System.Serializable] é essencial: permite que a Unity converta toda esta classe
    /// e a sua lista de paredes num ficheiro de texto JSON para guardar e carregar projetos.
    /// </summary>
    [System.Serializable]
    public class FloorPlanData
    {
        // Lista dinâmica que guarda todos os segmentos de parede desenhados pelo utilizador.
        // Em vez de guardar objetos 3D pesados, guardamos apenas as coordenadas matemáticas.
        public List<WallSegmentData> Walls = new List<WallSegmentData>();
        
        // Definições globais de arquitetura que serão usadas na hora de gerar a malha 3D (Extrusão)
        public float WallHeight = 2.8f;      // Altura padrão do teto em metros
        public float WallThickness = 0.15f;  // Espessura padrão do tijolo/parede em metros
    }
}