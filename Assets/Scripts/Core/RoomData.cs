namespace InteriorPlanner.Data
{
    /// <summary>
    /// Estrutura de dados simples focada apenas nas dimensões físicas do espaço.
    /// Serializada para poder ser guardada nos ficheiros de save do Windows.
    /// </summary>
    [System.Serializable]
    public class RoomData
    {
        public float Width;  // Largura (Eixo X no espaço 3D)
        public float Length; // Profundidade/Comprimento (Eixo Z no espaço 3D)
        public float Height; // Altura do teto (Eixo Y no espaço 3D)

        public RoomData(float width, float length, float height)
        {
            Width = width;
            Length = length;
            Height = height;
        }
    }
}