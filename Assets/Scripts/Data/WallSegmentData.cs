using UnityEngine;

namespace InteriorPlanner.Data
{
    /// <summary>
    /// Representa matematicamente uma única linha de parede na planta do projeto.
    /// Esta classe funciona como um "Vetor" num software de CAD, armazenando apenas 
    /// o início e o fim da linha num espaço 2D (vista de cima).
    /// </summary>
    [System.Serializable]
    public class WallSegmentData
    {
        // Ponto de origem da parede no chão (Eixos X e Z disfarçados de X e Y no Vector2)
        public Vector2 StartPoint;
        
        // Ponto de término da parede
        public Vector2 EndPoint;

        /// <summary>
        /// Construtor da classe. Facilita a criação rápida de paredes via código
        /// quando o utilizador clica e arrasta o rato pelo ecrã.
        /// </summary>
        public WallSegmentData(Vector2 startPoint, Vector2 endPoint)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
        }
    }
}